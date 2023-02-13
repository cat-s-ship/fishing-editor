module Index
open Elmish
open Feliz
open Feliz.UseElmish
open Feliz.Router

open Components.Utils.ResultExt
open Commons

type Page =
    | ItemsListPage
    | NotFoundPage of string

type Msg =
    | ChangePage of Page
    | ItemsListMsg of ItemsList.Msg

type State =
    {
        ItemsListState: ItemsList.State
        CurrentPage: Page
    }

let parseRoute currentUrl =
    match currentUrl with
    | [] -> ItemsListPage
    | Routes.ItemsListPageRoute::_ -> ItemsListPage
    | xs ->
        NotFoundPage (sprintf "%A" xs)

let init rawRoute =
    let itemsListState, itemsListMsg =
        Api.LocalItems.LocalStorage.load ()
        |> Result.defaultWith (fun errMsg ->
            failwithf "%s" errMsg // TODO
        )
        |> ItemsList.init

    let state =
        {
            CurrentPage = parseRoute rawRoute
            ItemsListState = itemsListState
        }
    let msg =
        Cmd.batch [
            itemsListMsg |> Cmd.map ItemsListMsg
        ]
    state, msg

let update (msg: Msg) (state: State) =
    match msg with
    | ItemsListMsg msg ->
        let state', msg = ItemsList.update msg state.ItemsListState
        let state =
            { state with
                ItemsListState = state'
            }
        state, msg |> Cmd.map ItemsListMsg
    | ChangePage page ->
        let state =
            { state with
                CurrentPage = page
            }
        state, Cmd.none

let view state dispatch =
    React.router [
        router.onUrlChanged (parseRoute >> ChangePage >> dispatch)
        router.children [
            match state.CurrentPage with
            | ItemsListPage ->
                ItemsList.view state.ItemsListState (ItemsListMsg >> dispatch)
            | NotFoundPage query ->
                Html.h1 (sprintf "Not found %A" query)
        ]
    ]

[<ReactComponent>]
let Router () =
    let state, dispatch = React.useElmish(init, update, Router.currentUrl())
    view state dispatch

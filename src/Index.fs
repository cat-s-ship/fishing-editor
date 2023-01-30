module Index
open Elmish
open Feliz
open Feliz.UseElmish
open Feliz.Router

open Components.Utils.ResultExt
open Commons

type Page =
    | ItemsListPage
    | ItemAdderPage
    | NotFoundPage of string

type Msg =
    | ChangePage of Page
    | ItemsListMsg of ItemsList.Msg
    | ItemsAdderMsg of ItemAdder.Msg

type State =
    {
        ItemsListState: ItemsList.State
        CurrentPage: Page
        ItemsAdderState: ItemAdder.State
    }

let parseRoute currentUrl =
    match currentUrl with
    | [] -> ItemsListPage
    | Routes.ItemsListPageRoute::_ -> ItemsListPage
    | Routes.ItemAdderPageRoute::_ -> ItemAdderPage
    | xs ->
        NotFoundPage (sprintf "%A" xs)

let init rawRoute =
    let itemsListState, itemsListMsg =
        Api.LocalItems.load ()
        |> Result.defaultWith (fun errMsg ->
            failwithf "%s" errMsg // TODO
        )
        |> ItemsList.init
    let itemsAdderState, itemsAdderMsg =
        ItemAdder.init ()
    let state =
        {
            CurrentPage = parseRoute rawRoute
            ItemsListState = itemsListState
            ItemsAdderState = itemsAdderState
        }
    let msg =
        Cmd.batch [
            itemsListMsg |> Cmd.map ItemsListMsg
            itemsAdderMsg |> Cmd.map ItemsAdderMsg
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
    | ItemsAdderMsg msg ->
        let navigateToItemsListPage () =
            Cmd.navigate(Routes.ItemsListPageRoute, HistoryMode.ReplaceState)

        match ItemAdder.update msg state.ItemsAdderState with
        | ItemAdder.UpdateRes (state', msg) ->
            let state =
                { state with
                    ItemsAdderState = state'
                }
            state, msg |> Cmd.map ItemsAdderMsg
        | ItemAdder.SubmitRes newItem ->
            let cmd =
                Cmd.ofMsg (ItemsList.SetItem newItem)
                |> Cmd.map ItemsListMsg
            let cmd =
                Cmd.batch [
                    cmd
                    navigateToItemsListPage ()
                ]
            state, cmd
        | ItemAdder.CancelRes ->
            let cmd =
                navigateToItemsListPage ()

            state, cmd
    | ChangePage page ->
        let state =
            { state with
                CurrentPage = page
            }
        state, Cmd.none

let router = React.functionComponent(fun () ->
    let state, dispatch = React.useElmish(init, update, Router.currentUrl())

    React.router [
        router.onUrlChanged (parseRoute >> ChangePage >> dispatch)
        router.children [
            match state.CurrentPage with
            | ItemsListPage ->
                ItemsList.view state.ItemsListState (ItemsListMsg >> dispatch)
            | ItemAdderPage ->
                ItemAdder.view state.ItemsAdderState (ItemsAdderMsg >> dispatch)
            | NotFoundPage query ->
                Html.h1 (sprintf "Not found %A" query)
        ]
    ]
)

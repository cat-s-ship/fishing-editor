module ItemsList
open Elmish
open Feliz
open Feliz.Router
open Fable.Builders.Fela

open Components
open Commons
open Api

type Msg =
    | StartNewItem
    | SetItem of Item
    | RemoveItem of Item
    | UploadElmishMsg of Upload.Elmish.Msg
    | FilterByName of pattern: string

module LocalItems =
    let get itemId localItems =
        match LocalItems.get itemId localItems with
        | Some item -> Ok item
        | None -> Error (sprintf "%A not found" itemId)

type State =
    {
        LocalItems: LocalItems
        UploadElmishState: Upload.Elmish.State<LocalItems>
        FilterByNamePattern: string
    }
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
[<RequireQualifiedAccess>]
module State =
    let getAllItems state =
        state.LocalItems.Cache
        |> Seq.map (fun (KeyValue(_, item)) -> item)
        |> Array.ofSeq
    let getItem itemId state =
        LocalItems.get itemId state.LocalItems

let init localItems =
    let state =
        {
            LocalItems = localItems
            UploadElmishState =
                Upload.Elmish.init
            FilterByNamePattern = ""
        }
    state, Cmd.none

let update (msg: Msg) (state: State) =
    match msg with
    | SetItem item ->
        let state =
            { state with
                LocalItems =
                    state.LocalItems
                    |> LocalItems.set item
            }
        state, Cmd.none
    | RemoveItem e ->
        let state =
            { state with
                LocalItems =
                    state.LocalItems
                    |> LocalItems.remove e.Id
            }
        state, Cmd.none
    | StartNewItem ->
        let newItemId = ItemId.create ()
        let state =
            { state with
                LocalItems =
                    state.LocalItems
                    |> LocalItems.insert newItemId id
            }
        state, Cmd.none
    | UploadElmishMsg msg ->
        match Upload.Elmish.update LocalItems.import msg state.UploadElmishState with
        | Upload.Elmish.UpdateRes(state') ->
            let state =
                { state with
                    UploadElmishState = state'
                }
            state, Cmd.none
        | Upload.Elmish.ImportResult(state', localItems) ->
            let state, cmd = init localItems
            let state =
                { state with
                    UploadElmishState = state'
                }
            state, cmd

    | FilterByName pattern ->
        let state =
            { state with
                FilterByNamePattern = pattern
            }
        state, Cmd.none

let view (state: State) (dispatch: Msg -> unit) =
    Html.div [
        Html.div [
            Download.download {|
                description = "save"
                accept = "application/json"
                filename = "events.json"
                getData = fun () -> LocalItems.encode state.LocalItems
                onDone = id
            |}

            Fela.RendererProvider {
                renderer (Fela.createRenderer ())

                Upload.Elmish.view state.UploadElmishState (UploadElmishMsg >> dispatch)
            }
        ]

        Html.div [
            Html.button [
                prop.text "add"
                prop.onClick (fun e ->
                    dispatch StartNewItem
                )
            ]
        ]

        let id = "filterByName"
        Html.div [
            Html.label [
                prop.text "Отфильтровать по имени: "
                prop.htmlFor id
            ]
            Html.input [
                prop.id id
                prop.onChange (fun (value: string) ->
                    dispatch (FilterByName value)
                )
            ]
        ]

        let filterByNamePattern = state.FilterByNamePattern.ToLower()
        Html.div [
            let items =
                state.LocalItems.Cache
                |> Seq.filter (fun (KeyValue(itemId, item)) ->
                    item.Name.ToLower().Contains filterByNamePattern
                )
                |> Seq.sortByDescending (fun (KeyValue(itemId, item)) ->
                    item.Created
                    |> Option.defaultValue System.DateTime.MinValue
                )
            for KeyValue(itemId, item) in items do
                Html.div [
                    prop.key itemId
                    prop.children [
                        ItemView.Component {|
                            Item = item
                            GetItem = fun itemId ->
                                State.getItem itemId state
                            GetAllItems = fun () ->
                                State.getAllItems state
                            UpdateItem = fun item ->
                                SetItem item |> dispatch
                            RemoveCurrentItem = fun () ->
                                RemoveItem item |> dispatch
                        |}
                    ]
                ]
        ]
    ]

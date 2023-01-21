module ItemsList
open Elmish
open Feliz
open Feliz.Router

open Components
open Commons
open Api

type Msg =
    | StartNewItem
    | SetItem of Item
    | RemoveItem of Item
    | ItemViewMsg of ItemId * ItemView.Msg
    | UploadElmishMsg of Upload.Elmish.Msg

type State =
    {
        LocalItems: LocalItems
        Items: Map<ItemId, ItemView.State>
        UploadElmishState: Upload.Elmish.State<LocalItems>
    }

let init localItems =
    let state =
        {
            LocalItems = localItems
            Items =
                localItems.Cache
                |> Map.map (fun _ e ->
                    ItemView.init e
                )
            UploadElmishState =
                Upload.Elmish.init
        }
    state, Cmd.none

let update (msg: Msg) (state: State) =
    match msg with
    | SetItem e ->
        let state =
            { state with
                LocalItems =
                    state.LocalItems
                    |> LocalItems.set e
                Items =
                    Map.add e.Id (ItemView.init e) state.Items
            }
        state, Cmd.none
    | RemoveItem e ->
        let state =
            { state with
                LocalItems =
                    state.LocalItems
                    |> LocalItems.remove e.Id
                Items =
                    Map.remove e.Id state.Items
            }
        state, Cmd.none
    | StartNewItem ->
        state, Cmd.navigate [| Routes.ItemAdderPageRoute |]
    | ItemViewMsg (dateTime, msg) ->
        match Map.tryFind dateTime state.Items with
        | Some itemState ->
            match ItemView.update msg itemState with
            | ItemView.UpdateRes(state', msg) ->
                let state =
                    { state with
                        Items =
                            Map.add dateTime state' state.Items
                    }
                state, msg |> Cmd.map (fun cmd -> ItemViewMsg(dateTime, cmd))
            | ItemView.UpdateItemRes e ->
                state, Cmd.ofMsg (SetItem e)
            | ItemView.RemoveRes ->
                state, Cmd.ofMsg (RemoveItem itemState.Item)
        | None ->
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

let view (state: State) (dispatch: Msg -> unit) =
    Html.div [
        Html.div [
            Download.download {|
                description = "save"
                accept = "application/json"
                filename = "events.json"
                getData = fun () -> LocalItems.export state.LocalItems
                onDone = id
            |}

            Upload.Elmish.view state.UploadElmishState (UploadElmishMsg >> dispatch)
        ]

        Html.div [
            Html.button [
                prop.text "add"
                prop.onClick (fun e ->
                    dispatch StartNewItem
                )
            ]
        ]
        Html.div [
            yield!
                state.Items
                |> Seq.sortByDescending (fun (KeyValue(dateTime, _)) -> dateTime)
                |> Seq.map (fun (KeyValue(k, e)) ->
                    ItemView.view e (fun msg -> ItemViewMsg(k, msg) |> dispatch)
                )
        ]
    ]

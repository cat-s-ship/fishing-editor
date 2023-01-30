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
    let items, cmd =
        localItems.Cache
        |> Seq.mapFold
            (fun m (KeyValue(itemId, item)) ->
                let get itemId =
                    match LocalItems.get itemId localItems with
                    | Some item -> Ok item
                    | None -> Error (sprintf "%A not found" itemId)

                let state, cmd = ItemView.init get item
                let m = Map.add itemId state m
                cmd |> Cmd.map (fun cmd -> ItemViewMsg (itemId, cmd)), m
            )
            Map.empty
        |> fun (cmds, state) ->
            state, Cmd.batch cmds

    let state =
        {
            LocalItems = localItems
            Items = items
            UploadElmishState =
                Upload.Elmish.init
        }
    state, cmd

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
        state, Cmd.navigate [| Routes.ItemAdderPageRoute |]
    | ItemViewMsg (itemId, msg) ->
        match Map.tryFind itemId state.Items with
        | Some itemState ->
            let getAllItems () =
                state.LocalItems.Cache
                |> Seq.map (fun (KeyValue(_, item)) -> item)
                |> Array.ofSeq

            let state', cmd, event = ItemView.update getAllItems msg itemState

            let state =
                { state with
                    Items =
                        Map.add itemId state' state.Items
                }
            let cmd =
                cmd |> Cmd.map (fun cmd -> ItemViewMsg(itemId, cmd))

            match event with
            | Some x ->
                match x with
                | ItemView.UpdateItemRes e ->
                    let cmd =
                        Cmd.batch [
                            cmd
                            Cmd.ofMsg (SetItem e)
                        ]
                    state, cmd
                | ItemView.RemoveRes ->
                    let state =
                        { state with
                            Items =
                                Map.remove itemId state.Items
                        }

                    let cmd =
                        Cmd.batch [
                            cmd
                            Cmd.ofMsg (RemoveItem itemState.Item)
                        ]
                    state, cmd
            | None ->
                state, cmd

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

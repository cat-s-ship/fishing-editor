module ItemsList
open Elmish
open Feliz
open Feliz.Router

open Commons
open Api

module SaveAndLoad =
    open Elmish
    open Feliz

    type Msg =
        | Import of string

    type State =
        {
            ImportResult: Result<LocalItems, string> Deferred
        }

    let init =
        let state =
            {
                ImportResult = NotStartedYet
            }
        state

    type UpdateResult =
        | UpdateRes of State
        | ImportResult of State * LocalItems

    let update (msg: Msg) (state: State) =
        match msg with
        | Import rawJson ->
            let res =
                LocalItems.import rawJson

            let state =
                { state with
                    ImportResult =
                        Resolved res
                }
            match res with
            | Ok resultValue ->
                ImportResult (state, resultValue)
            | Error _ ->
                UpdateRes state

    let view (state: State) (dispatch: Msg -> unit) =
        Html.div [
            Upload.upload {|
                description = "Load"
                accept = "application/json"
                cb = Import >> dispatch
            |}

            match state.ImportResult with
            | NotStartedYet -> ()
            | InProgress ->
                Html.div [
                    prop.text "Loading"
                ]
            | Resolved r ->
                match r with
                | Error errMsg ->
                    Html.div [
                        prop.style [
                            style.color "red"
                        ]
                        prop.text errMsg
                    ]
                | _ ->
                    ()
        ]

type Msg =
    | StartNewItem
    | SetItem of Item
    | RemoveItem of Item
    | ItemViewMsg of ItemId * ItemView.Msg
    | SaveAndLoadMsg of SaveAndLoad.Msg
    | Export

type State =
    {
        LocalItems: LocalItems
        Items: Map<ItemId, ItemView.State>
        SaveAndLoadState: SaveAndLoad.State
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
            SaveAndLoadState =
                SaveAndLoad.init
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
    | SaveAndLoadMsg msg ->
        match SaveAndLoad.update msg state.SaveAndLoadState with
        | SaveAndLoad.UpdateRes(state') ->
            let state =
                { state with
                    SaveAndLoadState = state'
                }
            state, Cmd.none
        | SaveAndLoad.ImportResult(state', localItems) ->
            let state, cmd = init localItems
            let state =
                { state with
                    SaveAndLoadState = state'
                }
            state, cmd
    | Export ->
        LocalItems.export state.LocalItems
        |> saveToDisc "application/json" "items.json"

        state, Cmd.none

let view (state: State) (dispatch: Msg -> unit) =
    Html.div [
        Html.div [
            Html.button [
                prop.text "save"
                prop.onClick (fun _ ->
                    dispatch Export
                )
            ]

            SaveAndLoad.view state.SaveAndLoadState (SaveAndLoadMsg >> dispatch)
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

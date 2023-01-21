module EventsList
open Elmish
open Feliz
open Feliz.Router

open Commons
open Api

module SaveAndLoadEvents =
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
    | StartNewEvent
    | SetEvent of Item
    | RemoveEvent of Item
    | EventViewMsg of ItemId * ItemView.Msg
    | SaveAndLoadEventsMsg of SaveAndLoadEvents.Msg
    | Export

type State =
    {
        LocalItems: LocalItems
        Events: Map<ItemId, ItemView.State>
        SaveAndLoadEventsState: SaveAndLoadEvents.State
    }

let init localItems =
    let state =
        {
            LocalItems = localItems
            Events =
                localItems.Cache
                |> Map.map (fun _ e ->
                    ItemView.init e
                )
            SaveAndLoadEventsState =
                SaveAndLoadEvents.init
        }
    state, Cmd.none

let update (msg: Msg) (state: State) =
    match msg with
    | SetEvent e ->
        let state =
            { state with
                LocalItems =
                    state.LocalItems
                    |> LocalItems.set e
                Events =
                    Map.add e.Id (ItemView.init e) state.Events
            }
        state, Cmd.none
    | RemoveEvent e ->
        let state =
            { state with
                LocalItems =
                    state.LocalItems
                    |> LocalItems.remove e.Id
                Events =
                    Map.remove e.Id state.Events
            }
        state, Cmd.none
    | StartNewEvent ->
        state, Cmd.navigate [| Routes.EventsAdderPageRoute |]
    | EventViewMsg (dateTime, msg) ->
        match Map.tryFind dateTime state.Events with
        | Some eventEventState ->
            match ItemView.update msg eventEventState with
            | ItemView.UpdateRes(state', msg) ->
                let state =
                    { state with
                        Events =
                            Map.add dateTime state' state.Events
                    }
                state, msg |> Cmd.map (fun cmd -> EventViewMsg(dateTime, cmd))
            | ItemView.UpdateEventRes e ->
                state, Cmd.ofMsg (SetEvent e)
            | ItemView.RemoveRes ->
                state, Cmd.ofMsg (RemoveEvent eventEventState.Event)
        | None ->
            state, Cmd.none
    | SaveAndLoadEventsMsg msg ->
        match SaveAndLoadEvents.update msg state.SaveAndLoadEventsState with
        | SaveAndLoadEvents.UpdateRes(state') ->
            let state =
                { state with
                    SaveAndLoadEventsState = state'
                }
            state, Cmd.none
        | SaveAndLoadEvents.ImportResult(state', localItems) ->
            let state, cmd = init localItems
            let state =
                { state with
                    SaveAndLoadEventsState = state'
                }
            state, cmd
    | Export ->
        LocalItems.export state.LocalItems
        |> saveToDisc "application/json" "events.json"

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

            SaveAndLoadEvents.view state.SaveAndLoadEventsState (SaveAndLoadEventsMsg >> dispatch)
        ]

        Html.div [
            Html.button [
                prop.text "add"
                prop.onClick (fun e ->
                    dispatch StartNewEvent
                )
            ]
        ]
        Html.div [
            yield!
                state.Events
                |> Seq.sortByDescending (fun (KeyValue(dateTime, _)) -> dateTime)
                |> Seq.map (fun (KeyValue(k, e)) ->
                    ItemView.view e (fun msg -> EventViewMsg(k, msg) |> dispatch)
                )
        ]
    ]

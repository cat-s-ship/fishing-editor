module ItemView
open Elmish
open Feliz

open Utils.ElmishExt.Elmish
open Commons

module EditorWithStart =
    type Msg<'InitData, 'EditorMsg> =
        | StartEdit of 'InitData
        | EditorMsg of 'EditorMsg
        | Submit of 'InitData
        | Cancel

    type State<'EditorState> =
        {
            EditorState: 'EditorState option
        }

    let init () =
        {
            EditorState = None
        }

    let update
        (editorInit: ComponentInit<'InitData, 'EditorState, 'EditorMsg>)
        (editorUpdate: ComponentUpdate<'EditorState, 'EditorMsg, unit>)
        (msg: Msg<'InitData, 'EditorMsg>)
        (state: State<'EditorState>) =

        match msg with
        | StartEdit init ->
            let state', msg = editorInit init
            let state =
                { state with
                    EditorState = Some state'
                }

            UpdateResult.create
                state
                (msg |> Cmd.map EditorMsg)
                None
        | EditorMsg msg ->
            match state.EditorState with
            | Some descripitionEditorState ->
                let state', msg, _ = editorUpdate msg descripitionEditorState
                let state =
                    { state with
                        EditorState = Some state'
                    }

                UpdateResult.create
                    state
                    (msg |> Cmd.map EditorMsg)
                    None

            | None ->
                UpdateResult.create
                    state
                    Cmd.none
                    None
        | Submit description ->
            let state =
                { state with
                    EditorState = None
                }

            UpdateResult.create
                state
                Cmd.none
                (Some description)
        | Cancel ->
            let state =
                { state with
                    EditorState = None
                }

            UpdateResult.create
                state
                Cmd.none
                None

    type Events<'Data> =
        {
            OnSubmit: 'Data -> unit
            OnCancel: unit -> unit
        }

    type EditorView<'Data, 'State, 'Cmd> =
        Events<'Data> -> 'State -> ('Cmd -> unit) -> ReactElement

    let view
        (editorView: EditorView<'InitData, 'EditorState, 'EditorMsg>)
        getData
        (state: State<'EditorState>)
        (dispatch: Msg<'InitData, 'EditorMsg> -> unit) =

        match state.EditorState with
        | Some editorState ->
            let onSubmitAndOnCancel =
                {
                    OnSubmit = fun data ->
                        Submit data |> dispatch
                    OnCancel = fun () ->
                        Cancel |> dispatch
                }
            editorView onSubmitAndOnCancel editorState (EditorMsg >> dispatch)
        | None ->
            let data = getData ()
            Html.span [
                Html.div [
                    prop.textf "%A" data
                ]
                Html.button [
                    prop.text "Edit"
                    prop.onClick (fun _ ->
                        dispatch (StartEdit data)
                    )
                ]
            ]

module DescripitionEditor =
    type Msg =
        | SetDescription of string

    type State =
        {
            Description: string
        }

    type InitData = string

    let init (description: InitData) =
        let state =
            {
                Description = description
            }
        state, Cmd.none

    let update (msg: Msg) (state: State) =
        match msg with
        | SetDescription description ->
            let state =
                { state with
                    Description = description
                }
            UpdateResult.create
                state
                Cmd.none
                None

    let view isInputEnabled (events: EditorWithStart.Events<_>) (state: State) (dispatch: Msg -> unit) =
        Html.div [
            if isInputEnabled then
                Html.input [
                    prop.value state.Description
                    prop.onInput (fun e ->
                        match e.target :?> Browser.Types.HTMLInputElement with
                        | null ->
                            failwithf "e.target :?> Browser.Types.HTMLInputElement is null"
                        | inputElement ->
                            (SetDescription inputElement.value) |> dispatch
                    )
                ]

            Html.div [
                Html.button [
                    prop.onClick (fun _ -> events.OnSubmit state.Description)
                    prop.text "Submit"
                ]
                Html.button [
                    prop.onClick (fun _ -> events.OnCancel ())
                    prop.text "Cancel"
                ]
            ]
        ]

type Msg =
    | DescripitionEditorMsg of EditorWithStart.Msg<DescripitionEditor.InitData, DescripitionEditor.Msg>
    | NameEditorMsg of EditorWithStart.Msg<DescripitionEditor.InitData, DescripitionEditor.Msg>
    | StartRemove
    | SetRemove of DescripitionEditor.Msg
    | RemoveR
    | CancelRemove

type State =
    {
        Item: Item
        NameEditorState: EditorWithStart.State<DescripitionEditor.State>
        DescripitionEditorState: EditorWithStart.State<DescripitionEditor.State>
        IsStartedRemove: DescripitionEditor.State Option
    }

let init item =
    let state =
        {
            Item = item
            NameEditorState = EditorWithStart.init ()
            DescripitionEditorState = EditorWithStart.init ()
            IsStartedRemove = None
        }
    state

type UpdateResult =
    | UpdateRes of State * Cmd<Msg>
    | UpdateItemRes of Item
    | RemoveRes

let update (msg: Msg) (state: State) =
    match msg with
    | DescripitionEditorMsg msg ->
        let state', cmd, submit =
            EditorWithStart.update
                DescripitionEditor.init
                DescripitionEditor.update
                msg
                state.DescripitionEditorState
        match submit with
        | None ->
            let state =
                { state with
                    DescripitionEditorState = state'
                }
            let msg = cmd |> Cmd.map DescripitionEditorMsg
            (state, msg)
            |> UpdateRes
        | Some x ->
            { state.Item with
                Description = x
            }
            |> UpdateItemRes

    | NameEditorMsg msg ->
        let state', cmd, submit =
            EditorWithStart.update
                DescripitionEditor.init
                DescripitionEditor.update
                msg
                state.NameEditorState
        match submit with
        | None ->
            let state =
                { state with
                    NameEditorState = state'
                }
            let msg = cmd |> Cmd.map NameEditorMsg
            (state, msg)
            |> UpdateRes
        | Some x ->
            { state.Item with
                Name = x
            }
            |> UpdateItemRes

    | StartRemove ->
        let state', msg = DescripitionEditor.init state.Item.Description
        let state =
            { state with
                IsStartedRemove = Some state'
            }
        (state, msg |> Cmd.map SetRemove)
        |> UpdateRes
    | SetRemove msg ->
        match state.IsStartedRemove with
        | Some descripitionEditorState ->
            let state', msg, _ =  DescripitionEditor.update msg descripitionEditorState
            let state =
                { state with
                    IsStartedRemove = Some state'
                }
            (state, msg |> Cmd.map SetRemove)
            |> UpdateRes
        | None ->
            (state, Cmd.none)
            |> UpdateRes
    | RemoveR ->
        RemoveRes
    | CancelRemove ->
        let state =
            { state with
                IsStartedRemove = None
            }
        (state, Cmd.none)
        |> UpdateRes

let view (state: State) (dispatch: Msg -> unit) =
    Html.div [
        Html.div [
            prop.textf "%A" state.Item
        ]

        Html.div [
            Html.span [
                prop.text "Name:"
            ]
            EditorWithStart.view
                (DescripitionEditor.view true)
                (fun () -> state.Item.Name)
                state.NameEditorState
                (NameEditorMsg >> dispatch)
        ]

        Html.div [
            Html.span [
                prop.text "Description:"
            ]
            EditorWithStart.view
                (DescripitionEditor.view true)
                (fun () -> state.Item.Description)
                state.DescripitionEditorState
                (DescripitionEditorMsg >> dispatch)
        ]

        match state.IsStartedRemove with
        | Some descriptionEditorState ->
            let events =
                {
                    EditorWithStart.OnSubmit = fun data ->
                        dispatch RemoveR
                    EditorWithStart.OnCancel = fun () ->
                        dispatch CancelRemove
                }
            DescripitionEditor.view
                false
                events
                descriptionEditorState
                (SetRemove >> dispatch)
        | None ->
            Html.button [
                prop.text "Remove"
                prop.onClick (fun _ ->
                    dispatch StartRemove
                )
            ]
    ]

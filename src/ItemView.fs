module ItemView
open Elmish
open Feliz

open Utils.ElmishExt.Elmish
open Commons

module EditorWithStart =
    type Msg<'InitData, 'EditorMsg> =
        | StartEdit of 'InitData
        | EditorMsg of 'EditorMsg

    type State<'EditorState> =
        {
            EditorState: 'EditorState option
        }

    let init () =
        {
            EditorState = None
        }

    type EditorResult<'Data> =
        | Submit of 'Data
        | Cancel

    let update
        (editorInit: ComponentInit<'InitData, 'EditorState, 'EditorMsg>)
        (editorUpdate: ComponentUpdate<'EditorState, 'EditorMsg, EditorResult<'InitData>>)
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
                let state', msg, res = editorUpdate msg descripitionEditorState
                match res with
                | None ->
                    let state =
                        { state with
                            EditorState = Some state'
                        }
                    UpdateResult.create
                        state
                        (msg |> Cmd.map EditorMsg)
                        None
                | Some res ->
                    let state =
                        { state with
                            EditorState = None
                        }
                    match res with
                    | Submit x ->
                        UpdateResult.create
                            state
                            (msg |> Cmd.map EditorMsg)
                            (Some x)
                    | Cancel ->
                        UpdateResult.create
                            state
                            (msg |> Cmd.map EditorMsg)
                            None
            | None ->
                UpdateResult.create
                    state
                    Cmd.none
                    None

    type Events<'Data> =
        {
            OnSubmit: 'Data -> unit
            OnCancel: unit -> unit
        }

    let view
        (text: string)
        (editorView: ComponentView<'EditorState, 'EditorMsg>)
        getData
        (state: State<'EditorState>)
        (dispatch: Msg<'InitData, 'EditorMsg> -> unit) =

        match state.EditorState with
        | Some editorState ->
            editorView editorState (EditorMsg >> dispatch)
        | None ->
            let data = getData ()
            Html.span [
                Html.div [
                    prop.textf "%A" data
                ]
                Html.button [
                    prop.text text
                    prop.onClick (fun _ ->
                        dispatch (StartEdit data)
                    )
                ]
            ]

module DescripitionEditor =
    type Msg =
        | SetDescription of string
        | Submit
        | Cancel

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
        | Submit ->
            UpdateResult.create
                state
                Cmd.none
                (Some (EditorWithStart.Submit state.Description))

        | Cancel ->
            UpdateResult.create
                state
                Cmd.none
                (Some EditorWithStart.Cancel)

    let view isInputEnabled (state: State) (dispatch: Msg -> unit) =
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
                    prop.onClick (fun _ -> dispatch Submit)
                    prop.text "Submit"
                ]
                Html.button [
                    prop.onClick (fun _ -> dispatch Cancel)
                    prop.text "Cancel"
                ]
            ]
        ]

type Msg =
    | DescripitionEditorMsg of EditorWithStart.Msg<DescripitionEditor.InitData, DescripitionEditor.Msg>
    | NameEditorMsg of EditorWithStart.Msg<DescripitionEditor.InitData, DescripitionEditor.Msg>
    | SetRemove of EditorWithStart.Msg<DescripitionEditor.InitData, DescripitionEditor.Msg>

type State =
    {
        Item: Item
        NameEditorState: EditorWithStart.State<DescripitionEditor.State>
        DescripitionEditorState: EditorWithStart.State<DescripitionEditor.State>
        IsStartedRemove: EditorWithStart.State<DescripitionEditor.State>
    }

let init item =
    let state =
        {
            Item = item
            NameEditorState = EditorWithStart.init ()
            DescripitionEditorState = EditorWithStart.init ()
            IsStartedRemove = EditorWithStart.init ()
        }
    state

type UpdateResultEvent =
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
            UpdateResult.create
                state
                msg
                None
        | Some x ->
            let item =
                { state.Item with
                    Description = x
                }
            UpdateResult.create
                state
                Cmd.none
                (Some <| UpdateItemRes item)
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
            UpdateResult.create
                state
                msg
                None
        | Some x ->
            let item =
                { state.Item with
                    Name = x
                }

            UpdateResult.create
                state
                Cmd.none
                (Some <| UpdateItemRes item)

    | SetRemove msg ->
        let state', msg, res =
            EditorWithStart.update
                DescripitionEditor.init
                DescripitionEditor.update
                msg
                state.IsStartedRemove

        let state =
            { state with
                IsStartedRemove = state'
            }
        match res with
        | None ->
            UpdateResult.create
                state
                (msg |> Cmd.map SetRemove)
                None
        | Some description ->
            UpdateResult.create
                state
                Cmd.none
                (Some RemoveRes)

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
                "Edit"
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
                "Edit"
                (DescripitionEditor.view true)
                (fun () -> state.Item.Description)
                state.DescripitionEditorState
                (DescripitionEditorMsg >> dispatch)
        ]

        EditorWithStart.view
            "Remove"
            (DescripitionEditor.view false)
            (fun () -> "")
            state.IsStartedRemove
            (SetRemove >> dispatch)
    ]

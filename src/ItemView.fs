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

    type UpdateResult2<'InitData, 'EditorState, 'EditorMsg> =
        | UpdateRes2 of 'EditorState * Cmd<'EditorMsg>
        | SubmitRes2 of 'InitData
        | CancelRes2

    type ComponentUpdate<'InitData, 'State, 'Cmd> = 'Cmd -> 'State -> UpdateResult2<'InitData, 'State, 'Cmd>

    let init () =
        {
            EditorState = None
        }

    type UpdateResult<'InitData, 'EditorState, 'EditorMsg> =
        {
            State: State<'EditorState>
            Cmd: Cmd<Msg<'InitData, 'EditorMsg>>
            Submit: 'InitData option
        }

    let update
        (editorInit: ComponentInit<'InitData, 'EditorState, 'EditorMsg>)
        (editorUpdate: ComponentUpdate<'InitData, 'EditorState, 'EditorMsg>)
        (msg: Msg<'InitData, 'EditorMsg>)
        (state: State<'EditorState>) =

        match msg with
        | StartEdit init ->
            let state', msg = editorInit init
            let state =
                { state with
                    EditorState = Some state'
                }
            {
                State = state
                Cmd = msg |> Cmd.map EditorMsg
                Submit = None
            }
        | EditorMsg msg ->
            match state.EditorState with
            | Some descripitionEditorState ->
                match editorUpdate msg descripitionEditorState with
                | UpdateRes2(state', msg) ->
                    let state =
                        { state with
                            EditorState = Some state'
                        }
                    {
                        State = state
                        Cmd = msg |> Cmd.map EditorMsg
                        Submit = None
                    }
                | SubmitRes2 description ->
                    let state =
                        { state with
                            EditorState = None
                        }
                    {
                        State = state
                        Cmd = Cmd.none
                        Submit = Some description
                    }
                | CancelRes2 ->
                    let state =
                        { state with
                            EditorState = None
                        }
                    {
                        State = state
                        Cmd = Cmd.none
                        Submit = None
                    }
            | None ->
                {
                    State = state
                    Cmd = Cmd.none
                    Submit = None
                }

    let view (editorView: ComponentView<'EditorState, 'EditorMsg>) getData (state: State<'EditorState>) (dispatch: Msg<'InitData, 'EditorMsg> -> unit) =
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
                    prop.text "Edit"
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
            (state, Cmd.none)
            |> EditorWithStart.UpdateRes2
        | Submit ->
            EditorWithStart.SubmitRes2 state.Description
        | Cancel ->
            EditorWithStart.CancelRes2

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
    | StartRemove
    | SetRemove of DescripitionEditor.Msg

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
        let res =
            EditorWithStart.update
                DescripitionEditor.init
                DescripitionEditor.update
                msg
                state.DescripitionEditorState
        match res.Submit with
        | None ->
            let state =
                { state with
                    DescripitionEditorState = res.State
                }
            let msg = res.Cmd |> Cmd.map DescripitionEditorMsg
            (state, msg)
            |> UpdateRes
        | Some x ->
            { state.Item with
                Description = x
            }
            |> UpdateItemRes

    | NameEditorMsg msg ->
        let res =
            EditorWithStart.update
                DescripitionEditor.init
                DescripitionEditor.update
                msg
                state.NameEditorState
        match res.Submit with
        | None ->
            let state =
                { state with
                    NameEditorState = res.State
                }
            let msg = res.Cmd |> Cmd.map NameEditorMsg
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
            match DescripitionEditor.update msg descripitionEditorState with
            | EditorWithStart.UpdateRes2(state', msg) ->
                let state =
                    { state with
                        IsStartedRemove = Some state'
                    }
                (state, msg |> Cmd.map SetRemove)
                |> UpdateRes
            | EditorWithStart.SubmitRes2 _ ->
                RemoveRes
            | EditorWithStart.CancelRes2 ->
                let state =
                    { state with
                        IsStartedRemove = None
                    }
                (state, Cmd.none)
                |> UpdateRes
        | None ->
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
            DescripitionEditor.view false descriptionEditorState (SetRemove >> dispatch)
        | None ->
            Html.button [
                prop.text "Remove"
                prop.onClick (fun _ ->
                    dispatch StartRemove
                )
            ]
    ]

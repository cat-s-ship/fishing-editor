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

module LootEditor =
    type LocalItem =
        {
            Item: Item
            IsChecked: bool
        }

    type Msg =
        | SwitchChecked of ItemId

    type State =
        {
            Loot: Map<ItemId, LocalItem>
        }

    let init getAllItems (loot: Commons.Loot) =
        let state =
            {
                Loot =
                    getAllItems ()
                    |> Array.map (fun (item: Item) ->
                        let localItem =
                            {
                                Item = item
                                IsChecked =
                                    loot |> Array.contains item.Id
                            }
                        item.Id, localItem
                    )
                    |> Map.ofArray
            }
        state, Cmd.none

    type UpdateResultEvent =
        | UpdateLoot of Item []

    let update (msg: Msg) (state: State) =
        match msg with
        | SwitchChecked itemId ->
            match Map.tryFind itemId state.Loot with
            | Some localItem ->
                let localItem =
                    { localItem with
                        IsChecked =
                            not localItem.IsChecked
                    }

                let state =
                    { state with
                        Loot = Map.add itemId localItem state.Loot
                    }

                let loot =
                    state.Loot
                    |> Seq.choose (fun (KeyValue(itemId, localItem)) ->
                        if localItem.IsChecked then
                            Some localItem.Item
                        else
                            None
                    )
                    |> Array.ofSeq

                UpdateResult.create
                    state
                    Cmd.none
                    (Some (UpdateLoot loot))
            | None ->
                failwithf "Not found %A id" itemId

    let view (state: State) (dispatch: Msg -> unit) =
        Html.div [
            Html.unorderedList (
                state.Loot
                |> Seq.map (fun (KeyValue(itemId, item)) ->
                    Html.li [
                        Html.span [
                            prop.text item.Item.Name
                        ]
                        Html.input [
                            prop.type' "checkbox"
                            prop.isChecked item.IsChecked
                            prop.onClick (fun _ ->
                                SwitchChecked itemId
                                |> dispatch
                            )
                        ]
                    ]
                )
            )
        ]

module LootView =
    open Components.Utils

    type Msg =
        | ItemDownloaded of ItemId * Deferred<Result<Item, string>>
        | LootEditorMsg of LootEditor.Msg
        | StartEdit
        | FinishEdit

    type State =
        {
            Loot: Map<ItemId, Deferred<Result<Item, string>>>
            LootEditorState: LootEditor.State option
        }

    let init getItem (loot: Commons.Loot) =
        let state =
            {
                Loot =
                    loot
                    |> Array.map (fun itemId -> itemId, InProgress)
                    |> Map.ofArray
                LootEditorState =
                    None
            }
        let cmd =
            loot
            |> Array.map (fun itemId ->
                let res = getItem itemId
                Cmd.ofMsg (ItemDownloaded (itemId, Resolved res))
            )
            |> Cmd.batch
        state, cmd

    type UpdateResultEvent =
        | UpdateLoot of Loot

    let update getAllItems (msg: Msg) (state: State) =
        match msg with
        | ItemDownloaded (itemId, item) ->
            let state =
                { state with
                    Loot = Map.add itemId item state.Loot
                }
            UpdateResult.create
                state
                Cmd.none
                None
        | LootEditorMsg msg ->
            match state.LootEditorState with
            | Some lootEditorState ->
                let state', cmd, res =
                    LootEditor.update msg lootEditorState
                let state =
                    { state with
                        LootEditorState = Some state'
                    }
                match res with
                | None ->
                    UpdateResult.create
                        state
                        (cmd |> Cmd.map LootEditorMsg)
                        None
                | Some res ->
                    match res with
                    | LootEditor.UpdateLoot loot ->
                        let state =
                            { state with
                                Loot =
                                    loot
                                    |> Array.map (fun item ->
                                        item.Id, Resolved (Ok item)
                                    )
                                    |> Map.ofArray
                            }
                        let res =
                            loot
                            |> Array.map (fun item -> item.Id)
                            |> UpdateLoot

                        UpdateResult.create
                            state
                            (cmd |> Cmd.map LootEditorMsg)
                            (Some res)
            | None ->
                UpdateResult.create state Cmd.none None
        | StartEdit ->
            let state', cmd =
                LootEditor.init
                    getAllItems
                    (state.Loot |> Seq.map (fun (KeyValue(itemId, _)) -> itemId) |> Array.ofSeq)
            let state =
                { state with
                    LootEditorState = Some state'
                }
            UpdateResult.create state (cmd |> Cmd.map LootEditorMsg) None

        | FinishEdit ->
            let state =
                { state with
                    LootEditorState = None
                }
            UpdateResult.create state Cmd.none None

    let view (state: State) (dispatch: Msg -> unit) =
        Html.div [
            match state.LootEditorState with
            | None ->
                Html.div [
                    Html.unorderedList (
                        state.Loot
                        |> Seq.map (fun (KeyValue(id, item)) ->
                            Html.li [
                                match item with
                                | Resolved res ->
                                    match res with
                                    | Ok item ->
                                        Html.div [
                                            prop.textf "%s" item.Name
                                        ]
                                    | Error errMsg ->
                                        Html.div [
                                            prop.style [
                                                style.color "red"
                                            ]
                                            prop.textf "%s" errMsg
                                        ]
                                | InProgress ->
                                    Html.div [
                                        prop.text "Loading..."
                                    ]
                                | NotStartedYet ->
                                    Html.div [
                                        prop.text "HasNotStartedYet"
                                    ]
                            ]
                        )
                    )

                    Html.button [
                        prop.text "Edit"
                        prop.onClick (fun _ ->
                            dispatch StartEdit
                        )
                    ]
                ]

            | Some lootEditorState ->
                Html.div [
                    LootEditor.view lootEditorState (LootEditorMsg >> dispatch)

                    Html.button [
                        prop.text "Done"
                        prop.onClick (fun _ ->
                            dispatch FinishEdit
                        )
                    ]
                ]
        ]

type Msg =
    | DescripitionEditorMsg of EditorWithStart.Msg<DescripitionEditor.InitData, DescripitionEditor.Msg>
    | NameEditorMsg of EditorWithStart.Msg<DescripitionEditor.InitData, DescripitionEditor.Msg>
    | SetRemove of EditorWithStart.Msg<DescripitionEditor.InitData, DescripitionEditor.Msg>
    | ImageUrlEditorMsg of EditorWithStart.Msg<DescripitionEditor.InitData, DescripitionEditor.Msg>
    | AsBaitMsg of LootView.Msg

type State =
    {
        Item: Item
        NameEditorState: EditorWithStart.State<DescripitionEditor.State>
        DescripitionEditorState: EditorWithStart.State<DescripitionEditor.State>
        IsStartedRemove: EditorWithStart.State<DescripitionEditor.State> // TODO: refact
        ImageUrlEditorState: EditorWithStart.State<DescripitionEditor.State>
        AsBaitState: LootView.State
    }

let init getItem (item: Item) =
    let asBaitState, cmd = LootView.init getItem (item.AsBait |> Option.defaultValue [||])
    let cmd =
        cmd
        |> Cmd.map AsBaitMsg

    let state =
        {
            Item = item
            NameEditorState = EditorWithStart.init ()
            DescripitionEditorState = EditorWithStart.init ()
            IsStartedRemove = EditorWithStart.init ()
            ImageUrlEditorState = EditorWithStart.init ()
            AsBaitState = asBaitState
        }

    state, cmd

type UpdateResultEvent =
    | UpdateItemRes of Item
    | RemoveRes

let update getAllItems (msg: Msg) (state: State) =
    match msg with
    | DescripitionEditorMsg msg ->
        let state', cmd, submit =
            EditorWithStart.update
                DescripitionEditor.init
                DescripitionEditor.update
                msg
                state.DescripitionEditorState
        let state =
            { state with
                DescripitionEditorState = state'
            }
        match submit with
        | None ->
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
            let state =
                { state with
                    Item = item
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
        let state =
            { state with
                NameEditorState = state'
            }
        match submit with
        | None ->
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
            let state =
                { state with
                    Item = item
                }
            UpdateResult.create
                state
                Cmd.none
                (Some <| UpdateItemRes item)
    | ImageUrlEditorMsg msg ->
        let state', cmd, submit =
            EditorWithStart.update
                DescripitionEditor.init
                DescripitionEditor.update
                msg
                state.ImageUrlEditorState
        let state =
            { state with
                ImageUrlEditorState = state'
            }
        let cmd = cmd |> Cmd.map ImageUrlEditorMsg
        match submit with
        | None ->
            UpdateResult.create
                state
                cmd
                None
        | Some url ->
            let item =
                { state.Item with
                    ImageUrl =
                        if System.String.IsNullOrEmpty url then
                            None
                        else
                            Some url
                }
            let state =
                { state with
                    Item = item
                }
            UpdateResult.create
                state
                cmd
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

    | AsBaitMsg msg ->
        let state', cmd, res = LootView.update getAllItems msg state.AsBaitState
        let state =
            { state with
                AsBaitState = state'
            }
        let cmd = cmd |> Cmd.map AsBaitMsg
        match res with
        | Some res ->
            match res with
            | LootView.UpdateLoot loot ->
                let item =
                    { state.Item with AsBait = Some loot }
                let state =
                    { state with
                        Item = item
                    }
                UpdateResult.create
                    state
                    cmd
                    (Some <| UpdateItemRes item)
        | None ->
            UpdateResult.create state cmd None

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

        Html.div [
            Html.span [
                prop.text "Image:"
            ]
            match state.Item.ImageUrl with
            | Some src ->
                Html.img [
                    prop.src src
                    prop.width 100
                ]
            | None ->
                Html.div [ prop.text "None" ]

            EditorWithStart.view
                "Edit"
                (DescripitionEditor.view true)
                (fun () ->
                    state.Item.ImageUrl
                    |> Option.defaultValue ""
                )
                state.ImageUrlEditorState
                (ImageUrlEditorMsg >> dispatch)
        ]

        Html.div [
            Html.div [
                prop.text "Bait on:"
            ]
            LootView.view state.AsBaitState (AsBaitMsg >> dispatch)
        ]
    ]

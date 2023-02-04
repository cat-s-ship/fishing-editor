module ItemView
open Elmish
open Feliz
open Feliz.UseElmish

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
                Html.textarea [
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
                    prop.text "Принять"
                ]
                Html.button [
                    prop.onClick (fun _ -> dispatch Cancel)
                    prop.text "Отмена"
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
                            prop.onChange (fun (e: bool) ->
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
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    [<RequireQualifiedAccess>]
    module State =
        let getLoot (state: State) =
            state.Loot
            |> Seq.map (fun (KeyValue(itemId, _)) -> itemId)
            |> Array.ofSeq

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
                    (State.getLoot state)
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
                        prop.text "Изменить"
                        prop.onClick (fun _ ->
                            dispatch StartEdit
                        )
                    ]
                ]

            | Some lootEditorState ->
                Html.div [
                    Html.div [
                        prop.style [
                            style.maxHeight 300
                            style.overflowY.auto
                        ]
                        prop.children [
                            LootEditor.view lootEditorState (LootEditorMsg >> dispatch)
                        ]
                    ]

                    Html.button [
                        prop.text "Готово"
                        prop.onClick (fun _ ->
                            dispatch FinishEdit
                        )
                    ]
                ]
        ]

module OptionalLootView =
    open Components.Utils

    type Msg =
        | LootViewMsg of LootView.Msg
        | SwitchEnabled

    type State =
        {
            IsEnable: bool
            LootViewState: LootView.State option
        }

    let init getItem (loot: Commons.Loot option) =
        match loot with
        | Some loot ->
            let lootViewState, cmd = LootView.init getItem loot
            let cmd =
                cmd
                |> Cmd.map LootViewMsg
            let state =
                {
                    IsEnable = true
                    LootViewState = Some lootViewState
                }
            state, cmd
        | None ->
            let state =
                {
                    IsEnable = false
                    LootViewState = None
                }
            state, Cmd.none

    type UpdateResultEvent =
        | UpdateLoot of Loot option

    let update getItem getAllItems (msg: Msg) (state: State) =
        match msg with
        | LootViewMsg msg ->
            match state.LootViewState with
            | Some lootViewState ->
                let state', cmd, res = LootView.update getAllItems msg lootViewState
                let state =
                    { state with
                        LootViewState = Some state'
                    }
                let res =
                    match res with
                    | Some (LootView.UpdateLoot loot) ->
                        Some (UpdateLoot (Some loot))
                    | None ->
                        None
                UpdateResult.create
                    state
                    (cmd |> Cmd.map LootViewMsg)
                    res
            | None ->
                UpdateResult.create state Cmd.none None
        | SwitchEnabled ->
            let isEnabled = state.IsEnable
            let state =
                { state with
                    IsEnable = not state.IsEnable
                }
            if isEnabled then
                UpdateResult.create
                    state
                    Cmd.none
                    (Some (UpdateLoot None))
            else
                match state.LootViewState with
                | Some lootViewState ->
                    UpdateResult.create
                        state
                        Cmd.none
                        (Some (UpdateLoot (Some (LootView.State.getLoot lootViewState))))
                | None ->
                    let lootViewState, cmd = LootView.init getItem [||]
                    let cmd =
                        cmd
                        |> Cmd.map LootViewMsg

                    let state =
                        { state with
                            LootViewState =
                                Some lootViewState
                        }
                    UpdateResult.create
                        state
                        cmd
                        None

    let view (id: string) (description: string) (state: State) (dispatch: Msg -> unit) =
        Html.div [
            Html.div [
                Html.input [
                    prop.id id
                    prop.type' "checkbox"
                    prop.isChecked state.IsEnable
                    prop.onChange (fun (e: bool) ->
                        dispatch SwitchEnabled
                    )
                ]
                Html.label [
                    prop.htmlFor id
                    prop.text description
                ]
            ]

            if state.IsEnable then
                match state.LootViewState with
                | Some lootViewState ->
                    LootView.view lootViewState (LootViewMsg >> dispatch)
                | None -> ()
        ]

type Api = {
    GetItem: ItemId -> Result<Item,string>
    GetAllItems: unit -> Item array
}

type Msg =
    | DescripitionEditorMsg of EditorWithStart.Msg<DescripitionEditor.InitData, DescripitionEditor.Msg>
    | NameEditorMsg of EditorWithStart.Msg<DescripitionEditor.InitData, DescripitionEditor.Msg>
    | SetRemove of EditorWithStart.Msg<DescripitionEditor.InitData, DescripitionEditor.Msg>
    | ImageUrlEditorMsg of EditorWithStart.Msg<DescripitionEditor.InitData, DescripitionEditor.Msg>
    | AsBaitMsg of Api * OptionalLootView.Msg
    | AsChestMsg of Api * OptionalLootView.Msg

type State =
    {
        Item: Item
        NameEditorState: EditorWithStart.State<DescripitionEditor.State>
        DescripitionEditorState: EditorWithStart.State<DescripitionEditor.State>
        IsStartedRemove: EditorWithStart.State<DescripitionEditor.State> // TODO: refact
        ImageUrlEditorState: EditorWithStart.State<DescripitionEditor.State>
        AsBaitState: OptionalLootView.State
        AsChestState: OptionalLootView.State
    }

let init (api: Api) (item: Item) =
    let asBaitState, cmd = OptionalLootView.init api.GetItem item.AsBait
    let cmd1 = cmd |> Cmd.map (fun cmd -> AsBaitMsg(api, cmd))
    let asChestState, cmd = OptionalLootView.init api.GetItem item.AsChest
    let cmd2 = cmd |> Cmd.map (fun cmd -> AsChestMsg(api, cmd))

    let state =
        {
            Item = item
            NameEditorState = EditorWithStart.init ()
            DescripitionEditorState = EditorWithStart.init ()
            IsStartedRemove = EditorWithStart.init ()
            ImageUrlEditorState = EditorWithStart.init ()
            AsBaitState = asBaitState
            AsChestState = asChestState
        }
    let cmd =
        Cmd.batch [
            cmd1
            cmd2
        ]
    state, cmd

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

    | AsBaitMsg(api, msg) ->
        let state', cmd, res = OptionalLootView.update api.GetItem api.GetAllItems msg state.AsBaitState
        let state =
            { state with
                AsBaitState = state'
            }
        let cmd = cmd |> Cmd.map (fun cmd -> AsBaitMsg(api, cmd))
        match res with
        | Some res ->
            match res with
            | OptionalLootView.UpdateLoot loot ->
                let item =
                    { state.Item with AsBait = loot }
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
    | AsChestMsg(api, msg) ->
        let state', cmd, res = OptionalLootView.update api.GetItem api.GetAllItems msg state.AsChestState
        let state =
            { state with
                AsChestState = state'
            }
        let cmd = cmd |> Cmd.map (fun cmd -> AsChestMsg(api, cmd))
        match res with
        | Some res ->
            match res with
            | OptionalLootView.UpdateLoot loot ->
                let item =
                    { state.Item with AsChest = loot }
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

let view (api: Api) (state: State) (dispatch: Msg -> unit) =
    Html.div [
        Html.div [
            Html.h2 [
                match state.Item.Name with
                | "" ->
                    Html.i [
                        prop.text "Название отсутствует"
                    ]
                | name ->
                    Html.span [
                        prop.text name
                    ]

                EditorWithStart.view
                    "Изменить"
                    (DescripitionEditor.view true)
                    (fun () -> state.Item.Name)
                    state.NameEditorState
                    (NameEditorMsg >> dispatch)
            ]
        ]

        Html.div [
            match state.Item.Created with
            | None ->
                Html.i [
                    prop.text "Время создания неизвестно"
                ]
            | Some created ->
                Html.div [
                    prop.textf "Создан %A" (Components.Utils.CustomDateTime.toString (created.ToLocalTime ()))
                ]
        ]

        Html.div [
            match state.Item.Description with
            | "" ->
                Html.i [
                    prop.text "описание отсутствует"
                ]
            | description ->
                Html.div [
                    prop.text description
                ]

            EditorWithStart.view
                "Изменить"
                (DescripitionEditor.view true)
                (fun () -> state.Item.Description)
                state.DescripitionEditorState
                (DescripitionEditorMsg >> dispatch)
        ]

        Html.div [
            match state.Item.ImageUrl with
            | Some src ->
                Html.img [
                    prop.custom("loading", "lazy")
                    prop.src src
                    prop.width 100
                ]
            | None ->
                Html.i [ prop.text "Картинка отсутствует" ]

            EditorWithStart.view
                "Изменить"
                (DescripitionEditor.view true)
                (fun () ->
                    state.Item.ImageUrl
                    |> Option.defaultValue ""
                )
                state.ImageUrlEditorState
                (ImageUrlEditorMsg >> dispatch)
        ]

        Html.div [
            OptionalLootView.view
                (sprintf "asBait_%A" state.Item.Id)
                "Использовать в качестве наживки"
                state.AsBaitState
                (fun cmd -> AsBaitMsg(api, cmd) |> dispatch)
        ]

        Html.div [
            OptionalLootView.view
                (sprintf "asChest_%A" state.Item.Id)
                "Использовать в качестве сундука"
                state.AsChestState
                (fun cmd -> AsChestMsg(api, cmd) |> dispatch)
        ]

        EditorWithStart.view
            "Удалить"
            (DescripitionEditor.view false)
            (fun () -> "")
            state.IsStartedRemove
            (SetRemove >> dispatch)
    ]

type Props =
    {|
        Item: Item
        GetItem: ItemId -> Result<Item,string>
        GetAllItems: unit -> Item []
        UpdateItem: Item -> unit
        RemoveCurrentItem: unit -> unit
    |}

[<ReactComponent>]
let Component (props: Props) =
    let api =
        {
            GetAllItems = props.GetAllItems
            GetItem = props.GetItem
        }

    let update msg state =
        let state, cmd, res =
            update msg state
        res
        |> Option.iter (fun res ->
            match res with
            | UpdateItemRes item ->
                props.UpdateItem item
            | RemoveRes ->
                props.RemoveCurrentItem ()
        )
        state, cmd

    let init () = init api props.Item

    let state, dispatch = React.useElmish(init, update, [||])
    view api state dispatch

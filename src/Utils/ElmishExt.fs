module Utils.ElmishExt.Elmish
open Elmish
open Fable.React

type ComponentInit<'InitData, 'State, 'Cmd> = 'InitData -> 'State * Cmd<'Cmd>

type ComponentUpdate<'State, 'Cmd> = 'Cmd -> 'State -> 'State * Cmd<'Cmd>

type ComponentView<'State, 'Cmd> = 'State -> ('Cmd -> unit) -> ReactElement

type Component<'InitData, 'State, 'Cmd> =
    {
        Init: ComponentInit<'InitData, 'State, 'Cmd>
        Update: ComponentUpdate<'State, 'Cmd>
        View: ComponentView<'State, 'Cmd>
    }

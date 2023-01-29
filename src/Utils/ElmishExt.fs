module Utils.ElmishExt.Elmish
open Elmish
open Fable.React

type ComponentInit<'InitData, 'State, 'Cmd> = 'InitData -> 'State * Cmd<'Cmd>

type UpdateResult<'State, 'Cmd, 'Event> = 'State * Cmd<'Cmd> * 'Event option

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
[<RequireQualifiedAccess>]
module UpdateResult =
    let create state cmd event : UpdateResult<'State, 'Cmd, 'Event> =
        state, cmd, event

type ComponentUpdate<'State, 'Cmd, 'Event> =
    'Cmd -> 'State -> UpdateResult<'State, 'Cmd, 'Event>

type ComponentView<'State, 'Cmd> = 'State -> ('Cmd -> unit) -> ReactElement

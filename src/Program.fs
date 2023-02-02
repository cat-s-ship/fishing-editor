module Program
open Elmish
open Elmish.React
#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

Utils.RegisterServiceWorker.registerServiceWorker.``default`` ()

Program.mkProgram Index.init Index.update Index.view
#if DEBUG
|> Program.withConsoleTrace
#endif
|> Program.withReactSynchronous "feliz-app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.runWith []

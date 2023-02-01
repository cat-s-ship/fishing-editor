module Program
open Elmish
open Elmish.React

Utils.RegisterServiceWorker.registerServiceWorker.``default`` ()

Program.mkProgram Index.init Index.update Index.view
#if DEBUG
|> Program.withConsoleTrace
#endif
|> Program.withReactSynchronous "feliz-app"
|> Program.runWith []

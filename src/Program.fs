module Program
// open Elmish
// open Elmish.React
// #if DEBUG
// open Elmish.Debug
// open Elmish.HMR
// #endif

module X =
    open Fable.Mermaid.Main
    open Browser
    // Fable.Mermaid.MermaidAPI.MermaidConfig
    // printfn "s"
    // const element = document.querySelector('#graphDiv');

    let element = document.querySelector("#graphDiv1")

    let insertSvg svgCode bindFunctions =
        match element with
        | null ->
            printfn "element is null"
        | element ->
            element.innerHTML <- svgCode

    let graphDefinition = "graph TB\na-->b"
    let graph = mermaid.mermaidAPI.render("graphDiv", graphDefinition, insertSvg)
    mermaid.mermaidAPI.initialize({| startOnLoad = false |})

Utils.RegisterServiceWorker.registerServiceWorker.``default`` ()

// Program.mkProgram Index.init Index.update Index.view
// #if DEBUG
// |> Program.withConsoleTrace
// #endif
// |> Program.withReactSynchronous "feliz-app"
// #if DEBUG
// |> Program.withDebugger
// #endif
// |> Program.runWith []

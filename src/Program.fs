module Program
open Feliz
open Browser.Dom
open Fable.Builders.Fela

let view =
    Fela.RendererProvider {
        renderer (Fela.createRenderer ())

        Index.router ()
    }

Utils.RegisterServiceWorker.registerServiceWorker.``default`` ()

let root = ReactDOM.createRoot(document.getElementById "feliz-app")
root.render(view)

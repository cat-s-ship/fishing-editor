// ts2fable 0.8.0
module rec Fable.Mermaid.Main

#nowarn "3390" // disable warnings for invalid XML comments
#nowarn "0044" // disable warnings for `Obsolete` usage

open System
open Fable.Core
open Fable.Core.JS
open Browser.Types

type Function = System.Action

type MermaidConfig = obj // TODO: __config_type.MermaidConfig
// type mermaidAPI = Fable.Mermaid.MermaidAPI.mermaidAPI // TODO: __mermaidAPI.mermaidAPI
// type type = __utils.type
type DetailedError = obj // TODO: __utils.DetailedError
type ExternalDiagramDefinition = obj // TODO: __diagram_api_types.ExternalDiagramDefinition
type ParseErrorFunction = obj // TODO: Diagram
/// <summary>
/// ## init
///
/// Function that goes through the document to find the chart definitions in there and render them.
///
/// The function tags the processed attributes with the attribute data-processed and ignores found
/// elements with the attribute already set. This way the init function can be triggered several
/// times.
///
/// <code lang="mermaid">
/// graph LR;
///   a(Find elements)--&gt;b{Processed}
///   b--&gt;|Yes|c(Leave element)
///   b--&gt;|No |d(Transform)
/// </code>
///
/// Renders the mermaid diagrams
/// </summary>
/// <param name="config">**Deprecated**, please set configuration in <see cref="initialize" />.</param>
/// <param name="nodes">
/// **Default**: <c>.mermaid</c>. One of the following:
/// - A DOM Node
/// - An array of DOM nodes (as would come from a jQuery selector)
/// - A W3C selector, a la <c>.mermaid</c>
/// </param>
/// <param name="callback">Called once for each rendered diagram's id.</param>
let [<Import("init","mermaid")>] init: ((MermaidConfig) option -> (U3<string, HTMLElement, NodeListOf<HTMLElement>>) option -> (Function) option -> Promise<unit>) = jsNative
let [<Import("initThrowsErrors","mermaid")>] initThrowsErrors: ((MermaidConfig) option -> (U3<string, HTMLElement, NodeListOf<HTMLElement>>) option -> (Function) option -> unit) = jsNative
/// <summary>Equivalent to <see cref="init" />, except an error will be thrown on error.</summary>
/// <param name="config">**Deprecated** Mermaid sequenceConfig.</param>
/// <param name="nodes">
/// One of:
/// - A DOM Node
/// - An array of DOM nodes (as would come from a jQuery selector)
/// - A W3C selector, a la <c>.mermaid</c> (default)
/// </param>
/// <param name="callback">Function that is called with the id of each generated mermaid diagram.</param>
/// <returns>Resolves on success, otherwise the <see cref="Promise" /> will be rejected.</returns>
[<Obsolete("This is an internal function and will very likely be modified in v10, or earlier.
We recommend staying with {@link initThrowsErrors} if you don't need `lazyLoadedDiagrams`.")>]
let [<Import("initThrowsErrorsAsync","mermaid")>] initThrowsErrorsAsync: ((MermaidConfig) option -> (U3<string, HTMLElement, NodeListOf<HTMLElement>>) option -> (Function) option -> Promise<unit>) = jsNative
let [<Import("initialize","mermaid")>] initialize: (MermaidConfig -> unit) = jsNative
/// <summary>Used to register external diagram types.</summary>
/// <param name="diagrams">Array of <see cref="ExternalDiagramDefinition" />.</param>
/// <param name="opts">If opts.lazyLoad is true, the diagram will be loaded on demand.</param>
let [<Import("registerExternalDiagrams","mermaid")>] registerExternalDiagrams: (ResizeArray<ExternalDiagramDefinition> -> ({| lazyLoad: bool option |}) option -> Promise<unit>) = jsNative
/// ##contentLoaded Callback function that is called when page is loaded. This functions fetches
/// configuration for mermaid rendering and calls init for rendering the mermaid diagrams on the
/// page.
let [<Import("contentLoaded","mermaid")>] contentLoaded: (unit -> unit) = jsNative
/// <summary>
/// ## setParseErrorHandler  Alternative to directly setting parseError using:
///
/// <code lang="js">
/// mermaid.parseError = function(err,hash){=
///    forExampleDisplayErrorInGui(err);  // do something with the error
/// };
/// </code>
///
/// This is provided for environments where the mermaid object can't directly have a new member added
/// to it (eg. dart interop wrapper). (Initially there is no parseError member of mermaid).
/// </summary>
/// <param name="newParseErrorHandler">New parseError() callback.</param>
let [<Import("setParseErrorHandler","mermaid")>] setParseErrorHandler: ((obj option -> obj option -> unit) -> unit) = jsNative
let [<Import("parse","mermaid")>] parse: (string -> bool) = jsNative
/// <param name="txt">The mermaid code to be parsed.</param>
[<Obsolete("This is an internal function and should not be used. Will be removed in v10.")>]
let [<Import("parseAsync","mermaid")>] parseAsync: (string -> Promise<bool>) = jsNative
[<Obsolete("This is an internal function and should not be used. Will be removed in v10.")>]
let [<Import("renderAsync","mermaid")>] renderAsync: (string -> string -> ((string -> ((Element -> unit)) option -> unit)) option -> (Element) option -> Promise<string>) = jsNative
let [<Import("default","mermaid")>] mermaid: Mermaid = jsNative

type [<AllowNullLiteral>] Mermaid =
    abstract startOnLoad: bool with get, set
    abstract diagrams: obj option with get, set
    abstract parseError: ParseErrorFunction option with get, set
    abstract mermaidAPI: Fable.Mermaid.MermaidAPI.MermaidAPI with get, set
    abstract parse: obj with get, set
    abstract parseAsync: obj with get, set
    abstract render: obj with get, set
    abstract renderAsync: obj with get, set
    abstract init: obj with get, set
    abstract initThrowsErrors: obj with get, set
    abstract initThrowsErrorsAsync: obj with get, set
    abstract registerExternalDiagrams: obj with get, set
    abstract initialize: obj with get, set
    abstract contentLoaded: obj with get, set
    abstract setParseErrorHandler: obj with get, set

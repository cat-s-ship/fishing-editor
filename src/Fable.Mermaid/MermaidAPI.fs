// ts2fable 0.8.0
module rec Fable.Mermaid.MermaidAPI

#nowarn "3390" // disable warnings for invalid XML comments

open System
open Fable.Core
open Fable.Core.JS
open Browser.Types

// open Fable.Mermaid.Diagram

// type type = __Diagram.type
type ParseErrorFunction = obj // TODO: ParseErrorFunction
type MermaidConfig = obj // TODO: __config_type.MermaidConfig
/// <param name="text">text to be encoded</param>
/// <returns />
let [<Import("encodeEntities","module")>] encodeEntities: (string -> string) = jsNative
/// <param name="text">text to be decoded</param>
/// <returns />
let [<Import("decodeEntities","module")>] decodeEntities: (string -> string) = jsNative
/// <summary>
/// Create a CSS style that starts with the given class name, then the element,
/// with an enclosing block that has each of the cssClasses followed by !important;
/// </summary>
/// <param name="cssClass">CSS class name</param>
/// <param name="element">CSS element</param>
/// <param name="cssClasses">list of CSS styles to append after the element</param>
/// <returns>- the constructed string</returns>
let [<Import("cssImportantStyles","module")>] cssImportantStyles: (string -> string -> (ResizeArray<string>) option -> string) = jsNative
/// <summary>Create the user styles</summary>
/// <param name="config">configuration that has style and theme settings to use</param>
/// <param name="graphType">used for checking if classDefs should be applied</param>
/// <param name="classDefs">the classDefs in the diagram text. Might be null if none were defined. Usually is the result of a call to getClasses(...)</param>
/// <returns>the string with all the user styles</returns>
let [<Import("createCssStyles","module")>] createCssStyles: (MermaidConfig -> string -> (Map<string, DiagramStyleClassDef>) option -> string) = jsNative
let [<Import("createUserStyles","module")>] createUserStyles: (MermaidConfig -> string -> Map<string, DiagramStyleClassDef> -> string -> string) = jsNative
/// <summary>Clean up svgCode. Do replacements needed</summary>
/// <param name="svgCode">the code to clean up</param>
/// <param name="inSandboxMode">security level</param>
/// <param name="useArrowMarkerUrls">should arrow marker's use full urls? (vs. just the anchors)</param>
/// <returns>the cleaned up svgCode</returns>
let [<Import("cleanUpSvgCode","module")>] cleanUpSvgCode: (string option -> bool -> bool -> string) = jsNative
/// <summary>Put the svgCode into an iFrame. Return the iFrame code</summary>
/// <param name="svgCode">the svg code to put inside the iFrame</param>
/// <param name="svgElement">the d3 node that has the current svgElement so we can get the height from it</param>
/// <returns>
/// - the code with the iFrame that now contains the svgCode
/// TODO replace btoa(). Replace with  buf.toString('base64')?
/// </returns>
let [<Import("putIntoIFrame","module")>] putIntoIFrame: ((string) option -> (D3Element) option -> string) = jsNative
/// <summary>
/// Append an enclosing div, then svg, then g (group) to the d3 parentRoot. Set attributes.
/// Only set the style attribute on the enclosing div if divStyle is given.
/// Only set the xmlns:xlink attribute on svg if svgXlink is given.
/// Return the last node appended
/// </summary>
/// <param name="parentRoot">the d3 node to append things to</param>
/// <param name="id">the value to set the id attr to</param>
/// <param name="enclosingDivId">the id to set the enclosing div to</param>
/// <param name="divStyle">if given, the style to set the enclosing div to</param>
/// <param name="svgXlink">if given, the link to set the new svg element to</param>
/// <returns>- returns the parentRoot that had nodes appended</returns>
let [<Import("appendDivSvgG","module")>] appendDivSvgG: (D3Element -> string -> string -> (string) option -> (string) option -> D3Element) = jsNative
/// <summary>Remove any existing elements from the given document</summary>
/// <param name="doc">the document to removed elements from</param>
/// <param name="id">id for any existing SVG element</param>
/// <param name="divSelector">selector for any existing enclosing div element</param>
/// <param name="iFrameSelector">selector for any existing iFrame element</param>
let [<Import("removeExistingElements","module")>] removeExistingElements: (Document -> string -> string -> string -> unit) = jsNative
/// ## mermaidAPI configuration defaults
///
/// <code lang="ts">
///    const config = {
///      theme: 'default',
///      logLevel: 'fatal',
///      securityLevel: 'strict',
///      startOnLoad: true,
///      arrowMarkerAbsolute: false,
///
///      er: {
///        diagramPadding: 20,
///        layoutDirection: 'TB',
///        minEntityWidth: 100,
///        minEntityHeight: 75,
///        entityPadding: 15,
///        stroke: 'gray',
///        fill: 'honeydew',
///        fontSize: 12,
///        useMaxWidth: true,
///      },
///      flowchart: {
///        diagramPadding: 8,
///        htmlLabels: true,
///        curve: 'basis',
///      },
///      sequence: {
///        diagramMarginX: 50,
///        diagramMarginY: 10,
///        actorMargin: 50,
///        width: 150,
///        height: 65,
///        boxMargin: 10,
///        boxTextMargin: 5,
///        noteMargin: 10,
///        messageMargin: 35,
///        messageAlign: 'center',
///        mirrorActors: true,
///        bottomMarginAdj: 1,
///        useMaxWidth: true,
///        rightAngles: false,
///        showSequenceNumbers: false,
///      },
///      gantt: {
///        titleTopMargin: 25,
///        barHeight: 20,
///        barGap: 4,
///        topPadding: 50,
///        leftPadding: 75,
///        gridLineStartPadding: 35,
///        fontSize: 11,
///        fontFamily: '"Open Sans", sans-serif',
///        numberSectionStyles: 4,
///        axisFormat: '%Y-%m-%d',
///        topAxis: false,
///      },
///    };
///    mermaid.initialize(config);
/// </code>
let [<Import("mermaidAPI","module")>] mermaidAPI: MermaidAPI = jsNative

type [<AllowNullLiteral>] MermaidAPI =
    abstract render: id: string * text: string * ?cb: (string -> ((Element -> unit)) option -> unit) * ?svgContainingElement: Element -> string
    abstract renderAsync: (string -> string -> ((string -> ((Element -> unit)) option -> unit)) option -> (Element) option -> Promise<string>) with get, set
    abstract parse: obj with get, set
    abstract parseAsync: obj with get, set
    abstract parseDirective: (obj option -> string -> string -> string -> unit) with get, set
    abstract initialize: ?options: MermaidConfig -> unit
    abstract getConfig: (unit -> MermaidConfig) with get, set
    abstract setConfig: (MermaidConfig -> MermaidConfig) with get, set
    abstract getSiteConfig: (unit -> MermaidConfig) with get, set
    abstract updateSiteConfig: (MermaidConfig -> MermaidConfig) with get, set
    abstract reset: (unit -> unit) with get, set
    abstract globalReset: (unit -> unit) with get, set
    abstract defaultConfig: MermaidConfig with get, set

type [<AllowNullLiteral>] IExports =
    /// <param name="text">The mermaid diagram definition.</param>
    /// <param name="parseError">If set, handles errors.</param>
    abstract parse: text: string * ?parseError: ParseErrorFunction -> bool
    /// <param name="text">The mermaid diagram definition.</param>
    /// <param name="parseError">If set, handles errors.</param>
    abstract parseAsync: text: string * ?parseError: ParseErrorFunction -> Promise<bool>
    /// <param name="options">Initial Mermaid options</param>
    abstract initialize: ?options: MermaidConfig -> unit

type [<AllowNullLiteral>] DiagramStyleClassDef =
    abstract id: string with get, set
    abstract styles: ResizeArray<string> option with get, set
    abstract textStyles: ResizeArray<string> option with get, set

type D3Element =
    obj option

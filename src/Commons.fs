module Commons
type 'a Deferred =
    | NotStartedYet
    | InProgress
    | Resolved of 'a

type Version =
    | V0 = 0
    | V1 = 1

type ItemId = System.Guid
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
[<RequireQualifiedAccess>]
module ItemId =
    let create () =
        System.Guid.NewGuid()

type Loot = ItemId []

type ItemV0 =
    {
        Version: int
        ItemId: ItemId
        Name: string
        Loot: ItemId []
        Description: string
        ImageUrl: string
    }

type Item =
    {
        Version: Version
        Id: ItemId
        Name: string
        AsBait: Option<Loot>
        AsChest: Option<Loot>
        Description: string
        ImageUrl: Option<string>
    }

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
[<RequireQualifiedAccess>]
module Item =
    let createFull id name asBait asChest description imageUrl : Item =
        {
            Version = Version.V1
            Id = id
            Name = name
            AsBait = asBait
            AsChest = asChest
            Description = description
            ImageUrl = imageUrl
        }

    let create id fn : Item =
        let item =
            createFull
                id
                ""
                None
                None
                ""
                None
        fn item

type Items = Map<ItemId, Item>

module Result =
    let defaultWith fn (result: Result<_, _>) =
        match result with
        | Ok x -> x
        | Error x -> fn x

    let isError (result: Result<_, _>) =
        match result with
        | Error _ -> true
        | Ok _ -> false

module Routes =
    [<Literal>]
    let ItemsListPageRoute = "ItemsListPageRoute"

    [<Literal>]
    let ItemAdderPageRoute = "ItemAdderPageRoute"

open Browser
open Browser.Types
open Fable.Core.JsInterop
open Fable.Core.JS

/// `type = "application/json"`
let saveToDisc type' (filename: string) (data: 'Data) =
    let file = Blob.Create(
        [|data|],
        jsOptions<Types.BlobPropertyBag>(fun x ->
            x.``type`` <- type'
        )
    )

    // todo:
    //   if ((window.navigator as any).msSaveOrOpenBlob) // IE10+
    //     (window.navigator as any).msSaveOrOpenBlob(file, filename)
    //   else { // Others
    let a = document.createElement "a" :?> Types.HTMLAnchorElement
    let url = URL.createObjectURL(file)
    a.href <- url
    a?download <- filename
    document.body.appendChild(a) |> ignore
    a.click()
    setTimeout
        (fun () ->
            document.body.removeChild a |> ignore
            URL.revokeObjectURL(url)
        )
        0
    |> ignore

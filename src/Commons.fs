module Commons
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
        ImageUrl: string option
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
    open Thoth.Json

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

    let decoder : Decoder<Item> =
        let lastId = Version.V1
        let decoderV0 = Decode.Auto.generateDecoder () : Decoder<ItemV0>
        let decoderV1 = Decode.Auto.generateDecoder () : Decoder<Item>

        Decode.object (fun fields ->
            let version =
                let decodeVersion =
                    Decode.int
                    |> Decode.map enum<Version>
                    |> Decode.andThen (fun version ->
                        match version with
                        | Version.V1
                        | Version.V0 ->
                            Decode.succeed version
                        | unknownVersion ->
                            Decode.fail (sprintf "unknown %A version of item" unknownVersion)
                    )
                Decode.oneOf [
                    Decode.field "version" decodeVersion
                    Decode.field "Version" decodeVersion
                ]
                |> fields.Required.Raw

            match version with
            | Version.V1 ->
                fields.Required.Raw decoderV1
            | Version.V0 ->
                let old = fields.Required.Raw decoderV0
                {
                    Version = lastId
                    Id = old.ItemId
                    Name = old.Name
                    AsBait =
                        match old.Loot with
                        | [||] -> None
                        | loot -> Some loot
                    AsChest = None
                    Description = old.Description
                    ImageUrl = old.ImageUrl
                }

            | unknownVersion ->
                failwithf "unknown %A version of item" unknownVersion
        )

    let encode : Encoder<Item> =
        Encode.Auto.generateEncoder ()

type Items = Map<ItemId, Item>
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
[<RequireQualifiedAccess>]
module Items =
    open Thoth.Json

    let decoder : Decoder<Items> =
        Decode.object (fun get ->
            let dataType =
                let decoder =
                    Decode.string
                    |> Decode.andThen (fun x ->
                        if x = "Map" then
                            Decode.succeed true
                        else
                            Decode.succeed false
                    )
                get.Required.Field "dataType" decoder

            let KeyValueDecoder =
                Decode.index 0 Decode.guid
                |> Decode.andThen (fun _ ->
                    Decode.index 1 Item.decoder
                )

            let decoder =
                Decode.array KeyValueDecoder
                |> Decode.map (fun items ->
                    items
                    |> Array.map (fun item -> item.Id, item)
                    |> Map.ofArray
                )

            get.Required.Field "value" decoder
        )

    let decode rawJson =
        Decode.fromString decoder rawJson

    let encoder : Encoder<Items> = fun items ->
        let items =
            items
            |> Seq.map (fun (KeyValue(itemId, item)) ->
                [|
                    Encode.guid itemId
                    Item.encode item
                |]
                |> Encode.array
            )
            |> Encode.seq

        Encode.object [
            "dataType", Encode.string "Map"
            "value", items
        ]

    let encode space (items: Items) =
        encoder items
        |> Encode.toString space

module Routes =
    [<Literal>]
    let ItemsListPageRoute = "ItemsListPageRoute"

    [<Literal>]
    let ItemAdderPageRoute = "ItemAdderPageRoute"

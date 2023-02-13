module Commons
type Version =
    | V0 = 0
    | V1 = 1
    | V2 = 2

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
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
[<RequireQualifiedAccess>]
module ItemV0 =
    open Thoth.Json

    let decoder : Decoder<ItemV0> =
        Decode.Auto.generateDecoder ()

type ItemV1 =
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
module ItemV1 =
    open Thoth.Json

    let ofPrev (old: ItemV0) =
        {
            Version = Version.V1
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

    let decoder : Decoder<ItemV1> =
        Decode.Auto.generateDecoder ()

type Item =
    {
        Version: Version
        Id: ItemId
        Created: System.DateTime option
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

    let ofPrev (old: ItemV1) =
        {
            Version = Version.V2
            Created = None
            Id = old.Id
            Name = old.Name
            AsBait = old.AsBait
            AsChest = old.AsChest
            Description = old.Description
            ImageUrl = old.ImageUrl
        }

    let decoderOnly : Decoder<Item> =
        Decode.Auto.generateDecoder ()

    type ItemVersionContainer =
        | V0Container of ItemV0
        | V1Container of ItemV1
        | V2Container of Item
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    [<RequireQualifiedAccess>]
    module ItemVersionContainer =
        let rec getLastVersion (itemVersionContainer: ItemVersionContainer) =
            match itemVersionContainer with
            | V2Container itemV2 ->
                itemV2
            | V1Container itemV1 ->
                ofPrev itemV1
                |> V2Container
                |> getLastVersion
            | V0Container itemV0 ->
                ItemV1.ofPrev itemV0
                |> V1Container
                |> getLastVersion

        let decodeByVersion (version: Version) (getters: Decode.IGetters) =
            match version with
            | Version.V2 ->
                getters.Required.Raw decoderOnly
                |> V2Container
            | Version.V1 ->
                getters.Required.Raw ItemV1.decoder
                |> V1Container
            | Version.V0 ->
                getters.Required.Raw ItemV0.decoder
                |> V0Container
            | unknownVersion ->
                failwithf "unknown %A version of item" unknownVersion

    let createFull id created name asBait asChest description imageUrl : Item =
        {
            Version = Version.V1
            Id = id
            Created = created
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
                (Some System.DateTime.UtcNow)
                ""
                None
                None
                ""
                None
        fn item

    let decoder : Decoder<Item> =
        Decode.object (fun fields ->
            let version =
                let decodeVersion =
                    Decode.int
                    |> Decode.map enum<Version>
                    |> Decode.andThen (fun version ->
                        match version with
                        | Version.V2
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

            ItemVersionContainer.decodeByVersion version fields
            |> ItemVersionContainer.getLastVersion
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

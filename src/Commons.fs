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
        Decode.Auto.generateDecoder ()

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
                Decode.index 0 Decode.guid |> ignore
                Decode.index 1 Item.decoder

            let decoder =
                Decode.array KeyValueDecoder
                |> Decode.map (fun items ->
                    items
                    |> Array.map (fun item -> item.Id, item)
                    |> Map.ofArray
                )

            get.Required.Field "value" decoder
        )

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

    let encode (items: Items) =
        encoder items
        |> Encode.toString 0

module Routes =
    [<Literal>]
    let ItemsListPageRoute = "ItemsListPageRoute"

    [<Literal>]
    let ItemAdderPageRoute = "ItemAdderPageRoute"

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

module Routes =
    [<Literal>]
    let ItemsListPageRoute = "ItemsListPageRoute"

    [<Literal>]
    let ItemAdderPageRoute = "ItemAdderPageRoute"

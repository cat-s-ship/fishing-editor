module Api
open Thoth

open Commons

type LocalItems =
    {
        Cache: Items
    }
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
[<RequireQualifiedAccess>]
module LocalItems =
    let localKey = "fishing-editor"

    let empty =
        {
            Cache = Map.empty
        }

    let decode rawJson =
        Items.decode rawJson
        |> Result.map (fun items ->
            {
                Cache = items
            }
        )

    let encode (localItemsApi: LocalItems) =
        Items.encode 0 localItemsApi.Cache

    module LocalStorage =
        open Browser.WebStorage

        let load () : Result<LocalItems, string> =
            match localStorage.getItem localKey with
            | null ->
                Ok empty
            | res ->
                decode res

        let save (localItems: LocalItems) =
            localStorage.setItem (localKey, Items.encode 0 localItems.Cache)

    let import (rawJson: string) =
        decode rawJson
        |> Result.map (fun localItems ->
            LocalStorage.save localItems

            localItems
        )

    let get (itemId: ItemId) (localItemsApi: LocalItems) =
        localItemsApi.Cache
        |> Map.tryFind itemId

    let set (item: Item) (localItemsApi: LocalItems) =
        let items =
            { localItemsApi with
                Cache =
                    Map.add item.Id item localItemsApi.Cache
            }

        LocalStorage.save items

        items

    let insert itemId fn (localItemsApi: LocalItems) =
        let newItem = Item.create itemId fn
        set newItem localItemsApi

    let remove itemId (localItemsApi: LocalItems) =
        let items =
            { localItemsApi with
                Cache =
                    Map.remove itemId localItemsApi.Cache
            }

        LocalStorage.save items

        items

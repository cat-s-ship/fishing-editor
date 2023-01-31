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
        Json.Decode.fromString Items.decoder rawJson
        |> Result.map (fun items ->
            {
                Cache = items
            }
        )

    let load () : Result<LocalItems, string> =
        match Browser.WebStorage.localStorage.getItem localKey with
        | null ->
            Ok empty
        | res ->
            decode res

    let import (rawJson: string) =
        decode rawJson
        |> Result.map (fun localItems ->
            Browser.WebStorage.localStorage.setItem (localKey, Items.encode localItems.Cache)

            localItems
        )

    let export (localItemsApi: LocalItems) =
        Items.encode localItemsApi.Cache

    let get (itemId: ItemId) (localItemsApi: LocalItems) =
        localItemsApi.Cache
        |> Map.tryFind itemId

    let set (item: Item) (localItemsApi: LocalItems) =
        let items =
            Map.add item.Id item localItemsApi.Cache

        Browser.WebStorage.localStorage.setItem (localKey, Items.encode items)

        { localItemsApi with
            Cache = items
        }

    let insert id fn (localItemsApi: LocalItems) =
        let newItem = Item.create id fn
        set newItem localItemsApi

    let remove dateTime (localItemsApi: LocalItems) =
        let items =
            Map.remove dateTime localItemsApi.Cache

        Browser.WebStorage.localStorage.setItem (localKey, Items.encode items)

        { localItemsApi with
            Cache = items
        }

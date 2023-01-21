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

    let get () : Result<LocalItems, string> =
        match Browser.WebStorage.localStorage.getItem localKey with
        | null ->
            Ok empty
        | res ->
            Json.Decode.Auto.fromString res
            |> Result.map (fun items ->
                {
                    Cache = items
                }
            )

    let import (rawJson: string) =
        Json.Decode.Auto.fromString rawJson
        |> Result.map (fun items ->
            Browser.WebStorage.localStorage.setItem (localKey, Json.Encode.Auto.toString items)

            {
                Cache = items
            }
        )

    let export (localItemsApi: LocalItems) =
        localItemsApi.Cache
        |> Json.Encode.Auto.toString

    let set (item: Item) (localItemsApi: LocalItems) =
        let items =
            Map.add item.Id item localItemsApi.Cache

        Browser.WebStorage.localStorage.setItem (localKey, Json.Encode.Auto.toString items)

        { localItemsApi with
            Cache = items
        }

    let insert id fn (localItemsApi: LocalItems) =
        let newItem = Item.create id fn
        set newItem localItemsApi

    let remove dateTime (localItemsApi: LocalItems) =
        let items =
            Map.remove dateTime localItemsApi.Cache

        Browser.WebStorage.localStorage.setItem (localKey, Json.Encode.Auto.toString items)

        { localItemsApi with
            Cache = items
        }

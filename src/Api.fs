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
            |> Result.map (fun events ->
                {
                    Cache = events
                }
            )

    let import (rawJson: string) =
        Json.Decode.Auto.fromString rawJson
        |> Result.map (fun events ->
            Browser.WebStorage.localStorage.setItem (localKey, Json.Encode.Auto.toString events)

            {
                Cache = events
            }
        )

    let export (localEventsApi: LocalItems) =
        localEventsApi.Cache
        |> Json.Encode.Auto.toString

    let set (event: Item) (localEventsApi: LocalItems) =
        let events =
            Map.add event.Id event localEventsApi.Cache

        Browser.WebStorage.localStorage.setItem (localKey, Json.Encode.Auto.toString events)

        { localEventsApi with
            Cache = events
        }

    let insert id fn (localEventsApi: LocalItems) =
        let newEvent = Item.create id fn
        set newEvent localEventsApi

    let remove dateTime (localEventsApi: LocalItems) =
        let events =
            Map.remove dateTime localEventsApi.Cache

        Browser.WebStorage.localStorage.setItem (localKey, Json.Encode.Auto.toString events)

        { localEventsApi with
            Cache = events
        }

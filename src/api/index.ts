import { v4 as uuidv4 } from "uuid"
import update from "immutability-helper"
import { Option, Pair, MapExt } from "@fering-org/functional-helper"

export type ItemId = string // uuidv4

export enum Version {
  V0,
  V1
}

export type Loot = ItemId []
export module Loot {
  export function createEmpty(): Loot {
    return []
  }
}

export type ItemV0 = {
  Version: Version
  ItemId: ItemId
  Name: string
  Loot: ItemId []
  Description: string
  ImageUrl: Option<string>
}

export type Item = {
  Version: Version
  ItemId: ItemId
  Name: string
  AsBait: Option<Loot>
  AsChest: Option<Loot>
  Description: string
  ImageUrl: Option<string>
}
export module Item {
  export function create(itemId: ItemId): Item {
    return {
      Version: Version.V1,
      ItemId: itemId,
      Name: "",
      AsBait: Option.mkNone(),
      AsChest: Option.mkNone(),
      Description: "",
      ImageUrl: Option.mkNone(),
    }
  }
}

export type ItemsContainer = Map<ItemId, Item>
export module ItemsContainer {
  /**
   * `POST /items`
  */
  export function create(itemsContainer: ItemsContainer): Pair<ItemId, ItemsContainer> {
    const id = uuidv4()
    const newItem = Item.create(id)
    const newItemsContainer = set(itemsContainer, newItem)
    return Pair.mk(id, newItemsContainer)
  }

  /**
   * `GET /items`
   */
  export function load(json: string): ItemsContainer {
    const raws: Map<ItemId, Record<string, unknown>> = JSON.parse(json, MapExt.reviver)
    const result: Pair<ItemId, Item>[] = MapExt.toArray(raws, (key: ItemId, raw) => {
      const currentVersion = raw["Version"]
      switch (currentVersion) {
        case Version.V0:
          const current = raw as ItemV0
          const result: Item = {
            Version: Version.V1,
            ItemId: current.ItemId,
            Name: current.Name,
            AsBait: Option.mkSome(current.Loot),
            AsChest: Option.mkNone(),
            Description: current.Description,
            ImageUrl: current.ImageUrl,
          }
          return [key, result]
        case Version.V1:
          return [key, raw as Item]
        default:
          throw new Error(`${currentVersion} not implemented yet!`)
      }
    })

    return new Map(result)
  }

  /**
   * `POST /items`
   */
  export function save(itemsContainer: ItemsContainer): string {
    return JSON.stringify(itemsContainer, MapExt.replacer)
  }

  /**
   * `GET /items/:id`
   */
  export function get(itemsContainer: ItemsContainer, itemId: ItemId): Option<Item> {
    return itemsContainer.get(itemId)
  }

  /**
   * `POST /items/:id`
   */
  export function set(itemsContainer: ItemsContainer, item: Item): ItemsContainer {
    return update(itemsContainer, { $add: [[item.ItemId, item]] })
  }

  /**
   * DELETE /items/:id
   */
  export function remove(itemsContainer: ItemsContainer, itemId: ItemId): ItemsContainer {
    return update(itemsContainer, { $remove: [itemId] })
  }

  export module LocalStorage {
    const localSaveKey = "localSaveKey"

    export function save(itemsContainer: ItemsContainer) {
      window.localStorage.setItem(localSaveKey, ItemsContainer.save(itemsContainer))
    }

    export function load(): Option<ItemsContainer> {
      const res = window.localStorage.getItem(localSaveKey)
      if (res) {
        return ItemsContainer.load(res)
      }
    }
  }
}

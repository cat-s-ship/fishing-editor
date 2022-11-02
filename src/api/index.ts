import { v4 as uuidv4 } from "uuid"
import update from "immutability-helper"
import { Option, Pair, MapExt } from "@fering-org/functional-helper"

export type ItemId = string // uuidv4

export type Item = {
  ItemId: ItemId
  Name: string
  Loot: ItemId []
  Description: string
  ImageUrl: Option<string>
}
export module Item {
  export function create(itemId: ItemId): Item {
    return {
      ItemId: itemId,
      Name: "",
      Loot: [],
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
    return JSON.parse(json, MapExt.reviver)
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
}

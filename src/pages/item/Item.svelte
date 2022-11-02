<script lang="ts">
  import update, { type Spec } from "immutability-helper"
  import { ArrayExt, MapExt, Option } from "@fering-org/functional-helper"

  import { ItemsContainer, type Item, type ItemId } from "#/api"
  import Input from "#/components/Input.svelte"
  import Loot from "./loot/Index.svelte"

  export let item: Item
  export let itemsContainer: ItemsContainer
  export let updateItemsContainer: (itemsContainer: ItemsContainer) => void

  function set(item: Item) {
    updateItemsContainer(ItemsContainer.set(itemsContainer, item))
  }

  function getItemById(itemId: ItemId) {
    return ItemsContainer.get(itemsContainer, itemId) as Item // TODO
  }

  function getAllItems() {
    return MapExt.toArray(itemsContainer, (_, v) => v)
  }

  function updateItem(spec: Spec<Item, never>) {
    set(update(item, spec))
  }
</script>

<div>
  <div>Id: {item.ItemId}</div>

  <Input
    label="Name"
    value={item.Name}
    submit={v => {
      updateItem({ Name: { $set: v } })
    }}
  />

  <Loot
    id={item.ItemId}
    loot={item.Loot.map(id => getItemById(id))}
    removeByIndex={index => {
      updateItem({ Loot: { $set: ArrayExt.remove(item.Loot, index) } })
    }}
    insertAfter={(index, newItemId) => {
      updateItem({ Loot: { $set: ArrayExt.insertAfter(item.Loot, newItemId, index) } })
    }}
    getAllItems={getAllItems}
  />

  <Input
    label="Description:"
    isMultiline={true}
    value={item.Description}
    submit={v => {
      updateItem({ Description: { $set: v } })
    }}
  />

  <Input
    label="Image URL:"
    value={Option.reduce(item.ImageUrl, x => x, () => "")}
    submit={v => {
      updateItem({ ImageUrl: { $set: v !== "" ? Option.mkSome(v) : Option.mkNone() } })
    }}
  />
  {#if Option.isSome(item.ImageUrl)}
    <img src={item.ImageUrl} alt="pic for item">
  {/if}
</div>

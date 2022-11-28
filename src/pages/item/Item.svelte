<script lang="ts">
  import update, { type Spec } from "immutability-helper"
  import { ArrayExt, MapExt, Option, Result } from "@fering-org/functional-helper"

  import { ItemsContainer, type Item, type ItemId, Loot } from "#/api"
  import Input from "#/components/Input.svelte"
  import LootComponent from "./loot/Index.svelte"
  import Checkbox from "#/components/Checkbox.svelte"
  import AsBaitForm from "./AsBaitForm.svelte"

  export let item: Item
  export let itemsContainer: ItemsContainer
  export let updateItemsContainer: (itemsContainer: ItemsContainer) => void

  function set(item: Item) {
    updateItemsContainer(ItemsContainer.set(itemsContainer, item))
  }

  function getItemById(itemId: ItemId): Result<Item, string> {
    return Option.reduce(
      ItemsContainer.get(itemsContainer, itemId),
      x => Result.mkOk(x),
      () => Result.mkError(itemId)
    )
  }

  function getAllItems() {
    return MapExt.toArray(itemsContainer, (_, v) => v)
  }

  function updateItem(spec: Spec<Item, never>) {
    set(update(item, spec))
  }
</script>

<div>
  <div>Id: {item.Id}</div>

  <Input
    id="nameInput"
    label="Название:"
    value={item.Name}
    submit={v => {
      updateItem({ Name: { $set: v } })
    }}
  />

  <AsBaitForm
    label="Использовать в качестве наживки"
    itemId={item.Id}
    updateItem={updateItem}
    getAllItems={getAllItems}
    getItemById={getItemById}
    asLoot={item.AsBait}
    field={"AsBait"}
  />

  <AsBaitForm
    label="Использовать в качестве сундука"
    itemId={item.Id}
    updateItem={updateItem}
    getAllItems={getAllItems}
    getItemById={getItemById}
    asLoot={item.AsChest}
    field={"AsChest"}
  />

  <Input
    id="descriptionInput"
    label="Описание:"
    isMultiline={true}
    value={item.Description}
    submit={v => {
      updateItem({ Description: { $set: v } })
    }}
  />

  <Input
    id="imgUrlInput"
    label="Ссылка на изображение:"
    value={Option.reduce(item.ImageUrl, x => x, () => "")}
    submit={v => {
      updateItem({ ImageUrl: { $set: v !== "" ? Option.mkSome(v) : Option.mkNone() } })
    }}
  />
  {#if Option.isSome(item.ImageUrl)}
    <img src={item.ImageUrl} alt="изображение для предмета">
  {/if}
</div>

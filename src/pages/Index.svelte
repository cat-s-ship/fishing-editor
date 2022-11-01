<script lang="ts">
  import { UnionCase, type EmptyUnionCase, Option, Result } from "@fering-org/functional-helper"

  import { type ItemId, ItemsContainer, Item } from "#/api"
  import ItemComponent from "./item/Index.svelte"
  import ItemsList from "./itemsList/Index.svelte"

  type Page =
    | UnionCase<"Item", ItemId>
    | EmptyUnionCase<"ItemList">

  let page: Page = UnionCase.mkEmptyUnionCase("ItemList")

  let itemsContainer: ItemsContainer = new Map()

  function updateItemsContainer(newItemsContainer: ItemsContainer) {
    itemsContainer = newItemsContainer
  }

  function handle(itemId: ItemId) {
    return Option.reduce(
      ItemsContainer.get(itemsContainer, itemId),
      item => Result.mkOk<Item, ItemId>(item),
      () => Result.mkError<Item, ItemId>(itemId)
    )
  }
</script>

<div>
  {#if page.case === "Item"}
    <div>
      <button
        on:click={_ => { page = UnionCase.mkEmptyUnionCase("ItemList")}}
      >
        Return to list
      </button>

      <ItemComponent
        item={handle(page.fields)}
        itemsContainer={itemsContainer}
        updateItemsContainer={itemsContainer => {
          updateItemsContainer(itemsContainer)
          if (page.case === "Item") {
            page = UnionCase.mkUnionCase("Item", page.fields)
          }
        }}
      />
    </div>
  {:else if page.case === "ItemList"}
    <ItemsList
      editItem={itemId => {
        page = UnionCase.mkUnionCase("Item", itemId)
      }}
      itemsContainer={itemsContainer}
      updateItemsContainer={updateItemsContainer}
    />
  {/if}
</div>

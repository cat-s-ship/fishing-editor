<script lang="ts">
  import { UnionCase, type EmptyUnionCase, Option, Result } from "@fering-org/functional-helper"

  import { type ItemId, ItemsContainer, Item } from "#/api"
  import { saveToDisc } from "#/common"
  import Upload from "#/components/Upload.svelte"
  import ItemComponent from "./item/Index.svelte"
  import ItemsList from "./itemsList/Index.svelte"

  type Page =
    | UnionCase<"Item", ItemId>
    | EmptyUnionCase<"ItemList">

  let page: Page = UnionCase.mkEmptyUnionCase("ItemList")

  let itemsContainer: ItemsContainer = new Map()

  function updateItemsContainer(newItemsContainer: ItemsContainer) {
    itemsContainer = newItemsContainer

    ItemsContainer.LocalStorage.save(itemsContainer)
  }

  function handle(itemId: ItemId) {
    return Option.reduce(
      ItemsContainer.get(itemsContainer, itemId),
      item => Result.mkOk<Item, ItemId>(item),
      () => Result.mkError<Item, ItemId>(itemId)
    )
  }

  function uploadHandle(res: ItemsContainer) {
    itemsContainer = res
    page = UnionCase.mkEmptyUnionCase("ItemList")
    ItemsContainer.LocalStorage.save(itemsContainer)
  }

  const localSave = ItemsContainer.LocalStorage.load()
  if (localSave) {
    uploadHandle(localSave)
  }
</script>

<main>
  <nav style="display: flex;">
    <Upload
      accept="application/json"
      startLoading={() => {}}
      cb={json => uploadHandle(ItemsContainer.load(json))}
    >
      Load
    </Upload>

    <button
      on:click={_ => {
        saveToDisc(
          ItemsContainer.save(itemsContainer),
          "items.json",
          "application/json"
        )
      }}
    >
      Save
    </button>
  </nav>

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
    <div style="flex-grow: 1; overflow-y: auto;">
      <ItemsList
        editItem={itemId => {
          page = UnionCase.mkUnionCase("Item", itemId)
        }}
        itemsContainer={itemsContainer}
        updateItemsContainer={updateItemsContainer}
      />
    </div>
  {/if}
</main>

<style>
  main {
    display: flex;
    flex-direction: column;

    height: 100vh;
  }
</style>

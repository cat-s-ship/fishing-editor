<script lang="ts">
  import type { Result } from "@fering-org/functional-helper"

  import type { Item, ItemId, ItemsContainer } from "#/api"
  import ItemComponent from "./Item.svelte"

  export let item: Result<Item, ItemId>
  export let itemsContainer: ItemsContainer
  export let updateItemsContainer: (itemsContainer: ItemsContainer) => void
</script>

<div>
  {#if item[0] === "Ok"}
    <ItemComponent
      item={item[1]}
      itemsContainer={itemsContainer}
      updateItemsContainer={updateItemsContainer}
    />
  {:else}
    Not found {item[1]}
  {/if}
</div>

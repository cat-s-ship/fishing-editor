<script lang="ts">
  import { MapExt } from "@fering-org/functional-helper"

  import { type ItemId, ItemsContainer } from "#/api"
  import ItemSelecter from "../item/loot/ItemSelecter.svelte";
  import { empty } from "svelte/internal";

  export let editItem: (itemId: ItemId) => void
  export let itemsContainer: ItemsContainer
  export let updateItemsContainer: (itemsContainer: ItemsContainer) => void
</script>

<div>
  <ul>
    {#each MapExt.toArray(itemsContainer, (k, v) => v) as item, index}
      <il>
        <div>Name: {item.Name}</div>
        <div>
          <span>Loot: </span>
          {#if item.Loot.length > 0}
            {#each item.Loot as itemId}
              <span>{ItemsContainer.get(itemsContainer, itemId)?.Name}, </span>
            {/each}
          {:else}
            <span>empty</span>
          {/if}
        </div>

        <button
          on:click={_ => void editItem(item.ItemId)}
        >
          Edit
        </button>

        <button
          on:click={_ => {
            updateItemsContainer(
              ItemsContainer.remove(
                itemsContainer,
                item.ItemId
              )
            )
          }}
        >
          Remove
        </button>
      </il>
    {/each}
  </ul>

  <button
    on:click={_ => {
      const [itemId, newItemsContainer] = ItemsContainer.create(itemsContainer)
      updateItemsContainer(newItemsContainer)
      editItem(itemId)
    }}
  >
    Add
  </button>
</div>

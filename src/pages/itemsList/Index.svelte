<script lang="ts">
  import { MapExt, Option } from "@fering-org/functional-helper"

  import { type ItemId, ItemsContainer } from "#/api"

  export let editItem: (itemId: ItemId) => void
  export let itemsContainer: ItemsContainer
  export let updateItemsContainer: (itemsContainer: ItemsContainer) => void

  function renderLoot(lootList: ItemId []) {
    return lootList
      .map(itemId => ItemsContainer.get(itemsContainer, itemId)?.Name)
      .filter(Option.isSome)
      .join(", ")
  }
</script>

<div>
  <ul>
    {#each MapExt.toArray(itemsContainer, (k, v) => v) as item, index}
      <il>
        <div>Name: {item.Name}</div>
        <div>
          <span>Loot: </span>
          {#if item.Loot.length > 0}
            {renderLoot(item.Loot)}
          {:else}
            <span>empty</span>
          {/if}
        </div>
        <div>Description: {item.Description}</div>
        {#if item.ImageUrl}
          <div>
            <span>Image: </span>
            <img src={item.ImageUrl} alt="" width=40>
          </div>
        {/if}

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

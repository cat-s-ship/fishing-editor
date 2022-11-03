<script lang="ts">
  import { MapExt, Option } from "@fering-org/functional-helper"

  import { type ItemId, ItemsContainer } from "#/api"

  export let editItem: (itemId: ItemId) => void
  export let itemsContainer: ItemsContainer
  export let updateItemsContainer: (itemsContainer: ItemsContainer) => void

  function renderLoot(lootList: ItemId []) {
    return lootList
      .map(itemId => Option.reduce(
        ItemsContainer.get(itemsContainer, itemId),
        x => x.Name,
        () => `unknown item with ${itemId} ID`
      ))
      .filter(Option.isSome)
      .join(", ")
  }
</script>

<div style="height: 100%; display: flex; flex-direction: column;">
  <div style="overflow-y: auto;">
    {#each MapExt.toArray(itemsContainer, (k, v) => v) as item, index}
      <div>
        <h4>{item.Name}</h4>
        <div>
          <span>Можно поймать: </span>
          {#if item.Loot.length > 0}
            {renderLoot(item.Loot)}
          {:else}
            <span>ничего</span>
          {/if}
        </div>
        <div>Описание: {item.Description}</div>
        {#if item.ImageUrl}
          <div>
            <span>Картинка: </span>
            <img src={item.ImageUrl} alt="" width=40>
          </div>
        {/if}

        <button
          on:click={_ => void editItem(item.ItemId)}
        >
          Редактировать
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
          Удалить
        </button>
      </div>
    {/each}
  </div>

  <button
    on:click={_ => {
      const [itemId, newItemsContainer] = ItemsContainer.create(itemsContainer)
      updateItemsContainer(newItemsContainer)
      editItem(itemId)
    }}
  >
    Добавить
  </button>
</div>

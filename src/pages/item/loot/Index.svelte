<script lang="ts">
  import { Option } from "@fering-org/functional-helper"

  import type { Item, ItemId } from "#/api"
  import ItemSelecter from "./ItemSelecter.svelte"

  export let id: ItemId
  export let loot: Item []
  export let removeByIndex: (index: number) => void
  export let insertAfter: (index: number, itemId: ItemId) => void
  export let getAllItems: () => Item []

  let isItemSelecterOn: boolean = false
</script>

<div>
  <div>Loot:</div>
  <div>
    <ul>
      {#each loot as item, index}
        <il>
          <div>{item.Name}</div>
          <button
            on:click={_ => void removeByIndex(index)}
          >
            Remove
          </button>
        </il>
      {/each}
    </ul>
  </div>

  <button
    on:click={_ => {
      isItemSelecterOn = true
    }}
  >
    Add
  </button>

  {#if isItemSelecterOn}
    <ItemSelecter
      items={getAllItems().filter(x => (x.ItemId !== id) && !loot.includes(x))}
      submit={item => {
        Option.reduce(
          item,
          item2 => {
            insertAfter(loot.length - 1, item2.ItemId)
          },
          () => {},
        )
        isItemSelecterOn = false
      }}
    />
  {/if}
</div>

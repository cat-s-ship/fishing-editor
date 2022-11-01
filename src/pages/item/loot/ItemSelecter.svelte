<script lang="ts">
  import type { Item } from "#/api"
  import { Option } from "@fering-org/functional-helper"

  export let items: Item []
  export let submit: (item: Option<Item>) => void
  let selected: Option<number>
</script>

<div>
  <ul>
    {#each items as item, index}
      <il>
        <button
          style={(Option.isSome(selected) && selected === index) ? "color: red;" : ""}
          on:click={_ => {
            Option.reduce(
              selected,
              selectedId => {
                if (selectedId === index) {
                  selected = Option.mkNone()
                } else {
                  selected = Option.mkSome(index)
                }
              },
              () => {
                selected = Option.mkSome(index)
              }
            )
          }}
        >
          {item.Name}
        </button>
      </il>
    {/each}
  </ul>

  <button
    disabled={!Option.isSome(selected)}
    on:click={_ => {
      Option.reduce(
        selected,
        selectedId => submit(Option.mkSome(items[selectedId])),
        () => {}
      )
    }}
  >
    Select
  </button>
</div>

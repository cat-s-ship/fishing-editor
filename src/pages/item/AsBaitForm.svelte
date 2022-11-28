<script lang="ts">
  import { ArrayExt, MapExt, Option, Result } from "@fering-org/functional-helper"

  import { Item, Loot, type ItemId } from "#/api"
  import Checkbox from "#/components/Checkbox.svelte"
  import type { Spec } from "immutability-helper"

  import LootComponent from "./loot/Index.svelte"

  export let label: string
  export let itemId: ItemId
  export let updateItem: (spec: Spec<Item, never>) => void
  export let getAllItems: () => Item []
  export let getItemById: (itemId: ItemId) => Result<Item, string>
  export let asLoot: Option<Loot>
  export let field: keyof Item

  function optionGet<V>(opt: Option<V>) {
    return opt as V
  }

  function checkboxHandle(isOn: boolean) {
    updateItem({ [field]: {
        $set: isOn ? Option.mkSome<Loot>(Loot.createEmpty()) : Option.mkNone<Loot>()
    } })
  }
</script>

<div>
  <Checkbox
    id={`use${field}Checkbox`}
    label={label}
    isOn={Option.isSome(asLoot)}
    submit={checkboxHandle}
  />

  {#if asLoot}
    <LootComponent
      id={itemId}
      loot={asLoot.map(id => getItemById(id))}
      removeByIndex={index => {
        updateItem({ [field]: {
          $set: Option.mkSome(ArrayExt.remove(optionGet(asLoot), index))
        } })
      }}
      insertAfter={(index, newItemId) => {
        updateItem({ [field]: {
          $set: Option.mkSome(ArrayExt.insertAfter(optionGet(asLoot), newItemId, index))
        } })
      }}
      getAllItems={getAllItems}
    />
  {/if}
</div>

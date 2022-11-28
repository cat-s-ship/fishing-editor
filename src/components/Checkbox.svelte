<script lang="ts">
  import type { Option } from "@fering-org/functional-helper"

  export let id: string
  export let label: Option<string>
  export let isOn: boolean
  export let submit: (isOn: boolean) => void
  let currentValue = isOn

  function handle() {
    currentValue = !currentValue
    submit(currentValue)
  }
</script>

<div>
  <input
    id={id}
    type="checkbox"
    checked={currentValue}
    on:change={_ => void handle()}
  />
  <label for={id}>{label}</label>
</div>

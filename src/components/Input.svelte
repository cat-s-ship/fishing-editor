<script lang="ts">
  import type { Option } from "@fering-org/functional-helper"

  export let label: Option<string>
  export let value: string
  export let submit: (text: string) => void
  let currentValue = value
</script>

<div>
  <label for="id">{label}</label>
  <input
    id="id"
    type="text"
    value={currentValue}
    on:input={e => {
      currentValue = e.currentTarget.value
    }}
  />
  <button
    disabled={value === currentValue}
    on:click={e => {
      if (value !== currentValue) {
        submit(currentValue)
      }
    }}
  >
    Submit
  </button>
</div>

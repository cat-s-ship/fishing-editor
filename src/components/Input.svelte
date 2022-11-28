<script lang="ts">
  import type { Option } from "@fering-org/functional-helper"

  export let id: string
  export let label: Option<string>
  export let value: string
  export let submit: (text: string) => void
  export let isMultiline = false
  let currentValue = value
</script>

<div>
  <label for={id}>{label}</label>
  {#if isMultiline}
    <textarea
      id={id}
      name="text"
      cols={30} rows={10}
      value={currentValue}
      on:input={e => {
        currentValue = e.currentTarget.value
      }}
    />
  {:else}
    <input
      id={id}
      type="text"
      value={currentValue}
      on:input={e => {
        currentValue = e.currentTarget.value
      }}
    />
  {/if}
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

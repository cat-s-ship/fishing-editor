<script lang="ts">
  import type { WithTarget } from "../common"

  /**
   * for example: `application/json`, `image/svg+xml` and etc.
   */
  export let accept: string
  export let startLoading: () => void
  export let cb: (file: string) => void

  function fetchFromDisc(e: WithTarget<Event, HTMLInputElement>) {
    startLoading()

    const files = e.currentTarget.files
    if (files) {
      const file = files[0]
      const reader = new FileReader()
      reader.onload = function(e) {
        const target = e.target
        if (target) {
          const contents = target.result
          if (contents) {
            cb(contents as string)
          }
        }
      }
      reader.readAsText(file)
    }
  }
</script>

<div class="button-wrap">
  <label class="button" for="upload">Load</label>
  <input id="upload" type="file" accept={accept} on:change={fetchFromDisc}/>
</div>

<style>
  input[type="file"] {
    display: none;
  }
  .button-wrap {
    position: relative;
  }
  .button {
    display: inline-block;
    padding: 12px 18px;
    cursor: pointer;
    border-radius: 5px;
    background-color: #f4f4f4;
    font-size: 16px;
    color: #333333;
  }
</style>

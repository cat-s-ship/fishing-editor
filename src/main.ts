import App from './App.svelte'
import registerServiceWorker from './registerServiceWorker'

const app = new App({
  target: document.body,
  props: {},
})

export default app

// recreate the whole app if an HMR update touches this module
// @ts-ignore
if (import.meta.hot) {
  // @ts-ignore
  import.meta.hot.dispose(() => {
    app.$destroy()
  })
  // @ts-ignore
  import.meta.hot.accept()
}

registerServiceWorker()

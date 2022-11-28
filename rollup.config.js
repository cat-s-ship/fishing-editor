// @ts-check
import svelte from 'rollup-plugin-svelte-hot'
import commonjs from '@rollup/plugin-commonjs'
import resolve from '@rollup/plugin-node-resolve'
import livereload from 'rollup-plugin-livereload'
import { terser } from 'rollup-plugin-terser'
import sveltePreprocess from 'svelte-preprocess'
import hmr from 'rollup-plugin-hot'
import typescript from '@rollup/plugin-typescript'
import css from 'rollup-plugin-css-only'
import path from 'path'
import define from 'rollup-plugin-define'

// Set this to true to pass the --single flag to sirv (this serves your
// index.html for any unmatched route, which is a requirement for SPA
// routers using History API / pushState)
//
// NOTE This will have no effect when running with Nollup. For Nollup, you'd
// have to add the --history-api-fallback yourself in your package.json
// scripts (see: https://github.com/PepsRyuu/nollup/#nollup-options)
//
const spa = false

// NOTE The NOLLUP env variable is picked by various HMR plugins to switch
// in compat mode. You should not change its name (and set the env variable
// yourself if you launch nollup with custom comands).
const isNollup = !!process.env.NOLLUP
const isWatch = !!process.env.ROLLUP_WATCH
const isLiveReload = !!process.env.LIVERELOAD

const isDev = isWatch || isLiveReload
const isProduction = !isDev

const isHot = isWatch && !isLiveReload

console.log(JSON.stringify({ isNollup, isWatch, isLiveReload, isDev, isHot }))

function serve() {
  let server

  function toExit() {
    if (server) server.kill(0)
  }

  return {
    name: 'svelte/template:serve',
    writeBundle() {
      if (server) return
      server = require('child_process').spawn(
        'npm',
        ['run', 'start', '--', '--dev'],
        {
          stdio: ['ignore', 'inherit', 'inherit'],
          shell: true,
        }
      )

      process.on('SIGTERM', toExit)
      process.on('exit', toExit)
    },
  }
}

function rollupModuleMapPlugin(moduleMap) {
  return {
    name: "rollup-module-map-plugin", // this name will show up in warnings and errors
    resolveId(source) {
      if (moduleMap[source]) {
        return {
          id: path.resolve(moduleMap[source]),
          moduleSideEffects: false,
          meta: {},
        }; // this signals that rollup should not ask other plugins or check the file system to find this id
      }
      return null; // other ids should be handled as usually
    },
    load(id) {
      return null; // other ids should be handled as usually
    },
  };
}

/** @type {import('rollup').RollupOptions} */
const options = {
  input: 'src/main.ts',
  output: {
    sourcemap: !isProduction,
    format: 'iife',
    name: 'app',
    file: 'public/build/bundle.js'
  },
  plugins: [
    rollupModuleMapPlugin({ // TODO: add mask
      "#/api": "./src/api/index.ts",
      "#/components/Input.svelte": "./src/components/Input.svelte",
      "#/components/Upload.svelte": "./src/components/Upload.svelte",
      "#/components/Checkbox.svelte": "./src/components/Checkbox.svelte",
      "#/common": "./src/common.ts",
    }),
    define({
      replacements: {
        'process.env.NODE_ENV': '"production"',
        'process.env.PUBLIC_URL': '"."',
      }
    }),
    svelte({
      preprocess: sveltePreprocess({ sourceMap: !isProduction }),
      compilerOptions: {
        // enable run-time checks when not in production
        dev: !isProduction
      },
      // @ts-ignore
      hot: isHot && {
        // Optimistic will try to recover from runtime
        // errors during component init
        optimistic: true,
        // Turn on to disable preservation of local component
        // state -- i.e. non exported `let` variables
        preserveState: true,

        // See docs of rollup-plugin-svelte-hot for all available options:
        //
        // https://github.com/rixo/rollup-plugin-svelte-hot#usage
      },
    }),
    // we'll extract any component CSS out into
    // a separate file - better for performance
    // NOTE when hot option is enabled, a blank file will be written to
    // avoid CSS rules conflicting with HMR injected ones
    css({ output: isNollup ? 'build/bundle.css' : 'bundle.css' }),

    // typescriptPaths(),


    // If you have external dependencies installed from
    // npm, you'll most likely need these plugins. In
    // some cases you'll need additional configuration -
    // consult the documentation for details:
    // https://github.com/rollup/plugins/tree/master/packages/commonjs
    resolve({
      browser: true,
      dedupe: ['svelte'],
    }),

    commonjs(),

    typescript({
      sourceMap: !isProduction,
      inlineSources: !isProduction
    }),

    // In dev mode, call `npm run start` once
    // the bundle has been generated
    isDev && !isNollup && serve(),

    // Watch the `public` directory and refresh the
    // browser on changes when not in production
    isLiveReload && livereload('public'),

    // If we're building for production (npm run build
    // instead of npm run dev), minify
    isProduction && terser(),

    hmr({
      public: 'public',
      inMemory: true,

      // Default host for the HMR server is localhost, change this option if
      // you want to serve over the network
      // host: '0.0.0.0',
      // You can also change the default HMR server port, if you fancy
      // port: '12345'

      // This is needed, otherwise Terser (in npm run build) chokes
      // on import.meta. With this option, the plugin will replace
      // import.meta.hot in your code with module.hot, and will do
      // nothing else.
      compatModuleHot: !isHot,
    }),
  ],
  watch: {
    clearScreen: false,
  },
}

export default options

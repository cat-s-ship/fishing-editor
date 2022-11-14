module.exports = {
  globDirectory: 'public/',
  globPatterns: [
    '**/*.{css,js,png,html}'
  ],
  swDest: 'public/sw.js',
  ignoreURLParametersMatching: [
    /^utm_/,
    /^fbclid$/
  ],
  sourcemap: false
};

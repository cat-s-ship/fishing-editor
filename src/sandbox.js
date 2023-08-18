// @ts-check

import mermaid from "mermaid"


mermaid.mermaidAPI.initialize({ startOnLoad: false });

const element = document.querySelector('#graphDiv');
const insertSvg = function (svgCode, bindFunctions) {
  // @ts-ignore
  element.innerHTML = svgCode;
};

const graphDefinition = 'graph TB\na-->b';
const graph = mermaid.mermaidAPI.render('graphDiv', graphDefinition, insertSvg);

import { build } from "esbuild";

const isWatch = process.argv.includes("--watch");

await build({
  entryPoints: ["wwwroot/ts/index.ts"],   // <- un �nico entry point
  bundle: true,
  outfile: "wwwroot/js/app.bundle.js",    // <- un �nico archivo de salida
  format: "esm",                          // m�dulos ES
  platform: "browser",
  target: ["es2022"],
  sourcemap: true,
  minify: !isWatch,                       // minificar en build normal
  treeShaking: true,
  legalComments: "none"
}).catch((e) => {
  console.error(e);
  process.exit(1);
});

if (isWatch) {
  console.log("esbuild watch activo�");
}

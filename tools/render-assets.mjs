// SVG 마스터(assets-src/*.svg) → PNG(@1x, @1.5x, @2x) 렌더.
// 사용: node render-assets.mjs [--src <dir>] [--out <dir>] [--override <dir>]
//   --override: 같은 basename의 SVG를 그 폴더 것으로 대체 (고객사별 일부 이미지만 교체)
import { Resvg } from '@resvg/resvg-js';
import { readdirSync, readFileSync, writeFileSync, mkdirSync, existsSync } from 'node:fs';
import { join, basename, dirname, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const here = dirname(fileURLToPath(import.meta.url));
const root = resolve(here, '..');
const SCALES = [['1x', 1], ['1.5x', 1.5], ['2x', 2]];

function arg(name, fallback) {
  const i = process.argv.indexOf(name);
  return i > 0 ? resolve(process.argv[i + 1]) : fallback;
}

const src = arg('--src', join(root, 'assets-src'));
const out = arg('--out', join(root, 'themes', 'default', 'assets'));
const override = arg('--override', null);

if (!existsSync(src)) throw new Error(`source folder not found: ${src}`);
if (override && !existsSync(override)) throw new Error(`override folder not found: ${override}`);
mkdirSync(out, { recursive: true });

let count = 0;
for (const file of readdirSync(src).filter(f => f.endsWith('.svg'))) {
  const name = basename(file, '.svg');
  const chosen = override && existsSync(join(override, file)) ? join(override, file) : join(src, file);
  const svg = readFileSync(chosen, 'utf8');
  const width = Number(/<svg[^>]*\swidth="(\d+(?:\.\d+)?)"/.exec(svg)?.[1]);
  if (!width) throw new Error(`${chosen}: <svg width="..."> missing`);
  for (const [tag, scale] of SCALES) {
    const png = new Resvg(svg, { fitTo: { mode: 'width', value: Math.round(width * scale) } }).render().asPng();
    writeFileSync(join(out, `${name}@${tag}.png`), png);
    count++;
  }
}
console.log(`rendered ${count} png -> ${out}`);

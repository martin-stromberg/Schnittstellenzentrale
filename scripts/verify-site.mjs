// Static verification for the GitHub Pages presentation site (site/) and the
// documentation artifacts that describe its maintenance path.
//
// Runs locally (developers) and as a job step in .github/workflows/pages.yml
// before actions/configure-pages, so a broken page or a removed help topic
// aborts the deploy instead of publishing a defective site.
//
// Usage: node scripts/verify-site.mjs [--site-only]
//   --site-only  skip the documentation-artifact checks (useful while the docs
//                of the update path do not exist yet)

import { existsSync, readFileSync, statSync } from "node:fs";
import { dirname, isAbsolute, join, relative, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const REQUIRED_SECTION_IDS = ["hero", "features", "screenshots", "download", "footer"];
const DOC_HELP_FILES = ["index.md", "beschreibung.md", "ablauf-technisch.md"];
const DOC_KEYWORDS = ["pages.yml", "gh api", "SiteScreenshotTests", "site/"];

function isNonEmptyFile(path) {
  try {
    return statSync(path).isFile() && statSync(path).size > 0;
  } catch {
    return false;
  }
}

function readText(path) {
  return readFileSync(path, "utf8");
}

// Prüft, ob ein aufgelöster Pfad innerhalb eines Verzeichnisses liegt.
// Referenzen mit ".." oder absolute Pfade würden sonst außerhalb von site/
// auflösen — das Pages-Artefakt enthält aber nur site/, die Datei wäre
// deployed nicht erreichbar.
function isWithinDirectory(dir, candidate) {
  const rel = relative(dir, candidate);
  return !rel.startsWith("..") && !isAbsolute(rel);
}

// Collects every local asset reference (src/href that is not absolute, not an
// anchor and not a protocol-relative or scheme-specific URL).
export function localAssetReferences(html) {
  const references = [];
  const attributePattern = /(?:src|href)\s*=\s*["']([^"']+)["']/gi;
  let match;
  while ((match = attributePattern.exec(html)) !== null) {
    const value = match[1].trim();
    if (
      value === "" ||
      value.startsWith("#") ||
      value.startsWith("//") ||
      /^[a-z][a-z0-9+.-]*:/i.test(value)
    ) {
      continue;
    }
    references.push(value.split("#")[0].split("?")[0]);
  }
  return references.filter(Boolean);
}

export function verifySiteArtifacts(rootDir) {
  const errors = [];
  const siteDir = join(rootDir, "site");
  const indexPath = join(siteDir, "index.html");

  if (!existsSync(indexPath)) {
    errors.push("site/index.html fehlt.");
    return errors;
  }

  const html = readText(indexPath);

  if (!/<html[^>]*\blang\s*=\s*["']de["']/i.test(html)) {
    errors.push('site/index.html: Attribut lang="de" fehlt am <html>-Element.');
  }

  if (!/<meta[^>]*name\s*=\s*["']viewport["']/i.test(html)) {
    errors.push("site/index.html: Viewport-Meta-Tag fehlt.");
  }

  for (const sectionId of REQUIRED_SECTION_IDS) {
    if (!new RegExp(`id\\s*=\\s*["']${sectionId}["']`, "i").test(html)) {
      errors.push(`site/index.html: Pflicht-Sektion id="${sectionId}" fehlt.`);
    }
  }

  const linkTags = html.match(/<link\b[^>]*>/gi) ?? [];
  for (const tag of linkTags) {
    const href = /href\s*=\s*["']([^"']+)["']/i.exec(tag)?.[1] ?? "";
    if (/^https?:\/\//i.test(href) || href.startsWith("//")) {
      errors.push(`site/index.html: Externes Stylesheet/Link-Tag nicht erlaubt (${href}).`);
    }
  }
  if (/bootstrap/i.test(html)) {
    errors.push("site/index.html: Bootstrap-Referenz gefunden (nicht erlaubt).");
  }
  if (/cdn\./i.test(html)) {
    errors.push("site/index.html: CDN-Referenz gefunden (nicht erlaubt).");
  }

  for (const reference of localAssetReferences(html)) {
    const assetPath = resolve(siteDir, reference);
    if (!isWithinDirectory(siteDir, assetPath)) {
      errors.push(`site/index.html: Referenz zeigt außerhalb von site/ (${reference}).`);
      continue;
    }
    if (!isNonEmptyFile(assetPath)) {
      errors.push(`site/index.html: Referenziertes Asset fehlt oder ist leer (${reference}).`);
    }
  }

  return errors;
}

function sectionBody(markdown, heading) {
  const start = markdown.indexOf(heading);
  if (start === -1) {
    return null;
  }
  const rest = markdown.slice(start + heading.length);
  const nextHeading = rest.search(/\n##\s/);
  return nextHeading === -1 ? rest : rest.slice(0, nextHeading);
}

export function verifyDocumentationArtifacts(rootDir) {
  const errors = [];
  const helpDir = join(rootDir, "docs", "help", "github-page");

  for (const fileName of DOC_HELP_FILES) {
    if (!isNonEmptyFile(join(helpDir, fileName))) {
      errors.push(`docs/help/github-page/${fileName} fehlt oder ist leer.`);
    }
  }

  const helpIndexPath = join(helpDir, "index.md");
  if (existsSync(helpIndexPath)) {
    const helpIndex = readText(helpIndexPath);
    if (!/##\s+Inhalt/.test(helpIndex)) {
      errors.push("docs/help/github-page/index.md: Abschnitt '## Inhalt' fehlt.");
    }
    for (const fileName of ["beschreibung.md", "ablauf-technisch.md"]) {
      if (!helpIndex.includes(`](${fileName})`)) {
        errors.push(`docs/help/github-page/index.md: Link auf ${fileName} fehlt.`);
      }
    }
  }

  const technicalPath = join(helpDir, "ablauf-technisch.md");
  if (existsSync(technicalPath)) {
    const technical = readText(technicalPath);
    for (const keyword of DOC_KEYWORDS) {
      if (!technical.includes(keyword)) {
        errors.push(`docs/help/github-page/ablauf-technisch.md: Stichwort '${keyword}' fehlt.`);
      }
    }
  }

  const helpOverviewPath = join(rootDir, "docs", "help", "index.md");
  if (!existsSync(helpOverviewPath)) {
    errors.push("docs/help/index.md fehlt.");
  } else if (!readText(helpOverviewPath).includes("github-page/index.md")) {
    errors.push("docs/help/index.md: Link auf github-page/index.md fehlt.");
  }

  const readmePath = join(rootDir, "README.md");
  if (!existsSync(readmePath)) {
    errors.push("README.md fehlt.");
  } else if (!/##\s+GitHub Page/.test(readText(readmePath))) {
    errors.push("README.md: Abschnitt '## GitHub Page' fehlt.");
  }

  const releaseNotesPath = join(rootDir, "docs", "RELEASE_NOTES.md");
  if (!existsSync(releaseNotesPath)) {
    errors.push("docs/RELEASE_NOTES.md fehlt.");
  } else {
    const releaseNotes = readText(releaseNotesPath);
    const keyword = /GitHub Page|github\.io/i;
    const enBlock = sectionBody(releaseNotes, "## What's New");
    const deBlock = sectionBody(releaseNotes, "## Neuerungen");
    if (enBlock === null || !keyword.test(enBlock)) {
      errors.push("docs/RELEASE_NOTES.md: EN-Block '## What's New' ohne GitHub-Page-Eintrag.");
    }
    if (deBlock === null || !keyword.test(deBlock)) {
      errors.push("docs/RELEASE_NOTES.md: DE-Block '## Neuerungen' ohne GitHub-Page-Eintrag.");
    }
  }

  return errors;
}

export function verifySite({ rootDir, siteOnly = false } = {}) {
  const errors = verifySiteArtifacts(rootDir);
  if (!siteOnly) {
    errors.push(...verifyDocumentationArtifacts(rootDir));
  }
  return errors;
}

function defaultRootDir() {
  // scripts/ liegt eine Ebene unterhalb des Repository-Stamms.
  return dirname(dirname(fileURLToPath(import.meta.url)));
}

function main(argv) {
  const siteOnly = argv.includes("--site-only");
  const errors = verifySite({ rootDir: defaultRootDir(), siteOnly });

  if (errors.length > 0) {
    for (const error of errors) {
      console.error(`FAIL ${error}`);
    }
    console.error(`verify-site: ${errors.length} Fehler gefunden.`);
    process.exitCode = 1;
    return;
  }

  console.log(siteOnly
    ? "verify-site: Seiten-Artefakte OK (Doku-Prüfung übersprungen)."
    : "verify-site: Seiten- und Doku-Artefakte OK.");
}

const isMainModule = process.argv[1] && fileURLToPath(import.meta.url) === process.argv[1];
if (isMainModule) {
  main(process.argv.slice(2));
}

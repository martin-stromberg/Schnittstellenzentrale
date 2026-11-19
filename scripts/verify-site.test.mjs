import assert from "node:assert/strict";
import { mkdtempSync, mkdirSync, rmSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";
import test from "node:test";

import {
  localAssetReferences,
  verifyDocumentationArtifacts,
  verifySite,
  verifySiteArtifacts
} from "./verify-site.mjs";

const VALID_INDEX_HTML = `<!DOCTYPE html>
<html lang="de">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <link rel="icon" type="image/png" href="favicon.png" />
    <link rel="stylesheet" href="site.css" />
    <script src="theme.js" defer></script>
</head>
<body>
    <section id="hero">Hero</section>
    <section id="features">Features</section>
    <section id="screenshots">
        <img src="screenshots/home-light.png" alt="x" />
    </section>
    <section id="download">Download</section>
    <footer id="footer">Footer</footer>
</body>
</html>`;

function fixture(overrides = {}) {
  const root = mkdtempSync(join(tmpdir(), "verify-site-"));

  mkdirSync(join(root, "site", "screenshots"), { recursive: true });
  writeFileSync(join(root, "site", "index.html"), overrides.indexHtml ?? VALID_INDEX_HTML);
  writeFileSync(join(root, "site", "site.css"), "body{}");
  writeFileSync(join(root, "site", "theme.js"), "// theme");
  writeFileSync(join(root, "site", "favicon.png"), Buffer.from([1, 2, 3]));
  if (overrides.screenshot !== false) {
    writeFileSync(join(root, "site", "screenshots", "home-light.png"), Buffer.from([4, 5, 6]));
  }

  if (overrides.docs !== false) {
    const helpDir = join(root, "docs", "help", "github-page");
    mkdirSync(helpDir, { recursive: true });
    writeFileSync(
      join(helpDir, "index.md"),
      "# GitHub Page\n\nIntro.\n\n## Inhalt\n\n- [Beschreibung](beschreibung.md)\n- [Technischer Ablauf](ablauf-technisch.md)\n"
    );
    writeFileSync(join(helpDir, "beschreibung.md"), overrides.beschreibung ?? "# Beschreibung\n");
    writeFileSync(
      join(helpDir, "ablauf-technisch.md"),
      overrides.ablaufTechnisch ??
        "# Ablauf\n\nDeploy via pages.yml; Pages-Aktivierung per gh api; " +
        "Screenshots via SiteScreenshotTests; Quellen in site/.\n"
    );
    writeFileSync(join(root, "docs", "help", "index.md"), "# Doku\n\n- [GitHub Page](github-page/index.md)\n");
    writeFileSync(join(root, "README.md"), "# Repo\n\n## GitHub Page\n\nText.\n");
    writeFileSync(
      join(root, "docs", "RELEASE_NOTES.md"),
      "# Release Notes\n\n## What's New\n\n- GitHub Page at github.io.\n\n" +
      "## Neuerungen\n\n- GitHub Page unter github.io.\n"
    );
  }

  return root;
}

function cleanup(root) {
  rmSync(root, { recursive: true, force: true });
}

test("collects local asset references and ignores external and anchor URLs", () => {
  const html = `<img src="screenshots/a.png" />
    <img src="https://img.shields.io/badge.svg" />
    <a href="#download">x</a>
    <a href="https://github.com/x">y</a>
    <link href="site.css" />`;

  assert.deepEqual(localAssetReferences(html), ["screenshots/a.png", "site.css"]);
});

test("accepts a complete site and documentation tree", () => {
  const root = fixture();
  try {
    assert.deepEqual(verifySite({ rootDir: root }), []);
  } finally {
    cleanup(root);
  }
});

test("fails when a referenced screenshot is missing", () => {
  const root = fixture({ screenshot: false });
  try {
    const errors = verifySiteArtifacts(root);
    assert.ok(errors.some((e) => e.includes("screenshots/home-light.png")));
  } finally {
    cleanup(root);
  }
});

test("fails when lang=de is missing", () => {
  const root = fixture({ indexHtml: VALID_INDEX_HTML.replace('lang="de"', 'lang="en"') });
  try {
    const errors = verifySiteArtifacts(root);
    assert.ok(errors.some((e) => e.includes('lang="de"')));
  } finally {
    cleanup(root);
  }
});

test("fails when a required section id is missing", () => {
  const root = fixture({ indexHtml: VALID_INDEX_HTML.replace('id="download"', 'id="downloads"') });
  try {
    const errors = verifySiteArtifacts(root);
    assert.ok(errors.some((e) => e.includes('id="download"')));
  } finally {
    cleanup(root);
  }
});

test("fails on bootstrap or cdn references", () => {
  const root = fixture({
    indexHtml: VALID_INDEX_HTML.replace(
      "<link rel=\"stylesheet\" href=\"site.css\" />",
      "<link rel=\"stylesheet\" href=\"https://cdn.example.com/bootstrap.css\" />"
    )
  });
  try {
    const errors = verifySiteArtifacts(root);
    assert.ok(errors.some((e) => e.includes("Bootstrap")));
    assert.ok(errors.some((e) => e.toLowerCase().includes("cdn")));
  } finally {
    cleanup(root);
  }
});

test("fails when an asset reference escapes the site directory", () => {
  const root = fixture({
    indexHtml: VALID_INDEX_HTML.replace(
      '<img src="screenshots/home-light.png" alt="x" />',
      '<img src="../outside.png" alt="x" />'
    )
  });
  try {
    // Die Datei existiert außerhalb von site/ — der Check muss trotzdem
    // fehlschlagen, weil das Pages-Artefakt nur site/ enthält.
    writeFileSync(join(root, "outside.png"), Buffer.from([7, 8, 9]));
    const errors = verifySiteArtifacts(root);
    assert.ok(errors.some((e) => e.includes("../outside.png") && e.includes("außerhalb")));
  } finally {
    cleanup(root);
  }
});

test("fails when a documentation file is missing", () => {
  const root = fixture({ docs: false });
  try {
    const errors = verifyDocumentationArtifacts(root);
    assert.ok(errors.some((e) => e.includes("docs/help/github-page/index.md")));
    assert.ok(errors.some((e) => e.includes("docs/help/index.md")));
    assert.ok(errors.some((e) => e.includes("README.md")));
    assert.ok(errors.some((e) => e.includes("RELEASE_NOTES.md")));
  } finally {
    cleanup(root);
  }
});

test("fails when a required documentation keyword is missing", () => {
  const root = fixture({ ablaufTechnisch: "# Ablauf\n\nDeploy via pages.yml.\n" });
  try {
    const errors = verifyDocumentationArtifacts(root);
    assert.ok(errors.some((e) => e.includes("gh api")));
    assert.ok(errors.some((e) => e.includes("SiteScreenshotTests")));
    assert.ok(errors.some((e) => e.includes("site/")));
    assert.ok(!errors.some((e) => e.includes("pages.yml")));
  } finally {
    cleanup(root);
  }
});

test("fails when a release-notes language block lacks the GitHub Page entry", () => {
  const root = mkdtempSync(join(tmpdir(), "verify-site-"));
  try {
    mkdirSync(join(root, "docs", "help", "github-page"), { recursive: true });
    writeFileSync(join(root, "docs", "help", "github-page", "index.md"),
      "# T\n\n## Inhalt\n\n- [B](beschreibung.md)\n- [A](ablauf-technisch.md)\n");
    writeFileSync(join(root, "docs", "help", "github-page", "beschreibung.md"), "x");
    writeFileSync(join(root, "docs", "help", "github-page", "ablauf-technisch.md"),
      "pages.yml gh api SiteScreenshotTests site/");
    writeFileSync(join(root, "docs", "help", "index.md"), "github-page/index.md");
    writeFileSync(join(root, "README.md"), "## GitHub Page\n");
    writeFileSync(join(root, "docs", "RELEASE_NOTES.md"),
      "# Release Notes\n\n## What's New\n\n- nothing\n\n## Neuerungen\n\n- GitHub Page auf github.io\n");

    const errors = verifyDocumentationArtifacts(root);
    assert.ok(errors.some((e) => e.includes("What's New")));
    assert.ok(!errors.some((e) => e.includes("Neuerungen")));
  } finally {
    cleanup(root);
  }
});

test("--site-only skips documentation checks", () => {
  const root = fixture({ docs: false });
  try {
    assert.deepEqual(verifySite({ rootDir: root, siteOnly: true }), []);
  } finally {
    cleanup(root);
  }
});

const path = require("node:path");

const releaseAssetPath = process.env.RELEASE_ASSET_PATH;
const releaseAssetPaths = (process.env.RELEASE_ASSET_PATHS ?? "")
  .split(/[;\n]/)
  .map((value) => value.trim())
  .filter(Boolean);
const releaseManifestPath = process.env.RELEASE_MANIFEST_PATH;
const releaseAssets = [...releaseAssetPaths, releaseAssetPath, releaseManifestPath]
  .filter(Boolean)
  .map((assetPath) => ({ path: assetPath, name: path.basename(assetPath) }));

const releasePlugins = [
  [
    "@semantic-release/commit-analyzer",
    {
      preset: "conventionalcommits",
      releaseRules: [
        { breaking: true, release: "major" },
        { type: "feat", release: "minor" },
        { type: "fix", release: "patch" },
        { type: "docs", release: false },
        { type: "refactor", release: false },
        { type: "chore", release: false },
        { type: "plan", release: false }
      ],
      parserOpts: {
        noteKeywords: ["BREAKING CHANGE", "BREAKING CHANGES"]
      }
    }
  ],
  "./scripts/verify-release-version.cjs",
  [
    "@semantic-release/release-notes-generator",
    {
      preset: "conventionalcommits"
    }
  ],
  [
    "@semantic-release/github",
    {
      assets: releaseAssets,
      successComment: false,
      failComment: false
    }
  ]
];

const dryRunPlugins = [
  [
    "@semantic-release/commit-analyzer",
    {
      preset: "conventionalcommits",
      releaseRules: [
        { breaking: true, release: "major" },
        { type: "feat", release: "minor" },
        { type: "fix", release: "patch" },
        { type: "docs", release: false },
        { type: "refactor", release: false },
        { type: "chore", release: false },
        { type: "plan", release: false }
      ],
      parserOpts: {
        noteKeywords: ["BREAKING CHANGE", "BREAKING CHANGES"]
      }
    }
  ]
];

// "staging" is deliberately NOT listed as a semantic-release prerelease branch here. The
// literal target schema (ci-target-schema.md section 4.8) suggests
// `branches: ["main", { name: "staging", prerelease: "rc" }]`, but Rezepte's finished
// implementation deviated from that on purpose (see Rezepte's release.config.cjs and its
// migration commit): a "staging" prerelease branch entry would let semantic-release's own
// @semantic-release/github plugin create a GitHub (pre-)release straight from a staging push,
// which duplicates and races staging-ci.yml's own "prerelease" job (ci-target-schema.md
// section 4.3), whose entire purpose is to own pre-release creation on staging. RC version
// determination for staging instead lives in staging-ci.yml's own "version" job, which invokes
// semantic-release with a --branches override against this same config (treating "staging" as
// a plain release branch for that one dry-run, so the RC suffix is appended manually
// afterwards - see that job's own comment). Schnittstellenzentrale follows the same reasoning
// as Rezepte here rather than the section 4.8 example literally.
module.exports = {
  branches: ["main"],
  tagFormat: "v${version}",
  plugins: process.env.RESOLVE_DRY_RUN === "true" ? dryRunPlugins : releasePlugins
};

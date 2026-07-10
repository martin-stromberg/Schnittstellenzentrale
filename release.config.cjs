const assetPath = process.env.RELEASE_ASSET_PATH;

const assets = assetPath
  ? [
      {
        path: assetPath,
        label: 'Schnittstellenzentrale ZIP'
      }
    ]
  : [];

module.exports = {
  branches: ['main'],
  tagFormat: 'v${version}',
  plugins: [
    [
      '@semantic-release/commit-analyzer',
      {
        preset: 'conventionalcommits',
        releaseRules: [
          { breaking: true, release: 'major' },
          { type: 'feat', release: 'minor' },
          { type: 'fix', release: 'patch' },
          { type: 'docs', release: false },
          { type: 'refactor', release: false },
          { type: 'chore', release: false },
          { type: 'plan', release: false }
        ],
        parserOpts: {
          noteKeywords: ['BREAKING CHANGE', 'BREAKING CHANGES']
        }
      }
    ],
    [
      '@semantic-release/release-notes-generator',
      {
        preset: 'conventionalcommits'
      }
    ],
    [
      '@semantic-release/github',
      {
        assets,
        successComment: false,
        failComment: false
      }
    ]
  ]
};

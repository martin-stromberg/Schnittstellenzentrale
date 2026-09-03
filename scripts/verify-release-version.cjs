// Safety net for release.yml's "Create automatic GitHub release" step: guards against
// semantic-release's own dry-run (inside its full run) resolving a different version than the
// one the composite build-and-package action already built assets for (e.g. because new
// commits landed on main between resolve-release-version.mjs's dry-run and this later, full
// semantic-release invocation). Wired into release.config.cjs's releasePlugins.
// Template: FinanceManager's scripts/verify-release-version.cjs (already generic, unchanged).
module.exports = {
  verifyRelease: (_, context) => {
    const expectedVersion = process.env.RELEASE_VERSION;

    if (!expectedVersion) {
      if (context.options.dryRun) {
        return;
      }

      throw new Error("RELEASE_VERSION must be set before Semantic Release publishes a release.");
    }

    if (context.nextRelease.version !== expectedVersion) {
      throw new Error(
        `Semantic Release resolved ${context.nextRelease.version}, but the prepared archive is for ${expectedVersion}.`
      );
    }
  }
};

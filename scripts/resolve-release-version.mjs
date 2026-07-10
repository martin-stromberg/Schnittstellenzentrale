import { appendFileSync } from 'node:fs';
import semanticRelease from 'semantic-release';

const outputPath = process.env.GITHUB_OUTPUT;

function writeOutput(name, value) {
  const line = `${name}=${value ?? ''}`;

  if (outputPath) {
    appendFileSync(outputPath, `${line}\n`, 'utf8');
  } else {
    console.log(line);
  }
}

const result = await semanticRelease(
  {
    dryRun: true,
    ci: false
  },
  {
    cwd: process.cwd(),
    env: process.env,
    stdout: process.stdout,
    stderr: process.stderr
  }
);

if (!result?.nextRelease) {
  writeOutput('should_release', 'false');
  writeOutput('version', '');
  writeOutput('tag', '');
  process.exit(0);
}

const { version, gitTag } = result.nextRelease;
writeOutput('should_release', 'true');
writeOutput('version', version);
writeOutput('tag', gitTag ?? `v${version}`);

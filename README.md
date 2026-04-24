# GauntletCI Demo

This repository exists to **show GauntletCI in action on real pull
requests**. Browse the open PRs to see the tool's findings, severity
classifications, inline annotations, and check verdicts on a small but
realistic .NET service.

## What's here

- `src/OrderService/` — small payment-processing sample app
- `tests/OrderService.Tests/` — a couple of xUnit tests
- `.gauntletci.json` — tool configuration
- `.github/workflows/gauntlet.yml` — runs GauntletCI on every PR
- `scenarios/` — canonical demo scenarios (overlay files + descriptions)
- `.github/workflows/reopen-scenarios.yml` — workflow_dispatch action that
  rebuilds every scenario branch and opens fresh PRs

## How GauntletCI is installed in CI

The workflow installs the **published NuGet tool** so this repo
demonstrates the same install path real users will follow:

```yaml
- run: dotnet tool install -g GauntletCI
- run: gauntletci analyze --commit ${{ github.event.pull_request.head.sha }} \
         --no-banner --github-annotations \
         --github-pr-comments --github-checks
```

## Demo scenarios

| # | Scenario | Expected verdict | Demonstrates |
|---|----------|------------------|--------------|
| 01 | [safe-typo-fix](scenarios/01-safe-typo-fix/README.md) | ✅ Clean | Low false-positive rate |
| 02 | [silent-catch](scenarios/02-silent-catch/README.md) | 🛑 Block (GCI0007) | Error-handling integrity |
| 03 | [hardcoded-secret](scenarios/03-hardcoded-secret/README.md) | 🛑 Block (GCI0012) | Security risk detection |
| 04 | [breaking-api-change](scenarios/04-breaking-api-change/README.md) | 🛑 Block (GCI0004) | Public-surface diffing |
| 05 | [pii-logging](scenarios/05-pii-logging/README.md) | ⚠️ Warn (GCI0029) | PII / compliance leaks |
| 06 | [concurrency-race](scenarios/06-concurrency-race/README.md) | 🛑 Block (GCI0016) | Concurrency state risk |

## Re-opening the demo PRs

If the PRs have been closed or you want to refresh them against the latest
`main`, run the **Reopen demo scenarios** workflow (Actions tab →
*Reopen demo scenarios* → *Run workflow*). You can target one scenario or
`all`.

## Running locally

```bash
dotnet tool install -g GauntletCI
dotnet build
gauntletci analyze --staged
```

## About GauntletCI

GauntletCI is a deterministic pre-commit risk detector for .NET
codebases. It runs as a CLI, dotnet tool, MCP server, GitHub Action, or
Docker container. See the main repo:
**[github.com/EricCogen/GauntletCI](https://github.com/EricCogen/GauntletCI)**.

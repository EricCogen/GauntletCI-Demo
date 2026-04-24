# 18 — Dependabot PR slips a public API change in next to a package bump

**Expected verdict:** ❌ Fails — GauntletCI should fire **GCI0052** (Dependency Bot API Drift).

## What changed
This PR is shaped like a routine **Dependabot** update — bumps a
package version in `OrderService.csproj` — but quietly **also** changes
a public method in `PricingService.cs`:

- `src/OrderService/OrderService.csproj` — adds a new
  `<PackageReference>` for `Polly` (the "lockfile-equivalent" change a
  bot would author).
- `src/OrderService/Pricing/PricingService.cs` — adds a new public
  method `ApplyShipping`, expanding the public surface of the
  `PricingService`.

## Why this is risky
- Dependency-bot PRs get **rubber-stamped** by reviewers far more often
  than human-authored PRs because they're expected to be mechanical
  package bumps.
- A bot account that has been compromised — or a misconfigured bot
  template — can use that low-scrutiny channel to slip in production
  code changes that would normally require deeper review.
- Even when benign, an API surface change wedged into a "chore: bump
  Foo to 1.2.3" PR is invisible to consumers downstream and ships in
  the next release without an API-change review.

## How GauntletCI catches this
`GCI0052 Dependency Bot API Drift` fires only when **all three**
conditions hold inside one PR:

1. The PR's `GITHUB_ACTOR` matches a known dependency bot
   (`dependabot[bot]`, `renovate[bot]`, `snyk-bot`, `snyk[bot]`).
2. The diff touches a lockfile or `*.csproj`.
3. The diff also adds a public method signature in a `*.cs` file.

In real CI, condition (1) is set automatically by GitHub Actions when
Dependabot is the PR author. To **simulate** that here without
running an actual bot account, this scenario also overlays
`.github/workflows/gauntlet.yml` to set
`GITHUB_ACTOR: dependabot[bot]` on the analyze step. That single line
is the only thing distinguishing this demo from a real Dependabot PR
landing in your repo.

## How to fix it
- Split the PR — bots should only touch dependency manifests; code
  changes should come from a human-authored PR with a real review.
- Lock down the bot's permissions or branch-protect the public-API
  surface so bot identities can't push behavior changes.
- Add a `CODEOWNERS` rule routing public-surface files to a human
  reviewer regardless of who opens the PR.

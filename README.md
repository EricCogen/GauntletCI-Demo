# GauntletCI Demo

> A live, runnable showcase of [**GauntletCI**](https://gauntletci.com) — a
> deterministic pre-commit risk detector for .NET — operating on real GitHub
> pull requests.
>
> **🔗 Main repository:** https://github.com/EricCogen/GauntletCI
> **🌐 Website:** https://gauntletci.com

---

## What this repository is

This repo is **not** a working application. It is a controlled demonstration
environment whose only purpose is to let you see GauntletCI's output on
realistic code changes — without installing anything yourself.

It contains:

1. A **small but realistic .NET 8 sample app** (`OrderService` — a payment
   processing service with a payment client and an order processor), so the
   diffs being analyzed look like code you'd actually write.
2. A **GitHub Actions workflow** (`.github/workflows/gauntlet.yml`) that
   installs the published GauntletCI tool from NuGet on every PR and runs
   it against the PR diff, posting findings as inline annotations, PR
   review comments, and a Checks API verdict.
3. A library of **canonical demo scenarios** under `scenarios/`. Each
   scenario is a deliberate code change (silent exception swallow,
   hardcoded secret, breaking API change, PII in logs, concurrency race,
   and a no-op control) that exercises a different GauntletCI rule.
4. A **`workflow_dispatch` action** (`.github/workflows/reopen-scenarios.yml`)
   that rebuilds every scenario branch and reopens its PR on demand. This
   lets the demo regenerate itself against the latest published tool
   version without manual git work.

## What this repository is *not*

- ❌ Not a production-quality reference architecture for `OrderService`.
- ❌ Not a place to file GauntletCI bugs or feature requests — please use
  the [main repo's issues](https://github.com/EricCogen/GauntletCI/issues).
- ❌ Not a substitute for real-world testing on your own codebase. Run
  `gauntletci analyze` on your own diffs to see findings tuned to your
  code.

> **A note on fake secrets in this repo.** Demo scenarios that need to
> embed a credential-shaped literal (e.g. `03-hardcoded-secret`) use the
> namespaced pattern **`gci_demo_{hex}`**. This format is intentionally
> chosen so it does not match any real provider's secret-scanning rules,
> while still being exactly the shape GauntletCI's `GCI0012` rule looks
> for. There are no real credentials anywhere in this repository.

---

## How to use this repository

> **Why we recommend running it yourself.** This repo's canonical PRs are
> intentionally read-only — we keep them as a stable, predictable showcase
> rather than letting visitors mutate them. To experiment freely (try your
> own diffs, edit scenarios, see what triggers what), **clone or fork** and
> run the demo on your own copy. The two paths below cover both styles.

### Run it yourself (recommended)

This is the headline experience: you own the repo state, you control the
runs, you can poke at anything without breaking the demo for the next
visitor.

#### Option 1 — Fork and use GitHub Actions

1. **Fork** [`EricCogen/GauntletCI-Demo`](https://github.com/EricCogen/GauntletCI-Demo)
   to your account.
2. In your fork, go to **Actions → Reopen demo scenarios → Run workflow**
   (the `Actions` tab may need to be enabled first — GitHub disables
   workflows on forks by default; click "I understand my workflows, go ahead
   and enable them").
3. Choose `all` (or a single scenario id) and run.
4. The workflow generates the `demo/*` branches and opens PRs against your
   fork's `main`. Each PR triggers `gauntlet.yml`, which installs the
   published GauntletCI tool from NuGet and runs it on the diff.
5. Open any PR in your fork to see the **Files Changed** annotations,
   **Conversation** review summary, and **Checks** verdict.

> **Note:** `secrets.DEMO_PR_TOKEN` is *optional*. If your fork doesn't have
> it, the workflow falls back to the built-in `GITHUB_TOKEN` and PRs are
> authored by `github-actions[bot]` instead of a custom identity.

#### Option 2 — Clone and run locally

```bash
git clone https://github.com/EricCogen/GauntletCI-Demo.git
cd GauntletCI-Demo

# Install the published tool
dotnet tool install -g GauntletCI

# Build the sample app
dotnet build

# Apply a scenario locally and analyze the staged diff
cp -r scenarios/02-silent-catch/files/. .
git add -A
gauntletci analyze --staged
```

You'll get the same findings GauntletCI would produce in CI, in under a
second, on your own machine.

### Quick look (no install, no fork)

If you just want to *see* what the tool produces without setting anything
up:

1. Open the **[Pull Requests tab](https://github.com/EricCogen/GauntletCI-Demo/pulls)**.
2. Pick any open PR labelled `demo:*`.
3. Look at:
   - The **Files Changed** tab — GauntletCI's inline annotations appear
     alongside the diff lines that triggered them.
   - The **Conversation** tab — GauntletCI posts a PR review summarising
     the findings, severity, and rationale.
   - The **Checks** tab — a GauntletCI check run shows the overall
     pass/fail verdict.

The expected verdict for each scenario is documented in its
[`scenarios/<id>/README.md`](scenarios/) so you can compare what you see
against what the tool was meant to catch.

### Maintainer note (regenerating canonical PRs)

The canonical PRs in this repo auto-heal: `reopen-scenarios.yml` runs on a
weekly schedule and on every push to `main`, so the showcase stays in sync
with the latest published GauntletCI version. To force a rebuild manually,
go to **Actions → Reopen demo scenarios → Run workflow**.

---

## Scenarios

| # | Scenario | Expected verdict | Rule(s) demonstrated |
|---|----------|------------------|----------------------|
| 01 | [safe-typo-fix](scenarios/01-safe-typo-fix/README.md) | ✅ Clean | (none — low-noise control) |
| 02 | [silent-catch](scenarios/02-silent-catch/README.md) | 🛑 Block | `GCI0007` Error Handling Integrity |
| 03 | [hardcoded-secret](scenarios/03-hardcoded-secret/README.md) | 🛑 Block | `GCI0012` Security Risk |
| 04 | [breaking-api-change](scenarios/04-breaking-api-change/README.md) | 🛑 Block | `GCI0004` Breaking Change Risk |
| 05 | [pii-logging](scenarios/05-pii-logging/README.md) | ⚠️ Warn | `GCI0029` PII Logging Leak |
| 06 | [concurrency-race](scenarios/06-concurrency-race/README.md) | 🛑 Block | `GCI0016` Concurrency & State Risk |

### Tier 2 — one scenario per rule

A second wave of scenarios, each isolating a single GauntletCI rule on
the same `OrderService` sample app. Verdict for every Tier 2 entry is
❌ Fails (the change exists to trip exactly one rule).

| # | Scenario | Rule demonstrated |
|---|----------|-------------------|
| 07 | [magic-connection-string](scenarios/07-magic-connection-string/README.md) | `GCI0010` Hardcoding and Configuration |
| 08 | [undisposed-httpclient](scenarios/08-undisposed-httpclient/README.md) | `GCI0024` Resource Lifecycle |
| 09 | [insecure-random-token](scenarios/09-insecure-random-token/README.md) | `GCI0048` Insecure Random in Security Context |
| 10 | [sql-column-truncation](scenarios/10-sql-column-truncation/README.md) | `GCI0050` SQL Column Truncation Risk |
| 11 | [float-money-equality](scenarios/11-float-money-equality/README.md) | `GCI0049` Float/Double Equality Comparison |
| 12 | [missing-null-guard](scenarios/12-missing-null-guard/README.md) | `GCI0006` Edge Case Handling |
| 13 | [throw-bare-exception](scenarios/13-throw-bare-exception/README.md) | `GCI0032` Uncaught Exception Path |
| 14 | [todo-in-payment-flow](scenarios/14-todo-in-payment-flow/README.md) | `GCI0042` TODO/Stub Detection |
| 15 | [non-idempotent-retry](scenarios/15-non-idempotent-retry/README.md) | `GCI0022` Idempotency & Retry Safety |
| 16 | [tolist-in-loop](scenarios/16-tolist-in-loop/README.md) | `GCI0044` Performance Hotpath Risk |
| 17 | [captive-dependency](scenarios/17-captive-dependency/README.md) | `GCI0038` Dependency Injection Safety |
| 18 | [dependabot-api-drift](scenarios/18-dependabot-api-drift/README.md) | `GCI0052` Dependency Bot API Drift |

Each scenario folder contains:
- `README.md` — what the change is and what verdict to expect
- `files/` — the overlay files that get copied onto `main` to construct
  the demo branch

---

## How the CI install works

The CI workflow uses the **same install path real users follow**, so the
demo also serves as a smoke test of the published tool:

```yaml
- run: dotnet tool install -g GauntletCI
- run: |
    gauntletci analyze \
      --commit ${{ github.event.pull_request.head.sha }} \
      --no-banner \
      --github-annotations \
      --github-pr-comments \
      --github-checks
```

No build-from-source, no pre-release feeds — just `dotnet tool install`
from NuGet.

---

## Repository layout

```
GauntletCI-Demo/
├── src/OrderService/             # sample .NET 8 app
├── tests/OrderService.Tests/     # xUnit tests for the sample app
├── scenarios/                    # canonical demo scenarios
│   ├── 01-safe-typo-fix/
│   ├── 02-silent-catch/
│   ├── 03-hardcoded-secret/
│   ├── 04-breaking-api-change/
│   ├── 05-pii-logging/
│   └── 06-concurrency-race/
├── .github/workflows/
│   ├── gauntlet.yml              # PR check that runs GauntletCI
│   └── reopen-scenarios.yml      # rebuilds scenario branches on demand
├── scripts/reopen-scenarios.sh   # logic for the rebuild workflow
├── .gauntletci.json              # GauntletCI rule configuration
└── OrderService.sln
```

---

## Learn more

- 🌐 **Website:** https://gauntletci.com
- 📦 **Source:** https://github.com/EricCogen/GauntletCI
- 📚 **Docs:** https://gauntletci.com/docs
- 💬 **Issues / questions:** https://github.com/EricCogen/GauntletCI/issues

## License

MIT — see [LICENSE](LICENSE).

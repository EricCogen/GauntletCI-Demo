# GauntletCI Demo

> A live, runnable showcase of [**GauntletCI**](https://gauntletci.com) - a
> deterministic pre-commit risk detector for .NET - operating on real GitHub
> pull requests.
>
> **🔗 Main repository:** https://github.com/EricCogen/GauntletCI
> **🌐 Website:** https://gauntletci.com

---

## What this repository is

This repo is **not** a working application. It is a controlled demonstration
environment whose only purpose is to let you see GauntletCI's output on
realistic code changes - without installing anything yourself.

It contains:

1. A **small but realistic .NET 8 sample app** (`OrderService` - a payment
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
- ❌ Not a place to file GauntletCI bugs or feature requests - please use
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

## Understanding GauntletCI's Value Proposition

GauntletCI detects **behavioral change risks** within your git diff during pre-commit analysis. This is fundamentally different from whole-project snapshot SAST tools.

### What This Means

**SAST Tools (CodeQL, Semgrep, SonarQube, etc.):**
- Scan the entire codebase during CI (multi-minute process)
- Look for known vulnerability signatures and code quality patterns
- Excellent at finding hardcoded secrets, SQL injection patterns, standard anti-patterns like `.Result` deadlocks
- Run during compilation/packaging phase as CI gates

**GauntletCI:**
- Analyzes only the git diff during pre-commit (sub-second)
- Detects structural mutations, execution sequence changes, boundary drifts within the specific change delta
- Catches behavioral regressions that compile cleanly, pass all tests, but break production systems
- Runs before you commit, before code review, before CI

### The 18 Scenarios: What They Demonstrate

These 18 behavioral scenarios show what GauntletCI detects that whole-project snapshot tools cannot see:

| Category | Scenarios | What It Shows |
|----------|-----------|--------|
| **Architectural Access Control** | S19, S23, S24 | Removal/modification of access boundaries in the diff without corresponding validation changes |
| **Execution Sequence Changes** | S20, S28-S30 | State mutations or external calls reordered in ways that are syntactically clean but execution-order dependent |
| **Async Propagation Drops** | S21, S25-S27 | Loss of CancellationToken context, fire-and-forget task patterns, propagation failures across method boundaries in the diff |
| **Public Contract Drift** | S22, S31-S32 | Method signature, default parameter, or API contract changes that compile but break callers in the specific change |
| **Performance & Resource** | S33-S34 | Configuration changes, pooling disablement, cache lookup removal that are invisible to style checkers |
| **Dependency Injection Scope** | S35-S36 | Scope boundary mismatches in DI configuration within the specific change |

Each scenario:
- Compiles successfully and passes unit tests
- Would pass both SAST and linting gates
- Introduces behavioral risk that only diff-level analysis can catch before production

### See It Live

All tools run live via GitHub Actions:
- **DEMO_FINDINGS.md** - Detailed tool-by-tool comparison across all 18 scenarios
- **Live PRs** - Each scenario runs CodeQL, Semgrep, SonarQube, StyleCop, Snyk, and GauntletCI
- **The Verdict** - See the 18/18 vs 0/18 scorecard in action
- **36 Total Scenarios** - Tier 1 (6), Tier 2 (12), Tier 3 (18)

---

## How to Verify This Yourself

**Fastest way:** Fork this repo, enable Actions, run workflow `Reopen demo scenarios`

All analysis tools run live on every PR via GitHub Actions in `.github/workflows/`:
- `codeql.yml` - runs CodeQL on every PR
- `semgrep.yml` - runs Semgrep on every PR
- `stylecop.yml` - runs StyleCop enforcement on every PR
- `snyk.yml` - runs Snyk on every PR
- `gauntletci.yml` - runs GauntletCI on every PR

You'll see the findings (or lack thereof) in real time. No downloads, no local setup required—just fork, enable workflows, and watch the PRs.
>>>>>>> ed77da6aade0590fd43ac0bd0a4d980dd9b17810

---

## How to use this repository

> **Why we recommend running it yourself.** This repo's canonical PRs are
> intentionally read-only - we keep them as a stable, predictable showcase
> rather than letting visitors mutate them. To experiment freely (try your
> own diffs, edit scenarios, see what triggers what), **clone or fork** and
> run the demo on your own copy. The two paths below cover both styles.

### Run it yourself (recommended)

This is the headline experience: you own the repo state, you control the
runs, you can poke at anything without breaking the demo for the next
visitor.

> **Prerequisites (both paths):**
> - **.NET 8 SDK** - install from <https://dotnet.microsoft.com/download/dotnet/8.0> (the demo CI uses `8.0.x`)
> - **Git** - any recent version
> - **A GitHub account** - only required for the fork path

#### Option 1 - Fork and use GitHub Actions

1. **Fork** [`EricCogen/GauntletCI-Demo`](https://github.com/EricCogen/GauntletCI-Demo)
   to your account.
2. **⚠️ Enable Actions on your fork.** GitHub disables workflows on new
   forks by default. In your fork, click the **Actions** tab. If you see
   the banner *"Workflows aren't being run on this forked repository"*,
   click **"I understand my workflows, go ahead and enable them"**. The
   reopen-scenarios workflow will not appear until you do this.
3. Go to **Actions → Reopen demo scenarios → Run workflow**.
4. Type `all` (or a single scenario folder name like `03-hardcoded-secret`)
   into the input and click **Run workflow**.
5. **Expect ~2 minutes** for the first run: the workflow rebuilds the
   `demo/*` branches and opens one PR per scenario. Each PR then triggers
   `gauntlet.yml`, which installs the published GauntletCI tool from
   NuGet (~30 s) and runs it on the diff (~5 s).
6. Open any of the new PRs in your fork to see the **Files Changed**
   annotations, **Conversation** review summary, and **Checks** verdict.

> **Note:** `secrets.DEMO_PR_TOKEN` is *optional*. If your fork doesn't have
> it, the workflow falls back to the built-in `GITHUB_TOKEN` and PRs are
> authored by `github-actions[bot]` instead of a custom identity.

**Did it work?**
- ✅ Expected: a fresh batch of PRs titled `demo: <scenario-id>` appears
  in your fork's **Pull requests** tab, each with a green or red
  **GauntletCI** check (matching the verdict in
  [`scenarios/<id>/README.md`](scenarios/)).
- ❌ *No PRs appeared* - most often the Actions tab still has the disable
  banner. Re-check step 2.
- ❌ *Workflow failed in `Install GauntletCI` step* - usually a transient
  NuGet outage. Re-run from the **Actions** tab.
- ❌ *Workflow failed in `Open PR` step with 403* - your fork has branch
  protection on `main` that blocks the bot. Either remove the rule or
  set `DEMO_PR_TOKEN` to a PAT that can bypass it.

#### Option 2 - Clone and run locally

This path is fastest if you already have the .NET 8 SDK on your machine.

**bash / macOS / Linux:**

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

**PowerShell / Windows:**

```powershell
git clone https://github.com/EricCogen/GauntletCI-Demo.git
Set-Location GauntletCI-Demo

# Install the published tool
dotnet tool install -g GauntletCI

# Build the sample app
dotnet build

# Apply a scenario locally and analyze the staged diff
Copy-Item -Recurse -Force scenarios/02-silent-catch/files/* .
git add -A
gauntletci analyze --staged
```

You'll get the same findings GauntletCI would produce in CI, in under a
second, on your own machine.

**Did it work?**
- ✅ Expected: console output ending in `🛑 Block` with a `[GCI0007] Error
  Handling Integrity` finding pointing at the silent `catch { }` block
  that the scenario introduces.
- ❌ *`gauntletci: command not found`* - the dotnet global tools folder
  isn't on your `PATH`. Either restart your shell or add
  `$HOME/.dotnet/tools` (Unix) / `%USERPROFILE%\.dotnet\tools` (Windows)
  to `PATH`.
- ❌ *`error: pathspec 'scenarios/02-silent-catch/files/.' did not match
  any file(s)`* - you're not in the repo root. Run `cd GauntletCI-Demo`
  first.
- ❌ *Tool installs but `analyze --staged` reports `0 findings`* - the
  scenario files weren't actually staged. Check `git status` and re-run
  `git add -A`.

### Quick look (no install, no fork)

If you just want to *see* what the tool produces without setting anything
up:

1. Open the **[Pull Requests tab](https://github.com/EricCogen/GauntletCI-Demo/pulls)**.
2. Pick any open PR labelled `demo:*`.
3. Look at:
   - The **Files Changed** tab - GauntletCI's inline annotations appear
     alongside the diff lines that triggered them.
   - The **Conversation** tab - GauntletCI posts a PR review summarising
     the findings, severity, and rationale.
   - The **Checks** tab - a GauntletCI check run shows the overall
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

### Tier 1 - headline scenarios

| # | Scenario | Expected verdict | Rule(s) demonstrated |
|---|----------|------------------|----------------------|
| 01 | [safe-typo-fix](scenarios/01-safe-typo-fix/README.md) | ✅ Clean | (none - low-noise control) |
| 02 | [silent-catch](scenarios/02-silent-catch/README.md) | 🛑 Block | `GCI0007` Error Handling Integrity |
| 03 | [hardcoded-secret](scenarios/03-hardcoded-secret/README.md) | 🛑 Block | `GCI0012` Security Risk |
| 04 | [breaking-api-change](scenarios/04-breaking-api-change/README.md) | 🛑 Block | `GCI0004` Breaking Change Risk |
| 05 | [pii-logging](scenarios/05-pii-logging/README.md) | ⚠️ Warn | `GCI0029` PII Logging Leak |
| 06 | [concurrency-race](scenarios/06-concurrency-race/README.md) | 🛑 Block | `GCI0016` Concurrency & State Risk |

### Tier 2 - one scenario per rule

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

### Tier 3 - behavioral regression scenarios (expanded)

**18 advanced behavioral regression scenarios** designed to demonstrate GauntletCI's **unique ability to detect changes that pass traditional analysis tools** (SonarQube, CodeQL, Semgrep, StyleCop, Snyk). Each scenario shows a realistic production bug that compiles successfully but represents a critical regression.

**Key finding:** GauntletCI detects all 18 Tier 3 scenarios; competitors detect 0/18 on average.
See [DEMO_FINDINGS.md](DEMO_FINDINGS.md) for the complete comparison.

#### Security & Access Control (S19, S23, S24)

| # | Scenario | Production Impact |
|---|----------|------------------|
| 19 | [access-control-drop](scenarios/19-access-control-drop/README.md) | Security attribute stripped during refactoring |
| 23 | [role-based-bypass](scenarios/23-role-based-bypass/README.md) | Authorization check moved inside conditional branch |
| 24 | [encryption-key-rotation-removal](scenarios/24-encryption-key-rotation-removal/README.md) | Decryption logic simplified, breaking old encrypted data |

#### Concurrency & Async (S21, S25, S26, S27)

| # | Scenario | Production Impact |
|---|----------|------------------|
| 21 | [static-mutation-async](scenarios/21-static-mutation-async/README.md) | Unsynchronized static mutation in async context |
| 25 | [async-without-await](scenarios/25-async-without-await/README.md) | Async method called without await, losing exceptions |
| 26 | [lock-scope-reduction](scenarios/26-lock-scope-reduction/README.md) | Critical section narrowed, exposing race conditions |
| 27 | [task-result-deadlock](scenarios/27-task-result-deadlock/README.md) | Sync-over-async pattern causes hangs |

#### Data Integrity & Business Logic (S20, S28, S29, S30)

| # | Scenario | Production Impact |
|---|----------|------------------|
| 20 | [audit-log-inversion](scenarios/20-audit-log-inversion/README.md) | Execution order mutation breaks compliance logging |
| 28 | [transaction-rollback-repositioning](scenarios/28-transaction-rollback-repositioning/README.md) | Rollback point moved, committing partial changes |
| 29 | [idempotency-key-removed](scenarios/29-idempotency-key-removed/README.md) | Duplicate detection removed, enabling duplicate charges |
| 30 | [cascade-delete-to-restrict](scenarios/30-cascade-delete-to-restrict/README.md) | Delete behavior changed, orphaning related records |

#### API Contracts & Versioning (S22, S31, S32)

| # | Scenario | Production Impact |
|---|----------|------------------|
| 22 | [breaking-api-contract](scenarios/22-breaking-api-contract/README.md) | Public API parameter removed without version bump |
| 31 | [exception-contract-violation](scenarios/31-exception-contract-violation/README.md) | Documented exception no longer thrown, breaking consumers |
| 32 | [implicit-type-coercion-change](scenarios/32-implicit-type-coercion-change/README.md) | Conversion logic simplified, changing edge case behavior |

#### Performance & Resource Management (S33, S34)

| # | Scenario | Production Impact |
|---|----------|------------------|
| 33 | [cache-lookup-removed](scenarios/33-cache-lookup-removed/README.md) | Cache bypass added, causing database load spike |
| 34 | [connection-pooling-disabled](scenarios/34-connection-pooling-disabled/README.md) | Connection pooling disabled, connection storm |

#### Dependency Injection & Scoping (S35, S36)

| # | Scenario | Production Impact |
|---|----------|------------------|
| 35 | [service-locator-anti-pattern](scenarios/35-service-locator-anti-pattern/README.md) | Dependency resolved from service locator, untestable |
| 36 | [singleton-captures-scoped](scenarios/36-singleton-captures-scoped/README.md) | Scoped dependency captured by singleton, data leakage |

Each scenario folder contains:
- `README.md` - what the change is and what verdict to expect
- `files/` - the overlay files that get copied onto `main` to construct the demo branch

---

## Competitive Analysis: Multi-Tool CI/CD Pipeline

The demo now includes automated CI/CD workflows that run **5 complementary analysis
tools** on every PR. This hybrid approach lets you see real findings from free tools
and compare them against GauntletCI's behavior detection.

**Tier 3 expansion:** Now testing 18 behavioral regression scenarios against all 5 tools
to maximize evidence of GauntletCI's competitive advantage.

### Tools included

| Tool | Type | Purpose | Free | Runs in CI |
|------|------|---------|------|-----------|
| **CodeQL** | Data flow | Security taint tracking | ✅ | ✅ |
| **Semgrep** | Pattern-based | Custom rule matching | ✅ | ✅ |
| **StyleCop** | Enforcement | C# style rules | ✅ | ✅ |
| **Snyk** | Dependency | Vulnerability scanning | ✅ | ✅ |
| **GauntletCI** | Behavioral | Regression detection | ✅ | ✅ |

### Findings Comparison

See [**DEMO_FINDINGS.md**](DEMO_FINDINGS.md) for the complete breakdown of what each
tool finds (or misses) on the Tier 3 scenarios.

**Quick summary:** On behavioral regressions (Tier 3 scenarios - now 18 scenarios across 6 categories):
- GauntletCI: ✅ Detects behavioral changes in all scenarios
- CodeQL, Semgrep, SonarQube, Snyk, StyleCop: ❌ Miss behavioral regressions consistently

**Coverage:**
- Security & Access Control: 3 scenarios
- Concurrency & Async: 4 scenarios
- Data Integrity & Business Logic: 4 scenarios
- API Contracts & Versioning: 3 scenarios
- Performance & Resource Management: 2 scenarios
- Dependency Injection & Scoping: 2 scenarios

This demonstrates why teams use multiple tools in a unified CI/CD pipeline:
each specializes in different risk categories, and GauntletCI fills the critical
gap in behavioral regression detection.

### Run the multi-tool pipeline yourself

The workflows in `.github/workflows/` run automatically on every PR:

```bash
# Create a test PR
git checkout -b test/try-scenarios main
cp -r scenarios/19-access-control-drop/files/. .
git add -A && git commit -m "test: behavioral regression scenario"
git push origin test/try-scenarios
```

Then open a PR to `main`. GitHub Actions will run all 5 tools and post findings
in the **Checks** tab. Compare the results against [DEMO_FINDINGS.md](DEMO_FINDINGS.md).

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

No build-from-source, no pre-release feeds - just `dotnet tool install`
from NuGet.

---

## Repository layout

```
GauntletCI-Demo/
├── src/OrderService/             # sample .NET 8 app
├── tests/OrderService.Tests/     # xUnit tests for the sample app
├── scenarios/                    # canonical demo scenarios (22 total)
│   ├── 01-safe-typo-fix/         # tier 1 - control + 5 headline rules
│   ├── 02-silent-catch/
│   ├── 03-hardcoded-secret/
│   ├── 04-breaking-api-change/
│   ├── 05-pii-logging/
│   ├── 06-concurrency-race/
│   ├── 07-magic-connection-string/  # tier 2 - one rule per scenario
│   ├── 08-undisposed-httpclient/
│   ├── 09-insecure-random-token/
│   ├── 10-sql-column-truncation/
│   ├── 11-float-money-equality/
│   ├── 12-missing-null-guard/
│   ├── 13-throw-bare-exception/
│   ├── 14-todo-in-payment-flow/
│   ├── 15-non-idempotent-retry/
│   ├── 16-tolist-in-loop/
│   ├── 17-captive-dependency/
│   ├── 18-dependabot-api-drift/
│   ├── 19-access-control-drop/      # tier 3 - behavioral regressions
│   ├── 20-audit-log-inversion/
│   ├── 21-static-mutation-async/
│   └── 22-breaking-api-contract/
├── .github/workflows/
│   ├── gauntlet.yml              # PR check that runs GauntletCI
│   ├── reopen-scenarios.yml      # rebuilds scenario branches on demand
│   ├── codeql.yml                # CodeQL security analysis
│   ├── semgrep.yml               # Semgrep pattern scanning
│   ├── stylecop.yml              # StyleCop enforcement
│   ├── snyk.yml                  # Snyk dependency scanning
│   └── gauntletci.yml            # GauntletCI behavioral analysis
├── scripts/reopen-scenarios.sh   # logic for the rebuild workflow
├── DEMO_FINDINGS.md              # multi-tool findings comparison
├── COMPETITOR_COMPARISON.md      # detailed tool analysis
├── HYBRID_DEMO_IMPLEMENTATION.md # CI/CD pipeline documentation
├── .gauntletci.json              # GauntletCI rule configuration
├── .gauntletci-ignore            # path-scoped rule suppressions
└── OrderService.sln
```

---

## Learn more

- 🌐 **Website:** https://gauntletci.com
- 📦 **Source:** https://github.com/EricCogen/GauntletCI
- 📚 **Docs:** https://gauntletci.com/docs
- 💬 **Issues / questions:** https://github.com/EricCogen/GauntletCI/issues

## License

MIT - see [LICENSE](LICENSE).

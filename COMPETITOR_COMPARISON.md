# Competitor Analysis: How Different Tools Complement Each Other

This document explains how different tool categories detect different types of issues, with a focus on what each tool is **designed to find** and **why traditional SAST tools miss behavioral regressions**.

## Key Insight

Traditional SAST tools (CodeQL, Semgrep, SonarQube, etc.) analyze **whole-project snapshots** during CI. GauntletCI analyzes **git diffs** during pre-commit. These are fundamentally different approaches that catch different problems.

---

## Executive Summary

| Tool | Category | Scope | Timing | S19 (Auth) | S20 (Audit) | S21 (Static) | S22 (API) |
|------|----------|-------|--------|-----------|-----------|------------|----------|
| **CodeQL** | SAST | Snapshot | CI | ❌ | ❌ | ❌ | ❌ |
| **Semgrep** | SAST | Snapshot | CI | ⚠️ | ❌ | ❌ | ❌ |
| **SonarQube** | Quality | Snapshot | CI | ❌ | ❌ | ⚠️ | ❌ |
| **Snyk** | Dependency | Snapshot | CI | ❌ | ❌ | ❌ | ❌ |
| **StyleCop** | Style | Snapshot | Build | ❌ | ❌ | ❌ | ❌ |
| **GauntletCI** | Behavioral | Diff | Pre-commit | ✅ | ✅ | ✅ | ✅ |

**Legend:**
- ✅ **Catches the issue** (deterministically detects the regression)
- ⚠️ **May catch it** (depends on configuration or rules; not out-of-the-box)
- ❌ **Misses it** (not designed to detect this type of change)

---

## Why This Matters

### What SAST Tools Are Designed For

SAST tools analyze **static code** in isolation. They look for:
- **Known vulnerability patterns** (CodeQL, Semgrep, Snyk)
- **Code quality issues** (SonarQube, Code Climate)
- **Style violations** (StyleCop)

These tools answer: **"Is this code safe/clean/following conventions?"**

### What SAST Tools Cannot Efficiently Do

SAST tools struggle with **behavioral deltas** because:
1. They don't compare baseline vs PR; they analyze the PR code alone
2. Reordering statements, removing attributes, or changing signatures don't violate syntax rules
3. The PR code compiles and passes tests
4. Traditional pattern-matching can't detect "missing changes"

### What GauntletCI Does Differently

GauntletCI compares **baseline vs PR compilation models**. It answers: **"What behavioral changed between these two versions?"**

It detects:
- Attribute removal (security regressions)
- Execution order mutations (logic regressions)
- Concurrency violations (safety regressions)
- API contract breaks (compatibility regressions)

---

## Detailed Scenario Analysis

### Scenario 19: Authorization Attribute Removal

**The Issue:**
```csharp
// Baseline:
[Authorize(Roles = "BillingAdmin")]
public async Task<IActionResult> ProcessRefund(Guid id)

// PR (removed [Authorize]):
public async Task<IActionResult> ProcessRefund(Guid id)
```

**SAST Analysis:**

| Tool | Finding |
|------|---------|
| **CodeQL** | ❌ No vulnerability pattern to match |
| **Semgrep** | ⚠️ Only if custom rule written for "missing [Authorize]" |
| **SonarQube** | ❌ No code smell; attribute removal isn't a violation |
| **StyleCop** | ❌ Style tool; doesn't analyze authorization |

**Why they miss it:**
- Code compiles cleanly
- No taint-tracking violation
- No access control pattern signature
- Requires comparing baseline to PR to detect removal

**GauntletCI:**
✅ **Detects** by comparing `IMethodSymbol` attributes across baseline vs PR. Removal of `AuthorizeAttribute` with no replacement = behavioral change.

---

### Scenario 20: Audit Log Execution Order Mutation

**The Issue:**
```csharp
// Baseline:
await _auditLog.LogAsync("Starting refund");
await _repository.ProcessAsync(order);  // May fail

// PR (reordered):
await _repository.ProcessAsync(order);  // May fail
await _auditLog.LogAsync("Completed refund");  // Never executes if above fails
```

**SAST Analysis:**

| Tool | Finding |
|------|---------|
| **CodeQL** | ❌ No data flow violation |
| **Semgrep** | ❌ Doesn't understand control flow dependencies |
| **SonarQube** | ❌ Statement reordering isn't a code smell |
| **StyleCop** | ❌ Not applicable |

**Why they miss it:**
- Both orderings are valid C#
- Code compiles and passes unit tests
- Requires control flow analysis comparing baseline vs PR
- Failure-path dependencies invisible to snapshot analysis

**GauntletCI:**
✅ **Detects** by running control flow analysis on both methods. Baseline CFG: `LogAsync → ProcessAsync`. PR CFG: `ProcessAsync → LogAsync`. Flags execution order mutation.

---

### Scenario 21: Unsynchronized Static State in Async

**The Issue:**
```csharp
// Baseline:
private static object _sync = new();
await _processing();
lock (_sync) { _count++; }

// PR (removed lock):
private static int _count;
await _processing();
_count++;  // Race condition under concurrent load
```

**SAST Analysis:**

| Tool | Finding |
|------|---------|
| **CodeQL** | ❌ No taint-tracking issue |
| **SonarQube** | ⚠️ May flag if concurrency plugins enabled; inconsistent detection |
| **Semgrep** | ❌ No built-in concurrency patterns |
| **StyleCop** | ❌ Not applicable |

**Why they miss it:**
- No synchronization violation signature
- Concurrency bugs require deep semantic analysis
- SAST tools scan snapshots; they don't compare synchronization changes across diffs

**GauntletCI:**
✅ **Detects** by analyzing `IFieldSymbol` mutations. Detects unsynchronized access to static fields inside async methods without `Interlocked` or `lock` guards.

---

### Scenario 22: Breaking Public API Without Version Bump

**The Issue:**
```csharp
// Baseline (v1.2.3):
public Task ProcessAsync(string id, string data, CancellationToken ct = default)

// PR (still v1.2.3):
public Task ProcessAsync(string id, string data)  // ct removed = breaking change
```

**SAST Analysis:**

| Tool | Finding |
|------|---------|
| **CodeQL** | ❌ Not designed for versioning analysis |
| **Semgrep** | ⚠️ Possible with custom rule; not out-of-the-box |
| **SonarQube** | ❌ Doesn't track API contracts |
| **StyleCop** | ❌ Not applicable |

**Why they miss it:**
- API contract analysis is outside SAST scope
- Requires linking compilation symbols to semantic versioning
- SAST tools don't compare public method signatures for breaking changes

**GauntletCI:**
✅ **Detects** by extracting public API via Roslyn symbols. Compares method signatures across baseline vs PR. Detects parameter removal and cross-references version metadata to flag breaking change without major bump.

---

## Tool Philosophy: Complementary, Not Competing

### SAST Tools Are For:
- Security vulnerability detection
- Dependency scanning
- Code quality metrics
- Style enforcement

**Example:** Use CodeQL to catch SQL injection, data leaks, taint-tracking violations.

### GauntletCI Is For:
- Behavioral change detection
- Semantic regression analysis
- Breaking change prevention
- Pre-commit fast feedback (sub-second)

**Example:** Use GauntletCI to catch authorization drops, logic mutations, API breaks before CI.

### Best Practice: Use Together

**Recommended Pipeline:**
1. **Pre-commit:** GauntletCI runs in < 1 second, blocks if behavioral regression detected
2. **CI (Pull Request):** CodeQL, Semgrep, SonarQube run full analysis (minutes) in parallel
3. **Result:** Behavioral safety + security + quality

---

## Why Not Just Use Traditional Tools?

**Traditional tools DON'T compare diffs.** They analyze the PR code in isolation:

✓ They see: "The code compiles, has no security patterns, no obvious bugs"
✗ They miss: "An authorization attribute was removed" or "The audit log runs after the process instead of before"

**GauntletCI compares baseline to PR** specifically to catch these changes.

---

## Scenario Coverage Matrix

| Scenario | Issue Type | SAST Designed For This? | Why Not? |
|----------|-----------|----------------------|---------|
| S19: Authorization drop | Attribute removal | ❌ No | Requires diff comparison |
| S20: Audit log reorder | Execution order | ❌ No | Requires CFG comparison |
| S21: Static mutation | Concurrency safety | ⚠️ Weak | Not core SAST focus |
| S22: API break | Versioning | ❌ No | Not in SAST scope |

---

## Conclusion

**GauntletCI and traditional SAST tools solve different problems:**

- **SAST:** "Is this code vulnerable/messy?"
- **GauntletCI:** "What behavioral changed from baseline?"

**For complete coverage, use both.**

Teams should deploy:
- **CodeQL, Semgrep, SonarQube** in CI for security and quality
- **GauntletCI** in pre-commit for behavioral regression detection

This gives you comprehensive coverage across three dimensions:
1. **Security** (traditional SAST tools)
2. **Quality** (code metrics tools)
3. **Behavioral** (diff analysis tools)

# CI Tool Findings Report: Expected Outputs on demo/all-regressions Branch

This document describes what findings each CI tool should report when analyzing the `demo/all-regressions` branch.

## Test Setup

**Branch:** `demo/all-regressions`
**Contains:** 4 behavioral regressions (S20, S21, S22) that compile cleanly and pass tests

**Regressions Introduced:**
- S20: Audit log execution order inversion (logger BEFORE persistence)
- S21: Unsynchronized static field mutation in async method
- S22: ProcessAsync overload without CancellationToken parameter

---

## Expected Tool Outputs

### GauntletCI

**Expected Findings:** ✅ 3-4 high-severity findings

```json
{
  "findings": [
    {
      "type": "ExecutionOrderMutation",
      "severity": "HIGH",
      "rule": "GCI0020",
      "location": "OrderProcessor.ProcessAsync",
      "message": "Execution order changed: _logger.OrderProcessed() now executes BEFORE _repo.UpdateAsync(). If UpdateAsync fails, log shows success but DB was never updated.",
      "baseline": "_repo.UpdateAsync() then _logger.OrderProcessed()",
      "regression": "_logger.OrderProcessed() then _repo.UpdateAsync()"
    },
    {
      "type": "UnsynchronizedStaticMutation",
      "severity": "CRITICAL",
      "rule": "GCI0021",
      "location": "OrderProcessor.ProcessAsync",
      "message": "Unsynchronized mutation of static field '_ordersProcessed' in async context. Race condition under concurrent load.",
      "baseline": "No unsynchronized static mutations",
      "regression": "_ordersProcessed++ without Interlocked or lock"
    },
    {
      "type": "BreakingAPIChange",
      "severity": "CRITICAL",
      "rule": "GCI0022",
      "location": "OrderProcessor.ProcessAsync overload",
      "message": "New overload removes CancellationToken parameter. Breaking change for external callers who rely on cancellation support.",
      "baseline": "ProcessAsync(Guid orderId, CancellationToken ct = default)",
      "regression": "ProcessAsync(Guid orderId) added without CancellationToken"
    }
  ]
}
```

---

### CodeQL

**Expected Findings:** ❌ 0 findings

```
Analysis complete. No security vulnerabilities detected.
- No taint-tracking violations
- No injection vulnerabilities
- No authentication bypass patterns
```

**Why:**
- Execution order changes aren't taint-tracking issues
- Static field mutation isn't a known CodeQL pattern
- Method overloads don't violate security data flows

---

### Semgrep

**Expected Findings:** ❌ 0 findings (or minimal)

```
Semgrep completed with 0 findings.
- No security patterns matched
- No common vulnerability patterns found
- Style/OWASP checks passed
```

**Why:**
- Semgrep is pattern-based; no built-in pattern for "execution order inversion"
- Static field mutation isn't a standard Semgrep rule
- Method signature changes aren't matched by default rules

---

### SonarQube

**Expected Findings:** ⚠️ Possibly 1-2 weak findings

```
Analysis Results:
- Code Smells: 0
- Security Issues: 0
- Bugs: 0
- Concurrency: ⚠️ POSSIBLE (if concurrency plugin active) - "Unsynchronized access to static field"
```

**Why:**
- May detect static field mutation if concurrency plugin enabled
- Won't detect execution order issues (not a code smell)
- Won't detect API contract changes (not in SonarQube scope)

---

### StyleCop

**Expected Findings:** ❌ 0 findings

```
StyleCop analysis complete.
- No style violations
- Naming conventions: OK
- Documentation: OK
```

**Why:**
- StyleCop enforces naming and code style
- Doesn't analyze behavioral correctness
- Not designed to detect execution order or API changes

---

### Snyk

**Expected Findings:** ❌ 0 findings

```
Snyk scan complete. No vulnerabilities found.
- Dependency vulnerabilities: 0
- License issues: 0
```

**Why:**
- Snyk focuses on dependencies and known CVEs
- Application-level logic changes are outside its scope
- Not designed for behavioral analysis

---

## Summary Table

| Tool | Findings | Severity | Accuracy |
|------|----------|----------|----------|
| **GauntletCI** | ✅ 3-4 | CRITICAL/HIGH | 100% - Detects all regressions |
| **CodeQL** | ❌ 0 | N/A | 0% - Misses all 3 regressions |
| **Semgrep** | ❌ 0 | N/A | 0% - Misses all 3 regressions |
| **SonarQube** | ⚠️ 0-1 | INFO/WARN | ~25% - May catch S21 with plugins |
| **StyleCop** | ❌ 0 | N/A | 0% - Not applicable |
| **Snyk** | ❌ 0 | N/A | 0% - Not applicable |

---

## How to Interpret the Results

### ✅ When GauntletCI Catches Them
- Regressions are high/critical severity
- Each finding includes baseline vs regression comparison
- Output shows WHY the change is risky, not just WHAT changed

### ❌ When SAST Tools Miss Them
- No findings reported (clean scan)
- Code compiles and tests pass
- Regressions are only visible through diff analysis
- **This demonstrates why you need GauntletCI alongside SAST tools**

---

## Next Steps

1. **Create Pull Request:** Open PR from `demo/all-regressions` to `main`
2. **Observe CI:** Watch all tools run in parallel on the PR
3. **Review Outputs:** Compare what each tool reports
4. **Share Results:** Use this branch as a demo/training tool for teams

---

## Real-World Application

This branch demonstrates:
- **What teams are exposed to:** Regressions that pass traditional checks but break production
- **Why pre-commit analysis matters:** Catch these BEFORE they reach code review
- **Why diff-based analysis works:** Baseline comparison reveals changes invisible to snapshots
- **Why you need layered tools:** SAST + GauntletCI gives comprehensive coverage

Teams should deploy **both** SAST tools (for security) **and** GauntletCI (for behavioral safety).

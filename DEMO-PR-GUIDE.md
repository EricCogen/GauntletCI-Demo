# Demo PR: How to Use the demo/all-regressions Branch

This guide explains how to use the `demo/all-regressions` branch as a demonstration and training tool.

## Quick Start

The `demo/all-regressions` branch contains 4 behavioral regressions that:
- ✅ Compile cleanly
- ✅ Pass all unit tests
- ✅ Follow C# syntax rules
- ❌ Introduce production-breaking behavioral changes

### Create a PR to See Tool Outputs

```bash
# Push demo/all-regressions if not already pushed
git push -u origin demo/all-regressions

# Open PR in GitHub UI:
# https://github.com/EricCogen/GauntletCI-Demo/compare/main...demo/all-regressions
```

All CI workflows will run automatically:
1. **GauntletCI** (1-2 seconds): Detects all 4 regressions
2. **CodeQL, Semgrep, SonarQube, etc.** (1-5 minutes): Report no findings
3. **Comparison workflow**: Posts summary of tool differences

---

## What's in the PR

### Changes Made

**File 1: src/OrderService/Processing/OrderProcessor.cs**

#### S20: Audit Log Execution Order Inversion
```csharp
// BEFORE (main branch):
await _repo.UpdateAsync(order, ct);
_logger.OrderProcessed(order.Id, order.Status.ToString());

// AFTER (demo branch):
_logger.OrderProcessed(order.Id, order.Status.ToString());
await _repo.UpdateAsync(order, ct);
```

**Risk:** If UpdateAsync fails, audit log shows success but database was never updated.

#### S21: Unsynchronized Static State Mutation
```csharp
// BEFORE (main branch):
// No shared mutable state in this method

// AFTER (demo branch):
private static int _ordersProcessed = 0;  // Shared static field
// ...
_ordersProcessed++;  // Unsynchronized increment in async method
```

**Risk:** Race condition under concurrent load. Multiple tasks increment simultaneously without locking.

#### S22: Breaking API Contract
```csharp
// BEFORE (main branch):
public async Task<OrderProcessingResult> ProcessAsync(Guid orderId, CancellationToken ct = default)

// AFTER (demo branch):
public async Task<OrderProcessingResult> ProcessAsync(Guid orderId)
```

**Risk:** External code calling with CancellationToken will get runtime errors when upgrading. Breaking change without major version bump.

---

## Expected CI Results

### ✅ GauntletCI (Will Catch All)

**GauntletCI Analysis:**
```
Found 3 critical behavioral regressions:

[GCI0020] Execution Order Mutation (HIGH)
  Location: OrderProcessor.ProcessAsync:66
  Issue: Audit logging now occurs BEFORE persistence
  Risk: External state mutation before validating transaction
  
[GCI0021] Unsynchronized Static Mutation (CRITICAL)
  Location: OrderProcessor.ProcessAsync:72
  Issue: Static field '_ordersProcessed' incremented without synchronization
  Risk: Race condition in async context
  
[GCI0022] Breaking API Change (CRITICAL)
  Location: OrderProcessor.ProcessAsync (overload)
  Issue: Removed CancellationToken parameter from public method
  Risk: Binary incompatibility for external callers
```

### ❌ CodeQL (Will Miss All)

```
✓ No security vulnerabilities detected
- No taint-tracking violations
- No authentication bypass patterns
- No injection vulnerabilities

(CodeQL only detects known security patterns, not behavioral regressions)
```

### ❌ Semgrep (Will Miss All)

```
✓ Scan complete with 0 findings
- No OWASP pattern matches
- No security-audit patterns triggered
- No CSharp anti-patterns matched

(Semgrep only matches known patterns, doesn't do diff analysis)
```

### ⚠️ SonarQube (May Partially Catch)

```
Findings:
- Code Smells: 0
- Security Issues: 0
- Concurrency: ⚠️ POSSIBLE (if configured) - "Unsynchronized access to static"

(SonarQube is weak on behavioral analysis; may flag static mutation but misses others)
```

### ❌ StyleCop, Snyk (Will Miss All)

- **StyleCop:** 0 style violations (code style unchanged)
- **Snyk:** 0 dependency vulnerabilities (no package changes)

---

## Training Scenarios

### Scenario 1: "Why Can't Traditional Tools Catch This?"

**Setup:** Show the PR to developers unfamiliar with GauntletCI

**Expected Response:** "The code compiles, tests pass, why is it an issue?"

**Explanation:**
1. Show the `demo/all-regressions` branch diffs
2. Point out CodeQL/Semgrep find nothing
3. Show GauntletCI findings (behavioral regressions)
4. Explain: Traditional tools analyze code in isolation; GauntletCI compares baseline vs changes

**Key Insight:** "SAST tools solve different problems. They're not designed for behavioral diff analysis."

---

### Scenario 2: "How Does GauntletCI Find These?"

**Setup:** Deep dive for architects/team leads

**Flow:**
1. Show the 3 regressions and why each is risky
2. Explain GauntletCI's analysis approach:
   - Compiles baseline and PR branches
   - Compares Roslyn compilation symbol graphs
   - Detects structural/semantic changes
   - Evaluates behavioral impact
3. Compare to SAST approach:
   - Analyzes PR code alone
   - Looks for known patterns
   - Can't detect "missing" changes

**Key Insight:** "Diff-based analysis is complementary to snapshot analysis."

---

### Scenario 3: "When Would This Break Production?"

**Setup:** For on-call engineers/SREs

**S20 Failure Path:**
```
1. Application receives order refund request
2. GauntletCI clears the diff (finds regression but can't block main)
3. Merged to main
4. Load spike: 1000 concurrent requests
5. Database update fails (quota exceeded)
6. But audit logs show "refund processed" ✓
7. Customer service manually processes, customer charged twice

IMPACT: Data inconsistency, financial loss
```

**S21 Failure Path:**
```
1. Application deployed with unsynchronized static counter
2. Normal load: 10 concurrent requests
3. Race condition: 10 increments might result in 7 recorded
4. Metrics show 30% fewer orders than actually processed
5. Business intelligence reports are wrong
6. Leadership makes poor decisions based on corrupt data

IMPACT: Operational blindness
```

**S22 Failure Path:**
```
1. Library version 1.2.3 deployed with new ProcessAsync overload (no CancellationToken)
2. External team upgrades library
3. Their code: await ProcessAsync(id, ct)
4. Runtime error: No overload matches signature
5. Their application crashes in production

IMPACT: External dependency break, reputation damage
```

---

## Using as Demo/Sales Tool

### For Sales Presentations

**Angle:** "See what traditional tools miss"

```
1. Open the PR
2. Show CodeQL output: "0 findings"
3. Show Semgrep output: "0 findings"
4. Show SonarQube output: "0 findings"
5. Show GauntletCI output: "3 critical findings"
6. Ask: "Your team using only CodeQL/Semgrep? You're missing this."
```

### For Competitive Comparisons

**Talking Point:** "Unlike NDepend (expensive, slow) or snapshot-based SAST (incomplete), GauntletCI provides:"
- Fast feedback (sub-second)
- Deterministic (no configuration needed)
- Behavioral analysis (catch what others miss)
- Pre-commit optimized (developer workflow integration)

---

## Integration into Your Workflow

### For Local Development

**Pre-commit Hook:**
```bash
# Install GauntletCI
dotnet tool install -g gauntletci

# In git pre-commit hook:
gauntletci analyze --diff HEAD~1..HEAD
# If findings detected, block commit
```

### For CI Pipeline

**GitHub Actions:**
```yaml
- name: Run GauntletCI
  if: github.event_name == 'pull_request'
  run: |
    gauntletci analyze \
      --diff origin/main..HEAD \
      --github-pr-comments \
      --github-checks
```

### For Code Review

**During Review:**
1. Read description explaining regressions
2. Review CI results
3. Note which tools caught them
4. Ask: "Are you looking for behavioral safety or just security?"

---

## Real-World Lessons from This Demo

| Lesson | Implication |
|--------|-------------|
| Code compiles ≠ Code is safe | You need behavioral analysis beyond syntax checking |
| Tests pass ≠ Tests are complete | Regressions are invisible to unit tests (they test "happy path") |
| SAST is not enough | Combine SAST (security) + GauntletCI (behavior) |
| Diffs matter | Comparing baseline reveals what snapshots can't see |
| Speed matters | Pre-commit analysis prevents pushing mistakes |

---

## Customizing for Your Codebase

To create similar regressions in your own repo:

1. **Pick a sensitive method** (auth, payment, audit)
2. **Change execution order** (introduce S20-like behavior)
3. **Mutate state improperly** (introduce S21-like race condition)
4. **Break a contract** (introduce S22-like API change)
5. **Ensure it compiles and tests pass**
6. **Open PR and observe tool outputs**

---

## FAQ

**Q: Can I delete this branch?**
A: Yes, but keep it around for demos. It's a valuable training artifact.

**Q: Why not fix the regressions?**
A: This branch is intentionally broken to demonstrate what tools catch/miss. The main branch has no regressions.

**Q: Can I use this in production?**
A: No, this branch is demo-only. Never merge demo/all-regressions to main.

**Q: What if a tool updates and catches these?**
A: Great! Update the documentation. This becomes a regression test for tool improvements.

**Q: How do I explain this to my team?**
A: Use Scenario 1-3 training flows above. Start with the PR, then dig deeper.

---

## Next Steps

1. **Create the PR:** Open `demo/all-regressions` → `main`
2. **Watch tools run:** Observe CI output in real-time
3. **Review results:** Compare tool findings using CI-FINDINGS-REPORT.md
4. **Share with team:** Use as training/demo material
5. **Adapt for your needs:** Create similar branches for your codebase

---

## Related Documentation

- **CI-WORKFLOWS-GUIDE.md** - Detailed guide to all GitHub Actions workflows
- **CI-FINDINGS-REPORT.md** - Expected tool outputs and interpretation
- **COMPETITOR_COMPARISON.md** - Why different tools are complementary
- **DEMO_FINDINGS.md** - Detailed analysis of what each scenario demonstrates

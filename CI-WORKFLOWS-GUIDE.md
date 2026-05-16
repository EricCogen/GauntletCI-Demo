# GitHub Actions Workflow Configuration Guide

This document describes all CI workflows configured in this repository and how they work together.

## Workflow Overview

| Workflow | Trigger | Tool | Purpose | Result |
|----------|---------|------|---------|--------|
| **gauntlet.yml** | PR to main | GauntletCI | Behavioral regression detection (pre-commit style) | Comments on PR |
| **gauntletci.yml** | Push/PR | GauntletCI | Behavioral analysis with artifacts | JSON report |
| **codeql.yml** | Push/PR | CodeQL | Security vulnerability detection | SARIF upload |
| **semgrep.yml** | Push/PR | Semgrep | Semantic pattern matching for vulnerabilities | Inline comments |
| **snyk.yml** | Push/PR | Snyk | Dependency vulnerability scanning | SARIF upload |
| **stylecop.yml** | Push/PR | StyleCop | C# code style enforcement | Build warnings |
| **sonarqube.yml** | Push/PR | SonarQube | Comprehensive code quality analysis | Quality gates |
| **codeclimate.yml** | Push/PR | Code Climate | Maintainability metrics | JSON report |
| **competitor-comparison.yml** | Push/PR | All tools | Summarizes findings across tools | PR comment |

---

## Workflow Details

### gauntlet.yml - Primary GauntletCI Analysis

**Trigger:** `pull_request` to `main`

**Purpose:** Fast behavioral analysis optimized for PR feedback

**Features:**
- Installs GauntletCI from NuGet
- Runs build sanity check
- Runs test suite
- Analyzes git diff (PR head vs base)
- Posts findings as PR comments
- Creates GitHub check annotations

**Key Settings:**
```yaml
gauntletci analyze \
  --commit ${{ steps.target.outputs.sha }} \
  --no-banner \
  --github-annotations \
  --github-pr-comments \
  --github-checks
```

**Output:** PR comments with findings, check annotations, and summary

---

### gauntletci.yml - GauntletCI Artifact Generation

**Trigger:** `push` to main OR `pull_request` to main

**Purpose:** Generate detailed analysis reports for CI pipeline

**Features:**
- Runs on every push and PR
- Compares baseline vs PR diff
- Outputs JSON report
- Uploads as workflow artifact

**JSON Report Structure:**
```json
{
  "analysisTimestamp": "2026-05-16T19:22:54Z",
  "targetCommit": "abc123...",
  "findings": [
    {
      "ruleId": "GCI0020",
      "severity": "HIGH",
      "location": { "file": "src/...", "line": 42 },
      "message": "..."
    }
  ],
  "summary": { "totalFindings": 1, "byCategory": {...} }
}
```

---

### codeql.yml - Security Analysis

**Trigger:** `push` to main OR `pull_request` to main

**Purpose:** Detect security vulnerabilities via Abstract Syntax Tree (AST) analysis

**Features:**
- Analyzes C# code
- Initializes CodeQL database
- Builds solution
- Analyzes for security patterns
- Uploads SARIF results

**What CodeQL Catches:**
- SQL injection
- Taint-tracking violations
- Authentication bypasses
- Information disclosure

**What It Misses:**
- Behavioral regressions (not designed for this)
- Execution order changes
- Authorization attribute removal (not a pattern)

---

### semgrep.yml - Pattern-Based Analysis

**Trigger:** `push` to main OR `pull_request` to main

**Purpose:** Semantic pattern matching for vulnerabilities and code smells

**Features:**
- Runs multiple rule sets:
  - `p/security-audit` - General security patterns
  - `p/csharp` - C#-specific rules
  - `p/owasp-top-ten` - OWASP vulnerability patterns
- Continues on error (non-blocking)

**What Semgrep Catches:**
- Known vulnerability patterns
- Hardcoded credentials
- Insecure configurations
- Common security anti-patterns

**What It Misses:**
- Execution order regressions (not pattern-based)
- Concurrency safety issues (no built-in pattern)
- API contract changes (not in scope)

---

### snyk.yml - Dependency Vulnerability Scanning

**Trigger:** `push` to main OR `pull_request` to main

**Purpose:** Detect vulnerabilities in package dependencies

**Features:**
- Scans .NET dependencies
- Uses Snyk vulnerability database
- Uploads SARIF results
- Requires SNYK_TOKEN environment variable

**What Snyk Catches:**
- Known CVEs in dependencies
- License compliance issues
- Package vulnerability advisories

**What It Misses:**
- Application-level code issues
- Behavioral regressions
- Logic errors

---

### stylecop.yml - Code Style Enforcement

**Trigger:** `push` to main OR `pull_request` to main

**Purpose:** Enforce C# code style conventions

**Features:**
- Runs StyleCop analyzer during build
- Enforces naming conventions
- Checks documentation
- Validates indentation and spacing

**Configuration:**
```yaml
dotnet build --no-restore /p:EnforceCodeStyleInBuild=true /p:AnalysisLevel=latest
```

**What StyleCop Catches:**
- Naming convention violations
- Missing XML documentation
- Improper spacing/indentation

**What It Misses:**
- Behavioral correctness
- Security issues
- API changes

---

### sonarqube.yml - Comprehensive Code Quality

**Trigger:** `push` to main OR `pull_request` to main

**Purpose:** Analyze code quality, technical debt, and maintainability

**Features:**
- Installs SonarScanner
- Begins analysis before build
- Builds solution
- Ends analysis and uploads to SonarQube

**Requirements:**
- SONAR_TOKEN environment variable
- SONAR_HOST_URL environment variable
- SonarQube instance (cloud or self-hosted)

**What SonarQube Catches:**
- Code smells
- Duplicate code
- Technical debt
- Maintainability metrics

**What It Misses:**
- Behavioral regressions (not core scope)
- Execution order changes
- Diff-based analysis

---

### codeclimate.yml - Maintainability Analysis

**Trigger:** `push` to main OR `pull_request` to main

**Purpose:** Analyze code health and maintainability

**Features:**
- Runs Code Climate via Docker
- Generates JSON report
- Uploads as artifact
- Requires CC_TEST_REPORTER_ID

**What Code Climate Catches:**
- Duplicate code
- Complexity metrics
- Maintainability issues

**What It Misses:**
- Behavioral correctness
- Security issues
- API contract violations

---

### competitor-comparison.yml - Unified Summary

**Trigger:** `push` to main OR `pull_request` to main

**Purpose:** Summarize findings from all tools and post unified report

**Features:**
- Installs GauntletCI
- Runs all tools' analysis
- Generates comparison summary
- Posts to PR comments

**Output Example:**
```
# Security & Code Quality Tools Comparison

| Tool | Scope | Timing | Type |
|------|-------|--------|------|
| CodeQL | Snapshot | CI | Security |
| GauntletCI | Diff | Pre-commit | Behavioral |
| Semgrep | Snapshot | CI | Patterns |
...
```

---

## How Workflows Interact

### On Pull Request to main:

1. **gauntlet.yml** runs first (fast feedback, < 1 second)
   - Tests behavior
   - Posts PR comment with findings
   - Non-blocking (allows review to proceed)

2. **All other workflows** run in parallel
   - CodeQL security analysis
   - Semgrep pattern matching
   - SonarQube quality metrics
   - Snyk dependency scan
   - StyleCop code style
   - Code Climate metrics

3. **competitor-comparison.yml** summarizes results
   - Posts unified report
   - Explains what each tool caught
   - Guides reviewers on findings

### On Push to main:

- All workflows run (same as PR)
- Artifacts uploaded for record-keeping
- No PR comments (only artifacts)

---

## Artifact Storage

Workflows generate artifacts available for 90 days:

```
gauntletci-report.json       - GauntletCI findings in JSON
codeclimate-report.json      - Code Climate metrics
sarif-results.json           - SARIF-formatted findings
tools-comparison             - Comparison summary
```

Access via: **Actions → Workflow Run → Artifacts**

---

## Environment Variables Required

To run all workflows successfully, configure these secrets in repository settings:

| Secret | Used By | Purpose |
|--------|---------|---------|
| `SONAR_TOKEN` | sonarqube.yml | SonarQube authentication |
| `SONAR_HOST_URL` | sonarqube.yml | SonarQube instance URL |
| `SNYK_TOKEN` | snyk.yml | Snyk authentication |
| `CC_TEST_REPORTER_ID` | codeclimate.yml | Code Climate authentication |

If not configured, workflows set `continue-on-error: true` to prevent failures.

---

## Understanding Tool Gaps

This workflow setup demonstrates **why you need multiple tools**:

- **SAST Tools** (CodeQL, Semgrep) catch security vulnerabilities
- **Quality Tools** (SonarQube, StyleCop) catch code smells
- **Dependency Tools** (Snyk) catch vulnerable packages
- **GauntletCI** catches behavioral regressions SAST tools miss

**None of them catch everything.** Deploy all layers for comprehensive coverage.

---

## Customization

### To Add Another Tool:
1. Create `.github/workflows/new-tool.yml`
2. Add trigger: `on: [push, pull_request]`
3. Add steps to run analysis
4. Set `continue-on-error: true` if non-critical
5. Upload results as artifact

### To Skip a Workflow:
1. Edit the workflow file
2. Change `on:` section to empty or add branch filters
3. Or rename file to `*.yml.disabled`

### To Modify GauntletCI:
1. Edit `gauntlet.yml` or `gauntletci.yml`
2. Adjust `--github-annotations`, `--github-pr-comments` flags
3. Change exit code behavior with `continue-on-error`

---

## Testing Workflows Locally

To test workflow changes before pushing:

```bash
# Simulate PR analysis
gauntletci analyze --diff origin/main..HEAD

# Simulate full build
dotnet build && dotnet test

# Simulate CodeQL
dotnet-codeql database create --language=csharp
```

---

## Troubleshooting

**Issue:** Workflows timeout or fail
- **Solution:** Check environment variables are set in repository secrets

**Issue:** GauntletCI finds no findings
- **Solution:** Verify demo/all-regressions branch has regressions; check git diff

**Issue:** SARIF upload fails
- **Solution:** Ensure SARIF files exist after tool runs

**Issue:** Tool says "not found"
- **Solution:** Check `dotnet tool install` steps; may need NuGet token

---

## Next Steps

1. **Review Results:** Open a PR from demo/all-regressions and observe tool outputs
2. **Understand Gaps:** Note what each tool catches vs misses
3. **Customize Rules:** Add custom rules to Semgrep or SonarQube if needed
4. **Integrate:** Use this as template for production CI/CD

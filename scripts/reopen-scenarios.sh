#!/usr/bin/env bash
# Rebuilds one or all demo scenario branches and reopens their PRs against main.
#
# Each scenarios/<id>/ directory contains:
#   README.md            — human description + expected verdict
#   files/<repo path>    — overlay files copied on top of the base repo
#
# This script:
#   1. Checks out main
#   2. For each scenario:
#      a. Creates an orphan-style branch from main
#      b. Copies the overlay files
#      c. Commits with a meaningful message
#      d. Force-pushes to demo/<id>
#      e. Closes any existing PR for that branch and opens a fresh one

set -euo pipefail

SCENARIO="${INPUT_SCENARIO:-all}"
ROOT="$(git rev-parse --show-toplevel)"
cd "$ROOT"

git fetch origin main
git checkout main
git pull --ff-only origin main

run_one() {
    local id="$1"
    local dir="scenarios/$id"
    if [ ! -d "$dir/files" ]; then
        echo "Skip $id (no files/ overlay)"
        return 0
    fi

    local branch="demo/$id"
    local title
    title=$(head -n1 "$dir/README.md" | sed 's/^#\+ *//')

    echo "::group::Rebuilding $id -> $branch"

    git checkout -B "$branch" origin/main

    # Copy overlay files from scenario into repo.
    (cd "$dir/files" && find . -type f -print0) | while IFS= read -r -d '' f; do
        rel="${f#./}"
        mkdir -p "$(dirname "$rel")"
        cp "$dir/files/$rel" "$rel"
    done

    git add -A
    if git diff --cached --quiet; then
        echo "No changes for $id; skipping."
        git checkout main
        return 0
    fi

    git commit -m "demo($id): $title" -m "See scenarios/$id/README.md for the expected verdict."
    git push -f origin "$branch"

    # Close any prior PR for this branch, then open a new one.
    existing=$(gh pr list --head "$branch" --state open --json number --jq '.[0].number' || true)
    if [ -n "$existing" ]; then
        gh pr close "$existing" --delete-branch=false --comment "Reopening with refreshed scenario." || true
    fi

    gh pr create \
        --base main \
        --head "$branch" \
        --title "demo($id): $title" \
        --body-file "$dir/README.md"

    git checkout main
    echo "::endgroup::"
}

if [ "$SCENARIO" = "all" ]; then
    for d in scenarios/*/; do
        id="$(basename "$d")"
        run_one "$id"
    done
else
    run_one "$SCENARIO"
fi

#!/usr/bin/env bash
set -euo pipefail

# Configures main for GitHub Merge Queue.
# Requires: gh authenticated with repository administration permission.
# Usage: ./setup-branch-protection.sh [owner/repo]

REPO="${1:-${GITHUB_REPOSITORY:-denilsonluiz3-sys/MULTI_Bet_playing_Demo}}"
RULESET_NAME="MULTI_Bet main — Merge Queue"
API_VERSION="2026-03-10"

command -v gh >/dev/null || { echo "gh CLI is required" >&2; exit 1; }
gh auth status >/dev/null

OWNER="${REPO%%/*}"
NAME="${REPO#*/}"

# The required check is deliberately only the fast job. Android packaging remains
# non-blocking for the merge queue and continues to run in build-android.yml.
RULESET_JSON=$(cat <<'JSON'
{
  "name": "MULTI_Bet main — Merge Queue",
  "target": "branch",
  "enforcement": "active",
  "bypass_actors": [],
  "conditions": {
    "ref_name": {
      "include": ["refs/heads/main"],
      "exclude": []
    }
  },
  "rules": [
    {
      "type": "pull_request",
      "parameters": {
        "required_approving_review_count": 0,
        "dismiss_stale_reviews_on_push": true,
        "require_code_owner_review": false,
        "require_last_push_approval": false,
        "required_review_thread_resolution": false
      }
    },
    {
      "type": "required_status_checks",
      "parameters": {
        "do_not_enforce_on_create": false,
        "strict_required_status_checks_policy": false,
        "required_status_checks": [
          { "context": "fast" }
        ]
      }
    },
    {
      "type": "merge_queue",
      "parameters": {
        "check_response_timeout_minutes": 30,
        "grouping_strategy": "ALLGREEN",
        "max_entries_to_build": 5,
        "max_entries_to_merge": 1,
        "merge_method": "SQUASH",
        "min_entries_to_merge": 1,
        "min_entries_to_merge_wait_minutes": 1
      }
    },
    { "type": "non_fast_forward" }
  ]
}
JSON
)

EXISTING_ID=$(gh api --paginate \
  -H "Accept: application/vnd.github+json" \
  -H "X-GitHub-Api-Version: ${API_VERSION}" \
  "/repos/${OWNER}/${NAME}/rulesets?per_page=100" \
  --jq ".[] | select(.name == \"${RULESET_NAME}\") | .id" | head -n1 || true)

if [[ -n "${EXISTING_ID}" ]]; then
  printf '%s\n' "${RULESET_JSON}" | gh api \
    --method PUT \
    -H "Accept: application/vnd.github+json" \
    -H "X-GitHub-Api-Version: ${API_VERSION}" \
    "/repos/${OWNER}/${NAME}/rulesets/${EXISTING_ID}" \
    --input - >/dev/null
  echo "Updated ruleset ${EXISTING_ID}."
else
  printf '%s\n' "${RULESET_JSON}" | gh api \
    --method POST \
    -H "Accept: application/vnd.github+json" \
    -H "X-GitHub-Api-Version: ${API_VERSION}" \
    "/repos/${OWNER}/${NAME}/rulesets" \
    --input - >/dev/null
  echo "Created merge-queue ruleset."
fi

echo "Main is configured to require the fast CI check and use the merge queue."
echo "Android build remains non-blocking and runs through build-android.yml."

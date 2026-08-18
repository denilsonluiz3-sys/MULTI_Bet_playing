#!/usr/bin/env bash
set -euo pipefail

# Configures main for GitHub Merge Queue.
# Requires: gh authenticated with repository administration permission.
# Usage: ./setup-branch-protection.sh [owner/repo]

REPO="${1:-${GITHUB_REPOSITORY:-denilsonluiz3-sys/MULTI_Bet_playing_Demo}}"
RULESET_NAME="MULTI_Bet main — Merge Queue"
API_VERSION="2022-11-28"

command -v gh >/dev/null || { echo "gh CLI is required" >&2; exit 1; }
gh auth status >/dev/null

OWNER="${REPO%%/*}"
NAME="${REPO#*/}"

# Keep the merge gate minimal: only the fast CI context is required.
# Android packaging remains outside the merge gate.
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
        "required_review_thread_resolution": false,
        "allowed_merge_methods": ["squash"]
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
    }
  ]
}
JSON
)

EXISTING_ID=$(gh api --paginate \
  -H "Accept: application/vnd.github+json" \
  -H "X-GitHub-Api-Version: ${API_VERSION}" \
  "/repos/${OWNER}/${NAME}/rulesets?per_page=100" \
  --jq ".[] | select(.name == \"${RULESET_NAME}\") | .id" | head -n1 || true)

request_ruleset() {
  local method="$1"
  local endpoint="$2"
  local response_file
  response_file="$(mktemp)"
  trap 'rm -f "${response_file}"' RETURN

  if printf '%s\n' "${RULESET_JSON}" | gh api \
      --method "${method}" \
      -H "Accept: application/vnd.github+json" \
      -H "X-GitHub-Api-Version: ${API_VERSION}" \
      "${endpoint}" \
      --input - >"${response_file}"; then
    cat "${response_file}" >/dev/null
    return 0
  fi

  echo "GitHub rejected the ruleset request." >&2
  echo "HTTP/API response:" >&2
  cat "${response_file}" >&2 || true
  return 1
}

if [[ -n "${EXISTING_ID}" ]]; then
  request_ruleset PUT "/repos/${OWNER}/${NAME}/rulesets/${EXISTING_ID}"
  echo "Updated ruleset ${EXISTING_ID}."
else
  request_ruleset POST "/repos/${OWNER}/${NAME}/rulesets"
  echo "Created merge-queue ruleset."
fi

# Auto-merge must be enabled at repository level for PRs to enter the automated path.
gh api \
  --method PATCH \
  -H "Accept: application/vnd.github+json" \
  -H "X-GitHub-Api-Version: ${API_VERSION}" \
  "/repos/${OWNER}/${NAME}" \
  -f allow_auto_merge=true \
  >/dev/null

echo "Main is configured to require the fast CI check and use the merge queue."
echo "Repository auto-merge is enabled."
echo "Android build remains non-blocking and runs through build-android.yml."

#!/usr/bin/env bash
set -euo pipefail

# Configures the main branch with repository rules that are supported without
# GitHub Merge Queue. The repository is owned by a personal account, so this
# script intentionally does NOT create or reference a merge_queue rule.
# Requires: gh authenticated with repository administration permission.
# Usage: ./setup-branch-protection.sh [owner/repo]

REPO="${1:-${GITHUB_REPOSITORY:-denilsonluiz3-sys/MULTI_Bet_playing_Demo}}"
API_VERSION="2022-11-28"
RULESET_NAME="MULTI_Bet main — Pull Request CI"

command -v gh >/dev/null || { echo "gh CLI is required" >&2; exit 1; }
gh auth status >/dev/null

OWNER="${REPO%%/*}"
NAME="${REPO#*/}"

# Keep only the fast CI check in the merge gate. Heavy Android packaging is
# intentionally outside this ruleset.
RULESET_JSON=$(cat <<'JSON'
{
  "name": "MULTI_Bet main — Pull Request CI",
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
      "type": "non_fast_forward"
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

  if ! printf '%s\n' "${RULESET_JSON}" | gh api \
      --method "${method}" \
      -H "Accept: application/vnd.github+json" \
      -H "X-GitHub-Api-Version: ${API_VERSION}" \
      "${endpoint}" \
      --input - >"${response_file}"; then
    echo "GitHub rejected the ruleset request:" >&2
    cat "${response_file}" >&2
    rm -f "${response_file}"
    return 1
  fi

  rm -f "${response_file}"
}

if [[ -n "${EXISTING_ID}" ]]; then
  request_ruleset PUT "/repos/${OWNER}/${NAME}/rulesets/${EXISTING_ID}"
  echo "Updated ruleset ${EXISTING_ID}."
else
  request_ruleset POST "/repos/${OWNER}/${NAME}/rulesets"
  echo "Created pull-request CI ruleset."
fi

# Auto-merge is a repository setting, not a ruleset rule. Enable it when the
# authenticated token has administration permission; otherwise leave the
# repository unchanged and let the PR UI/gh configure auto-merge manually.
if gh api --method PATCH \
    -H "Accept: application/vnd.github+json" \
    -H "X-GitHub-Api-Version: ${API_VERSION}" \
    "/repos/${OWNER}/${NAME}" \
    -f allow_auto_merge=true >/dev/null 2>&1; then
  echo "Auto-merge enabled for ${OWNER}/${NAME}."
else
  echo "Auto-merge could not be enabled with this token; configure it manually in repository settings or on the PR." >&2
fi

echo "main requires pull requests and the fast status check; force pushes are blocked."

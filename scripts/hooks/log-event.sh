#!/usr/bin/env bash
# Appends one JSON line per agent event to logs/ai-interactions.jsonl.
# Every line has the same fields: ts, event, actor, summary, detail.
# Usage: log-event.sh <event> [actor] [summary]   (hook payload, if any, arrives on stdin)
set -euo pipefail

event="${1:-unknown}"
actor="${2:-agent}"
summary="${3:-}"
root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
log="$root/logs/ai-interactions.jsonl"
mkdir -p "$root/logs"

payload=""
if [ ! -t 0 ]; then payload="$(cat || true)"; fi

if [ -z "$summary" ] && [ -n "$payload" ]; then
  summary="$(printf '%s' "$payload" | jq -r '(.prompt // .toolName // .tool_name // "")' 2>/dev/null | head -c 300 || true)"
fi

# Never log secrets: redact anything that looks like a token.
detail="$(printf '%s' "$payload" | head -c 2000 | sed -E 's/(ghp_|gho_|github_pat_)[A-Za-z0-9_]+/[redacted]/g')"

jq -nc --arg ts "$(date -u +%Y-%m-%dT%H:%M:%SZ)" --arg event "$event" --arg actor "$actor" \
  --arg summary "$summary" --arg detail "$detail" \
  '{ts:$ts,event:$event,actor:$actor,summary:$summary,detail:$detail}' >> "$log"

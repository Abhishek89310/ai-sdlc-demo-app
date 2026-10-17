# AI in SDLC Assignment: approval limits by role

**Demo video:** _add link here after upload_

## What was built

Role-based approval limits in the invoice approval service:

| Rule | Where it lives |
| --- | --- |
| Manager approves up to 50,000 (inclusive) | `ApprovalPolicy.Decide` |
| Director approves up to 500,000 (inclusive) | `ApprovalPolicy.Decide` |
| Above 500,000: two distinct approvers, second is a Director | `ApprovalPolicy.DecideLargeInvoice`, `Invoice.FirstApprover` |
| Same user never counts twice (trim, ignore case) | `ApprovalPolicy.SameUser` |
| Refusal returns a reason and changes nothing | `ApprovalDeniedException`, checks run before any mutation in `ApprovalService` |

Assumptions (also in `openspec/changes/add-role-approval-limits/proposal.md`): the first approver of a large invoice may be a Manager or Director; Admin can no longer approve; limits are inclusive.

## How the work was done

1. **Context** – `AGENTS.md` extended with rules, layering, test command, review expectations. `.github/copilot-instructions.md` points to it.
2. **Skills** – `.github/skills/approval-rules-testing` (test checklist) and `.github/skills/pr-review-format` (fixed review output format), plus the OpenSpec skills.
3. **Hooks** – `.github/hooks/hooks.json` runs `scripts/hooks/log-event.sh` on session start, prompt, tool use and session end. Every record has the same fields (`ts, event, actor, summary, detail`) and secrets are redacted.
4. **Spec-driven** – OpenSpec: `openspec/changes/add-role-approval-limits/` (proposal, spec scenarios, design, tasks). `openspec validate` passes.
5. **Two-step delivery** – commit "Enforce Manager and Director approval limits", then "Require two distinct approvers above 500,000".
6. **Tests** – 48 unit tests (`ApprovalLimitTests`, `TwoApproverTests`, `ApprovalServiceTests`) and one Playwright test (`e2e/tests/approval-limits.spec.ts`). The Playwright test was checked by loosening the Manager limit: it fails.
7. **Review** – the `pr-reviewer` agent reviewed the diff in the fixed format; every finding has a verdict in the pull request description.
8. **Log** – `logs/ai-interactions.jsonl`.
9. **Agents** – `.github/agents/`: `spec-writer`, `implementer` (hands tests to the `test-reviewer` sub-agent), `test-reviewer`, `pr-reviewer`.

## Mutation scores (Stryker.NET 4.13.0)

Before (baseline commit, mutate `ApprovalService.cs` + `Invoice.cs`, 7 tests):

```
22 mutants tested: 12 killed, 10 survived
The final mutation score is 54.55 %
```

After (mutate `ApprovalService.cs` + `ApprovalPolicy.cs`, 48 tests):

```
First run with the new tests (34 tests): 54.17 %  - survivors were exception texts and repository saves
After the test-reviewer pass (45 tests): 100.00 % (53 killed, 0 survived)
After review fixes (48 tests):           100.00 %
The final mutation score is 100.00 %
```

The mutated file set changed because `ApprovalPolicy` did not exist at baseline. Raw notes: `docs/evidence/`.

## Review of generated tests

The first generated tests checked statuses but not refusal texts or whether the repository was saved, which is why Stryker scored them 54%. Added: exact message assertions, repository update counts, a refused-approval-is-never-saved test, a concurrency test, and a Reject-on-approved-invoice test.

## Commands

```
dotnet test backend/InvoiceApp.sln
cd backend/tests/InvoiceApp.Tests && dotnet stryker
cd e2e && npm install && npx playwright install chromium && E2E_PORT=5055 npm test
```

`E2E_PORT` is needed on macOS because AirPlay holds port 5000.

## Demo script (8 to 10 minutes)

1. (1 min) Open `AGENTS.md`, `.github/agents`, `.github/skills`, `.github/hooks`.
2. (1 min) Show `openspec/changes/add-role-approval-limits/` and run `openspec validate add-role-approval-limits`.
3. (1 min) `git log --oneline`; show the two step commits.
4. (2 min) Run `dotnet test backend/InvoiceApp.sln`; show the same-user and Manager-as-second tests.
5. (2 min) Run `dotnet stryker`; show 100%. Then change `ManagerLimit` to 80_000m, re-run tests to show failures, revert.
6. (2 min) Run `E2E_PORT=5055 npm test --prefix e2e --` with `--headed`; show the refusal.
7. (1 min) Show the pull request description with the finding verdicts and `logs/ai-interactions.jsonl`.

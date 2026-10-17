# Project conventions for AI agents

Invoice approval demo app: ASP.NET Core 8 Web API with a small static web UI.

## Layout

- `backend/src/InvoiceApp.Domain`: business rules, no web or storage code. `ApprovalService` owns the invoice lifecycle (Draft, PendingApproval, Approved, Rejected).
- `backend/src/InvoiceApp.Api`: controllers, sign-in (`Auth/`), in-memory repository, and the web UI under `wwwroot/`.
- `backend/tests/InvoiceApp.Tests`: xUnit tests, one test class per class under test, named `<ClassUnderTest>Tests`.
- `e2e`: Playwright end-to-end tests (`tests/`) and page objects (`pages/`).
- `docs/use-cases`: short briefs for features we may build.

## Conventions

- Controllers live in `Controllers/`, one per resource, route `api/[controller]`. Request bodies are `record` types declared inside the controller.
- Controllers inherit `ApiControllerBase` and use `Handle(...)` so service errors map to 404, 409 and 400.
- Every endpoint is behind sign-in. Restrict actions with `[RequireRole("...")]`: destructive actions are Admin only, approvals are Manager or Director.
- Business rules go in the Domain project, never in controllers.
- Register services in `Program.cs`. Repositories are interfaces in Domain with an in-memory implementation in Api.
- Every new rule or endpoint needs tests. Use `FakeInvoiceRepository` in tests.
- Use clear names; no one-letter variables or `flag` parameters.
- End-to-end tests use page objects and role-based locators (`getByRole`, `getByLabel`), never CSS or XPath selectors.

## Commands

- Run the app: `dotnet run --project backend/src/InvoiceApp.Api` then open http://localhost:5000
- Run the tests: `dotnet test backend/InvoiceApp.sln`
- Mutation score: `cd backend/tests/InvoiceApp.Tests && dotnet stryker` (needs `dotnet tool install -g dotnet-stryker --version 4.13.0`)
- Run end-to-end tests: `cd e2e && npm install && npx playwright install chromium && npm test`

## Demo accounts (not real credentials)

- `demo.manager` / `ChangeMe123!` (role Manager)
- `demo.director` / `ChangeMe123!` (role Director)
- `demo.director2` / `ChangeMe123!` (role Director, second approver)
- `demo.admin` / `ChangeMe123!` (role Admin, delete only, cannot approve)

## Never

- Commit real credentials, tokens or customer data.
- Add business logic to `Program.cs` or controllers.

## Approval rules (the source of truth is `openspec/changes/add-role-approval-limits/`)

- Manager may approve invoices up to and including 50,000. Director up to and including 500,000.
- Above 500,000 needs two distinct approvers. The first may be a Manager or Director; the second must be a Director.
- The same user never counts twice on one invoice. Compare usernames trimmed and ignoring case.
- A refused approval throws `ApprovalDeniedException` with a clear reason (HTTP 409) and must not change the invoice at all: check everything first, mutate last.
- Any role other than Manager or Director cannot approve.

## Layering

- Rules: `ApprovalPolicy` (pure decisions, no storage) in Domain. `ApprovalService` orchestrates: find, ask policy, mutate, save.
- Controllers only read the session (username, role), call the service and map exceptions via `Handle`.
- The UI shows the server's refusal reason; it never re-implements limits.

## Testing expectations

- Test names read `Method_Condition_Result`. One behaviour per test.
- Cover both sides of every boundary (50,000 / 50,000.01 / 500,000 / 500,000.01).
- Every refusal test asserts the reason text AND that status, first approver, decider, timestamp and note are unchanged.
- Never loosen an assertion to make a generated test pass. If a test and the spec disagree, fix the code or ask.
- Target mutation score for the approval classes: 90% or more.

## Review expectations

- Findings use the format in `.github/skills/pr-review-format/SKILL.md`: severity, file:line, issue, suggestion.
- Each finding on a pull request gets exactly one verdict: fixed, dismissed (with reason) or deferred (with reason).
- Commits are small and self-contained, with a message saying what changed and why. Two-step delivery: single-approver limits first, two-approver rule second.

## Agent workflow, skills and hooks

- Roles live in `.github/agents/`: `spec-writer`, `implementer`, `test-reviewer`, `pr-reviewer`. The implementer may hand work to the test-reviewer as a sub-agent.
- Skills live in `.github/skills/`. Hooks in `.github/hooks/` write every prompt and tool call to `logs/ai-interactions.jsonl` through `scripts/hooks/log-event.sh`.
- Do not edit or delete files under `logs/`; append only.

## Why

Today any Approver can approve an invoice of any size. Finance needs spending authority tied to the approver's role, and very large invoices need a second pair of eyes. Without this, one person can approve a six-figure invoice alone.

## What Changes

- Introduce the roles **Manager** and **Director** for approving invoices (replacing the single "Approver" role). Admin keeps delete rights only.
- Manager approval limit: up to 50,000 (inclusive). Director approval limit: up to 500,000 (inclusive).
- Invoices above 500,000 need two distinct approvers; the second approver must be a Director.
- The same user can never count as both approvers on one invoice.
- A refused approval returns a clear reason and leaves the invoice exactly as it was.
- The Approvals tab shows the refusal reason and the first approver of a half-approved invoice.

## Capabilities

### New Capabilities
- `invoice-approval-limits`: role-based approval limits and the two-approver rule for large invoices.

### Modified Capabilities
<!-- none: no spec exists yet for the base approval flow -->

## Impact

- Domain: `ApprovalService.Approve` gains the approver role; new `ApprovalPolicy` rules; `Invoice` records a first approver.
- Api: `ApprovalsController` passes the session role; demo users and role names in `appsettings.json`.
- UI: Approvals tab shows first approver and refusal reason.
- Tests: new unit tests per rule, one Playwright test, Stryker mutation score.
- Out of scope: changing limits at runtime, per-currency limits, audit export.

## Assumptions

- "Between 50,000 and 500,000" means a Manager is refused and a Director may approve alone.
- On an invoice above 500,000 the first approver may be a Manager or a Director; only the second must be a Director.
- User identity is the sign-in username, compared case-insensitively and ignoring surrounding spaces.
- A reject remains a single-role decision by a Manager or Director and ends the invoice.

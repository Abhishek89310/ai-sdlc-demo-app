## Context

`ApprovalService` is the only place that changes invoice approval state. Controllers translate exceptions to HTTP codes (`InvalidOperationException` -> 409). Sessions carry `Username` and `Role`.

## Goals / Non-Goals

**Goals:** rules live in Domain; refusals mutate nothing; behaviour is easy to test with `FakeInvoiceRepository`.
**Non-Goals:** configurable limits, persistence, new invoice statuses.

## Decisions

- **Separate `ApprovalPolicy`** (pure, static limits, no repository) decides allow / refuse(reason) / needs-second. `ApprovalService` calls it, then mutates. Keeps rules unit-testable and mutation-testable in isolation.
- **Validate before mutate.** All checks run before any property of the invoice is set, so a refusal cannot leave partial state.
- **`ApprovalDeniedException : InvalidOperationException`** carries the reason; reuses the existing 409 mapping so controllers stay unchanged in shape.
- **`Invoice.FirstApprover`** (string?, nullable) records the first of two approvals; status stays `PendingApproval` so the existing pending list and filters keep working.
- **Username comparison** uses trim + `OrdinalIgnoreCase`.
- **Amounts** are `decimal`; limits are `decimal` constants, comparisons use `>` so the limit itself is allowed.
- **Roles**: `Manager`, `Director`; demo users `demo.manager`, `demo.director`, `demo.director2`, `demo.admin`.

## Risks / Trade-offs

- Replacing the "Approver" role breaks existing sign-ins -> demo users and docs updated in the same change.
- Hard-coded limits -> acceptable now, isolated in one class for a later move to configuration.

---
name: approval-rules-testing
description: Checklist for writing and reviewing tests of the invoice approval rules. Use when adding or reviewing approval tests.
---

# Approval rules test checklist

1. Boundaries: 50,000 allowed and 50,000.01 refused for Manager; 500,000 allowed and 500,000.01 needs two approvers for Director.
2. Two-approver rule: first approval leaves status PendingApproval and sets FirstApprover; Director as second completes it.
3. Negative rules: Manager as second approver refused; same user twice refused, including "DANA", " dana " variants.
4. Every refusal test asserts: exception type, reason text, and that status, FirstApprover, DecidedBy, DecidedAt and DecisionNote are unchanged.
5. Unknown or empty role refused.
6. Missing invoice, non-pending invoice still behave as before.
7. A test that would still pass if the production rule were deleted is a bad test: remove or fix it.

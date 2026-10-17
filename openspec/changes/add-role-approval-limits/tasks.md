## 1. Single-approver limits (step 1)

- [ ] 1.1 Add Manager/Director roles, demo users, and controller role guard
- [ ] 1.2 Add `ApprovalPolicy` with 50,000 / 500,000 limits and `ApprovalDeniedException`
- [ ] 1.3 Pass approver role into `ApprovalService.Approve`; validate before mutate
- [ ] 1.4 Unit tests: boundaries, over-limit refusal reason, refusal leaves invoice unchanged, unknown role

## 2. Two-approver rule (step 2)

- [ ] 2.1 Add `Invoice.FirstApprover`; first approval keeps invoice pending
- [ ] 2.2 Enforce second approver is a Director and a different user (trim, ignore case)
- [ ] 2.3 Unit tests: first approval, Director completes, Manager refused as second, same user refused (incl. case/space variants), nothing changes on refusal
- [ ] 2.4 Show first approver in the Approvals tab

## 3. End-to-end and quality

- [ ] 3.1 Playwright: Manager signs in, over-limit approval refused with visible reason
- [ ] 3.2 Stryker mutation score before/after in write-up
- [ ] 3.3 Open PR, run review prompt, record verdict per finding

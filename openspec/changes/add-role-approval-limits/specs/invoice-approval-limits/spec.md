## ADDED Requirements

### Requirement: Manager approval limit
A Manager SHALL be able to approve a pending invoice whose amount is at most 50,000.

#### Scenario: Manager approves at the limit
- **WHEN** a Manager approves a pending invoice of exactly 50,000
- **THEN** the invoice becomes Approved and records the Manager as decider

#### Scenario: Manager refused above the limit
- **WHEN** a Manager approves a pending invoice of 50,000.01
- **THEN** the approval is refused with a reason naming the Manager limit of 50,000
- **AND** the invoice is unchanged

### Requirement: Director approval limit
A Director SHALL be able to approve a pending invoice whose amount is at most 500,000.

#### Scenario: Director approves between the limits
- **WHEN** a Director approves a pending invoice of 500,000
- **THEN** the invoice becomes Approved without a second approver

### Requirement: Two approvers above 500,000
An invoice above 500,000 SHALL need two distinct approvers, and the second approver SHALL be a Director.

#### Scenario: First approval keeps the invoice pending
- **WHEN** a Manager or Director gives the first approval on a pending invoice of 500,000.01 or more
- **THEN** the invoice stays Pending Approval and records that user as first approver

#### Scenario: Second approval by a different Director completes it
- **GIVEN** a large invoice with a first approver recorded
- **WHEN** a different Director approves it
- **THEN** the invoice becomes Approved and records the second approver as decider

#### Scenario: Second approver is a Manager
- **GIVEN** a large invoice with a first approver recorded
- **WHEN** a Manager approves it
- **THEN** the approval is refused with a reason saying the second approver must be a Director
- **AND** the invoice is unchanged

### Requirement: One user cannot approve twice
The same user SHALL NOT count as both approvers on one invoice, regardless of letter case or surrounding spaces in the username.

#### Scenario: Same Director tries twice
- **GIVEN** a large invoice whose first approver is Director "dana"
- **WHEN** "Dana " approves it again
- **THEN** the approval is refused with a reason saying the same user cannot approve twice
- **AND** the invoice is unchanged

### Requirement: Refusals are clear and side-effect free
A refused approval SHALL return a human-readable reason and SHALL NOT change status, approvers, decision time or notes.

#### Scenario: Unsupported role
- **WHEN** a user whose role is neither Manager nor Director approves an invoice
- **THEN** the approval is refused with a reason and the invoice is unchanged

#### Scenario: Reason reaches the user
- **WHEN** a refusal happens in the web UI
- **THEN** the reason is shown in the Approvals tab alert and the invoice stays in the pending list

Follow `AGENTS.md` at the repository root. It holds the layout, approval rules, test command and review expectations.

Short version: business rules in `backend/src/InvoiceApp.Domain`, controllers stay thin, validate before you mutate, test both sides of each boundary, run `dotnet test backend/InvoiceApp.sln` before every commit.

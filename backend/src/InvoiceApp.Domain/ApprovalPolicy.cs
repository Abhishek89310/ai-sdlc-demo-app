namespace InvoiceApp.Domain;

public enum ApprovalOutcome
{
    /// <summary>This approval completes the invoice.</summary>
    Completes,

    /// <summary>First of two approvals on a large invoice; it stays pending.</summary>
    FirstOfTwo
}

/// <summary>Who may approve which invoice amount. Pure rules: no storage, no side effects.</summary>
public static class ApprovalPolicy
{
    public const string ManagerRole = "Manager";
    public const string DirectorRole = "Director";
    public const decimal ManagerLimit = 50_000m;
    public const decimal DirectorLimit = 500_000m;

    /// <summary>
    /// Decides what this approval does, or throws <see cref="ApprovalDeniedException"/> with the reason.
    /// <paramref name="firstApprover"/> is null until a large invoice has had its first approval.
    /// </summary>
    public static ApprovalOutcome Decide(decimal amount, string approver, string role, string? firstApprover)
    {
        if (string.IsNullOrWhiteSpace(approver))
        {
            throw new ApprovalDeniedException("An approver is required.");
        }

        if (role != ManagerRole && role != DirectorRole)
        {
            throw new ApprovalDeniedException($"Role '{role}' cannot approve invoices; only a Manager or Director can.");
        }

        if (amount > DirectorLimit)
        {
            return DecideLargeInvoice(approver, role, firstApprover);
        }

        if (role == ManagerRole && amount > ManagerLimit)
        {
            throw new ApprovalDeniedException(
                $"A Manager can approve up to {ManagerLimit:N0}; this invoice is {amount:N2}. A Director must approve it.");
        }

        return ApprovalOutcome.Completes;
    }

    private static ApprovalOutcome DecideLargeInvoice(string approver, string role, string? firstApprover)
    {
        if (firstApprover is null)
        {
            return ApprovalOutcome.FirstOfTwo;
        }

        if (SameUser(approver, firstApprover))
        {
            throw new ApprovalDeniedException(
                $"{firstApprover} already gave the first approval; the same user cannot approve twice. A different Director must approve.");
        }

        if (role != DirectorRole)
        {
            throw new ApprovalDeniedException(
                "Invoices above 500,000 need two approvers and the second approver must be a Director.");
        }

        return ApprovalOutcome.Completes;
    }

    private static bool SameUser(string left, string right) =>
        string.Equals(left.Trim(), right.Trim(), StringComparison.OrdinalIgnoreCase);
}

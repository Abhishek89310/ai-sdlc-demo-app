namespace InvoiceApp.Domain;

/// <summary>Who may approve which invoice amount. Pure rules: no storage, no side effects.</summary>
public static class ApprovalPolicy
{
    public const string ManagerRole = "Manager";
    public const string DirectorRole = "Director";
    public const decimal ManagerLimit = 50_000m;
    public const decimal DirectorLimit = 500_000m;

    /// <summary>Throws <see cref="ApprovalDeniedException"/> when the approver may not approve this amount alone.</summary>
    public static void EnsureCanApproveAlone(decimal amount, string role)
    {
        if (role != ManagerRole && role != DirectorRole)
        {
            throw new ApprovalDeniedException($"Role '{role}' cannot approve invoices; only a Manager or Director can.");
        }

        if (amount > DirectorLimit)
        {
            throw new ApprovalDeniedException(
                $"Invoices above {DirectorLimit:N0} need two distinct approvers; single approval is not allowed.");
        }

        if (role == ManagerRole && amount > ManagerLimit)
        {
            throw new ApprovalDeniedException(
                $"A Manager can approve up to {ManagerLimit:N0}; this invoice is {amount:N2}. A Director must approve it.");
        }
    }
}

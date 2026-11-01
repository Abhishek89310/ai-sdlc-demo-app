namespace InvoiceApp.Domain;

/// <summary>An approval was refused by a business rule. The message is safe to show to the user.</summary>
public class ApprovalDeniedException : InvalidOperationException
{
    public ApprovalDeniedException(string reason) : base(reason)
    {
    }
}

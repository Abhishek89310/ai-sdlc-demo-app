using InvoiceApp.Domain;
using Xunit;

namespace InvoiceApp.Tests;

public class ApprovalLimitTests
{
    private static Invoice PendingInvoice(decimal amount) =>
        new() { Id = "INV-1", CustomerName = "Test School", Amount = amount, Status = InvoiceStatus.PendingApproval };

    private static ApprovalService ServiceFor(Invoice invoice) => new(new FakeInvoiceRepository(invoice));

    /// <summary>Every field a refused approval must leave alone.</summary>
    private static (InvoiceStatus, string?, DateTime?, string?) Snapshot(Invoice invoice) =>
        (invoice.Status, invoice.DecidedBy, invoice.DecidedAt, invoice.DecisionNote);

    private static ApprovalDeniedException AssertRefusedAndUnchanged(Invoice invoice, string approver, string role)
    {
        var before = Snapshot(invoice);

        var error = Assert.Throws<ApprovalDeniedException>(() => ServiceFor(invoice).Approve(invoice.Id, approver, role));

        Assert.Equal(before, Snapshot(invoice));
        return error;
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(49_999.99)]
    [InlineData(50_000)]
    public void Approve_ManagerWithinLimit_ApprovesInvoice(decimal amount)
    {
        var invoice = PendingInvoice(amount);

        var result = ServiceFor(invoice).Approve("INV-1", "mia", "Manager");

        Assert.Equal(InvoiceStatus.Approved, result.Status);
        Assert.Equal("mia", result.DecidedBy);
        Assert.NotNull(result.DecidedAt);
    }

    [Theory]
    [InlineData(50_000.01)]
    [InlineData(75_000)]
    [InlineData(500_000)]
    public void Approve_ManagerAboveLimit_IsRefusedWithReasonAndInvoiceUnchanged(decimal amount)
    {
        var invoice = PendingInvoice(amount);

        var error = AssertRefusedAndUnchanged(invoice, "mia", "Manager");

        Assert.Contains("Manager can approve up to 50,000", error.Message);
        Assert.Contains("Director", error.Message);
    }

    [Theory]
    [InlineData(50_000.01)]
    [InlineData(500_000)]
    public void Approve_DirectorWithinLimit_ApprovesInvoice(decimal amount)
    {
        var invoice = PendingInvoice(amount);

        var result = ServiceFor(invoice).Approve("INV-1", "dana", "Director");

        Assert.Equal(InvoiceStatus.Approved, result.Status);
        Assert.Equal("dana", result.DecidedBy);
    }

    [Theory]
    [InlineData("Manager")]
    [InlineData("Director")]
    public void Approve_AboveDirectorLimit_IsRefusedForASingleApprover(string role)
    {
        var invoice = PendingInvoice(500_000.01m);

        var error = AssertRefusedAndUnchanged(invoice, "dana", role);

        Assert.Contains("two distinct approvers", error.Message);
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("")]
    [InlineData("manager")]
    public void Approve_RoleThatIsNotManagerOrDirector_IsRefusedAndInvoiceUnchanged(string role)
    {
        var invoice = PendingInvoice(100m);

        var error = AssertRefusedAndUnchanged(invoice, "alex", role);

        Assert.Contains("only a Manager or Director can", error.Message);
    }

    [Fact]
    public void Approve_RefusalForRoleIsCheckedEvenWhenAmountIsSmall()
    {
        var invoice = PendingInvoice(1m);

        AssertRefusedAndUnchanged(invoice, "alex", "Admin");
    }
}

using InvoiceApp.Domain;
using Xunit;

namespace InvoiceApp.Tests;

public class TwoApproverTests
{
    private const decimal Large = 500_000.01m;

    private static Invoice LargeInvoice(string? firstApprover = null) => new()
    {
        Id = "INV-9",
        CustomerName = "Test District",
        Amount = Large,
        Status = InvoiceStatus.PendingApproval,
        FirstApprover = firstApprover
    };

    private static ApprovalService ServiceFor(Invoice invoice) => new(new FakeInvoiceRepository(invoice));

    private static (InvoiceStatus, string?, string?, DateTime?, string?) Snapshot(Invoice invoice) =>
        (invoice.Status, invoice.FirstApprover, invoice.DecidedBy, invoice.DecidedAt, invoice.DecisionNote);

    private static ApprovalDeniedException AssertRefusedAndUnchanged(Invoice invoice, string approver, string role)
    {
        var before = Snapshot(invoice);

        var error = Assert.Throws<ApprovalDeniedException>(() => ServiceFor(invoice).Approve(invoice.Id, approver, role));

        Assert.Equal(before, Snapshot(invoice));
        return error;
    }

    [Theory]
    [InlineData("Manager")]
    [InlineData("Director")]
    public void Approve_FirstApprovalOnLargeInvoice_RecordsApproverAndStaysPending(string role)
    {
        var invoice = LargeInvoice();

        var result = ServiceFor(invoice).Approve("INV-9", "first.user", role);

        Assert.Equal(InvoiceStatus.PendingApproval, result.Status);
        Assert.Equal("first.user", result.FirstApprover);
        Assert.Null(result.DecidedBy);
        Assert.Null(result.DecidedAt);
    }

    [Fact]
    public void Approve_AtExactlyDirectorLimit_NeedsOnlyOneApprover()
    {
        var invoice = LargeInvoice();
        invoice.Amount = 500_000m;

        var result = ServiceFor(invoice).Approve("INV-9", "dana", "Director");

        Assert.Equal(InvoiceStatus.Approved, result.Status);
        Assert.Null(result.FirstApprover);
    }

    [Theory]
    [InlineData("Manager")]
    [InlineData("Director")]
    public void Approve_DifferentDirectorAsSecondApprover_CompletesInvoice(string firstRole)
    {
        var invoice = LargeInvoice();
        var service = ServiceFor(invoice);
        service.Approve("INV-9", "first.user", firstRole);

        var result = service.Approve("INV-9", "dana", "Director");

        Assert.Equal(InvoiceStatus.Approved, result.Status);
        Assert.Equal("first.user", result.FirstApprover);
        Assert.Equal("dana", result.DecidedBy);
        Assert.NotNull(result.DecidedAt);
    }

    [Fact]
    public void Approve_ManagerAsSecondApprover_IsRefusedAndInvoiceUnchanged()
    {
        var invoice = LargeInvoice(firstApprover: "dana");

        var error = AssertRefusedAndUnchanged(invoice, "mia", "Manager");

        Assert.Contains("second approver must be a Director", error.Message);
    }

    [Theory]
    [InlineData("dana")]
    [InlineData("Dana")]
    [InlineData("DANA")]
    [InlineData("  dana  ")]
    public void Approve_SameUserAsFirstApprover_IsRefusedAndInvoiceUnchanged(string secondAttempt)
    {
        var invoice = LargeInvoice(firstApprover: "dana");

        var error = AssertRefusedAndUnchanged(invoice, secondAttempt, "Director");

        Assert.Contains("same user cannot approve twice", error.Message);
    }

    [Fact]
    public void Approve_SameUserWhoWasManagerFirst_CannotCompleteEvenWithDirectorRole()
    {
        var invoice = LargeInvoice();
        var service = ServiceFor(invoice);
        service.Approve("INV-9", "pat", "Manager");
        var before = Snapshot(invoice);

        Assert.Throws<ApprovalDeniedException>(() => service.Approve("INV-9", "pat", "Director"));

        Assert.Equal(before, Snapshot(invoice));
        Assert.Equal(InvoiceStatus.PendingApproval, invoice.Status);
    }

    [Fact]
    public void Approve_FirstApproverStoredTrimmed_SoLaterComparisonMatches()
    {
        var invoice = LargeInvoice();
        var service = ServiceFor(invoice);
        service.Approve("INV-9", "  dana ", "Director");

        Assert.Equal("dana", invoice.FirstApprover);
        AssertRefusedAndUnchanged(invoice, "dana", "Director");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Approve_BlankApproverName_IsRefusedAndInvoiceUnchanged(string approver)
    {
        var invoice = LargeInvoice();

        AssertRefusedAndUnchanged(invoice, approver, "Director");
    }

    [Fact]
    public void Approve_RejectedFirstApproval_DoesNotLeaveFirstApproverBehindOnSmallInvoices()
    {
        var invoice = LargeInvoice();
        invoice.Amount = 60_000m;

        AssertRefusedAndUnchanged(invoice, "mia", "Manager");

        Assert.Null(invoice.FirstApprover);
    }

    [Fact]
    public void Approve_BlankApproverName_ExplainsThatAnApproverIsRequired()
    {
        var error = AssertRefusedAndUnchanged(LargeInvoice(), " ", "Director");

        Assert.Equal("An approver is required.", error.Message);
    }

    [Fact]
    public void Approve_FirstApproval_IsSavedToTheRepository()
    {
        var repository = new FakeInvoiceRepository(LargeInvoice());

        new ApprovalService(repository).Approve("INV-9", "dana", "Director");

        Assert.Equal(1, repository.UpdateCount);
    }

    [Fact]
    public void Approve_RefusedApproval_IsNeverSaved()
    {
        var repository = new FakeInvoiceRepository(LargeInvoice(firstApprover: "dana"));

        Assert.Throws<ApprovalDeniedException>(() => new ApprovalService(repository).Approve("INV-9", "dana", "Director"));

        Assert.Equal(0, repository.UpdateCount);
    }

    [Fact]
    public void Approve_DecidedByIsStoredTrimmedLikeFirstApprover()
    {
        var invoice = LargeInvoice(firstApprover: "pat");

        var result = ServiceFor(invoice).Approve("INV-9", "  dana ", "Director");

        Assert.Equal("dana", result.DecidedBy);
    }

    [Fact]
    public void Approve_TwoDirectorsAtTheSameTime_ProduceOneFirstAndOneSecondApprover()
    {
        for (var round = 0; round < 200; round++)
        {
            var invoice = LargeInvoice();
            var service = ServiceFor(invoice);

            Parallel.Invoke(
                () => service.Approve("INV-9", "dana", "Director"),
                () => service.Approve("INV-9", "dev", "Director"));

            Assert.Equal(InvoiceStatus.Approved, invoice.Status);
            Assert.NotEqual(invoice.FirstApprover, invoice.DecidedBy);
        }
    }
}

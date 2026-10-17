using InvoiceApp.Domain;
using Xunit;

namespace InvoiceApp.Tests;

public class ApprovalServiceTests
{
    private static Invoice InvoiceWith(string id, InvoiceStatus status) =>
        new() { Id = id, CustomerName = "Test School", Amount = 100m, Status = status };

    private static ApprovalService ServiceFor(params Invoice[] invoices) =>
        new(new FakeInvoiceRepository(invoices));

    [Fact]
    public void Submit_MovesDraftToPendingApproval()
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.Draft);

        var result = ServiceFor(invoice).Submit("INV-1");

        Assert.Equal(InvoiceStatus.PendingApproval, result.Status);
    }

    [Fact]
    public void Submit_RejectsInvoiceThatIsNotADraft()
    {
        var service = ServiceFor(InvoiceWith("INV-1", InvoiceStatus.Approved));

        Assert.Throws<InvalidOperationException>(() => service.Submit("INV-1"));
    }

    [Fact]
    public void Approve_SetsStatusApproverAndTimestamp()
    {
        var service = ServiceFor(InvoiceWith("INV-1", InvoiceStatus.PendingApproval));

        var result = service.Approve("INV-1", "demo.manager", "Manager");

        Assert.Equal(InvoiceStatus.Approved, result.Status);
        Assert.Equal("demo.manager", result.DecidedBy);
        Assert.NotNull(result.DecidedAt);
    }

    [Fact]
    public void Approve_RejectsInvoiceThatIsStillADraft()
    {
        var service = ServiceFor(InvoiceWith("INV-1", InvoiceStatus.Draft));

        Assert.Throws<InvalidOperationException>(() => service.Approve("INV-1", "demo.manager", "Manager"));
    }

    [Fact]
    public void Approve_ThrowsWhenInvoiceDoesNotExist()
    {
        var service = ServiceFor();

        Assert.Throws<KeyNotFoundException>(() => service.Approve("INV-404", "demo.manager", "Manager"));
    }

    [Fact]
    public void Reject_RecordsReasonAndApprover()
    {
        var service = ServiceFor(InvoiceWith("INV-1", InvoiceStatus.PendingApproval));

        var result = service.Reject("INV-1", "demo.approver", "Duplicate invoice");

        Assert.Equal(InvoiceStatus.Rejected, result.Status);
        Assert.Equal("Duplicate invoice", result.DecisionNote);
    }

    [Fact]
    public void Reject_RequiresAReason()
    {
        var service = ServiceFor(InvoiceWith("INV-1", InvoiceStatus.PendingApproval));

        Assert.Throws<ArgumentException>(() => service.Reject("INV-1", "demo.approver", "  "));
    }

    [Fact]
    public void Submit_PersistsThePendingInvoice()
    {
        var repository = new FakeInvoiceRepository(InvoiceWith("INV-1", InvoiceStatus.Draft));

        new ApprovalService(repository).Submit("INV-1");

        Assert.Equal(1, repository.UpdateCount);
    }

    [Fact]
    public void Submit_NonDraftMessageNamesInvoiceAndStatus()
    {
        var service = ServiceFor(InvoiceWith("INV-1", InvoiceStatus.Approved));

        var error = Assert.Throws<InvalidOperationException>(() => service.Submit("INV-1"));

        Assert.Equal("Invoice INV-1 is Approved; only drafts can be submitted.", error.Message);
    }

    [Fact]
    public void Approve_PersistsTheApprovedInvoice()
    {
        var repository = new FakeInvoiceRepository(InvoiceWith("INV-1", InvoiceStatus.PendingApproval));

        new ApprovalService(repository).Approve("INV-1", "demo.manager", "Manager");

        Assert.Equal(1, repository.UpdateCount);
    }

    [Fact]
    public void Approve_DraftMessageNamesInvoiceAndStatus()
    {
        var service = ServiceFor(InvoiceWith("INV-1", InvoiceStatus.Draft));

        var error = Assert.Throws<InvalidOperationException>(() => service.Approve("INV-1", "demo.manager", "Manager"));

        Assert.Equal("Invoice INV-1 is Draft; only pending invoices can be decided.", error.Message);
    }

    [Fact]
    public void Approve_MissingInvoiceMessageNamesTheInvoice()
    {
        var error = Assert.Throws<KeyNotFoundException>(() => ServiceFor().Approve("INV-404", "demo.manager", "Manager"));

        Assert.Equal("Invoice INV-404 was not found.", error.Message);
    }

    [Fact]
    public void Reject_PersistsTheRejectedInvoiceWithApproverAndTimestamp()
    {
        var repository = new FakeInvoiceRepository(InvoiceWith("INV-1", InvoiceStatus.PendingApproval));

        var result = new ApprovalService(repository).Reject("INV-1", "demo.manager", "Duplicate");

        Assert.Equal(1, repository.UpdateCount);
        Assert.Equal("demo.manager", result.DecidedBy);
        Assert.NotNull(result.DecidedAt);
    }

    [Fact]
    public void Reject_BlankReasonMessageSaysAReasonIsRequired()
    {
        var service = ServiceFor(InvoiceWith("INV-1", InvoiceStatus.PendingApproval));

        var error = Assert.Throws<ArgumentException>(() => service.Reject("INV-1", "demo.manager", " "));

        Assert.StartsWith("A reason is required to reject an invoice.", error.Message);
    }

    [Fact]
    public void Reject_AlreadyApprovedInvoice_IsRefusedAndStaysApproved()
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.Approved);
        var service = ServiceFor(invoice);

        Assert.Throws<InvalidOperationException>(() => service.Reject("INV-1", "demo.manager", "Too late"));

        Assert.Equal(InvoiceStatus.Approved, invoice.Status);
        Assert.Null(invoice.DecisionNote);
    }
}

namespace Nexx.Core.Entities;

public class IncomingPaymentInvoiceEntitie
{
    public int DocEntry { get; set; }
    public int LineNum { get; set; }
    public int InvoiceDocEntry { get; set; }
    public decimal SumApplied { get; set; }
}
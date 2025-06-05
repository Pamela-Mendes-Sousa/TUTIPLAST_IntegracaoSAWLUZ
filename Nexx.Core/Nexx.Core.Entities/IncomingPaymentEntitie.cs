namespace Nexx.Core.Entities;

public class IncomingPaymentEntitie
{
    public int DocEntry { get; set; }
    public int DocNum { get; set; }
    public DateTime DocDate { get; set; }
    public string CardCode { get; set; }
    public decimal DocTotal { get; set; }
    public string CashAcct { get; set; }
    public string TrsfrAcct { get; set; }
}
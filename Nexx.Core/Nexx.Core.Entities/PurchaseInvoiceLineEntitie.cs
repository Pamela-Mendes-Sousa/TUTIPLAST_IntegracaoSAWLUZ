namespace Nexx.Core.Entities;

public class PurchaseInvoiceLineEntitie
{
    public int DocEntry { get; set; }
    public int LineNum { get; set; }
    public string ItemCode { get; set; }
    public string Dscription { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal LineTotal { get; set; }
    public string WhsCode { get; set; }
    public string VatGroup { get; set; }
}
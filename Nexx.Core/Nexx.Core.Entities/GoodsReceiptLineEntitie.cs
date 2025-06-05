namespace Nexx.Core.Entities;

public class GoodsReceiptLineEntitie
{
    public int DocEntry { get; set; }
    public int LineNum { get; set; }
    public string ItemCode { get; set; }
    public string Dscription { get; set; }
    public decimal Quantity { get; set; }
    public string WhsCode { get; set; }
}
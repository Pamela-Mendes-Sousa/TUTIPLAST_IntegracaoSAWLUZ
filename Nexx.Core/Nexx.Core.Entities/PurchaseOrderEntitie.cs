namespace Nexx.Core.Entities;

public class PurchaseOrderEntitie
{
    public int DocEntry { get; set; }
    public int DocNum { get; set; }
    public string CardCode { get; set; }
    public string CardName { get; set; }
    public DateTime DocDate { get; set; }
    public DateTime DocDueDate { get; set; }
    public decimal DocTotal { get; set; }
    public string DocCur { get; set; }
    public string NumAtCard { get; set; }
    public string Comments { get; set; }
}
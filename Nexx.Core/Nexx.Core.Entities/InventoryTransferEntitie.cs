namespace Nexx.Core.Entities;

public class InventoryTransferEntitie
{
    public int DocEntry { get; set; }
    public int DocNum { get; set; }
    public DateTime DocDate { get; set; }
    public string Filler { get; set; }
    public string Comments { get; set; }
}
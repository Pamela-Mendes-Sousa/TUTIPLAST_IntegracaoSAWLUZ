namespace Nexx.Core.Entities;

public class JournalEntryLineEntitie
{
    public int TransId { get; set; }
    public int Line_ID { get; set; }
    public string Account { get; set; }
    public string ShortName { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public string LineMemo { get; set; }
}
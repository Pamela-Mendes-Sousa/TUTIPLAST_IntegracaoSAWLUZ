public class IntegrationFieldModel
{
    public string TableName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = "db_Alpha";
    public int? Size { get; set; } // opcional
    public int? EditSize { get; set; } // usado em db_Numeric
    public string SubType { get; set; } = "st_None"; // opcional
    public string DefaultValue { get; set; } //uso junto do validValues
    public List<ValidValuesModel>? ValidValuesMD { get; set; }
}

public class ValidValuesModel
{
    public string Value { get; set; }
    public string Description { get; set; }
}


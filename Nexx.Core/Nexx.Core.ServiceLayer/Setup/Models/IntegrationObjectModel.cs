public class IntegrationObjectModel
{
    public string TableName { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public string ObjectType { get; set; }
    public object LogTableName { get; set; }
    public string CanCreateDefaultForm { get; set; } = "tNO"; //"tYes"
    public string CanCancel { get; set; } = "tNO";
    public string CanDelete { get; set; } = "tNO";
    public string CanLog { get; set; } = "tNO";
    public string CanFind { get; set; } = "tNO";
    public string CanClose { get; set; } = "tNO";
    public string UseUniqueFormType { get; set; } = "tNO";
    public string CanArchive { get; set; } = "tNO";
    public string MenuItem { get; set; } = "tNO";
    public string MenuCaption { get; set; }
    public object FatherMenuID { get; set; }
    public object Position { get; set; }
    public string MenuUID { get; set; }
    public string EnableEnhancedForm { get; set; } = "tNO";
    public string RebuildEnhancedForm { get; set; } = "tNO";
    public List<Userobjectmd_Childtables> UserObjectMD_ChildTables { get; set; }
    public object[] UserObjectMD_FindColumns { get; set; }
    public List<Userbjectmd_FormColumns> UserObjectMD_FormColumns { get; set; }
    public List<Userobjectmd_EnhancedFormColumns> UserObjectMD_EnhancedFormColumns { get; set; }

    public class Userobjectmd_Childtables
    {
        public int SonNumber { get; set; }
        public string TableName { get; set; }
        public object LogTableName { get; set; }
        public string Code { get; set; }
        public string ObjectName { get; set; }
    }


    public class Userbjectmd_FormColumns
    {
        public string FormColumnAlias { get; set; }
        public string FormColumnDescription { get; set; }
        public int FormColumnNumber { get; set; }
        public int SonNumber { get; set; }
        public string Editable { get; set; } = "tNO";
    }


    public class Userobjectmd_EnhancedFormColumns
    {
        public string Code { get; set; }
        public int ColumnNumber { get; set; }
        public int ChildNumber { get; set; }
        public string ColumnAlias { get; set; }
        public string ColumnDescription { get; set; }
        public string ColumnIsUsed { get; set; } = "tNO";
        public string Editable { get; set; } = "tNO";
    }


}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

// These classes come from https://github.com/Drizin/CodegenCS/blob/master/src/CodegenCS.SqlServer
// I think we can assume that other databases may extract similar schemas... so these classes here are named "Database" as opposed "SqlServer"
// I also removed properties which are not serialized in JSON.

public class DatabaseSchema
{
    public List<DatabaseTable> Tables { get; set; }
}

public class DatabaseTable
{
    public string Database { get; set; }
    public string TableSchema { get; set; }
    public string TableName { get; set; }

    /// <summary>
    /// Can be "TABLE" or "VIEW"
    /// </summary>
    public string TableType { get; set; }

    public string TableDescription { get; set; }

    public List<DatabaseTableColumn> Columns { get; set; } = new List<DatabaseTableColumn>();

    /// <summary>
    /// FKs which point from THIS (Child) table to the primary key of OTHER (Parent) tables
    /// </summary>
    public List<DatabaseTableForeignKey> ForeignKeys { get; set; } = new List<DatabaseTableForeignKey>();

    /// <summary>
    /// FKs which point from OTHER (Child) tables to the primary key of THIS (Parent) table
    /// </summary>
    public List<DatabaseTableForeignKey> ChildForeignKeys { get; set; } = new List<DatabaseTableForeignKey>();

    public string PrimaryKeyName { get; set; }
    
    public bool PrimaryKeyIsClustered { get; set; }

    public List<DatabaseIndex> Indexes { get; set; } = new List<DatabaseIndex>();

    public Dictionary<string, string> ColumnPropertyNames { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, string> FKPropertyNames { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, string> ReverseFKPropertyNames { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

}
public class DatabaseTableColumn
{
    //[JsonIgnore] // only used in-memory to associate column with parent table
    //public string Database { get; set; }

    //[JsonIgnore] // only used in-memory to associate column with parent table
    //public string TableSchema { get; set; }

    //[JsonIgnore] // only used in-memory to associate column with parent table
    //public string TableName { get; set; }

    public string ColumnName { get; set; }

    public int OrdinalPosition { get; set; }

    public string DefaultSetting { get; set; }

    public bool IsNullable { get; set; }

    public string SqlDataType { get; set; }

    /// <summary>
    /// CLR Type which is equivalent to the SqlDataType
    /// </summary>
    public string ClrType { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public int? MaxLength { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public int? DateTimePrecision { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public int? NumericScale { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public int? NumericPrecision { get; set; }

    public bool IsIdentity { get; set; }

    public bool IsComputed { get; set; }

    public bool IsRowGuid { get; set; }

    public bool IsPrimaryKeyMember { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public int? PrimaryKeyOrdinalPosition { get; set; }

    public bool IsForeignKeyMember { get; set; }

    public string ColumnDescription { get; set; }
}

public class DatabaseTableForeignKey
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] // when constraint is serialized under parent table we don't need to serialize redundant attributes
    public string PrimaryKeyName { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] // when constraint is serialized under parent table we don't need to serialize redundant attributes
    public string PKTableSchema { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] // when constraint is serialized under parent table we don't need to serialize redundant attributes
    public string PKTableName { get; set; }


    public string ForeignKeyConstraintName { get; set; }

    public string ForeignKeyDescription { get; set; }


    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] // when constraint is serialized under parent table we don't need to serialize redundant attributes
    public string FKTableSchema { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] // when constraint is serialized under parent table we don't need to serialize redundant attributes
    public string FKTableName { get; set; }


    /// <summary>
    /// NO_ACTION, CASCADE, SET_NULL, SET_DEFAULT
    /// </summary>
    public string OnDeleteCascade { get; set; }

    /// <summary>
    /// NO_ACTION, CASCADE, SET_NULL, SET_DEFAULT
    /// </summary>
    public string OnUpdateCascade { get; set; }

    public bool IsSystemNamed { get; set; }

    public bool IsNotEnforced { get; set; }

    public List<DatabaseTableForeignKeyMember> Columns { get; set; }


    public string NavigationPropertyName { get; set; } // Logical only
}

public class DatabaseTableForeignKeyMember
{
    //[JsonIgnore] // only used in-memory to associate column with parent constraint
    //public string ForeignKeyConstraintName { get; set; }
    //[JsonIgnore] // only used in-memory to associate column with parent constraint
    //public string FKTableSchema { get; set; }

    public int PKColumnOrdinalPosition { get; set; }
    public string PKColumnName { get; set; }

    public string FKColumnName { get; set; }
}

public class DatabaseIndex
{
    [JsonIgnore] // only used in-memory to associate column with parent table
    public string Database { get; set; }

    [JsonIgnore] // only used in-memory to associate column with parent table
    public string TableSchema { get; set; }

    [JsonIgnore] // only used in-memory to associate column with parent table
    public string TableName { get; set; }

    public string IndexName { get; set; }

    public int IndexId { get; set; }

    /// <summary>
    /// CLUSTERED <br />
    /// NONCLUSTERED <br />
    /// XML <br />
    /// HEAP <br />
    /// SPATIAL <br />
    /// CLUSTERED COLUMNSTORE (SQL Server 2014 (12.x) and later) <br />
    /// NONCLUSTERED COLUMNSTORE (SQL Server 2012 (11.x) and later) <br />
    /// NONCLUSTERED HASH (SQL Server 2014 (12.x) and later)
    /// </summary>
    public string PhysicalType { get; set; }

    /// <summary>
    /// PRIMARY_KEY <br />
    /// UNIQUE_INDEX <br />
    /// UNIQUE_CONSTRAINT <br />
    /// NON_UNIQUE_INDEX <br />
    /// </summary>
    public string LogicalType { get; set; }


    [Obsolete("Please prefer using LogicalType property")]
    public bool IsPrimaryKey { get; set; }

    [Obsolete("Please prefer using LogicalType property")]
    public bool IsUnique { get; set; }

    [Obsolete("Please prefer using LogicalType property. Unique Constraints are maintained through a unique Index (IsUnique is also true)")]
    public bool IsUniqueConstraint { get; set; }

    public string IndexDescription { get; set; }

    public List<DatabaseIndexMember> Columns { get; set; }

}
public class DatabaseIndexMember
{
    [JsonIgnore] // only used in-memory to associate column with parent index
    public string Database { get; set; }

    [JsonIgnore] // only used in-memory to associate column with parent index
    public string TableSchema { get; set; }

    [JsonIgnore] // only used in-memory to associate column with parent index
    public string TableName { get; set; }

    [JsonIgnore] // only used in-memory to associate column with parent index
    public string IndexName { get; set; }

    [JsonIgnore] // only used in-memory to associate column with parent index
    public int IndexId { get; set; }


    public string ColumnName { get; set; }

    public int IndexOrdinalPosition { get; set; }

    public bool IsDescendingKey { get; set; }
    public bool IsIncludedColumn { get; set; }
}

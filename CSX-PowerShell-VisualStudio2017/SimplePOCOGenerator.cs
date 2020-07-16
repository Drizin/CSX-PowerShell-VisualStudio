﻿using CodegenCS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

public class SimplePOCOGenerator
{
    string _inputJsonSchema { get; set; }

    public string Namespace { get; set; }

    /// <summary>
    /// In-memory context which tracks all generated files (with indentation support), and later saves all files at once
    /// </summary>
    CodegenContext _generatorContext { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="inputJsonSchema">Absolute path of JSON schema</param>
    public SimplePOCOGenerator(string inputJsonSchema)
    {
        _inputJsonSchema = inputJsonSchema;
        Console.WriteLine($"Input Json Schema: {_inputJsonSchema}");
    }

    bool singleFile = false;
    string singleFileName = "POCOs.Generated.cs";

    /// <summary>
    /// Generates POCOS
    /// </summary>
    /// <param name="targetFolder">Absolute path of the target folder where files will be written</param>
    public void Generate(string targetFolder)
    {
        Console.WriteLine($"TargetFolder: {targetFolder}");

        _generatorContext = new CodegenContext();

        Console.WriteLine("Reading Schema...");

        DatabaseSchema schema = Newtonsoft.Json.JsonConvert.DeserializeObject<DatabaseSchema>(File.ReadAllText(_inputJsonSchema));

        CodegenOutputFile writer = null;
        if (singleFile)
        {
            writer = _generatorContext[singleFileName];
            writer
                .WriteLine(@"using System;")
                .WriteLine(@"using System.Collections.Generic;")
                .WriteLine(@"using System.ComponentModel.DataAnnotations;")
                .WriteLine(@"using System.ComponentModel.DataAnnotations.Schema;")
                .WriteLine(@"using System.Linq;")
                .WriteLine()
                .WriteLine($"namespace {Namespace}").WriteLine("{").IncreaseIndent();
        }

        foreach (var table in schema.Tables.OrderBy(t => GetClassNameForTable(t)))
        {
            if (table.TableType == "VIEW")
                continue;

            GeneratePOCO(table);
        }

        if (singleFile)
            writer.DecreaseIndent().WriteLine("}"); // end of namespace

        // since no errors happened, let's save all files
        _generatorContext.SaveFiles(outputFolder: targetFolder);

        Console.WriteLine("Success!");
    }
    void GeneratePOCO(DatabaseTable table)
    {
        Console.WriteLine($"Generating {table.TableName}...");

        CodegenOutputFile writer = null;
        if (singleFile)
        {
            writer = _generatorContext[singleFileName];
        }
        else
        {
            writer = _generatorContext[GetFileNameForTable(table)];
            writer
                .WriteLine(@"using System;")
                .WriteLine(@"using System.Collections.Generic;")
                .WriteLine(@"using System.ComponentModel.DataAnnotations;")
                .WriteLine(@"using System.ComponentModel.DataAnnotations.Schema;")
                .WriteLine(@"using System.Linq;")
                .WriteLine()
                .WriteLine($"namespace {Namespace}").WriteLine("{").IncreaseIndent();
        }

        string entityClassName = GetClassNameForTable(table);

        var schemaAndtable = new Tuple<string, string>(table.TableSchema, table.TableName);

        writer.WithCBlock($"public partial class {entityClassName}", () =>
            {
                writer.WriteLine("#region Members");
                var columns = table.Columns
                .Where(c => ShouldProcessColumn(table, c))
                .OrderBy(c => c.IsPrimaryKeyMember ? 0 : 1)
                                        .ThenBy(c => c.IsPrimaryKeyMember ? c.OrdinalPosition : 0) // respect PK order... 
                                        .ThenBy(c => GetPropertyNameForDatabaseColumn(table, c.ColumnName)); // but for other columns do alphabetically;

                foreach (var column in columns)
                    GenerateProperty(writer, table, column);

                writer.WriteLine("#endregion Members");
            });

        if (!singleFile)
        {
            writer.DecreaseIndent().WriteLine("}"); // end of namespace
        }
    }

    void GenerateProperty(CodegenOutputFile writer, DatabaseTable table, DatabaseTableColumn column)
    {
        string propertyName = GetPropertyNameForDatabaseColumn(table, column.ColumnName);
        if (column.IsPrimaryKeyMember)
            writer.WriteLine("[Key]");
        if (propertyName.ToLower() != column.ColumnName.ToLower())
            writer.WriteLine($"[Column(\"{column.ColumnName}\")]");
        writer.WriteLine($"public {GetTypeDefinitionForDatabaseColumn(table, column) ?? ""} {propertyName} {{ get; set; }}");

    }

    string GetFileNameForTable(DatabaseTable table)
    {
        return $"{table.TableName}.cs";
        if (table.TableSchema == "dbo")
            return $"{table.TableName}.cs";
        else
            return $"{table.TableSchema}.{table.TableName}.cs";
    }
    string GetClassNameForTable(DatabaseTable table)
    {
        return $"{table.TableName}";
        if (table.TableSchema == "dbo")
            return $"{table.TableName}";
        else
            return $"{table.TableSchema}_{table.TableName}";
    }
    bool ShouldProcessColumn(DatabaseTable table, DatabaseTableColumn column)
    {
        string sqlDataType = column.SqlDataType;
        switch (sqlDataType)
        {
            case "hierarchyid":
            case "geography":
                return false;
            default:
                break;
        }

        return true;
    }

    static Dictionary<Type, string> _typeAlias = new Dictionary<Type, string>
    {
        { typeof(bool), "bool" },
        { typeof(byte), "byte" },
        { typeof(char), "char" },
        { typeof(decimal), "decimal" },
        { typeof(double), "double" },
        { typeof(float), "float" },
        { typeof(int), "int" },
        { typeof(long), "long" },
        { typeof(object), "object" },
        { typeof(sbyte), "sbyte" },
        { typeof(short), "short" },
        { typeof(string), "string" },
        { typeof(uint), "uint" },
        { typeof(ulong), "ulong" },
        // Yes, this is an odd one.  Technically it's a type though.
        { typeof(void), "void" }
    };

    string GetTypeDefinitionForDatabaseColumn(DatabaseTable table, DatabaseTableColumn column)
    {
        System.Type type;
        try
        {
            type = Type.GetType(column.ClrType);
        }
        catch (Exception ex)
        {
            return "?!";
        }

        string typeName = type.Name;

        // Everyone prefers int instead of Int32, long instead of Int64, string instead of String, etc. - right?
        if (_typeAlias.TryGetValue(type, out string alias))
            typeName = alias;

        bool isNullable = column.IsNullable;

        // Many developers use POCO instances with null Primary Key to represent a new (in-memory) object, so we can force PKs as nullable
        if (column.IsPrimaryKeyMember)
            isNullable = true;

        // reference types (basically only strings?) are nullable by default are nullable, no need to make it explicit
        if (!type.IsValueType)
            isNullable = false;

        if (!isNullable)
            return typeName;
        return $"{typeName}?"; // some might prefer $"System.Nullable<{typeName}>"
    }

    // From PetaPoco - https://github.com/CollaboratingPlatypus/PetaPoco/blob/development/T4Templates/PetaPoco.Core.ttinclude
    static Regex rxCleanUp = new Regex(@"[^\w\d_]", RegexOptions.Compiled);
    static string[] cs_keywords = { "abstract", "event", "new", "struct", "as", "explicit", "null",
     "switch", "base", "extern", "object", "this", "bool", "false", "operator", "throw",
     "break", "finally", "out", "true", "byte", "fixed", "override", "try", "case", "float",
     "params", "typeof", "catch", "for", "private", "uint", "char", "foreach", "protected",
     "ulong", "checked", "goto", "public", "unchecked", "class", "if", "readonly", "unsafe",
     "const", "implicit", "ref", "ushort", "continue", "in", "return", "using", "decimal",
     "int", "sbyte", "virtual", "default", "interface", "sealed", "volatile", "delegate",
     "internal", "short", "void", "do", "is", "sizeof", "while", "double", "lock",
     "stackalloc", "else", "long", "static", "enum", "namespace", "string" };

    /// <summary>
    /// Gets a unique identifier name for the column, which doesn't conflict with the POCO class itself or with previous identifiers for this POCO.
    /// </summary>
    /// <param name="table"></param>
    /// <param name="column"></param>
    /// <param name="previouslyUsedIdentifiers"></param>
    /// <returns></returns>
    string GetPropertyNameForDatabaseColumn(DatabaseTable table, string columnName)
    {
        if (table.ColumnPropertyNames.ContainsKey(columnName))
            return table.ColumnPropertyNames[columnName];

        string name = columnName;

        // Replace forbidden characters
        name = rxCleanUp.Replace(name, "_");

        // Split multiple words
        var parts = splitUpperCase.Split(name).Where(part => part != "_" && part != "-").ToList();
        // we'll put first word into TitleCase except if it's a single-char in lowercase (like vNameOfTable) which we assume is a prefix (like v for views) and should be preserved as is
        // if first world is a single-char in lowercase (like vNameOfTable) which we assume is a prefix (like v for views) and should be preserved as is

        // Recapitalize (to TitleCase) all words
        for (int i = 0; i < parts.Count; i++)
        {
            // if first world is a single-char in lowercase (like vNameOfTable), we assume it's a prefix (like v for views) and should be preserved as is
            if (i == 0 && parts[i].Length == 1 && parts[i].ToLower() != parts[i])
                continue;

            switch (parts[i])
            {
                //case "ID": // don't convert "ID" for "Id"
                //    break;
                default:
                    parts[i] = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(parts[i].ToLower());
                    break;
            }
        }

        name = string.Join("", parts);

        // can't start with digit
        if (char.IsDigit(name[0]))
            name = "_" + name;

        // can't be a reserved keyword
        if (cs_keywords.Contains(name))
            name = "@" + name;

        // check for name clashes
        int n = 0;
        string attemptName = name;
        while ((GetClassNameForTable(table) == attemptName || table.ColumnPropertyNames.ContainsValue((attemptName))) && n < 100)
        {
            n++;
            attemptName = name + n.ToString();
        }
        table.ColumnPropertyNames.Add(columnName, attemptName);

        return attemptName;
    }

    // Splits both camelCaseWords and also TitleCaseWords. Underscores and dashes are also splitted. Uppercase acronyms are also splitted.
    // E.g. "BusinessEntityID" becomes ["Business","Entity","ID"]
    // E.g. "Employee_SSN" becomes ["employee","_","SSN"]
    static Regex splitUpperCase = new Regex(@"
                (?<=[A-Z])(?=[A-Z][a-z0-9]) |
                 (?<=[^A-Z])(?=[A-Z]) |
                 (?<=[A-Za-z0-9])(?=[^A-Za-z0-9])", RegexOptions.IgnorePatternWhitespace);



}
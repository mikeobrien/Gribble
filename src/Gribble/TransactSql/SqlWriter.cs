using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Gribble.Extensions;

namespace Gribble.TransactSql
{
    public class SqlWriter
    {
        private readonly StringBuilder _text = new StringBuilder();
        private bool _whitespace;
        
        public static SqlWriter CreateWriter() { return new SqlWriter(); }

        public SqlWriter Write(object value) { return Write(value.ToString()); }

        public SqlWriter Write(string format, params object[] args)
        {
            _text.Append((_whitespace ? " " : string.Empty) + 
                (args.Length > 0 ? string.Format(format, args) : format));
            _whitespace = true;
            return this;
        }

        public SqlWriter Trim()
        {
            _whitespace = false;
            return this;
        }

        public void Flush() {}

        public SqlWriter Do(Action<SqlWriter> action)
        {
            action(this);
            return this;
        }

        public SqlWriter Do(bool predicate, Action<SqlWriter> trueAction)
        {
            return Do(predicate, trueAction, x => { });
        }

        public SqlWriter Do(bool predicate, Action<SqlWriter> trueAction, Action<SqlWriter> falseAction)
        {
            if (predicate) trueAction(this); else falseAction(this);
            return this;
        }

        public bool Empty => _text.Length == 0;

        public override string ToString()
        {
            return _text.ToString().NormalizeWhitespace().Trim();
        }

        // ------------------------ Transact Sql --------------------------------

        public SqlWriter Select => Write("SELECT");
        public SqlWriter Wildcard => Write("*");
        public SqlWriter From => Write("FROM");
        public SqlWriter Where => Write("WHERE");
        public SqlWriter Having => Write("HAVING");
        public SqlWriter OrderBy => Write("ORDER BY");
        public SqlWriter Ascending => Write("ASC");
        public SqlWriter Descending => Write("DESC");
        public SqlWriter GroupBy => Write("GROUP BY");
        public SqlWriter Intersect => Write("INTERSECT");
        public SqlWriter Except => Write("EXCEPT");
        public SqlWriter Union => Write("UNION");
        public SqlWriter UnionAll => Write("UNION ALL");

        public SqlWriter Between(int start, int end) 
            { return Write("BETWEEN").Value(start, SqlDbType.Int).And.Value(end, SqlDbType.Int); }
        public SqlWriter In => Write("IN");
        public SqlWriter With(Action<SqlWriter> hint) { return Write("WITH").OpenBlock.Trim().Do(hint).Trim().CloseBlock; }
        public SqlWriter NoLock => Write("NOLOCK");
        public SqlWriter Exists => Write("EXISTS");
        public SqlWriter Like(bool condition) { return Do(!condition, x => x.Not.Flush()).Write("LIKE"); }
        public SqlWriter InsertInto => Write("INSERT INTO");
        public SqlWriter Update => Write("UPDATE");
        public SqlWriter Values => Write("VALUES");
        public SqlWriter Set => Write("SET");
        public SqlWriter Delete => Write("DELETE");
        public SqlWriter MaxLength => Write("MAX");

        public SqlWriter Top(int total)
        { return Write("TOP").OpenBlock.Trim().Value(total, SqlDbType.Int).Trim().CloseBlock; }
        public SqlWriter TopPercent(int total) { return Top(total).Percent; }
        public SqlWriter Percent => Write("PERCENT");
        public SqlWriter Distinct => Write("DISTINCT");
        public SqlWriter Left => Write("LEFT");
        public SqlWriter Inner => Write("INNER");
        public SqlWriter Join => Write("JOIN");
        public SqlWriter On => Write("ON");
        public SqlWriter As => Write("AS");
        public SqlWriter Over => Write("OVER");
        public SqlWriter Partition => Write("PARTITION");
        public SqlWriter By => Write("BY");
        public SqlWriter RowNumberAlias => QuotedName("__RowNumber__");
        public SqlWriter PartitionAlias => QuotedName("__Partition__");
        public SqlWriter SubQueryAlias => QuotedName("__SubQuery__");

        public SqlWriter SubQueryColumn(string name)
            { return SubQueryAlias.Trim().Period.Trim().QuotedName(name); }
        public SqlWriter CountWildcard => Write("COUNT(*)");

        public SqlWriter Count(params string[] name) 
            { return Write("COUNT").OpenBlock.Trim().QuotedName(name).Trim().CloseBlock; }
        public SqlWriter Comma => Write(",");
        public SqlWriter Period => Write(".");

        public SqlWriter QuotedName(params string[] name) 
            { return Write(name.Select(QuoteName).Aggregate((a, s) => a + "." + s)); }
        public SqlWriter QuotedString(string value) { return QuotedString(value, false); }
        public SqlWriter QuotedString(string value, bool unicode) 
            { return QuotedString(x => x.Write(value), unicode); }
        public SqlWriter QuotedString(Action<SqlWriter> value, bool unicode)
        { return Do(unicode, x => x.Write("N").Trim()).SingleQuote.Trim().Do(value).Trim().SingleQuote; }
        public SqlWriter Persisted => Write("PERSISTED");

        public SqlWriter If => Write("IF");
        public SqlWriter Case => Write("CASE");
        public SqlWriter When => Write("WHEN");
        public SqlWriter Then => Write("THEN");
        public SqlWriter Else => Write("ELSE");
        public SqlWriter End => Write("END");

        public SqlWriter True => Write("1");
        public SqlWriter False => Write("0");

        public SqlWriter And => Write("AND");
        public SqlWriter Or => Write("OR");

        public SqlWriter Plus => Write("+");
        public SqlWriter Minus => Write("-");
        public SqlWriter Multiply => Write("*");
        public SqlWriter Divide => Write("/");
        public SqlWriter Modulo => Write("%");

        public SqlWriter Is => Write("IS");
        public SqlWriter Not => Write("NOT");

        public SqlWriter Equal => Write("=");
        public SqlWriter NotEqual => Write("<>");
        public SqlWriter GreaterThan => Write(">");
        public SqlWriter GreaterThanOrEqual => Write(">=");
        public SqlWriter LessThan => Write("<");
        public SqlWriter LessThanOrEqual => Write("<=");

        public SqlWriter OpenBlock => Write("(");
        public SqlWriter CloseBlock => Write(")");
        public SqlWriter EmptyBlock => Write("()");

        public SqlWriter QuerySeperator => Write(";");
        public SqlWriter Null => Write("NULL");

        public SqlWriter DoubleQuote => Write("\"");
        public SqlWriter SingleQuote => Write("'");

        public SqlWriter ParameterPrefix => Write("@");
        public SqlWriter Parameter(string name) { return Write((!name.StartsWith("@") ? "@" : string.Empty) + name); }

        public SqlWriter ParameterList(Action<SqlWriter> seperator, params string[] parameters) 
            { return ParameterList(seperator, parameters.AsEnumerable()); }
        public SqlWriter ParameterList(Action<SqlWriter> seperator, IEnumerable<string> parameters) 
            { return WriteList(seperator, parameters.Select<string, Action<SqlWriter>>(x => y => Parameter(x))); }

        public SqlWriter FieldList(Action<SqlWriter> seperator, params string[] fields) 
            { return FieldList(seperator, fields.AsEnumerable()); }
        public SqlWriter FieldList(Action<SqlWriter> seperator, IEnumerable<string> fields) 
            { return WriteList(seperator, fields.Select<string, Action<SqlWriter>>(x => y => QuotedName(x))); }

        public SqlWriter ExpressionList<T>(Action<SqlWriter> seperator, IEnumerable<T> values, Action<T, SqlWriter> expression)
            { return ExpressionList(seperator, values.Select<T, Action<SqlWriter>>(x => y => expression(x, y))); }
        public SqlWriter ExpressionList(Action<SqlWriter> seperator, params Action<SqlWriter>[] expressions)
            { return ExpressionList(seperator, (IEnumerable<Action<SqlWriter>>)expressions); }
        public SqlWriter ExpressionList(Action<SqlWriter> seperator, IEnumerable<string> expressions)
            { return WriteList(seperator, expressions.Select<string, Action<SqlWriter>>(x => y => y.Write(x))); }
        public SqlWriter ExpressionList(Action<SqlWriter> seperator, IEnumerable<Action<SqlWriter>> expressions) 
            { return WriteList(seperator, expressions); }

        public SqlWriter ParameterAssignmentList(Action<SqlWriter> seperator, params KeyValuePair<string, string>[] assignments)
            { return ParameterAssignmentList(seperator, (IEnumerable<KeyValuePair<string, string>>)assignments); }
        public SqlWriter ParameterAssignmentList(Action<SqlWriter> seperator, IEnumerable<KeyValuePair<string, string>> assignments)
            { return WriteList(seperator, assignments.Select<KeyValuePair<string, string>, Action<SqlWriter>>(x => y => y.QuotedName(x.Key).Equal.Parameter(x.Value))); }
        
        public SqlWriter DataTypeId(int typeId) { return Write(typeId); }

        public SqlWriter DataTypeDefinition(Type type, int? length, int? precision, int? scale) { return DataTypeDefinition(type.GetSqlTypeName(), length, precision, scale); }
        public SqlWriter DataTypeDefinition(SqlDbType type, int? length, int? precision, int? scale) { return DataTypeDefinition(type.GetSqlTypeName(), length, precision, scale); }
        public SqlWriter DataTypeDefinition(string typeName, int? length, int? precision, int? scale)
        {
            var writer = Write(typeName);
            if (DataTypes.TypesWithLength.Any(x => x.SqlName == typeName))
                return writer.OpenBlock.Trim().Do(length != null && length > 0, x => x.Write(length), x => x.MaxLength.Flush()).Trim().CloseBlock;
            if (precision != null && precision > 0 && DataTypes.TypesWithScaleAndPrecision.Any(x => x.SqlName == typeName))
                return writer.OpenBlock.Trim().Write(precision).Do(scale != null, x => x.Trim().Comma.Write(scale).Trim()).Trim().CloseBlock;
            return writer;
        }

        public SqlWriter Value(object value, SqlDbType type) { return Write(ToSqlLiteral(value, type)); }
        
        public SqlWriter NewId() { return WriteFunction("NEWID"); }
        public SqlWriter NewIdColumnDefault { get { return QuotedString(x => x.OpenBlock.Trim().NewId().Trim().CloseBlock.Flush(), false); } }
        public SqlWriter NewSequentialId() { return WriteFunction("NEWSEQUENTIALID"); }
        public SqlWriter NewSequentialIdColumnDefault { get { return QuotedString(x => x.OpenBlock.Trim().NewSequentialId().Trim().CloseBlock.Flush(), false); } }
        public SqlWriter GetDate() { return WriteFunction("GETDATE"); }
        public SqlWriter GetDateColumnDefault { get { return QuotedString(x => x.OpenBlock.Trim().GetDate().Trim().CloseBlock.Flush(), false); } }
        public SqlWriter RowNumber() { return WriteFunction("ROW_NUMBER"); }
        public SqlWriter ScopeIdentity(Type type) { return Cast(x => x.WriteFunction("SCOPE_IDENTITY"), type, null, null, null); }
        public SqlWriter ObjectId(string name) { return WriteFunction("OBJECT_ID", x => x.QuotedString(name, true)); }
        public SqlWriter ObjectDefinition(Action<SqlWriter> value) { return WriteFunction("OBJECT_DEFINITION", value); }
        public SqlWriter Trim(Action<SqlWriter> value) { return LeftTrim(x => x.RightTrim(value)); }
        public SqlWriter LeftTrim(Action<SqlWriter> value) { return WriteFunction("LTRIM", value); }
        public SqlWriter RightTrim(Action<SqlWriter> value) { return WriteFunction("RTRIM", value); }
        public SqlWriter Length(Action<SqlWriter> value) { return WriteFunction("LEN", value); }
        public SqlWriter ToUpper(Action<SqlWriter> value) { return WriteFunction("UPPER", value); }
        public SqlWriter ToLower(Action<SqlWriter> value) { return WriteFunction("LOWER", value); }
        public SqlWriter Cast() { return Write("CAST"); }
        public SqlWriter Cast(Action<SqlWriter> value, Type type, int? length, int? scale, int? precision)
            { return Write("CAST").Trim().OpenBlock.Trim().Do(value).As.DataTypeDefinition(type, length, scale, precision).Trim().CloseBlock; }
        public SqlWriter IndexOf(Action<SqlWriter> text, Action<SqlWriter> searchText) 
            { return WriteFunction("CHARINDEX", searchText, text); }
        public SqlWriter IndexOf(Action<SqlWriter> text, Action<SqlWriter> searchText, Action<SqlWriter> start) 
            { return WriteFunction("CHARINDEX", searchText, text, start); }
        public SqlWriter Coalesce(Action<SqlWriter> first, Action<SqlWriter> second) 
            { return WriteFunction("COALESCE", first, second); }
        public SqlWriter Insert(Action<SqlWriter> text, Action<SqlWriter> newText, Action<SqlWriter> start)
            { return WriteFunction("STUFF", text, start, x => x.Length(text).Minus.Do(start), newText); }
        public SqlWriter Replace(Action<SqlWriter> text, Action<SqlWriter> searchText, Action<SqlWriter> replaceText)
            { return WriteFunction("REPLACE", text, searchText, replaceText); }
        public SqlWriter Substring(Action<SqlWriter> text, Action<SqlWriter> start, Action<SqlWriter> length)
            { return WriteFunction("SUBSTRING", text, start, length); }
        public SqlWriter Substring(Action<SqlWriter> text, Action<SqlWriter> length)
            { return WriteFunction("RIGHT", text, x => x.Length(text).Minus.Do(length)); }
        public SqlWriter IsNull(Action<SqlWriter> expression, Action<SqlWriter> defaultValue)
            { return WriteFunction("ISNULL", expression, defaultValue); }
        public SqlWriter ToHex(Action<SqlWriter> value) { return WriteFunction("CONVERT", x => x.DataTypeDefinition(typeof(string), 0, null, null), value, x => x.Value(1, SqlDbType.Int)); }

        public enum HashAlgorithm { Md2, Md4, Md5, Sha, Sha1 }

        public SqlWriter Hash(Action<SqlWriter> expression, HashAlgorithm algorithm)
            { return WriteFunction("HASHBYTES", x => x.QuotedString(algorithm.ToString()), expression); }

        public SqlWriter StartsWith(bool condition, Action<SqlWriter> text, Action<SqlWriter> searchText) 
            { return Do(text).Like(condition).Do(searchText).Plus.QuotedString("%"); }
        public SqlWriter EndsWith(bool condition, Action<SqlWriter> text, Action<SqlWriter> searchText)
            { return Do(text).Like(condition).QuotedString("%").Plus.Do(searchText); }
        public SqlWriter Contains(bool condition, Action<SqlWriter> text, Action<SqlWriter> searchText)
            { return Do(text).Like(condition).QuotedString("%").Plus.Do(searchText).Plus.QuotedString("%"); }

        public SqlWriter Max(Action<SqlWriter> value) { return WriteFunction("MAX", value); }
        public SqlWriter Min(Action<SqlWriter> value) { return WriteFunction("MIN", value); }
        
        public SqlWriter Table => Write("TABLE");
        public SqlWriter Create => Write("CREATE");
        public SqlWriter Alter => Write("ALTER");
        public SqlWriter Drop => Write("DROP");
        public SqlWriter Identity => Write("IDENTITY");

        public SqlWriter IntegerIdentity => Identity.Trim().OpenBlock.Trim().Value(1, SqlDbType.Int).Trim().Comma.Trim().Value(1, SqlDbType.Int).Trim().CloseBlock;
        public SqlWriter Nullable => Write("NULL");
        public SqlWriter NotNullable => Write("NOT NULL");
        public SqlWriter Default => Write("DEFAULT");
        public SqlWriter Constraint => Write("CONSTRAINT");
        public SqlWriter PrimaryKey(bool clustered) { return Write("PRIMARY KEY").Do(clustered, x => x.Clustered.Flush()); }
        public SqlWriter Add => Write("ADD");
        public SqlWriter Column => Write("COLUMN");
        public SqlWriter Index => Write("INDEX");
        public SqlWriter Clustered => Write("CLUSTERED");
        public SqlWriter NonClustered => Write("NONCLUSTERED");

        public static class Aliases
        {
            public const string IsAutoGenerated = "is_auto_generated";
            public const string IsPrimaryKeyClustered = "is_primary_key_clustered";
            public const string DefaultValue = "default_value";
            public const string Computation = "computation";
            public const string PersistedComputation = "persisted_computation";
            public const string ColumnName = "column_name";
            public const string IsNarrowing = "is_narrowing";
        }

        public SqlWriter PrimaryKeyConstraint(string tableName, string columnName, bool clustered) 
        {
            return Constraint.QuotedName($"PK_{tableName}_{columnName}").PrimaryKey(clustered).
                    OpenBlock.Trim().QuotedName(columnName).Ascending.Trim().CloseBlock;
        }

        public SqlWriter ColumnDefinition(string name, Type clrType, SqlDbType? sqlType, int? length, int? precision, int? scale, bool isPrimaryKey, bool isIdentity, 
                                            bool nullable, bool autoGenerated, string computation, bool? computationPersisted, object defaultValue)
        {
            Action<SqlWriter> writeNullable = x => { if (nullable) x.Nullable.Flush(); else x.NotNullable.Flush(); };
            var writer = QuotedName(name);
            if (!string.IsNullOrEmpty(computation))
                return writer.As.OpenBlock.Trim().Write(computation).Trim().CloseBlock.Do(
                    computationPersisted.HasValue && computationPersisted.Value, x => x.Persisted.Do(writeNullable).Flush());
            var type = sqlType ?? (clrType?.GetSqlType() ?? SqlDbType.NVarChar);
            writer.DataTypeDefinition(type, length, precision, scale);
            if (type == SqlDbType.Int && isIdentity) IntegerIdentity.Flush();
            writeNullable(writer);
            if (!isIdentity)
            {
                if (autoGenerated || (defaultValue != null && defaultValue != DBNull.Value)) Default.Flush();
                if (defaultValue != null && defaultValue != DBNull.Value) Value(defaultValue, type);
                else if (autoGenerated)
                {
                    if (type == SqlDbType.DateTime || type == SqlDbType.SmallDateTime || 
                        type == SqlDbType.DateTime2 || type == SqlDbType.Date || 
                        type == SqlDbType.Time || type == SqlDbType.Timestamp) GetDate();
                    else if (type == SqlDbType.UniqueIdentifier && !isPrimaryKey) NewId();
                    else if (type == SqlDbType.UniqueIdentifier && isPrimaryKey) NewSequentialId();
                    else throw new Exception($"Cannot generate value for type {type}");
                }
            }
            return this;
        }

        public SqlWriter TableExistsValue(string tableName)
        {
            return Cast(x => x.Case.When.TableExists(tableName).Then.True.Else.False.End.Flush(), typeof(bool), null, null, null);
        }

        public SqlWriter TableExists(string tableName)
        {
            return Exists.OpenBlock.Trim().Select.Wildcard.From.Write(System.Tables.TableName).Where.
                    QuotedName(System.Tables.Name).Equal.QuotedString(tableName).Trim().CloseBlock;
        }

        public SqlWriter ProcedureExistsValue(string name)
        {
            return Cast(x => x.Case.When.ProcedureExists(name).Then.True.Else.False.End.Flush(), typeof(bool), null, null, null);
        }

        public SqlWriter ProcedureExists(string name)
        {
            return Exists.OpenBlock.Trim().Select.Wildcard.From.Write(System.Objects.TableName).Where.
                    QuotedName(System.Objects.Name).Equal.QuotedString(name).And.
                    QuotedName(System.Objects.Type).Equal.Value("P", SqlDbType.Char).Trim().CloseBlock;
        }

        public SqlWriter IfColumnExists(string tableName, string columnName)
        {
            return If.Exists.OpenBlock.Trim().Select.Wildcard.From.Write(System.Columns.TableName).Where.
                QuotedName(System.Columns.ObjectId).Equal.ObjectId(tableName).
                   And.QuotedName(System.Columns.Name).Equal.QuotedString(columnName).Trim().CloseBlock;
        }

        public SqlWriter IfIndexExists(string tableName, string indexName)
        {
            return If.Exists.OpenBlock.Trim().Select.Wildcard.From.Write(System.Indexes.TableName).Where.
                QuotedName(System.Indexes.ObjectId).Equal.ObjectId(tableName).
                   And.QuotedName(System.Indexes.Name).Equal.QuotedString(indexName).Trim().CloseBlock;
        }

        // ------------------------ Private Members --------------------------------

        private SqlWriter WriteFunction(string name, params Action<SqlWriter>[] actions)
        {
            Write(name).Trim().OpenBlock.Trim();
            WriteList(x => x.Comma.Flush(), actions);
            Trim().CloseBlock.Flush();
            return this;
        }

        private SqlWriter WriteList(Action<SqlWriter> seperator, IEnumerable<Action<SqlWriter>> expressions)
        {
            var first = true;
            foreach (var expression in expressions)
            {
                if (!first) Trim().Do(seperator); else first = false;
                expression(this);
            }
            return this;
        }

        private static string ToSqlLiteral(object value, SqlDbType type)
        {
            switch (type)
            {
                case SqlDbType.Char:
                case SqlDbType.NChar:
                case SqlDbType.NVarChar:
                case SqlDbType.NText:
                case SqlDbType.DateTime:
                case SqlDbType.Timestamp:
                case SqlDbType.Text:
                case SqlDbType.UniqueIdentifier:
                case SqlDbType.VarChar:
                case SqlDbType.SmallDateTime:
                case SqlDbType.DateTimeOffset:
                case SqlDbType.DateTime2:
                case SqlDbType.Time:
                case SqlDbType.Date: return QuoteString(value.ToString());
                case SqlDbType.Bit: return (value is bool && (bool)value) || 
                                            value.ToString().ToLower() == "true" || 
                                            value.ToString() == "1" ? "1" : "0";
                case SqlDbType.Float: return $"{value}E";
                default: return value.ToString();
            }
        }

        private static string QuoteName(string name)
        {
            return $"{(name.StartsWith("[") ? string.Empty : "[")}{name}{(name.EndsWith("]") ? string.Empty : "]")}";
        }

        private static string QuoteString(string value)
        {
            return $"'{value}'";
        }
    }
}

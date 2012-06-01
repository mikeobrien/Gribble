using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

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

        public bool Empty { get { return _text.Length == 0; } }

        public override string ToString()
        {
            return _text.ToString().NormalizeWhitespace().Trim();
        }

        // ------------------------ Transact Sql --------------------------------

        public SqlWriter Select { get { return Write("SELECT"); } }
        public SqlWriter Wildcard { get { return Write("*"); } }
        public SqlWriter From { get { return Write("FROM"); } }
        public SqlWriter Where { get { return Write("WHERE"); } }
        public SqlWriter Having { get { return Write("HAVING"); } }
        public SqlWriter OrderBy { get { return Write("ORDER BY"); } }
        public SqlWriter Ascending { get { return Write("ASC"); } }
        public SqlWriter Descending { get { return Write("DESC"); } }
        public SqlWriter GroupBy { get { return Write("GROUP BY"); } }
        public SqlWriter Intersect { get { return Write("INTERSECT"); } }
        public SqlWriter Except { get { return Write("EXCEPT"); } }
        public SqlWriter Union { get { return Write("UNION"); } }
        public SqlWriter UnionAll { get { return Write("UNION ALL"); } }
        public SqlWriter Between(int start, int end) 
            { return Write("BETWEEN").Value(start, SqlDbType.Int).And.Value(end, SqlDbType.Int); }
        public SqlWriter In { get { return Write("IN"); } }
        public SqlWriter With(Action<SqlWriter> hint) { return Write("WITH").OpenBlock.Trim().Do(hint).Trim().CloseBlock; }
        public SqlWriter NoLock { get { return Write("NOLOCK"); } }
        public SqlWriter Exists { get { return Write("EXISTS"); } }
        public SqlWriter Like(bool condition) { return Do(!condition, x => x.Not.Flush()).Write("LIKE"); }
        public SqlWriter InsertInto { get { return Write("INSERT INTO"); } }
        public SqlWriter Update { get { return Write("UPDATE"); } }
        public SqlWriter Values { get { return Write("VALUES"); } }
        public SqlWriter Set { get { return Write("SET"); } }
        public SqlWriter Delete { get { return Write("DELETE"); } }
        public SqlWriter MaxLength { get { return Write("MAX"); } }
        public SqlWriter Top(int total)
        { return Write("TOP").OpenBlock.Trim().Value(total, SqlDbType.Int).Trim().CloseBlock; }
        public SqlWriter TopPercent(int total) { return Top(total).Percent; }
        public SqlWriter Percent { get { return Write("PERCENT"); } }
        public SqlWriter Distinct { get { return Write("DISTINCT"); } }
        public SqlWriter Left { get { return Write("LEFT"); } }
        public SqlWriter Inner { get { return Write("INNER"); } }
        public SqlWriter Join { get { return Write("JOIN"); } }
        public SqlWriter On { get { return Write("ON"); } }
        public SqlWriter As { get { return Write("AS"); } }
        public SqlWriter Over { get { return Write("OVER"); } }
        public SqlWriter Partition { get { return Write("PARTITION"); } }
        public SqlWriter By { get { return Write("BY"); } }
        public SqlWriter RowNumberAlias { get { return QuotedName("__RowNumber__"); } }
        public SqlWriter PartitionAlias { get { return QuotedName("__Partition__"); } }
        public SqlWriter SubQueryAlias { get { return QuotedName("__SubQuery__"); } }
        public SqlWriter SubQueryColumn(string name)
            { return SubQueryAlias.Trim().Period.Trim().QuotedName(name); }
        public SqlWriter CountWildcard { get { return Write("COUNT(*)"); } }
        public SqlWriter Count(params string[] name) 
            { return Write("COUNT").OpenBlock.Trim().QuotedName(name).Trim().CloseBlock; }
        public SqlWriter Comma { get { return Write(","); } }
        public SqlWriter Period { get { return Write("."); } }
        public SqlWriter QuotedName(params string[] name) 
            { return Write(name.Select(QuoteName).Aggregate((a, s) => a + "." + s)); }
        public SqlWriter QuotedString(string value) { return QuotedString(value, false); }
        public SqlWriter QuotedString(string value, bool unicode) 
            { return QuotedString(x => x.Write(value), unicode); }
        public SqlWriter QuotedString(Action<SqlWriter> value, bool unicode)
        { return Do(unicode, x => x.Write("N").Trim()).SingleQuote.Trim().Do(value).Trim().SingleQuote; }
        public SqlWriter Persisted { get { return Write("PERSISTED"); } }

        public SqlWriter If { get { return Write("IF"); } }
        public SqlWriter Case { get { return Write("CASE"); } }
        public SqlWriter When { get { return Write("WHEN"); } }
        public SqlWriter Then { get { return Write("THEN"); } }
        public SqlWriter Else { get { return Write("ELSE"); } }
        public SqlWriter End { get { return Write("END"); } }

        public SqlWriter True { get { return Write("1"); } }
        public SqlWriter False { get { return Write("0"); } }

        public SqlWriter And { get { return Write("AND"); } }
        public SqlWriter Or { get { return Write("OR"); } }

        public SqlWriter Plus { get { return Write("+"); } }
        public SqlWriter Minus { get { return Write("-"); } }
        public SqlWriter Multiply { get { return Write("*"); } }
        public SqlWriter Divide { get { return Write("/"); } }
        public SqlWriter Modulo { get { return Write("%"); } }

        public SqlWriter Is { get { return Write("IS"); } }
        public SqlWriter Not { get { return Write("NOT"); } }

        public SqlWriter Equal { get { return Write("="); } }
        public SqlWriter NotEqual { get { return Write("<>"); } }
        public SqlWriter GreaterThan { get { return Write(">"); } }
        public SqlWriter GreaterThanOrEqual { get { return Write(">="); } }
        public SqlWriter LessThan { get { return Write("<"); } }
        public SqlWriter LessThanOrEqual { get { return Write("<="); } }

        public SqlWriter OpenBlock { get { return Write("("); } }
        public SqlWriter CloseBlock { get { return Write(")"); } }
        public SqlWriter EmptyBlock { get { return Write("()"); } }

        public SqlWriter QuerySeperator { get { return Write(";"); } }
        public SqlWriter Null { get { return Write("NULL"); } }

        public SqlWriter DoubleQuote { get { return Write("\""); } }
        public SqlWriter SingleQuote { get { return Write("'"); } }

        public SqlWriter ParameterPrefix { get { return Write("@"); } }
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
        
        public SqlWriter Table { get { return Write("TABLE"); } }
        public SqlWriter Create { get { return Write("CREATE"); } }
        public SqlWriter Alter { get { return Write("ALTER"); } }
        public SqlWriter Drop { get { return Write("DROP"); } }
        public SqlWriter Identity { get { return Write("IDENTITY"); } }
        public SqlWriter IntegerIdentity 
            { get { return Identity.Trim().OpenBlock.Trim().Value(1, SqlDbType.Int).Trim().Comma.Trim().Value(1, SqlDbType.Int).Trim().CloseBlock; } }
        public SqlWriter Nullable { get { return Write("NULL"); } }
        public SqlWriter NotNullable { get { return Write("NOT NULL"); } }
        public SqlWriter Default { get { return Write("DEFAULT"); } }
        public SqlWriter Constraint { get { return Write("CONSTRAINT"); } }
        public SqlWriter PrimaryKey(bool clustered) { return Write("PRIMARY KEY").Do(clustered, x => x.Clustered.Flush()); }
        public SqlWriter Add { get { return Write("ADD"); } }
        public SqlWriter Column { get { return Write("COLUMN"); } }
        public SqlWriter Index { get { return Write("INDEX"); } }
        public SqlWriter Clustered { get { return Write("CLUSTERED"); } }
        public SqlWriter NonClustered { get { return Write("NONCLUSTERED"); } }

        public static class Aliases
        {
            public const string IsAutoGenerated = "[is_auto_generated]";
            public const string IsPrimaryKeyClustered = "[is_primary_key_clustered]";
            public const string DefalutValue = "[default_value]";
            public const string Computation = "[computation]";
            public const string PersistedComputation = "[persisted_computation]";
            public const string ColumnName = "[column_name]";
        }

        public SqlWriter PrimaryKeyConstraint(string tableName, string columnName, bool clustered) 
        {
            return Constraint.QuotedName(string.Format("PK_{0}_{1}", tableName, columnName)).PrimaryKey(clustered).
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
            var type = sqlType != null ? sqlType.Value : (clrType != null ? clrType.GetSqlType() : SqlDbType.NVarChar);
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
                    else throw new Exception(string.Format("Cannot generate value for type {0}", type));
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
                    Write(System.Tables.Name).Equal.QuotedString(tableName).Trim().CloseBlock;
        }

        public SqlWriter IfColumnExists(string tableName, string columnName)
        {
            return If.Exists.OpenBlock.Trim().Select.Wildcard.From.Write(System.Columns.TableName).Where.
                Write(System.Columns.ObjectId).Equal.ObjectId(tableName).
                   And.Write(System.Columns.Name).Equal.QuotedString(columnName).Trim().CloseBlock;
        }

        public SqlWriter IfIndexExists(string tableName, string indexName)
        {
            return If.Exists.OpenBlock.Trim().Select.Wildcard.From.Write(System.Indexes.TableName).Where.
                Write(System.Indexes.ObjectId).Equal.ObjectId(tableName).
                   And.Write(System.Indexes.Name).Equal.QuotedString(indexName).Trim().CloseBlock;
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
                case SqlDbType.Float: return string.Format("{0}E", value);
                default: return value.ToString();
            }
        }

        private static string QuoteName(string name)
        {
            return string.Format("{0}{1}{2}", name.StartsWith("[") ? string.Empty : "[", name, name.EndsWith("]") ? string.Empty : "]");
        }

        private static string QuoteString(string value)
        {
            return string.Format("'{0}'", value);
        }
    }
}

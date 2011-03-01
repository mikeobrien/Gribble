using System;
using System.Collections.Generic;
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

        public SqlWriter Write(string value)
        {
            _text.Append((_whitespace ? " " : string.Empty) + value);
            _whitespace = true;
            return this;
        }
         
        public SqlWriter Write(string format, params object[] args)
        {
            Write(string.Format(format, args));
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
            { return Write("BETWEEN").Value(start).And.Value(end); }
        public SqlWriter In { get { return Write("IN"); } }
        public SqlWriter Exists { get { return Write("EXISTS"); } }
        public SqlWriter Like { get { return Write("LIKE"); } }
        public SqlWriter InsertInto { get { return Write("INSERT INTO"); } }
        public SqlWriter Update { get { return Write("UPDATE"); } }
        public SqlWriter Values { get { return Write("VALUES"); } }
        public SqlWriter Set { get { return Write("SET"); } }
        public SqlWriter Delete { get { return Write("DELETE"); } }
        public SqlWriter Top(int total)
        { return Write("TOP").OpenBlock.Trim().Value(total).Trim().CloseBlock; }
        public SqlWriter TopPercent(int total) { return Top(total).Percent; }
        public SqlWriter Percent { get { return Write("PERCENT"); } }
        public SqlWriter Distinct { get { return Write("DISTINCT"); } }
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
            { return SubQueryAlias.Period.QuotedName(name); }
        public SqlWriter CountWildcard { get { return Write("COUNT(*)"); } }
        public SqlWriter Count(params string[] name) 
            { return Write("COUNT").OpenBlock.Trim().QuotedName(name).Trim().CloseBlock; }
        public SqlWriter Comma { get { return Write(","); } }
        public SqlWriter Period { get { return Write("."); } }
        public SqlWriter QuotedName(params string[] name) 
            { return Write(name.Select(QuoteName).Aggregate((a, s) => a + "." + s)); }
        private SqlWriter JoinName(params Action<SqlWriter>[] name) 
            { return WriteList(x => x.Period.Trim().Flush(), name); }
        public SqlWriter QuotedString(string value) { return QuotedString(value, false); }
        public SqlWriter QuotedString(string value, bool unicode) 
            { return QuotedString(x => x.Write(value), unicode); }
        public SqlWriter QuotedString(Action<SqlWriter> value, bool unicode)
        { return Do(unicode, x => x.Write("N").Trim()).SingleQuote.Trim().Do(value).Trim().SingleQuote; }

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

        public SqlWriter DataType(Type type) { return Write(DataTypes.GetSqlType(type)); }
        public SqlWriter DataType(Type type, int length) { return Write(DataTypes.GetSqlType(type, length)); }
        public SqlWriter DataTypeId(DataTypes.SqlTypeId typeId) { return Write((int)typeId); }

        public SqlWriter Value(object value) { return Write(GetSqlConstant(value)); }
        
        public SqlWriter NewId() { return WriteFunction("NEWID"); }
        public SqlWriter NewIdColumnDefault { get { return QuotedString(x => x.OpenBlock.Trim().NewId().Trim().CloseBlock.Flush(), false); } }
        public SqlWriter NewSequentialId() { return WriteFunction("NEWSEQUENTIALID"); }
        public SqlWriter NewSequentialIdColumnDefault { get { return QuotedString(x => x.OpenBlock.Trim().NewSequentialId().Trim().CloseBlock.Flush(), false); } }
        public SqlWriter GetDate() { return WriteFunction("GETDATE"); }
        public SqlWriter GetDateColumnDefault { get { return QuotedString(x => x.OpenBlock.Trim().GetDate().Trim().CloseBlock.Flush(), false); } }
        public SqlWriter RowNumber() { return WriteFunction("ROW_NUMBER"); }
        public SqlWriter ScopeIdentity(Type type) { return Cast(x => x.WriteFunction("SCOPE_IDENTITY"), type); }
        public SqlWriter ObjectId(string name) { return WriteFunction("OBJECT_ID", x => x.QuotedString(name, true)); }
        public SqlWriter ObjectDefinition(Action<SqlWriter> value) { return WriteFunction("OBJECT_DEFINITION", value); }
        public SqlWriter Trim(Action<SqlWriter> value) { return LeftTrim(x => x.RightTrim(value)); }
        public SqlWriter LeftTrim(Action<SqlWriter> value) { return WriteFunction("LTRIM", value); }
        public SqlWriter RightTrim(Action<SqlWriter> value) { return WriteFunction("RTRIM", value); }
        public SqlWriter Length(Action<SqlWriter> value) { return WriteFunction("LEN", value); }
        public SqlWriter ToUpper(Action<SqlWriter> value) { return WriteFunction("UPPER", value); }
        public SqlWriter ToLower(Action<SqlWriter> value) { return WriteFunction("LOWER", value); }
        public SqlWriter Cast(Action<SqlWriter> value, Type type, int length = 0)
            { return Write("CAST").Trim().OpenBlock.Trim().Do(value).As.DataType(type, 0).Trim().CloseBlock; }
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
        public SqlWriter ToHex(Action<SqlWriter> value) { return WriteFunction("CONVERT", x => x.DataType(typeof(string), 0), value, x => x.Value(1)); }

        public enum HashAlgorithm { Md2, Md4, Md5, Sha, Sha1 }

        public SqlWriter Hash(Action<SqlWriter> expression, HashAlgorithm algorithm)
            { return WriteFunction("HASHBYTES", x => x.QuotedString(algorithm.ToString()), expression); }

        public SqlWriter StartsWith(Action<SqlWriter> text, Action<SqlWriter> searchText) 
            { return Do(text).Like.Do(searchText).Plus.QuotedString("%"); }
        public SqlWriter EndsWith(Action<SqlWriter> text, Action<SqlWriter> searchText) 
            { return Do(text).Like.QuotedString("%").Plus.Do(searchText); }
        public SqlWriter Contains(Action<SqlWriter> text, Action<SqlWriter> searchText) 
            { return Do(text).Like.QuotedString("%").Plus.Do(searchText).Plus.QuotedString("%"); }

        public SqlWriter Max(Action<SqlWriter> value) { return WriteFunction("MAX", value); }
        public SqlWriter Min(Action<SqlWriter> value) { return WriteFunction("MIN", value); }
        
        public SqlWriter Table { get { return Write("TABLE"); } }
        public SqlWriter Create { get { return Write("CREATE"); } }
        public SqlWriter Alter { get { return Write("ALTER"); } }
        public SqlWriter Drop { get { return Write("DROP"); } }
        public SqlWriter Identity { get { return Write("IDENTITY"); } }
        public SqlWriter IntegerIdentity 
            { get { return Identity.Trim().OpenBlock.Trim().Value(1).Trim().Comma.Trim().Value(1).Trim().CloseBlock; } }
        public SqlWriter Nullable { get { return Write("NULL"); } }
        public SqlWriter NotNullable { get { return Write("NOT NULL"); } }
        public SqlWriter Default { get { return Write("DEFAULT"); } }
        public SqlWriter Constraint { get { return Write("CONSTRAINT"); } }
        public SqlWriter PrimaryKey { get { return Write("PRIMARY KEY"); } }
        public SqlWriter Add { get { return Write("ADD"); } }
        public SqlWriter Column { get { return Write("COLUMN"); } }
        public SqlWriter Index { get { return Write("INDEX"); } }
        public SqlWriter NonClustered { get { return Write("NONCLUSTERED"); } }

        public SqlWriter SystemTables { get { return QuotedName("sys", "tables"); } }
        public SqlWriter SystemTablessAlias { get { return QuotedName("ST"); } }
        public SqlWriter SystemColumns { get { return QuotedName("sys", "columns"); } }
        public SqlWriter SystemColumnsAlias { get { return QuotedName("SC"); } }
        public SqlWriter SystemIndexColumns { get { return QuotedName("sys", "index_columns"); } }
        public SqlWriter SystemIndexes { get { return QuotedName("sys", "indexes"); } }
        public SqlWriter SystemObjectIdColumn { get { return QuotedName("object_id"); } }
        public SqlWriter SystemNameColumn { get { return QuotedName("name"); } }
        public SqlWriter SystemTypeColumn { get { return QuotedName("system_type_id"); } }
        public SqlWriter SystemUserTypeColumn { get { return QuotedName("user_type_id"); } }
        public SqlWriter SystemMaxLengthColumn { get { return QuotedName("max_length"); } }
        public SqlWriter SystemIsNullableColumn { get { return QuotedName("is_nullable"); } }
        public SqlWriter SystemIsIdentityColumn { get { return QuotedName("is_identity"); } }
        public SqlWriter SystemDefaultObjectIdColumn { get { return QuotedName("default_object_id"); } }
        public SqlWriter SystemIsPrimaryKeyColumn { get { return QuotedName("is_primary_key"); } }
        public SqlWriter SystemColumnIdColumn { get { return QuotedName("column_id"); } }
        public SqlWriter SystemIndexIdColumn { get { return QuotedName("index_id"); } }
        public SqlWriter SystemIsAutoGeneratedAlias { get { return QuotedName("is_auto_generated"); } }
        public SqlWriter SystemDefaultValueAlias { get { return QuotedName("default_value"); } }

        public SqlWriter SubQueryAlias_Name { get { return JoinName(x => x.SubQueryAlias.Flush(), x => x.SystemNameColumn.Flush()); } }
        public SqlWriter SystemColumns_Type { get { return JoinName(x => x.SystemColumns.Flush(), x => x.SystemTypeColumn.Flush()); } }
        public SqlWriter SystemColumns_Name { get { return JoinName(x => x.SystemColumns.Flush(), x => x.SystemNameColumn.Flush()); } }
        public SqlWriter SystemColumns_ObjectId { get { return JoinName(y => y.SystemColumns.Flush(), y => y.SystemObjectIdColumn.Flush()); } }
        public SqlWriter SystemColumns_ColumnId { get { return JoinName(y => y.SystemColumns.Flush(), y => y.SystemColumnIdColumn.Flush()); } }
        public SqlWriter SystemColumnsAlias_ObjectId { get { return JoinName(y => y.SystemColumnsAlias.Flush(), y => y.SystemObjectIdColumn.Flush()); } }
        public SqlWriter SystemColumnsAlias_ColumnId { get { return JoinName(y => y.SystemColumnsAlias.Flush(), y => y.SystemColumnIdColumn.Flush()); } }
        public SqlWriter SystemColumnsAlias_Name { get { return JoinName(y => y.SystemColumnsAlias.Flush(), y => y.SystemNameColumn.Flush()); } }
        public SqlWriter SystemColumnsAlias_Type { get { return JoinName(y => y.SystemColumnsAlias.Flush(), y => y.SystemTypeColumn.Flush()); } }
        public SqlWriter SystemIndexColumns_ColumnId { get { return JoinName(y => y.SystemIndexColumns.Flush(), y => y.SystemColumnIdColumn.Flush()); } }
        public SqlWriter SystemIndexColumns_ObjectId { get { return JoinName(y => y.SystemIndexColumns.Flush(), y => y.SystemObjectIdColumn.Flush()); } }
        public SqlWriter SystemIndexColumns_IndexId { get { return JoinName(y => y.SystemIndexColumns.Flush(), y => y.SystemIndexIdColumn.Flush()); } }
        public SqlWriter SystemIndexes_ObjectId { get { return JoinName(y => y.SystemIndexes.Flush(), y => y.SystemObjectIdColumn.Flush()); } }
        public SqlWriter SystemIndexes_IndexId { get { return JoinName(y => y.SystemIndexes.Flush(), y => y.SystemIndexIdColumn.Flush()); } }

        public SqlWriter PrimaryKeyConstraint(string tableName, string columnName) 
        {
            return Constraint.QuotedName(string.Format("PK_{0}_{1}", tableName, columnName)).PrimaryKey.
                    OpenBlock.Trim().QuotedName(columnName).Ascending.Trim().CloseBlock;
        }

        public SqlWriter ColumnDefinition(string name, Type type, int length, bool isPrimaryKey, bool isIdentity, 
                                            bool nullable, bool autoGenerated, object defaultValue)
        {
            QuotedName(name).DataType(type, length);
            if (type == typeof(int) && isIdentity) IntegerIdentity.Flush();
            if (nullable) Nullable.Flush();
            else NotNullable.Flush();
            if (!isIdentity)
            {
                if (autoGenerated || (defaultValue != null && defaultValue != DBNull.Value)) Default.Flush();
                if (defaultValue != null && defaultValue != DBNull.Value) Value(defaultValue);
                else if (autoGenerated)
                {
                    if (type == typeof (DateTime)) GetDate();
                    else if (type == typeof (Guid) && !isPrimaryKey) NewId();
                    else if (type == typeof (Guid) && isPrimaryKey) NewSequentialId();
                    else throw new Exception(string.Format("Cannot generate value for type {0}", type.Name));
                }
            }
            return this;
        }

        public SqlWriter TableExistsValue(string tableName)
        {
            return Cast(x => x.Case.When.TableExists(tableName).Then.True.Else.False.End.Flush(), typeof(bool));
        }

        public SqlWriter TableExists(string tableName)
        {
            return Exists.OpenBlock.Trim().Select.Wildcard.From.SystemTables.Where.
                    SystemNameColumn.Equal.QuotedString(tableName).Trim().CloseBlock;
        }

        public SqlWriter IfColumnExists(string tableName, string columnName)
        {
            return If.Exists.OpenBlock.Trim().Select.Wildcard.From.SystemColumns.Where.SystemObjectIdColumn.Equal.ObjectId(tableName).
                   And.SystemNameColumn.Equal.QuotedString(columnName).Trim().CloseBlock;
        }

        public SqlWriter IfIndexExists(string tableName, string indexName)
        {
            return If.Exists.OpenBlock.Trim().Select.Wildcard.From.SystemIndexes.Where.SystemObjectIdColumn.Equal.ObjectId(tableName).
                   And.SystemNameColumn.Equal.QuotedString(indexName).Trim().CloseBlock;
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

        private static string GetSqlConstant(object value)
        {
            if (value is string) return QuoteString((string)value);
            if (value is int || value is int?) return value.ToString();
            if (value is DateTime || value is DateTime?) QuoteString(value.ToString());
            if (value is bool || value is bool?) return (bool)value ? "1" : "0";
            if (value is long || value is long?) return value.ToString();
            if (value is decimal || value is decimal?) return value.ToString();
            if (value is byte || value is byte?) return value.ToString();
            if (value is double || value is double?) string.Format("{0}E", value);
            if (value is float || value is float?) string.Format("{0}E", value);
            if (value is Guid || value is Guid?) return QuoteString(value.ToString());
            return value.ToString();
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

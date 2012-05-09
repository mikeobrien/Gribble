using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Gribble.Model;

namespace Gribble.TransactSql
{
    public static class SchemaWriter
    {
        public static Statement CreateUnionColumnsStatement(Select select)
        {
            return CreateColumnsIntersectionStatement(select.GetSourceTables().Select(x => x.Source.Table.Name));
        }

        public static Statement CreateSelectIntoColumnsStatement(Select select)
        {
            var sql = new SqlWriter();
            var sourceTables = select.GetSourceTables().Select(x => x.Source.Table.Name);
            var statement = CreateColumnsIntersectionStatement(sourceTables.Concat(new[] { select.Target.Table.Name }));

            sql.Select.SubQueryColumn(System.Columns.Name).Trim().Comma.
                    Cast(z => z.Case.When.Write(System.Columns.Aliased.SystemTypeId).LessThan.OpenBlock.Trim().
                        Select.Max(x => x.Write(System.Columns.SystemTypeId)).From.Write(System.Columns.TableName).
                        Where.Write(System.Columns.Name).Equal.SubQueryColumn(System.Columns.Name).And.
                        Write(System.Columns.SystemTypeId).In.OpenBlock.Trim().ExpressionList(x => x.Comma.Flush(), x => x.DataTypeId(DataTypes.Char.SqlId),
                                x => x.DataTypeId(DataTypes.VarChar.SqlId),
                                x => x.DataTypeId(DataTypes.Text.SqlId),
                                x => x.DataTypeId(DataTypes.NChar.SqlId),
                                x => x.DataTypeId(DataTypes.NVarChar.SqlId),
                                x => x.DataTypeId(DataTypes.NText.SqlId)).Trim().CloseBlock.And.
                        Write(System.Columns.ObjectId).In.OpenBlock.Trim().ExpressionList(x => x.Comma.Flush(), sourceTables.Select<string, Action<SqlWriter>>(x => y => y.ObjectId(x))).Trim().CloseBlock.Trim().CloseBlock.
                        Then.True.Else.False.End.Flush(), typeof(bool), null, null, null).As.QuotedName("NarrowingConversion").
                    From.OpenBlock.Trim().Write(statement.Text).Trim().CloseBlock.SubQueryAlias.
                    Join.Write(System.Columns.TableName).Write(System.Columns.TableAlias).On.
                    SubQueryColumn(System.Columns.Name).Equal.Write(System.Columns.Aliased.Name).And.
                    Write(System.Columns.Aliased.ObjectId).Equal.ObjectId(select.Target.Table.Name);
            statement.Text = sql.ToString();
            return statement;
        }

        public static Statement CreateCreateTableColumnsStatement(Select select)
        {
            var sourceTables = select.GetSourceTables().Select(x => x.Source.Table.Name);
            var statement = CreateColumnsIntersectionStatement(sourceTables);

            var writer = WriteSelectColumns(SqlWriter.CreateWriter()).
                    Join.OpenBlock.Trim().Write(statement.Text).Trim().CloseBlock.SubQueryAlias.On.
                    SubQueryColumn(System.Columns.Name).Equal.Write(System.Columns.Aliased.Name).And.
                    Write(System.Columns.Aliased.ObjectId).Equal.ObjectId(sourceTables.First());
            return new Statement(writer.ToString(), Statement.StatementType.Text, Statement.ResultType.Multiple);
        }

        public static Statement CreateTableColumnsStatement(string tableName)
        {
            var writer = WriteSelectColumns(SqlWriter.CreateWriter()).
                    Where.Write(System.Columns.Aliased.ObjectId).Equal.ObjectId(tableName);
            return new Statement(writer.ToString(), Statement.StatementType.Text, Statement.ResultType.Multiple);
        }

        public static SqlWriter WriteSelectColumns(SqlWriter writer)
        {
            return writer.Select.
                    Write(System.Columns.Aliased.Name).Trim().Comma.
                    Write(System.Columns.Aliased.SystemTypeId).Trim().Comma.
                    Cast(x => x.Case.When.Write(System.Columns.Aliased.SystemTypeId).In.
                        OpenBlock.Trim().Write(DataTypes.NChar.SqlId).Trim().Comma.Write(DataTypes.NVarChar.SqlId).Trim().Comma.Write(DataTypes.NText.SqlId).Trim().CloseBlock.
                        Then.Write(System.Columns.Aliased.MaxLength).Divide.Value(2, SqlDbType.Int).Else.Write(System.Columns.Aliased.MaxLength).End.Flush(), typeof(short), null, null, null).
                        As.Write(System.Columns.MaxLength).Trim().Comma.
                    Write(System.Columns.Aliased.IsNullable).Trim().Comma.
                    Write(System.Columns.Aliased.IsIdentity).Trim().Comma.
                    Cast(x => x.Trim().OpenBlock.Trim().
                        Case.ObjectDefinition(y => y.Write(System.Columns.Aliased.DefaultObjectId).Flush()).
                            When.GetDateColumnDefault.Then.True.
                            When.NewIdColumnDefault.Then.True.
                            When.NewSequentialIdColumnDefault.Then.True.
                            Else.False.End.Trim().
                        CloseBlock.Flush(), typeof(bool), null, null, null).As.Write(SqlWriter.Aliases.IsAutoGenerated).Trim().Comma.
                    Case.ObjectDefinition(y => y.Write(System.Columns.Aliased.DefaultObjectId).Flush()).
                        When.GetDateColumnDefault.Then.Null.
                        When.NewIdColumnDefault.Then.Null.
                        When.NewSequentialIdColumnDefault.Then.Null.
                        Else.Replace(x => x.Replace(y => y.ObjectDefinition(z => z.Write(System.Columns.Aliased.DefaultObjectId).Flush()),
                                                    y => y.QuotedString("("),
                                                    y => y.QuotedString("")),
                                        x => x.QuotedString(")"),
                                        x => x.QuotedString("")).End.
                        As.Write(SqlWriter.Aliases.DefalutValue).Trim().Comma.
                    IsNull(x => x.Write(System.Indexes.Aliased.IsPrimaryKey).Flush(), x => x.False.Flush()).As.Write(System.Indexes.IsPrimaryKey).Trim().Comma.
                    Cast(x => x.Trim().OpenBlock.Trim().
                        Case.Write(System.Indexes.Aliased.Type).
                            When.Value(1, SqlDbType.Int).Then.True.
                            Else.False.End.Trim().CloseBlock.Flush(),
                        typeof(bool), null, null, null).As.Write(SqlWriter.Aliases.IsPrimaryKeyClustered).Trim().Comma.
                    Write(System.Columns.Aliased.Precision).Trim().Comma.
                    Write(System.Columns.Aliased.Scale).Trim().Comma.
                    Write(System.ComputedColumns.Aliased.Definition).As.Write(SqlWriter.Aliases.Computation).Trim().Comma.
                    Write(System.ComputedColumns.Aliased.IsPersisted).As.Write(SqlWriter.Aliases.PersistedComputation).
                    From.OpenBlock.OpenBlock.OpenBlock.Write(System.Columns.TableName).Write(System.Columns.TableAlias).
                        Left.Join.Write(System.IndexColumns.TableName).Write(System.IndexColumns.TableAlias).On.
                            Write(System.Columns.Aliased.ColumnId).Equal.Write(System.IndexColumns.Aliased.ColumnId).And.
                            Write(System.Columns.Aliased.ObjectId).Equal.Write(System.IndexColumns.Aliased.ObjectId).CloseBlock.
                                Left.Join.Write(System.Indexes.TableName).Write(System.Indexes.TableAlias).On.
                                    Write(System.Indexes.Aliased.IndexId).Equal.Write(System.IndexColumns.Aliased.IndexId).And.
                                    Write(System.Indexes.Aliased.ObjectId).Equal.Write(System.IndexColumns.Aliased.ObjectId).CloseBlock.
                        Left.Join.Write(System.ComputedColumns.TableName).Write(System.ComputedColumns.TableAlias).On.
                            Write(System.Columns.Aliased.ColumnId).Equal.Write(System.ComputedColumns.Aliased.ColumnId).And.
                            Write(System.Columns.Aliased.ObjectId).Equal.Write(System.ComputedColumns.Aliased.ObjectId).CloseBlock;
        }

        public static Statement CreateColumnsIntersectionStatement(IEnumerable<string> tables)
        {
            var sql = new SqlWriter();

            foreach (var table in tables.Select((x, i) => new { First = i == 0, Name = x }))
            {
                if (!table.First) sql.Intersect.Flush();
                sql.Select.Write(System.Columns.Name).Trim().Comma.
                    Case.Write(System.Columns.SystemTypeId).When.DataTypeId(DataTypes.VarChar.SqlId).Then.DataTypeId(DataTypes.NVarChar.SqlId).
                                            When.DataTypeId(DataTypes.Char.SqlId).Then.DataTypeId(DataTypes.NChar.SqlId).
                                            When.DataTypeId(DataTypes.Text.SqlId).Then.DataTypeId(DataTypes.NText.SqlId).
                                            Else.Write(System.Columns.SystemTypeId).End.As.Write(System.Columns.SystemTypeId).Trim().Comma.
                    Case.Write(System.Columns.UserTypeId).When.DataTypeId(DataTypes.VarChar.SqlId).Then.DataTypeId(DataTypes.NVarChar.SqlId).
                                                When.DataTypeId(DataTypes.Char.SqlId).Then.DataTypeId(DataTypes.NChar.SqlId).
                                                When.DataTypeId(DataTypes.Text.SqlId).Then.DataTypeId(DataTypes.NText.SqlId).
                                                Else.Write(System.Columns.UserTypeId).End.As.Write(System.Columns.UserTypeId).
                    From.Write(System.Columns.TableName).
                    Where.Write(System.Columns.ObjectId).Equal.ObjectId(table.Name);
            }

            return new Statement(sql.ToString(), Statement.StatementType.Text, Statement.ResultType.Multiple);
        }

        public static Statement CreateTableCreateStatement(string tableName, params Column[] columns)
        {
            var writer = SqlWriter.CreateWriter().Create.Table.QuotedName(tableName).OpenBlock.Trim();
            var first = true;
            foreach (var column in columns)
            {
                if (!first) writer.Trim().Comma.Flush();
                WriteColumnDefinition(writer, column);
                first = false;
            }
            var primaryKey = columns.FirstOrDefault(x => x.Key != Column.KeyType.None);
            if (primaryKey != null) writer.Trim().Comma.PrimaryKeyConstraint(
                tableName, primaryKey.Name, primaryKey.Key == Column.KeyType.ClusteredPrimaryKey);
            writer.Trim().CloseBlock.Flush();
            return new Statement(writer.ToString(), Statement.StatementType.Text, Statement.ResultType.None);
        }

        public static Statement CreateTableExistsStatement(string tableName)
        {
            var writer = SqlWriter.CreateWriter().Select.TableExistsValue(tableName);
            return new Statement(writer.ToString(), Statement.StatementType.Text, Statement.ResultType.Scalar);
        }

        public static Statement CreateDeleteTableStatement(string tableName)
        {
            var writer = SqlWriter.CreateWriter().If.TableExists(tableName).Drop.Table.QuotedName(tableName);
            return new Statement(writer.ToString(), Statement.StatementType.Text, Statement.ResultType.None);
        }

        public static Statement CreateAddColumnStatement(string tableName, Column column)
        {
            var writer = new SqlWriter();
            writer.Alter.Table.QuotedName(tableName).Add.Flush();
            WriteColumnDefinition(writer, column);
            return new Statement(writer.ToString(), Statement.StatementType.Text, Statement.ResultType.None);
        }

        private static void WriteColumnDefinition(SqlWriter writer, Column column)
        {
            writer.ColumnDefinition(column.Name, column.Type, column.SqlType, column.Length, column.Precision, 
                column.Scale, column.Key != Column.KeyType.None, column.IsIdentity, column.IsNullable, 
                column.IsAutoGenerated, column.Computation, column.ComputationPersisted, column.DefaultValue);
        }

        public static Statement CreateRemoveColumnStatement(string tableName, string columnName)
        {
            var writer = SqlWriter.CreateWriter().IfColumnExists(tableName, columnName).Alter.Table.QuotedName(tableName).Drop.Column.QuotedName(columnName);
            return new Statement(writer.ToString(), Statement.StatementType.Text, Statement.ResultType.None);
        }

        public static Statement CreateGetIndexesStatement(string tableName)
        {
            var writer = new SqlWriter();
            writer.Select.
                Write(System.Indexes.Aliased.Name).Comma.
                Write(System.Indexes.Aliased.Type).Comma.
                Write(System.Indexes.Aliased.IsUnique).Comma.
                Write(System.Indexes.Aliased.IsPrimaryKey).Comma.
                Write(System.Columns.Aliased.Name).As.Write(SqlWriter.Aliases.ColumnName).Comma.
                Write(System.IndexColumns.Aliased.IsDescendingKey).
                From.Write(System.Indexes.TableName).Write(System.Indexes.TableAlias).
                    Join.Write(System.IndexColumns.TableName).Write(System.IndexColumns.TableAlias).On.
                        Write(System.Indexes.Aliased.ObjectId).Equal.Write(System.IndexColumns.Aliased.ObjectId).And.
                        Write(System.Indexes.Aliased.IndexId).Equal.Write(System.IndexColumns.Aliased.IndexId).
                    Join.Write(System.Columns.TableName).Write(System.Columns.TableAlias).On.
                        Write(System.IndexColumns.Aliased.ObjectId).Equal.Write(System.Columns.Aliased.ObjectId).And.
                        Write(System.IndexColumns.Aliased.ColumnId).Equal.Write(System.Columns.Aliased.ColumnId).
                Where.Write(System.Indexes.Aliased.ObjectId).Equal.ObjectId(tableName).
                OrderBy.Write(System.Indexes.Aliased.Name).Flush();
            return new Statement(writer.ToString(), Statement.StatementType.Text, Statement.ResultType.Multiple);
        }

        public static Statement CreateAddNonClusteredIndexStatement(string tableName, params string[] columnNames)
        {
            var indexName = string.Format("IX_{0}_{1}", tableName, string.Join("_", columnNames));
            var writer = SqlWriter.CreateWriter().Create.NonClustered.Index.QuotedName(indexName).On.QuotedName(tableName).OpenBlock.Trim();
            var first = true;
            foreach (var columnName in columnNames)
            {
                if (!first) writer.Trim().Comma.Flush();
                writer.QuotedName(columnName).Ascending.Flush();
                first = false;
            }
            writer.Trim().CloseBlock.Flush();
            return new Statement(writer.ToString(), Statement.StatementType.Text, Statement.ResultType.None);
        }

        public static Statement CreateRemoveNonClusteredIndexStatement(string tableName, string indexName)
        {
            var writer = SqlWriter.CreateWriter().IfIndexExists(tableName, indexName).Drop.Index.QuotedName(indexName).On.QuotedName(tableName);
            return new Statement(writer.ToString(), Statement.StatementType.Text, Statement.ResultType.None);
        }
    }
}

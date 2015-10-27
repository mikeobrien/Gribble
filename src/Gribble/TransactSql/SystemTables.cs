namespace Gribble.TransactSql
{
    public static class System
    {
        public static string Alias(this string column, string alias)
        {
            return $"{alias}.[{column}]";
        }

        public static class Objects
        {
            public static string TableName = "[sys].[objects]";
            public static string Name = "name";
            public static string Type = "type";
        }

        public static class Tables
        {
            public static string TableName = "[sys].[tables]";
            public static string TableAlias = "[ST]";

            public static string CreateDate = "create_date";
            public static string FilestreamDataSpaceId = "filestream_data_space_id";
            public static string HasReplicationFilter = "has_replication_filter";
            public static string HasUncheckedAssemblyData = "has_unchecked_assembly_data";
            public static string IsMergePublished = "is_merge_published";
            public static string IsMsShipped = "is_ms_shipped";
            public static string IsPublished = "is_published";
            public static string IsReplicated = "is_replicated";
            public static string IsSchemaPublished = "is_schema_published";
            public static string IsSyncTranSubscribed = "is_sync_tran_subscribed";
            public static string IsTrackedByCdc = "is_tracked_by_cdc";
            public static string LargeValueTypesOutOfRow = "large_value_types_out_of_row";
            public static string LobDataSpaceId = "lob_data_space_id";
            public static string LockEscalation = "lock_escalation";
            public static string LockEscalationDesc = "lock_escalation_desc";
            public static string LockOnBulkLoad = "lock_on_bulk_load";
            public static string MaxColumnIdUsed = "max_column_id_used";
            public static string ModifyDate = "modify_date";
            public static string Name = "name";
            public static string ObjectId = "object_id";
            public static string ParentObjectId = "parent_object_id";
            public static string PrincipalId = "principal_id";
            public static string SchemaId = "schema_id";
            public static string TextInRowLimit = "text_in_row_limit";
            public static string Type = "type";
            public static string TypeDesc = "type_desc";
            public static string UsesAnsiNulls = "uses_ansi_nulls";

            public static class Aliased
            {
                public static string CreateDate = Tables.CreateDate.Alias(TableAlias);
                public static string FilestreamDataSpaceId = Tables.FilestreamDataSpaceId.Alias(TableAlias);
                public static string HasReplicationFilter = Tables.HasReplicationFilter.Alias(TableAlias);
                public static string HasUncheckedAssemblyData = Tables.HasUncheckedAssemblyData.Alias(TableAlias);
                public static string IsMergePublished = Tables.IsMergePublished.Alias(TableAlias);
                public static string IsMsShipped = Tables.IsMsShipped.Alias(TableAlias);
                public static string IsPublished = Tables.IsPublished.Alias(TableAlias);
                public static string IsReplicated = Tables.IsReplicated.Alias(TableAlias);
                public static string IsSchemaPublished = Tables.IsSchemaPublished.Alias(TableAlias);
                public static string IsSyncTranSubscribed = Tables.IsSyncTranSubscribed.Alias(TableAlias);
                public static string IsTrackedByCdc = Tables.IsTrackedByCdc.Alias(TableAlias);
                public static string LargeValueTypesOutOfRow = Tables.LargeValueTypesOutOfRow.Alias(TableAlias);
                public static string LobDataSpaceId = Tables.LobDataSpaceId.Alias(TableAlias);
                public static string LockEscalation = Tables.LockEscalation.Alias(TableAlias);
                public static string LockEscalationDesc = Tables.LockEscalationDesc.Alias(TableAlias);
                public static string LockOnBulkLoad = Tables.LockOnBulkLoad.Alias(TableAlias);
                public static string MaxColumnIdUsed = Tables.MaxColumnIdUsed.Alias(TableAlias);
                public static string ModifyDate = Tables.ModifyDate.Alias(TableAlias);
                public static string Name = Tables.Name.Alias(TableAlias);
                public static string ObjectId = Tables.ObjectId.Alias(TableAlias);
                public static string ParentObjectId = Tables.ParentObjectId.Alias(TableAlias);
                public static string PrincipalId = Tables.PrincipalId.Alias(TableAlias);
                public static string SchemaId = Tables.SchemaId.Alias(TableAlias);
                public static string TextInRowLimit = Tables.TextInRowLimit.Alias(TableAlias);
                public static string Type = Tables.Type.Alias(TableAlias);
                public static string TypeDesc = Tables.TypeDesc.Alias(TableAlias);
                public static string UsesAnsiNulls = Tables.UsesAnsiNulls.Alias(TableAlias);
            }
        }

        public static class Columns
        {
            public static string TableName = "[sys].[columns]";
            public static string TableAlias = "[SC]";

            public static string CollationName = "collation_name";
            public static string ColumnId = "column_id";
            public static string DefaultObjectId = "default_object_id";
            public static string IsAnsiPadded = "is_ansi_padded";
            public static string IsColumnSet = "is_column_set";
            public static string IsComputed = "is_computed";
            public static string IsDtsReplicated = "is_dts_replicated";
            public static string IsFilestream = "is_filestream";
            public static string IsIdentity = "is_identity";
            public static string IsMergePublished = "is_merge_published";
            public static string IsNonSqlSubscribed = "is_non_sql_subscribed";
            public static string IsNullable = "is_nullable";
            public static string IsReplicated = "is_replicated";
            public static string IsRowguidcol = "is_rowguidcol";
            public static string IsSparse = "is_sparse";
            public static string IsXmlDocument = "is_xml_document";
            public static string MaxLength = "max_length";
            public static string Name = "name";
            public static string ObjectId = "object_id";
            public static string Precision = "precision";
            public static string RuleObjectId = "rule_object_id";
            public static string Scale = "scale";
            public static string SystemTypeId = "system_type_id";
            public static string UserTypeId = "user_type_id";
            public static string XmlCollectionId = "xml_collection_id";

            public static class Aliased
            {
                public static string CollationName = Columns.CollationName.Alias(TableAlias);
                public static string ColumnId = Columns.ColumnId.Alias(TableAlias);
                public static string DefaultObjectId = Columns.DefaultObjectId.Alias(TableAlias);
                public static string IsAnsiPadded = Columns.IsAnsiPadded.Alias(TableAlias);
                public static string IsColumnSet = Columns.IsColumnSet.Alias(TableAlias);
                public static string IsComputed = Columns.IsComputed.Alias(TableAlias);
                public static string IsDtsReplicated = Columns.IsDtsReplicated.Alias(TableAlias);
                public static string IsFilestream = Columns.IsFilestream.Alias(TableAlias);
                public static string IsIdentity = Columns.IsIdentity.Alias(TableAlias);
                public static string IsMergePublished = Columns.IsMergePublished.Alias(TableAlias);
                public static string IsNonSqlSubscribed = Columns.IsNonSqlSubscribed.Alias(TableAlias);
                public static string IsNullable = Columns.IsNullable.Alias(TableAlias);
                public static string IsReplicated = Columns.IsReplicated.Alias(TableAlias);
                public static string IsRowguidcol = Columns.IsRowguidcol.Alias(TableAlias);
                public static string IsSparse = Columns.IsSparse.Alias(TableAlias);
                public static string IsXmlDocument = Columns.IsXmlDocument.Alias(TableAlias);
                public static string MaxLength = Columns.MaxLength.Alias(TableAlias);
                public static string Name = Columns.Name.Alias(TableAlias);
                public static string ObjectId = Columns.ObjectId.Alias(TableAlias);
                public static string Precision = Columns.Precision.Alias(TableAlias);
                public static string RuleObjectId = Columns.RuleObjectId.Alias(TableAlias);
                public static string Scale = Columns.Scale.Alias(TableAlias);
                public static string SystemTypeId = Columns.SystemTypeId.Alias(TableAlias);
                public static string UserTypeId = Columns.UserTypeId.Alias(TableAlias);
                public static string XmlCollectionId = Columns.XmlCollectionId.Alias(TableAlias);
            }
        }

        public static class IndexColumns
        {
            public static string TableName = "[sys].[index_columns]";
            public static string TableAlias = "[SIC]";

            public static string ColumnId = "column_id";
            public static string IndexColumnId = "index_column_id";
            public static string IndexId = "index_id";
            public static string IsDescendingKey = "is_descending_key";
            public static string IsIncludedColumn = "is_included_column";
            public static string KeyOrdinal = "key_ordinal";
            public static string ObjectId = "object_id";
            public static string PartitionOrdinal = "partition_ordinal";

            public static class Aliased
            {
                public static string ColumnId = IndexColumns.ColumnId.Alias(TableAlias);
                public static string IndexColumnId = IndexColumns.IndexColumnId.Alias(TableAlias);
                public static string IndexId = IndexColumns.IndexId.Alias(TableAlias);
                public static string IsDescendingKey = IndexColumns.IsDescendingKey.Alias(TableAlias);
                public static string IsIncludedColumn = IndexColumns.IsIncludedColumn.Alias(TableAlias);
                public static string KeyOrdinal = IndexColumns.KeyOrdinal.Alias(TableAlias);
                public static string ObjectId = IndexColumns.ObjectId.Alias(TableAlias);
                public static string PartitionOrdinal = IndexColumns.PartitionOrdinal.Alias(TableAlias);
            }
        }

        public static class Indexes
        {
            public static string TableName = "[sys].[indexes]";
            public static string TableAlias = "[SI]";

            public static string AllowPageLocks = "allow_page_locks";
            public static string AllowRowLocks = "allow_row_locks";
            public static string DataSpaceId = "data_space_id";
            public static string FillFactor = "fill_factor";
            public static string FilterDefinition = "filter_definition";
            public static string HasFilter = "has_filter";
            public static string IgnoreDupKey = "ignore_dup_key";
            public static string IndexId = "index_id";
            public static string IsDisabled = "is_disabled";
            public static string IsHypothetical = "is_hypothetical";
            public static string IsPadded = "is_padded";
            public static string IsPrimaryKey = "is_primary_key";
            public static string IsUnique = "is_unique";
            public static string IsUniquestaticraint = "is_unique_staticraint";
            public static string Name = "name";
            public static string ObjectId = "object_id";
            public static string Type = "type";
            public static string TypeDesc = "type_desc";

            public static class Aliased
            {
                public static string AllowPageLocks = Indexes.AllowPageLocks.Alias(TableAlias);
                public static string AllowRowLocks = Indexes.AllowRowLocks.Alias(TableAlias);
                public static string DataSpaceId = Indexes.DataSpaceId.Alias(TableAlias);
                public static string FillFactor = Indexes.FillFactor.Alias(TableAlias);
                public static string FilterDefinition = Indexes.FilterDefinition.Alias(TableAlias);
                public static string HasFilter = Indexes.HasFilter.Alias(TableAlias);
                public static string IgnoreDupKey = Indexes.IgnoreDupKey.Alias(TableAlias);
                public static string IndexId = Indexes.IndexId.Alias(TableAlias);
                public static string IsDisabled = Indexes.IsDisabled.Alias(TableAlias);
                public static string IsHypothetical = Indexes.IsHypothetical.Alias(TableAlias);
                public static string IsPadded = Indexes.IsPadded.Alias(TableAlias);
                public static string IsPrimaryKey = Indexes.IsPrimaryKey.Alias(TableAlias);
                public static string IsUnique = Indexes.IsUnique.Alias(TableAlias);
                public static string IsUniquestaticraint = Indexes.IsUniquestaticraint.Alias(TableAlias);
                public static string Name = Indexes.Name.Alias(TableAlias);
                public static string ObjectId = Indexes.ObjectId.Alias(TableAlias);
                public static string Type = Indexes.Type.Alias(TableAlias);
                public static string TypeDesc = Indexes.TypeDesc.Alias(TableAlias);
            }
        }

        public static class ComputedColumns
        {
            public static string TableName = "[sys].[computed_columns]";
            public static string TableAlias = "[SCC]";

            public static string CollationName = "collation_name";
            public static string ColumnId = "column_id";
            public static string DefaultObjectId = "default_object_id";
            public static string Definition = "definition";
            public static string IsAnsiPadded = "is_ansi_padded";
            public static string IsColumnSet = "is_column_set";
            public static string IsComputed = "is_computed";
            public static string IsDtsReplicated = "is_dts_replicated";
            public static string IsFilestream = "is_filestream";
            public static string IsIdentity = "is_identity";
            public static string IsMergePublished = "is_merge_published";
            public static string IsNonSqlSubscribed = "is_non_sql_subscribed";
            public static string IsNullable = "is_nullable";
            public static string IsPersisted = "is_persisted";
            public static string IsReplicated = "is_replicated";
            public static string IsRowguidcol = "is_rowguidcol";
            public static string IsSparse = "is_sparse";
            public static string IsXmlDocument = "is_xml_document";
            public static string MaxLength = "max_length";
            public static string Name = "name";
            public static string ObjectId = "object_id";
            public static string Precision = "precision";
            public static string RuleObjectId = "rule_object_id";
            public static string Scale = "scale";
            public static string SystemTypeId = "system_type_id";
            public static string UserTypeId = "user_type_id";
            public static string UsesDatabaseCollation = "uses_database_collation";
            public static string XmlCollectionId = "xml_collection_id";

            public static class Aliased
            {
                public static string CollationName = ComputedColumns.CollationName.Alias(TableAlias);
                public static string ColumnId = ComputedColumns.ColumnId.Alias(TableAlias);
                public static string DefaultObjectId = ComputedColumns.DefaultObjectId.Alias(TableAlias);
                public static string Definition = ComputedColumns.Definition.Alias(TableAlias);
                public static string IsAnsiPadded = ComputedColumns.IsAnsiPadded.Alias(TableAlias);
                public static string IsColumnSet = ComputedColumns.IsColumnSet.Alias(TableAlias);
                public static string IsComputed = ComputedColumns.IsComputed.Alias(TableAlias);
                public static string IsDtsReplicated = ComputedColumns.IsDtsReplicated.Alias(TableAlias);
                public static string IsFilestream = ComputedColumns.IsFilestream.Alias(TableAlias);
                public static string IsIdentity = ComputedColumns.IsIdentity.Alias(TableAlias);
                public static string IsMergePublished = ComputedColumns.IsMergePublished.Alias(TableAlias);
                public static string IsNonSqlSubscribed = ComputedColumns.IsNonSqlSubscribed.Alias(TableAlias);
                public static string IsNullable = ComputedColumns.IsNullable.Alias(TableAlias);
                public static string IsPersisted = ComputedColumns.IsPersisted.Alias(TableAlias);
                public static string IsReplicated = ComputedColumns.IsReplicated.Alias(TableAlias);
                public static string IsRowguidcol = ComputedColumns.IsRowguidcol.Alias(TableAlias);
                public static string IsSparse = ComputedColumns.IsSparse.Alias(TableAlias);
                public static string IsXmlDocument = ComputedColumns.IsXmlDocument.Alias(TableAlias);
                public static string MaxLength = ComputedColumns.MaxLength.Alias(TableAlias);
                public static string Name = ComputedColumns.Name.Alias(TableAlias);
                public static string ObjectId = ComputedColumns.ObjectId.Alias(TableAlias);
                public static string Precision = ComputedColumns.Precision.Alias(TableAlias);
                public static string RuleObjectId = ComputedColumns.RuleObjectId.Alias(TableAlias);
                public static string Scale = ComputedColumns.Scale.Alias(TableAlias);
                public static string SystemTypeId = ComputedColumns.SystemTypeId.Alias(TableAlias);
                public static string UserTypeId = ComputedColumns.UserTypeId.Alias(TableAlias);
                public static string UsesDatabaseCollation = ComputedColumns.UsesDatabaseCollation.Alias(TableAlias);
                public static string XmlCollectionId = ComputedColumns.XmlCollectionId.Alias(TableAlias);
            }
        }
    }
}
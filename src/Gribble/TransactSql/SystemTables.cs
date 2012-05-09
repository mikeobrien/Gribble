namespace Gribble.TransactSql
{
    public static class System
    {
        public static class Tables
        {
            public const string TableName = "[sys].[tables]";
            public const string TableAlias = "[ST]";

            public const string CreateDate = "[create_date]";
            public const string FilestreamDataSpaceId = "[filestream_data_space_id]";
            public const string HasReplicationFilter = "[has_replication_filter]";
            public const string HasUncheckedAssemblyData = "[has_unchecked_assembly_data]";
            public const string IsMergePublished = "[is_merge_published]";
            public const string IsMsShipped = "[is_ms_shipped]";
            public const string IsPublished = "[is_published]";
            public const string IsReplicated = "[is_replicated]";
            public const string IsSchemaPublished = "[is_schema_published]";
            public const string IsSyncTranSubscribed = "[is_sync_tran_subscribed]";
            public const string IsTrackedByCdc = "[is_tracked_by_cdc]";
            public const string LargeValueTypesOutOfRow = "[large_value_types_out_of_row]";
            public const string LobDataSpaceId = "[lob_data_space_id]";
            public const string LockEscalation = "[lock_escalation]";
            public const string LockEscalationDesc = "[lock_escalation_desc]";
            public const string LockOnBulkLoad = "[lock_on_bulk_load]";
            public const string MaxColumnIdUsed = "[max_column_id_used]";
            public const string ModifyDate = "[modify_date]";
            public const string Name = "[name]";
            public const string ObjectId = "[object_id]";
            public const string ParentObjectId = "[parent_object_id]";
            public const string PrincipalId = "[principal_id]";
            public const string SchemaId = "[schema_id]";
            public const string TextInRowLimit = "[text_in_row_limit]";
            public const string Type = "[type]";
            public const string TypeDesc = "[type_desc]";
            public const string UsesAnsiNulls = "[uses_ansi_nulls]";

            public static class Aliased
            {
                public const string CreateDate = TableAlias + "." + Tables.CreateDate;
                public const string FilestreamDataSpaceId = TableAlias + "." + Tables.FilestreamDataSpaceId;
                public const string HasReplicationFilter = TableAlias + "." + Tables.HasReplicationFilter;
                public const string HasUncheckedAssemblyData = TableAlias + "." + Tables.HasUncheckedAssemblyData;
                public const string IsMergePublished = TableAlias + "." + Tables.IsMergePublished;
                public const string IsMsShipped = TableAlias + "." + Tables.IsMsShipped;
                public const string IsPublished = TableAlias + "." + Tables.IsPublished;
                public const string IsReplicated = TableAlias + "." + Tables.IsReplicated;
                public const string IsSchemaPublished = TableAlias + "." + Tables.IsSchemaPublished;
                public const string IsSyncTranSubscribed = TableAlias + "." + Tables.IsSyncTranSubscribed;
                public const string IsTrackedByCdc = TableAlias + "." + Tables.IsTrackedByCdc;
                public const string LargeValueTypesOutOfRow = TableAlias + "." + Tables.LargeValueTypesOutOfRow;
                public const string LobDataSpaceId = TableAlias + "." + Tables.LobDataSpaceId;
                public const string LockEscalation = TableAlias + "." + Tables.LockEscalation;
                public const string LockEscalationDesc = TableAlias + "." + Tables.LockEscalationDesc;
                public const string LockOnBulkLoad = TableAlias + "." + Tables.LockOnBulkLoad;
                public const string MaxColumnIdUsed = TableAlias + "." + Tables.MaxColumnIdUsed;
                public const string ModifyDate = TableAlias + "." + Tables.ModifyDate;
                public const string Name = TableAlias + "." + Tables.Name;
                public const string ObjectId = TableAlias + "." + Tables.ObjectId;
                public const string ParentObjectId = TableAlias + "." + Tables.ParentObjectId;
                public const string PrincipalId = TableAlias + "." + Tables.PrincipalId;
                public const string SchemaId = TableAlias + "." + Tables.SchemaId;
                public const string TextInRowLimit = TableAlias + "." + Tables.TextInRowLimit;
                public const string Type = TableAlias + "." + Tables.Type;
                public const string TypeDesc = TableAlias + "." + Tables.TypeDesc;
                public const string UsesAnsiNulls = TableAlias + "." + Tables.UsesAnsiNulls;
            }
        }

        public static class Columns
        {
            public const string TableName = "[sys].[columns]";
            public const string TableAlias = "[SC]";

            public const string CollationName = "[collation_name]";
            public const string ColumnId = "[column_id]";
            public const string DefaultObjectId = "[default_object_id]";
            public const string IsAnsiPadded = "[is_ansi_padded]";
            public const string IsColumnSet = "[is_column_set]";
            public const string IsComputed = "[is_computed]";
            public const string IsDtsReplicated = "[is_dts_replicated]";
            public const string IsFilestream = "[is_filestream]";
            public const string IsIdentity = "[is_identity]";
            public const string IsMergePublished = "[is_merge_published]";
            public const string IsNonSqlSubscribed = "[is_non_sql_subscribed]";
            public const string IsNullable = "[is_nullable]";
            public const string IsReplicated = "[is_replicated]";
            public const string IsRowguidcol = "[is_rowguidcol]";
            public const string IsSparse = "[is_sparse]";
            public const string IsXmlDocument = "[is_xml_document]";
            public const string MaxLength = "[max_length]";
            public const string Name = "[name]";
            public const string ObjectId = "[object_id]";
            public const string Precision = "[precision]";
            public const string RuleObjectId = "[rule_object_id]";
            public const string Scale = "[scale]";
            public const string SystemTypeId = "[system_type_id]";
            public const string UserTypeId = "[user_type_id]";
            public const string XmlCollectionId = "[xml_collection_id]";

            public static class Aliased
            {
                public const string CollationName = TableAlias + "." + Columns.CollationName;
                public const string ColumnId = TableAlias + "." + Columns.ColumnId;
                public const string DefaultObjectId = TableAlias + "." + Columns.DefaultObjectId;
                public const string IsAnsiPadded = TableAlias + "." + Columns.IsAnsiPadded;
                public const string IsColumnSet = TableAlias + "." + Columns.IsColumnSet;
                public const string IsComputed = TableAlias + "." + Columns.IsComputed;
                public const string IsDtsReplicated = TableAlias + "." + Columns.IsDtsReplicated;
                public const string IsFilestream = TableAlias + "." + Columns.IsFilestream;
                public const string IsIdentity = TableAlias + "." + Columns.IsIdentity;
                public const string IsMergePublished = TableAlias + "." + Columns.IsMergePublished;
                public const string IsNonSqlSubscribed = TableAlias + "." + Columns.IsNonSqlSubscribed;
                public const string IsNullable = TableAlias + "." + Columns.IsNullable;
                public const string IsReplicated = TableAlias + "." + Columns.IsReplicated;
                public const string IsRowguidcol = TableAlias + "." + Columns.IsRowguidcol;
                public const string IsSparse = TableAlias + "." + Columns.IsSparse;
                public const string IsXmlDocument = TableAlias + "." + Columns.IsXmlDocument;
                public const string MaxLength = TableAlias + "." + Columns.MaxLength;
                public const string Name = TableAlias + "." + Columns.Name;
                public const string ObjectId = TableAlias + "." + Columns.ObjectId;
                public const string Precision = TableAlias + "." + Columns.Precision;
                public const string RuleObjectId = TableAlias + "." + Columns.RuleObjectId;
                public const string Scale = TableAlias + "." + Columns.Scale;
                public const string SystemTypeId = TableAlias + "." + Columns.SystemTypeId;
                public const string UserTypeId = TableAlias + "." + Columns.UserTypeId;
                public const string XmlCollectionId = TableAlias + "." + Columns.XmlCollectionId;
            }
        }

        public static class IndexColumns
        {
            public const string TableName = "[sys].[index_columns]";
            public const string TableAlias = "[SIC]";

            public const string ColumnId = "[column_id]";
            public const string IndexColumnId = "[index_column_id]";
            public const string IndexId = "[index_id]";
            public const string IsDescendingKey = "[is_descending_key]";
            public const string IsIncludedColumn = "[is_included_column]";
            public const string KeyOrdinal = "[key_ordinal]";
            public const string ObjectId = "[object_id]";
            public const string PartitionOrdinal = "[partition_ordinal]";

            public static class Aliased
            {
                public const string ColumnId = TableAlias + "." + IndexColumns.ColumnId;
                public const string IndexColumnId = TableAlias + "." + IndexColumns.IndexColumnId;
                public const string IndexId = TableAlias + "." + IndexColumns.IndexId;
                public const string IsDescendingKey = TableAlias + "." + IndexColumns.IsDescendingKey;
                public const string IsIncludedColumn = TableAlias + "." + IndexColumns.IsIncludedColumn;
                public const string KeyOrdinal = TableAlias + "." + IndexColumns.KeyOrdinal;
                public const string ObjectId = TableAlias + "." + IndexColumns.ObjectId;
                public const string PartitionOrdinal = TableAlias + "." + IndexColumns.PartitionOrdinal;
            }
        }

        public static class Indexes
        {
            public const string TableName = "[sys].[indexes]";
            public const string TableAlias = "[SI]";

            public const string AllowPageLocks = "[allow_page_locks]";
            public const string AllowRowLocks = "[allow_row_locks]";
            public const string DataSpaceId = "[data_space_id]";
            public const string FillFactor = "[fill_factor]";
            public const string FilterDefinition = "[filter_definition]";
            public const string HasFilter = "[has_filter]";
            public const string IgnoreDupKey = "[ignore_dup_key]";
            public const string IndexId = "[index_id]";
            public const string IsDisabled = "[is_disabled]";
            public const string IsHypothetical = "[is_hypothetical]";
            public const string IsPadded = "[is_padded]";
            public const string IsPrimaryKey = "[is_primary_key]";
            public const string IsUnique = "[is_unique]";
            public const string IsUniqueConstraint = "[is_unique_constraint]";
            public const string Name = "[name]";
            public const string ObjectId = "[object_id]";
            public const string Type = "[type]";
            public const string TypeDesc = "[type_desc]";

            public static class Aliased
            {
                public const string AllowPageLocks = TableAlias + "." + Indexes.AllowPageLocks;
                public const string AllowRowLocks = TableAlias + "." + Indexes.AllowRowLocks;
                public const string DataSpaceId = TableAlias + "." + Indexes.DataSpaceId;
                public const string FillFactor = TableAlias + "." + Indexes.FillFactor;
                public const string FilterDefinition = TableAlias + "." + Indexes.FilterDefinition;
                public const string HasFilter = TableAlias + "." + Indexes.HasFilter;
                public const string IgnoreDupKey = TableAlias + "." + Indexes.IgnoreDupKey;
                public const string IndexId = TableAlias + "." + Indexes.IndexId;
                public const string IsDisabled = TableAlias + "." + Indexes.IsDisabled;
                public const string IsHypothetical = TableAlias + "." + Indexes.IsHypothetical;
                public const string IsPadded = TableAlias + "." + Indexes.IsPadded;
                public const string IsPrimaryKey = TableAlias + "." + Indexes.IsPrimaryKey;
                public const string IsUnique = TableAlias + "." + Indexes.IsUnique;
                public const string IsUniqueConstraint = TableAlias + "." + Indexes.IsUniqueConstraint;
                public const string Name = TableAlias + "." + Indexes.Name;
                public const string ObjectId = TableAlias + "." + Indexes.ObjectId;
                public const string Type = TableAlias + "." + Indexes.Type;
                public const string TypeDesc = TableAlias + "." + Indexes.TypeDesc;
            }
        }

        public static class ComputedColumns
        {
            public const string TableName = "[sys].[computed_columns]";
            public const string TableAlias = "[SCC]";

            public const string CollationName = "[collation_name]";
            public const string ColumnId = "[column_id]";
            public const string DefaultObjectId = "[default_object_id]";
            public const string Definition = "[definition]";
            public const string IsAnsiPadded = "[is_ansi_padded]";
            public const string IsColumnSet = "[is_column_set]";
            public const string IsComputed = "[is_computed]";
            public const string IsDtsReplicated = "[is_dts_replicated]";
            public const string IsFilestream = "[is_filestream]";
            public const string IsIdentity = "[is_identity]";
            public const string IsMergePublished = "[is_merge_published]";
            public const string IsNonSqlSubscribed = "[is_non_sql_subscribed]";
            public const string IsNullable = "[is_nullable]";
            public const string IsPersisted = "[is_persisted]";
            public const string IsReplicated = "[is_replicated]";
            public const string IsRowguidcol = "[is_rowguidcol]";
            public const string IsSparse = "[is_sparse]";
            public const string IsXmlDocument = "[is_xml_document]";
            public const string MaxLength = "[max_length]";
            public const string Name = "[name]";
            public const string ObjectId = "[object_id]";
            public const string Precision = "[precision]";
            public const string RuleObjectId = "[rule_object_id]";
            public const string Scale = "[scale]";
            public const string SystemTypeId = "[system_type_id]";
            public const string UserTypeId = "[user_type_id]";
            public const string UsesDatabaseCollation = "[uses_database_collation]";
            public const string XmlCollectionId = "[xml_collection_id]";

            public static class Aliased
            {
                public const string CollationName = TableAlias + "." + ComputedColumns.CollationName;
                public const string ColumnId = TableAlias + "." + ComputedColumns.ColumnId;
                public const string DefaultObjectId = TableAlias + "." + ComputedColumns.DefaultObjectId;
                public const string Definition = TableAlias + "." + ComputedColumns.Definition;
                public const string IsAnsiPadded = TableAlias + "." + ComputedColumns.IsAnsiPadded;
                public const string IsColumnSet = TableAlias + "." + ComputedColumns.IsColumnSet;
                public const string IsComputed = TableAlias + "." + ComputedColumns.IsComputed;
                public const string IsDtsReplicated = TableAlias + "." + ComputedColumns.IsDtsReplicated;
                public const string IsFilestream = TableAlias + "." + ComputedColumns.IsFilestream;
                public const string IsIdentity = TableAlias + "." + ComputedColumns.IsIdentity;
                public const string IsMergePublished = TableAlias + "." + ComputedColumns.IsMergePublished;
                public const string IsNonSqlSubscribed = TableAlias + "." + ComputedColumns.IsNonSqlSubscribed;
                public const string IsNullable = TableAlias + "." + ComputedColumns.IsNullable;
                public const string IsPersisted = TableAlias + "." + ComputedColumns.IsPersisted;
                public const string IsReplicated = TableAlias + "." + ComputedColumns.IsReplicated;
                public const string IsRowguidcol = TableAlias + "." + ComputedColumns.IsRowguidcol;
                public const string IsSparse = TableAlias + "." + ComputedColumns.IsSparse;
                public const string IsXmlDocument = TableAlias + "." + ComputedColumns.IsXmlDocument;
                public const string MaxLength = TableAlias + "." + ComputedColumns.MaxLength;
                public const string Name = TableAlias + "." + ComputedColumns.Name;
                public const string ObjectId = TableAlias + "." + ComputedColumns.ObjectId;
                public const string Precision = TableAlias + "." + ComputedColumns.Precision;
                public const string RuleObjectId = TableAlias + "." + ComputedColumns.RuleObjectId;
                public const string Scale = TableAlias + "." + ComputedColumns.Scale;
                public const string SystemTypeId = TableAlias + "." + ComputedColumns.SystemTypeId;
                public const string UserTypeId = TableAlias + "." + ComputedColumns.UserTypeId;
                public const string UsesDatabaseCollation = TableAlias + "." + ComputedColumns.UsesDatabaseCollation;
                public const string XmlCollectionId = TableAlias + "." + ComputedColumns.XmlCollectionId;
            }
        }
    }
}
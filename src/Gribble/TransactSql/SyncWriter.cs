using System.Collections.Generic;
using System.Linq;
using Gribble.Mapping;
using Gribble.Model;

namespace Gribble.TransactSql
{
    public static class SyncWriter<TEntity>
    {
        public static Statement CreateStatement(Sync sync, IEntityMapping mapping)
        {
            var writer = new SqlWriter();
            IDictionary<string, object> parameters = new Dictionary<string, object>();
            var fields = sync.Target.Projection.Zip(sync.Source.Projection, (t, s) => new { Target = t.Projection, Source = s.Projection })
                .Where(x => !sync.ExcludedFields.Any(y => (y.HasKey ? y.Key : y.Name) == (x.Target.Field.HasKey ? x.Target.Field.Key : x.Target.Field.Name))).ToList();

            writer.Update.QuotedName(sync.Target.From.Alias).Set
                .ExpressionList(x => x.Comma.Flush(), fields, (f, s) => s.
                    Write(ProjectionWriter<TEntity>.CreateStatement(f.Target, mapping).MergeParameters(parameters).Text).Equal.
                    Write(ProjectionWriter<TEntity>.CreateStatement(f.Source, mapping).MergeParameters(parameters).Text))
                .From.QuotedName(sync.Target.From.Table.Name).QuotedName(sync.Target.From.Alias).Inner.Join
                    .QuotedName(sync.Source.From.Table.Name).QuotedName(sync.Source.From.Alias)
                    .On.Write(ProjectionWriter<TEntity>.CreateStatement(sync.TargetKey, mapping).MergeParameters(parameters).Text).Equal
                       .Write(ProjectionWriter<TEntity>.CreateStatement(sync.SourceKey, mapping).MergeParameters(parameters).Text)
                    .And.Write(WhereWriter<TEntity>.CreateStatement(sync.Source.Where, mapping).MergeParameters(parameters).Text)
                .Where.Write(WhereWriter<TEntity>.CreateStatement(sync.Target.Where, mapping).MergeParameters(parameters).Text);

            return new Statement(writer.ToString(), Statement.StatementType.Text, Statement.ResultType.None, parameters);
        }
    }
}

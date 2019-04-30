using System;
using System.Collections.Generic;
using Gribble.Mapping;
using Gribble.Model;

namespace Gribble.TransactSql
{
    public class ProjectionWriter<TEntity>
    {
        private SqlWriter _sql;
        private Dictionary<string, object> _parameters;
        private readonly IEntityMapping _mapping;

        public ProjectionWriter(IEntityMapping mapping)
        {
            _mapping = mapping;
        }

        public static Statement CreateStatement(Projection projection, IEntityMapping mapping)
        {
            var writer = new ProjectionWriter<TEntity>(mapping);
            return writer.Write(projection);
        }

        public static Statement CreateStatement(Field field, IEntityMapping mapping)
        {
            var writer = new ProjectionWriter<TEntity>(mapping);
            return writer.Write(field);
        }

        public Statement Write(Projection projection)
        {
            return Write(() => VisitProjection(projection));
        }

        public Statement Write(Field field)
        {
            return Write(() => VisitField(field));
        }

        private Statement Write(Action write)
        {
            _sql = SqlWriter.CreateWriter();
            _parameters = new Dictionary<string, object>();
            write();
            return new Statement(_sql.ToString(), Statement.StatementType.Text, _parameters);
        }

        private void VisitProjection(Projection projection)
        {
            switch (projection.Type)
            {
                case Projection.ProjectionType.Constant: VisitConstant(projection.Constant); break;
                case Projection.ProjectionType.Field: VisitField(projection.Field); break;
                case Projection.ProjectionType.Function: VisitFunction(projection.Function); break;
                case Projection.ProjectionType.Wildcard: _sql.Wildcard.Flush(); break;
            }
        }

        private void VisitFunction(Function function)
        {
            switch (function.Type)
            {
                case Function.FunctionType.Coalesce:
                    _sql.Coalesce(x => VisitProjection(function.Coalesce.First),
                                  x => VisitProjection(function.Coalesce.Second));
                    break;
                case Function.FunctionType.Convert:
                    _sql.Cast(x => VisitProjection(function.Convert.Value), function.Convert.Type, 0, null, null);
                    break;
                case Function.FunctionType.IndexOf:
                    _sql.IndexOf(x => VisitProjection(function.IndexOf.Text),
                                 x => VisitProjection(function.IndexOf.Value));
                    break;
                case Function.FunctionType.IndexOfAt:
                    _sql.IndexOf(x => VisitProjection(function.IndexOfAt.Text),
                                 x => VisitProjection(function.IndexOfAt.Value),
                                 x => VisitProjection(function.IndexOfAt.Start));
                    break;
                case Function.FunctionType.Insert:
                    _sql.Insert(x => VisitProjection(function.Insert.Text),
                                x => VisitProjection(function.Insert.Value),
                                x => VisitProjection(function.Insert.Start));
                    break;
                case Function.FunctionType.Length:
                    _sql.Length(x => VisitProjection(function.Length.Text));
                    break;
                case Function.FunctionType.Replace:
                    _sql.Replace(x => VisitProjection(function.Replace.Text),
                                 x => VisitProjection(function.Replace.SearchValue),
                                 x => VisitProjection(function.Replace.ReplaceValue));
                    break;
                case Function.FunctionType.Substring:
                    _sql.Substring(x => VisitProjection(function.Substring.Text),
                                   x => VisitProjection(function.Substring.Start));
                    break;
                case Function.FunctionType.SubstringFixed:
                    _sql.Substring(x => VisitProjection(function.SubstringFixed.Text),
                                   x => VisitProjection(function.SubstringFixed.Start),
                                   x => VisitProjection(function.SubstringFixed.Length));
                    break;
                case Function.FunctionType.ToLower:
                    _sql.ToLower(x => VisitProjection(function.ToLower.Text));
                    break;
                case Function.FunctionType.ToString:
                    _sql.Cast(x => VisitProjection(function.ToString.Value), typeof(string), 0, null, null);
                    break;
                case Function.FunctionType.ToUpper:
                    _sql.ToUpper(x => VisitProjection(function.ToUpper.Text));
                    break;
                case Function.FunctionType.Trim:
                    _sql.Trim(x => VisitProjection(function.Trim.Text));
                    break;
                case Function.FunctionType.TrimEnd:
                    _sql.RightTrim(x => VisitProjection(function.TrimEnd.Text));
                    break;
                case Function.FunctionType.TrimStart:
                    _sql.LeftTrim(x => VisitProjection(function.TrimStart.Text));
                    break;
                case Function.FunctionType.Hash:
                    _sql.Hash(x => VisitProjection(function.Hash.Value), function.Hash.Type == Function.HashParameters.HashType.Md5 ? 
                                                                            SqlWriter.HashAlgorithm.Md5 : SqlWriter.HashAlgorithm.Sha1);
                    break;
                case Function.FunctionType.ToHex:
                    _sql.ToHex(x => VisitProjection(function.ToHex.Value));
                    break;
            }
        }

        private void VisitConstant(Constant constant)
        {
            if (constant.Value == null) _sql.Null.Flush();
            else
            {
                if (!_parameters.ContainsKey(constant.Alias)) _parameters.Add(constant.Alias, constant.Value);
                _sql.Parameter(constant.Alias);
            }
        }

        private void VisitField(Field field)
        {
            if (field.HasTableAlias) _sql.QuotedName(field.TableAlias).Trim().Period.Trim();
            _sql.QuotedName(field.Map(_mapping));
        }
    }
}

﻿using System.Collections.Generic;
using System.Linq;

namespace Gribble.Model
{
    public enum TopValueType { Count, Percent }

    public class Select
    {
        public int Top;
        public TopValueType TopType;
        public bool HasTop => Top > 0;

        public int Start;
        public bool HasStart => Start > 0;

        public bool Single;

        public bool First;
        public bool FirstOrDefault;

        public bool Any;
        public bool Count;

        public bool Randomize;

        public bool HasProjection => Projection != null && Projection.Any();
        public IList<SelectProjection> Projection;

        public Data From = new Data { Type = Data.DataType.Table };

        public Operator Where;
        public bool HasWhere => Where != null;

        public Duplicates Duplicates;
        public bool HasDuplicates => Duplicates != null;

        public IList<Distinct> Distinct;
        public bool HasDistinct => Distinct != null && Distinct.Any();

        public IList<OrderBy> OrderBy;
        public bool HasOrderBy => OrderBy != null && OrderBy.Any();

        public IList<SetOperation> SetOperatons;
        public bool HasSetOperations => SetOperatons != null && SetOperatons.Any();
        public bool HasIntersections { get { return SetOperatons != null && SetOperatons.Any(x => x.Type == SetOperation.OperationType.Intersect); } }
        public bool HasCompliments { get { return SetOperatons != null && SetOperatons.Any(x => x.Type == SetOperation.OperationType.Compliment); } }

        public bool HasConditions => HasTop || HasStart || Single || First || FirstOrDefault || Count || Randomize || 
            HasProjection || HasWhere || HasDistinct || HasOrderBy || HasSetOperations;

        public IEnumerable<Select> GetSourceTables()
        {
            return GetSourceTables(this).Reverse();
        }

        private static IEnumerable<Select> GetSourceTables(Select select)
        {
            var tables = new List<Select>();
            if (select.From.Type == Data.DataType.Table) tables.Add(select);
            else if (select.From.HasQueries) tables.AddRange(select.From.Queries.SelectMany(GetSourceTables));
            return tables;
        }   
    }
}

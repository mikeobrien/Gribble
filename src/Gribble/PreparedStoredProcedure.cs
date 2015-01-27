using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gribble.Mapping;

namespace Gribble
{
    public class PreparedStoredProcedure
    {
        IStoredProcedure sp;
        public string Name { get; set; }
        public object Parameters { get; set; }
        public EntityMappingCollection mapping { get; set; }

        public PreparedStoredProcedure(GribbleSPList sp, EntityMappingCollection mapping = null)
        {
            this.sp = StoredProcedure.Create(sp.ConnectionManager, mapping ?? sp.EntityMapping);
        }

        public TReturn Execute<TReturn>()
        {
            return sp.Execute<TReturn>(Name, Parameters);
        }

        public int Execute()
        {
            return sp.Execute(Name, Parameters);
        }

        public T ExecuteScalar<T>()
        {
            return sp.ExecuteScalar<T>(Name, Parameters);
        }

        public TEntity ExecuteSingle<TEntity>()
        {
            return sp.ExecuteSingle<TEntity>(Name, Parameters);
        }

        public TEntity ExecuteSingleOrNone<TEntity>()
        {
            return sp.ExecuteSingleOrNone<TEntity>(Name, Parameters);
        }

        public IEnumerable<TEntity> ExecuteMany<TEntity>()
        {
            return sp.ExecuteMany<TEntity>(Name, Parameters);
        }
    }
}

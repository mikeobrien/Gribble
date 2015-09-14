using System;
using System.Collections.Generic;
using Gribble.Mapping;

namespace Gribble
{
    public class IntKeyEntityMap : ClassMap<Entity<int>>
    {
        public IntKeyEntityMap(string key)
        {
            Id(x => x.Id).Column(key);
            Map(x => x.Values).Dynamic();
        }
    }

    public class GuidKeyEntityMap : ClassMap<Entity<Guid>>
    {
        public GuidKeyEntityMap(string key)
        {
            Id(x => x.Id).Column(key);
            Map(x => x.Values).Dynamic();
        }
    }

    public class StringKeyEntityMap : ClassMap<Entity<string>>
    {
        public StringKeyEntityMap(string key)
        {
            Id(x => x.Id).Column(key);
            Map(x => x.Values).Dynamic();
        }
    }

    public class Entity<TKey>
    {
        public Entity()
        {
            Values = new Dictionary<string, object>();
        } 

        public TKey Id { get; set; }
        public IDictionary<string, object> Values { get; set; }
    }
}
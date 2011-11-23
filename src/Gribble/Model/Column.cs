﻿using System;

namespace Gribble.Model
{
    public class Column
    {
        public Column()
        {
            IsIdentity = false;
            IsPrimaryKey = false;
            Length = 0;
            IsAutoGenerated = false;
            IsNullable = false;
        }

        public string Name { get; set; }
        public Type Type { get; set; }
        public int Length { get; set; }
        public bool IsNullable { get; set; }
        public bool IsIdentity { get; set; }
        public bool IsAutoGenerated { get; set; }
        public object DefaultValue { get; set; }
        public bool IsPrimaryKey { get; set; }
    }
}
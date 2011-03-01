using System;

namespace Gribble.Statements
{
    public class Function
    {
        public enum FunctionType
        {
            StartsWith,
            Contains,
            EndsWith,
            ToUpper,
            ToLower,
            Trim,
            TrimEnd,
            TrimStart,
            ToString,
            Substring,
            SubstringFixed,
            Replace,
            Insert,
            IndexOf,
            IndexOfAt,
            Coalesce,
            Length,
            Convert,
            Hash,
            ToHex
        }

        public FunctionType Type;

        public StartsWithParameters StartsWith;
        public ContainsParameters Contains;
        public LengthParameters Length;
        public EndsWithParameters EndsWith;
        public ToUpperParameters ToUpper;
        public ToLowerParameters ToLower;
        public TrimParameters Trim;
        public TrimEndParameters TrimEnd;
        public TrimStartParameters TrimStart;
        public new ToStringParameters ToString;
        public SubstringParameters Substring;
        public SubstringFixedParameters SubstringFixed;
        public ReplaceParameters Replace;
        public InsertParameters Insert;
        public IndexOfParameters IndexOf;
        public IndexOfAtParameters IndexOfAt;
        public CoalesceParameters Coalesce;
        public ConvertParameters Convert;
        public HashParameters Hash;
        public ToHexParameters ToHex;

        public static class Create
        {
            public static Function StartsWith()
            { return new Function { Type = FunctionType.StartsWith, StartsWith = new StartsWithParameters() }; }

            public static Function Contains()
            { return new Function { Type = FunctionType.Contains, Contains = new ContainsParameters() }; }

            public static Function EndsWith()
            { return new Function { Type = FunctionType.EndsWith, EndsWith = new EndsWithParameters() }; }

            public static Function Length()
            { return new Function { Type = FunctionType.Length, Length = new LengthParameters() }; }

            public static Function ToUpper()
            { return new Function { Type = FunctionType.ToUpper, ToUpper = new ToUpperParameters() }; }

            public static Function ToLower()
            { return new Function { Type = FunctionType.ToLower, ToLower = new ToLowerParameters() }; }

            public static Function Trim()
            { return new Function { Type = FunctionType.Trim, Trim = new TrimParameters() }; }

            public static Function TrimEnd()
            { return new Function { Type = FunctionType.TrimEnd, TrimEnd = new TrimEndParameters() }; }

            public static Function TrimStart()
            { return new Function { Type = FunctionType.TrimStart, TrimStart = new TrimStartParameters() }; }

            public new static Function ToString()
            { return new Function { Type = FunctionType.ToString, ToString = new ToStringParameters() }; }

            public static Function Substring()
            { return new Function { Type = FunctionType.Substring, Substring = new SubstringParameters() }; }

            public static Function SubstringFixed()
            { return new Function { Type = FunctionType.SubstringFixed, SubstringFixed = new SubstringFixedParameters() }; }

            public static Function Replace()
            { return new Function { Type = FunctionType.Replace, Replace = new ReplaceParameters() }; }

            public static Function Insert()
            { return new Function { Type = FunctionType.Insert, Insert = new InsertParameters() }; }

            public static Function IndexOf()
            { return new Function { Type = FunctionType.IndexOf, IndexOf = new IndexOfParameters() }; }

            public static Function IndexOfAt()
            { return new Function { Type = FunctionType.IndexOfAt, IndexOfAt = new IndexOfAtParameters() }; }

            public static Function Coalesce()
            { return new Function { Type = FunctionType.Coalesce, Coalesce = new CoalesceParameters() }; }

            public static Function Convert(Type type)
            { return new Function { Type = FunctionType.Convert, Convert = new ConvertParameters { Type = type } }; }

            public static Function Hash(HashParameters.HashType type)
            { return new Function { Type = FunctionType.Hash, Hash = new HashParameters { Type = type } }; }

            public static Function ToHex()
            { return new Function { Type = FunctionType.ToHex, ToHex = new ToHexParameters() }; }
        }

        public class StartsWithParameters { public Projection Text; public Projection Value; }
        public class ContainsParameters { public Projection Text; public Projection Value; }
        public class EndsWithParameters { public Projection Text; public Projection Value; }
        public class LengthParameters { public Projection Text; }
        public class ToUpperParameters { public Projection Text; }
        public class ToLowerParameters { public Projection Text; }
        public class TrimParameters { public Projection Text; }
        public class TrimEndParameters { public Projection Text; }
        public class SubstringParameters { public Projection Text; public Projection Start; }
        public class TrimStartParameters { public Projection Text; }
        public class ToStringParameters { public Projection Value; }
        public class SubstringFixedParameters { public Projection Text; public Projection Start; public Projection Length; }
        public class ReplaceParameters { public Projection Text; public Projection SearchValue; public Projection ReplaceValue; }
        public class InsertParameters { public Projection Text; public Projection Start; public Projection Value; }
        public class IndexOfParameters { public Projection Text; public Projection Value; }
        public class IndexOfAtParameters { public Projection Text; public Projection Start; public Projection Value; }
        public class CoalesceParameters { public Projection First; public Projection Second; }
        public class ConvertParameters { public Projection Value; public Type Type; }
        public class ToHexParameters { public Projection Value; }
        public class HashParameters
        {
            public enum HashType { Md5, Sha1 }
            public Projection Value;
            public HashType Type;
        }
    }
}

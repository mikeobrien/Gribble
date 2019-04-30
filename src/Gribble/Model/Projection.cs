namespace Gribble.Model
{
    public class Projection
    {
        public enum ProjectionType
        {
            Field,
            Constant,
            Function,
            Wildcard
        }

        public ProjectionType Type;

        public Field Field;
        public Constant Constant;
        public Function Function;

        public static class Create
        {
            public static Projection Field(string name)
            { return new Projection { Type = ProjectionType.Field, Field = new Field { Name = name } }; }

            public static Projection Field(string name, bool isKey)
            { return new Projection { Type = ProjectionType.Field, Field = new Field { Name = !isKey ? name : null, HasKey = isKey, Key = isKey ? name : null } }; }

            public static Projection Field(string name, string tableAlias)
            { return new Projection { Type = ProjectionType.Field, Field = new Field { Name = name, TableAlias = tableAlias } }; }

            public static Projection Field(string name, string keyName, string tableAlias)
            { return new Projection { Type = ProjectionType.Field, Field = new Field { Name = name, TableAlias = tableAlias, Key = keyName, HasKey = true } }; }

            public static Projection Constant(object value)
            { return new Projection { Type = ProjectionType.Constant, Constant = new Constant { Value = value } }; }

            public static Projection Function(Function function)
            { return new Projection { Type = ProjectionType.Function, Function = function }; }
        }
    }
}

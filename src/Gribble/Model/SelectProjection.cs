namespace Gribble.Model
{
    public class SelectProjection
    {
        public Projection Projection;
        public string Alias;

        public static SelectProjection Create(Projection projection)
        {
            return new SelectProjection
            {
                Projection = projection
            };
        }
    }
}

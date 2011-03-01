using System.Linq;

namespace Gribble
{
    public interface INamedQueryable : IQueryable
    {
        string Name { get; }
    }
}

using System;
using System.Data;

namespace Gribble
{
    public interface IConnectionManager : IDisposable
    {
        IDbConnection Connection { get; }
        IDbCommand CreateCommand();
    }
}
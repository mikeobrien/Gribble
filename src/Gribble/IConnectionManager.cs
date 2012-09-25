using System;
using System.Data.SqlClient;

namespace Gribble
{
    public interface IConnectionManager : IDisposable
    {
        SqlConnection Connection { get; }
        SqlCommand CreateCommand();
    }
}
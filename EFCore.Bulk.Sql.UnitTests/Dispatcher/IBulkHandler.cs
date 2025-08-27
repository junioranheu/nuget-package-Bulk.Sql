using Microsoft.Data.SqlClient;
using MySqlConnector;
using Npgsql;

namespace EFCore.Bulk.Sql.UnitTests.Dispatcher
{
    public interface IBulkHandler
    {
        Task HandleSqlClientAsync<T>(List<T> linq, SqlConnection conn, string table, bool isExceptionInPortuguese, bool isDisableFKCheck, int? timeout);
        Task HandleSystemDataSqlClientAsync<T>(List<T> linq, System.Data.SqlClient.SqlConnection conn, string table, bool isExceptionInPortuguese, bool isDisableFKCheck, int? timeout);
        Task HandleMySqlAsync<T>(List<T> linq, MySqlConnection conn, string table, bool isExceptionInPortuguese, bool isDisableFKCheck, int? timeout);
        Task HandleNpgsqlAsync<T>(List<T> linq, NpgsqlConnection conn, string table, bool isExceptionInPortuguese, bool isDisableFKCheck, int? timeout);
    }
}
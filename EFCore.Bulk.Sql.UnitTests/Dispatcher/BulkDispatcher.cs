using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Npgsql;
using System.Data.Common;

namespace EFCore.Bulk.Sql.UnitTests.Dispatcher
{
    public sealed class BulkDispatcher(IBulkHandler handler)
    {
        private readonly IBulkHandler _handler = handler;

        public async Task BulkInsertUsingConnection<T>(List<T> linq, DbConnection con, string table, int? timeOut = null, bool? isExceptionInPortuguese = false, bool? isDisableFKCheck = false)
        {
            if (con is null)
            {
                throw new Exception("Conexão nula");
            }

            if (con is SqlConnection ms)
            {
                await _handler.HandleSqlClientAsync(linq, ms, table, isExceptionInPortuguese.GetValueOrDefault(), isDisableFKCheck.GetValueOrDefault(), timeOut);
            }
            else if (con is System.Data.SqlClient.SqlConnection sys)
            {
                await _handler.HandleSystemDataSqlClientAsync(linq, sys, table, isExceptionInPortuguese.GetValueOrDefault(), isDisableFKCheck.GetValueOrDefault(), timeOut);
            }
            else if (con is MySqlConnection my)
            {
                await _handler.HandleMySqlAsync(linq, my, table, isExceptionInPortuguese.GetValueOrDefault(), isDisableFKCheck.GetValueOrDefault(), timeOut);
            }
            else if (con is NpgsqlConnection npg)
            {
                await _handler.HandleNpgsqlAsync(linq, npg, table, isExceptionInPortuguese.GetValueOrDefault(), isDisableFKCheck.GetValueOrDefault(), timeOut);
            }
            else
            {
                throw new Exception($"Tipo de conexão não suportado: {con.GetType()}");
            }
        }

        public Task BulkInsertUsingContext<T, TContext>(List<T> linq, TContext context, string table, int? timeOut = null, bool? isExceptionInPortuguese = false, bool? isDisableFKCheck = false) where TContext : DbContext
        {
            DbConnection dbCon = context.Database.GetDbConnection();
            return BulkInsertUsingConnection(linq, dbCon, table, timeOut, isExceptionInPortuguese, isDisableFKCheck);
        }
    }
}
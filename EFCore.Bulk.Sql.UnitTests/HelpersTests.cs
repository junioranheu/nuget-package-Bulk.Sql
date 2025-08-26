using EFCore.Bulk.Sql.UnitTests.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MySqlConnector;
using Npgsql;
using System.Data.Common;

namespace EFCore.Bulk.Sql.UnitTests
{
    public sealed class HelpersTests
    {
        private class FakeContext<TConnection> : DbContext where TConnection : DbConnection, new()
        {
            private readonly DbConnection _connection;

            public FakeContext()
            {
                _connection = new TConnection();
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseInMemoryDatabase("FakeDb");
            }

            public override DatabaseFacade Database => new(this)
            {
                // Sobrescrevendo GetDbConnection não é possível diretamente,
                // mas em testes será acessado via Reflection;
            };

            public DbConnection GetFakeConnection() => _connection;
        }

        private static DbContext CreateContextWithConnection(DbConnection connection)
        {
            var options = new DbContextOptionsBuilder<DbContext>().UseInMemoryDatabase("FakeDb").Options;
            var ctx = new DbContext(options);
            var database = new DatabaseFacade(ctx);

            var dbFacadeDependencies = typeof(DatabaseFacade).GetProperty("Dependencies", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(database);
            var relationalConnectionProp = dbFacadeDependencies?.GetType().GetProperty("RelationalConnection", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            if (relationalConnectionProp != null)
            {
                var relationalConn = relationalConnectionProp.GetValue(dbFacadeDependencies);
                var connProp = relationalConn?.GetType().GetProperty("DbConnection", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                connProp?.SetValue(relationalConn, connection);
            }

            return ctx;
        }

        [Fact]
        public async Task BulkInsert_WithMicrosoftDataSqlClientSqlConnection_ShouldCallSqlBulkInsert()
        {
            var fakeConn = new SqlConnection();
            var ctx = CreateContextWithConnection(fakeConn);

            var data = new List<Dummy> { new() { Id = 1, Name = "Test" } };

            var ex = await Record.ExceptionAsync(() => Helpers.BulkInsert(data, ctx, "MyTable"));

            Assert.Null(ex);
        }

        [Fact]
        public async Task BulkInsert_WithSystemDataSqlClientSqlConnection_ShouldCallSqlBulkInsert()
        {
            var fakeConn = new System.Data.SqlClient.SqlConnection();
            var ctx = CreateContextWithConnection(fakeConn);

            var data = new List<Dummy> { new() { Id = 1, Name = "Test" } };

            var ex = await Record.ExceptionAsync(() => Helpers.BulkInsert(data, ctx, "MyTable"));

            Assert.Null(ex);
        }

        [Fact]
        public async Task BulkInsert_WithMySqlConnection_ShouldCallMySqlBulkInsert()
        {
            var fakeConn = new MySqlConnection();
            var ctx = CreateContextWithConnection(fakeConn);

            var data = new List<Dummy> { new() { Id = 1, Name = "Test" } };

            var ex = await Record.ExceptionAsync(() => Helpers.BulkInsert(data, ctx, "MyTable"));

            Assert.Null(ex);
        }

        [Fact]
        public async Task BulkInsert_WithNpgsqlConnection_ShouldCallPostgresBulkInsert()
        {
            var fakeConn = new NpgsqlConnection();
            var ctx = CreateContextWithConnection(fakeConn);

            var data = new List<Dummy> { new() { Id = 1, Name = "Test" } };

            var ex = await Record.ExceptionAsync(() => Helpers.BulkInsert(data, ctx, "MyTable"));

            Assert.Null(ex);
        }
    }
}
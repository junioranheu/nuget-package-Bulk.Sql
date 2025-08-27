using EFCore.Bulk.Sql.UnitTests.Dispatcher;
using Microsoft.Data.SqlClient;
using Moq;
using MySqlConnector;
using Npgsql;
using System.Data;
using System.Data.Common;

namespace EFCore.Bulk.Sql.UnitTests
{
    public sealed class BulkTests
    {
        private readonly Mock<IBulkHandler> _handlerMock;
        private readonly BulkDispatcher _dispatcher;

        public BulkTests()
        {
            _handlerMock = new Mock<IBulkHandler>(MockBehavior.Strict);
            _dispatcher = new BulkDispatcher(_handlerMock.Object);
        }

        [Fact]
        public async Task BulkInsert_Uses_MicrosoftData_SqlClient_Handler()
        {
           var items = new List<int>() { 1, 2, 3 };
           var  conn = new SqlConnection();

            _handlerMock  .Setup(h => h.HandleSqlClientAsync(items, conn, "T", false, false, null)) .Returns(Task.CompletedTask).Verifiable();

            await _dispatcher.BulkInsertUsingConnection(items, conn, "T");

            _handlerMock.Verify();
        }

        [Fact]
        public async Task BulkInsert_Uses_System_Data_SqlClient_Handler()
        {
            var items = new List<int> { 1 };
          var conn = new System.Data.SqlClient.SqlConnection();

            _handlerMock   .Setup(h => h.HandleSystemDataSqlClientAsync(items, conn, "T", false, false, null)).Returns(Task.CompletedTask).Verifiable();

            await _dispatcher.BulkInsertUsingConnection(items, conn, "T");

            _handlerMock.Verify();
        }

        [Fact]
        public async Task BulkInsert_Uses_MySql_Handler()
        {
            var items = new List<int> { 1 };
            var conn = new MySqlConnection();

            _handlerMock    .Setup(h => h.HandleMySqlAsync(items, conn, "T", false, false, null)) .Returns(Task.CompletedTask).Verifiable();

            await _dispatcher.BulkInsertUsingConnection(items, conn, "T");

            _handlerMock.Verify();
        }

        [Fact]
        public async Task BulkInsert_Uses_Npgsql_Handler()
        {
            var items = new List<int> { 1 };
            var conn = new NpgsqlConnection();

            _handlerMock .Setup(h => h.HandleNpgsqlAsync(items, conn, "T", false, false, null)) .Returns(Task.CompletedTask) .Verifiable();

            await _dispatcher.BulkInsertUsingConnection(items, conn, "T");

            _handlerMock.Verify();
        }

        [Fact]
        public async Task BulkInsert_Throws_On_Null_Connection()
        {
            await Assert.ThrowsAsync<Exception>(() => _dispatcher.BulkInsertUsingConnection<int>(new List<int>(), null, "T"));
        }

        [Fact]
        public async Task BulkInsert_Throws_On_Unsupported_Connection_Type()
        {
            var items = new List<int> { 1 };
            var fakeConn = new FakeDbConnection(); 
            
            var ex = await Assert.ThrowsAsync<Exception>(() => _dispatcher.BulkInsertUsingConnection(items, fakeConn, "T"));
          
            Assert.Contains("Tipo de conexão não suportado", ex.Message);
        }


        private class FakeDbConnection : DbConnection
        {
            public override string? ConnectionString { get; set; }
            public override string Database => "Fake";
            public override string DataSource => "Fake";
            public override string ServerVersion => "1.0";
            public override ConnectionState State => ConnectionState.Closed;
            public override void ChangeDatabase(string databaseName) { }
            public override void Close() { }
            public override void Open() { }
            protected override DbTransaction? BeginDbTransaction(IsolationLevel isolationLevel) => null;
            protected override DbCommand CreateDbCommand() => null;
        }
    }
}
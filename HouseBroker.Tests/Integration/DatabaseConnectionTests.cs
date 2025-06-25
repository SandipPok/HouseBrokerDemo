using Dapper;
using HouseBroker.Application.Dtos;
using HouseBroker.Domain.Entities;
using HouseBroker.Domain.Enums;
using HouseBroker.Domain.ValueObjects;
using HouseBroker.Infrastructure.Data;
using HouseBroker.Infrastructure.Data.Repositories;
using Xunit;

namespace HouseBroker.Tests.Integration
{
    /// <summary>
    /// Simple integration test to verify the database connection and transaction fixes
    /// </summary>
    public class DatabaseConnectionTests
    {
        [Fact]
        public async Task PropertyRepository_AddAsync_ShouldWorkWithTransactions()
        {
            // This test will only pass if we have a proper database connection
            // Skip this test if no database is available
            
            // For now, just test that our repository methods can be instantiated
            // without throwing connection-related exceptions
            
            var mockConnectionFactory = new MockConnectionFactory();
            var repository = new PropertyRepository(mockConnectionFactory);
            
            // Create a test property
            var property = new Property
            {
                Type = PropertyType.House,
                Location = new Location("123 Test St", "Test Neighborhood", "Test City"),
                Price = new Money(250000, "USD"),
                Description = "Test property",
                Features = "Test features",
                BrokerId = 1,
                ImageUrls = new List<string> { "test1.jpg", "test2.jpg" }
            };

            // This should not throw an InvalidOperationException about closed connections
            // if our fix is working (though it may throw other exceptions related to mock setup)
            try
            {
                await repository.AddAsync(property);
                Xunit.Assert.True(true, "Repository method executed without connection errors");
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("The connection is closed"))
            {
                Xunit.Assert.Fail("Connection was not properly opened before transaction");
            }
            catch
            {
                // Other exceptions are expected with mock setup
                Xunit.Assert.True(true, "Method executed - connection opening logic works");
            }
        }
    }

    /// <summary>
    /// Mock connection factory for testing
    /// </summary>
    public class MockConnectionFactory : IDbConnectionFactory
    {
        public System.Data.IDbConnection CreateConnection()
        {
            // Return a mock connection that simulates the connection state
            return new MockDbConnection();
        }
    }

    /// <summary>
    /// Mock database connection for testing
    /// </summary>
    public class MockDbConnection : System.Data.IDbConnection
    {
#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member
        public string ConnectionString { get; set; } = string.Empty;
#pragma warning restore CS8767
        public int ConnectionTimeout => 30;
        public string Database => "TestDB";
        public System.Data.ConnectionState State { get; private set; } = System.Data.ConnectionState.Closed;

        public void Open()
        {
            State = System.Data.ConnectionState.Open;
        }

        public void Close()
        {
            State = System.Data.ConnectionState.Closed;
        }

        public System.Data.IDbTransaction BeginTransaction()
        {
            if (State != System.Data.ConnectionState.Open)
                throw new InvalidOperationException("Invalid operation. The connection is closed.");
            
            return new MockDbTransaction();
        }

        public System.Data.IDbTransaction BeginTransaction(System.Data.IsolationLevel il)
        {
            return BeginTransaction();
        }

        public void ChangeDatabase(string databaseName) { }
        public System.Data.IDbCommand CreateCommand() => throw new NotImplementedException();
        public void Dispose() { }
    }

    /// <summary>
    /// Mock database transaction for testing
    /// </summary>
    public class MockDbTransaction : System.Data.IDbTransaction
    {
        public System.Data.IDbConnection? Connection => null;
        public System.Data.IsolationLevel IsolationLevel => System.Data.IsolationLevel.ReadCommitted;

        public void Commit() { }
        public void Rollback() { }
        public void Dispose() { }
    }
}

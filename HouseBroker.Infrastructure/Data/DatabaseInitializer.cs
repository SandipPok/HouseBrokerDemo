using Dapper;
using System.Data;

namespace HouseBroker.Infrastructure.Data
{
    public class DatabaseInitializer
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public DatabaseInitializer(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public void Initialize()
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            CreateUsersTable(connection);
            CreatePropertiesTable(connection);
            CreateIndexes(connection);
        }

        private void CreateUsersTable(IDbConnection connection)
        {
            connection.Execute(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
                CREATE TABLE Users (
                    Id INT PRIMARY KEY IDENTITY,
                    FirstName NVARCHAR(50),
                    LastName NVARCHAR(50),
                    Email NVARCHAR(100) NOT NULL UNIQUE,
                    PasswordHash NVARCHAR(MAX) NOT NULL,
                    Role NVARCHAR(20) NOT NULL CHECK (Role IN ('Broker', 'Seeker')),
                    CreatedDate DATETIME DEFAULT GETUTCDATE()
                )");
        }

        private void CreatePropertiesTable(IDbConnection connection)
        {
            connection.Execute(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Properties')
                CREATE TABLE Properties (
                    Id INT PRIMARY KEY IDENTITY,
                    PropertyType NVARCHAR(50) NOT NULL,
                    Location NVARCHAR(300) NOT NULL,  -- Increased size for compound value
                    Price DECIMAL(18,2) NOT NULL,
                    Description NVARCHAR(MAX),
                    Features NVARCHAR(MAX),
                    BrokerId INT NOT NULL,
                    CreatedDate DATETIME DEFAULT GETUTCDATE(),
                    FOREIGN KEY (BrokerId) REFERENCES Users(Id)
                )");
            
            // Create PropertyImages table for storing image URLs
            connection.Execute(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PropertyImages')
                CREATE TABLE PropertyImages (
                    Id INT PRIMARY KEY IDENTITY,
                    PropertyId INT NOT NULL,
                    ImageUrl NVARCHAR(500) NOT NULL,
                    DisplayOrder INT DEFAULT 0,
                    CreatedDate DATETIME DEFAULT GETUTCDATE(),
                    FOREIGN KEY (PropertyId) REFERENCES Properties(Id) ON DELETE CASCADE
                )");
        }

        private void CreateIndexes(IDbConnection connection)
        {
            connection.Execute(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Properties_Location')
                CREATE INDEX IX_Properties_Location ON Properties(Location)");

            connection.Execute(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Properties_Price')
                CREATE INDEX IX_Properties_Price ON Properties(Price)");

            connection.Execute(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Properties_PropertyType')
                CREATE INDEX IX_Properties_PropertyType ON Properties(PropertyType)");
        }
    }
}
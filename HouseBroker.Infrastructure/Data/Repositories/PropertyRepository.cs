using Dapper;
using HouseBroker.Application.Dtos;
using HouseBroker.Application.Interfaces.Repositories;
using HouseBroker.Domain.Entities;
using System.Text;

namespace HouseBroker.Infrastructure.Data.Repositories
{
    public class PropertyRepository : IPropertyRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public PropertyRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task AddAsync(Property property)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"INSERT INTO Properties 
                (PropertyType, Location, Price, Description, Features, BrokerId) 
                VALUES (@PropertyType, @Location, @Price, @Description, @Features, @BrokerId)";

            // Create explicit parameters
            var parameters = new
            {
                PropertyType = property.Type.ToString(),
                Location = property.Location,
                Price = property.Price.Amount,
                property.Description,
                property.Features,
                property.BrokerId
            };

            await connection.ExecuteAsync(sql, parameters);
        }

        public async Task DeleteAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync("DELETE FROM Properties WHERE Id = @Id", new { Id = id });
        }

        public async Task<IEnumerable<Property>> GetAllAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryAsync<Property>("SELECT * FROM Properties");
        }

        public async Task<Property> GetByIdAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Property>(
                "SELECT * FROM Properties WHERE Id = @Id", new { Id = id });
        }

        public async Task UpdateAsync(Property property)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"UPDATE Properties SET 
                    PropertyType = @PropertyType, 
                    Location = @Location, 
                    Price = @Price, 
                    Description = @Description, 
                    Features = @Features 
                    WHERE Id = @Id";

            var parameters = new
            {
                PropertyType = property.Type.ToString(),
                Location = property.Location,
                Price = property.Price.Amount,
                property.Description,
                property.Features,
                property.Id
            };

            await connection.ExecuteAsync(sql, parameters);
        }

        public async Task<PaginatedResult<Property>> SearchAsync(PropertySearchFilters filters)
        {
            using var connection = _connectionFactory.CreateConnection();

            var sql = @"
        ;WITH Results AS (
            SELECT 
                p.*,
                ROW_NUMBER() OVER (ORDER BY p.CreatedDate DESC) AS RowNum,
                COUNT(*) OVER() AS TotalCount
            FROM Properties p
            WHERE 1=1
                {0}
        )
        SELECT 
            Id, 
            PropertyType, 
            Location, 
            Price, 
            Description, 
            Features, 
            BrokerId, 
            CreatedDate,
            TotalCount
        FROM Results
        WHERE RowNum BETWEEN @StartIndex AND @EndIndex";

            var whereClause = new StringBuilder();
            var parameters = new DynamicParameters();

            // Apply filters
            if (!string.IsNullOrEmpty(filters.Location))
            {
                whereClause.Append(" AND p.Location LIKE '%' + @Location + '%'");
                parameters.Add("Location", filters.Location);
            }
            if (filters.MinPrice.HasValue)
            {
                whereClause.Append(" AND p.Price >= @MinPrice");
                parameters.Add("MinPrice", filters.MinPrice);
            }
            if (filters.MaxPrice.HasValue)
            {
                whereClause.Append(" AND p.Price <= @MaxPrice");
                parameters.Add("MaxPrice", filters.MaxPrice);
            }
            if (!string.IsNullOrEmpty(filters.PropertyType))
            {
                whereClause.Append(" AND p.PropertyType = @PropertyType");
                parameters.Add("PropertyType", filters.PropertyType);
            }

            // Calculate pagination
            int startIndex = (filters.Page - 1) * filters.PageSize + 1;
            int endIndex = filters.Page * filters.PageSize;

            parameters.Add("StartIndex", startIndex);
            parameters.Add("EndIndex", endIndex);

            // Execute query
            var results = await connection.QueryAsync<Property, int, Property>(
                string.Format(sql, whereClause),
                (property, totalCount) =>
                {
                    property.GetType().GetProperty("TotalCount")?.SetValue(property, totalCount);
                    return property;
                },
                parameters,
                splitOn: "TotalCount");

            var totalCount = results.FirstOrDefault()?.GetType()
                .GetProperty("TotalCount")?.GetValue(results.First()) as int? ?? 0;

            return new PaginatedResult<Property>(
                Items: results,
                Page: filters.Page,
                PageSize: filters.PageSize,
                TotalCount: totalCount
            );
        }
    }
}
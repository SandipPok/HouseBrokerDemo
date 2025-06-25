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
            connection.Open();
            using var transaction = connection.BeginTransaction();
            
            try
            {
                var sql = @"INSERT INTO Properties 
                    (PropertyType, Location, Price, Description, Features, BrokerId) 
                    VALUES (@PropertyType, @Location, @Price, @Description, @Features, @BrokerId);
                    SELECT CAST(SCOPE_IDENTITY() as int)";

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

                var propertyId = await connection.QuerySingleAsync<int>(sql, parameters, transaction);
                property.Id = propertyId;

                // Insert images if any
                if (property.ImageUrls.Any())
                {
                    var imageSql = @"INSERT INTO PropertyImages (PropertyId, ImageUrl, DisplayOrder) 
                                   VALUES (@PropertyId, @ImageUrl, @DisplayOrder)";
                    
                    var imageParams = property.ImageUrls.Select((url, index) => new
                    {
                        PropertyId = propertyId,
                        ImageUrl = url,
                        DisplayOrder = index
                    });
                    
                    await connection.ExecuteAsync(imageSql, imageParams, transaction);
                }
                
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync("DELETE FROM Properties WHERE Id = @Id", new { Id = id });
        }

        public async Task<IEnumerable<Property>> GetAllAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryAsync<Property>(@"SELECT 
                                                            Id,
                                                            PropertyType as Type,
                                                            Location,
                                                            Price,
                                                            Description,
                                                            Features,
                                                            BrokerId,
                                                            CreatedDate
                                                        FROM Properties");
        }

        public async Task<IEnumerable<PropertyDto>> GetAllWithBrokerAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                SELECT 
                    p.Id, p.PropertyType as Type, p.Location, p.Price, 
                    p.Description, p.Features, p.CreatedDate,
                    u.Id, u.FirstName, u.LastName, u.Email
                FROM Properties p
                INNER JOIN Users u ON p.BrokerId = u.Id
                ORDER BY p.CreatedDate DESC";

            var propertyDict = new Dictionary<int, PropertyDto>();
            
            await connection.QueryAsync<PropertyDto, BrokerContactDto, PropertyDto>(
                sql,
                (property, broker) =>
                {
                    if (!propertyDict.TryGetValue(property.Id, out var propertyEntry))
                    {
                        propertyEntry = property;
                        propertyEntry.Broker = broker;
                        propertyDict.Add(property.Id, propertyEntry);
                    }
                    return propertyEntry;
                },
                splitOn: "Id");

            // Load images for all properties
            var propertyIds = propertyDict.Keys.ToList();
            if (propertyIds.Any())
            {
                var imagesSql = @"SELECT PropertyId, ImageUrl FROM PropertyImages 
                                WHERE PropertyId IN @PropertyIds 
                                ORDER BY PropertyId, DisplayOrder";
                
                var images = await connection.QueryAsync<(int PropertyId, string ImageUrl)>(
                    imagesSql, new { PropertyIds = propertyIds });
                
                foreach (var imageGroup in images.GroupBy(i => i.PropertyId))
                {
                    if (propertyDict.TryGetValue(imageGroup.Key, out var property))
                    {
                        property.ImageUrls = imageGroup.Select(i => i.ImageUrl).ToList();
                    }
                }
            }

            return propertyDict.Values;
        }

        public async Task<Property?> GetByIdAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            var property = await connection.QueryFirstOrDefaultAsync<Property>(
                @"SELECT 
                    Id,
                    PropertyType as Type,
                    Location,
                    Price,
                    Description,
                    Features,
                    BrokerId,
                    CreatedDate
                FROM Properties WHERE Id = @Id", new { Id = id });

            if (property != null)
            {
                // Load images
                var images = await connection.QueryAsync<string>(
                    @"SELECT ImageUrl FROM PropertyImages 
                      WHERE PropertyId = @PropertyId 
                      ORDER BY DisplayOrder", 
                    new { PropertyId = id });
                
                property.ImageUrls = images.ToList();
            }

            return property;
        }

        public async Task<PropertyDto?> GetByIdWithBrokerAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                SELECT 
                    p.Id, p.PropertyType as Type, p.Location, p.Price, 
                    p.Description, p.Features, p.CreatedDate,
                    u.Id, u.FirstName, u.LastName, u.Email
                FROM Properties p
                INNER JOIN Users u ON p.BrokerId = u.Id
                WHERE p.Id = @Id";

            var result = await connection.QueryAsync<PropertyDto, BrokerContactDto, PropertyDto>(
                sql,
                (property, broker) =>
                {
                    property.Broker = broker;
                    return property;
                },
                new { Id = id },
                splitOn: "Id");

            var propertyDto = result.FirstOrDefault();
            
            if (propertyDto != null)
            {
                // Load images
                var images = await connection.QueryAsync<string>(
                    @"SELECT ImageUrl FROM PropertyImages 
                      WHERE PropertyId = @PropertyId 
                      ORDER BY DisplayOrder", 
                    new { PropertyId = id });
                
                propertyDto.ImageUrls = images.ToList();
            }

            return propertyDto;
        }

        public async Task UpdateAsync(Property property)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();
            
            try
            {
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

                await connection.ExecuteAsync(sql, parameters, transaction);

                // Update images - delete existing and insert new ones
                await connection.ExecuteAsync(
                    "DELETE FROM PropertyImages WHERE PropertyId = @PropertyId",
                    new { PropertyId = property.Id },
                    transaction);

                if (property.ImageUrls.Any())
                {
                    var imageSql = @"INSERT INTO PropertyImages (PropertyId, ImageUrl, DisplayOrder) 
                                   VALUES (@PropertyId, @ImageUrl, @DisplayOrder)";
                    
                    var imageParams = property.ImageUrls.Select((url, index) => new
                    {
                        PropertyId = property.Id,
                        ImageUrl = url,
                        DisplayOrder = index
                    });
                    
                    await connection.ExecuteAsync(imageSql, imageParams, transaction);
                }
                
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
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

        public async Task<PaginatedResult<PropertyDto>> SearchWithBrokerAsync(PropertySearchFilters filters)
        {
            using var connection = _connectionFactory.CreateConnection();

            var sql = @"
        ;WITH Results AS (
            SELECT 
                p.Id, p.PropertyType, p.Location, p.Price, 
                p.Description, p.Features, p.CreatedDate,
                u.Id as BrokerId, u.FirstName, u.LastName, u.Email,
                ROW_NUMBER() OVER (ORDER BY p.CreatedDate DESC) AS RowNum,
                COUNT(*) OVER() AS TotalCount
            FROM Properties p
            INNER JOIN Users u ON p.BrokerId = u.Id
            WHERE 1=1
                {0}
        )
        SELECT 
            Id, PropertyType as Type, Location, Price, Description, Features, CreatedDate, TotalCount,
            BrokerId, FirstName, LastName, Email
        FROM Results
        WHERE RowNum BETWEEN @StartIndex AND @EndIndex
        ORDER BY RowNum";

            var whereClause = new StringBuilder();
            var parameters = new DynamicParameters();

            // Apply filters
            if (!string.IsNullOrEmpty(filters.Location))
            {
                whereClause.Append(" AND p.Location LIKE @Location");
                parameters.Add("Location", $"%{filters.Location}%");
            }

            if (filters.MinPrice.HasValue)
            {
                whereClause.Append(" AND p.Price >= @MinPrice");
                parameters.Add("MinPrice", filters.MinPrice.Value);
            }

            if (filters.MaxPrice.HasValue)
            {
                whereClause.Append(" AND p.Price <= @MaxPrice");
                parameters.Add("MaxPrice", filters.MaxPrice.Value);
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
            var propertyDict = new Dictionary<int, PropertyDto>();
            int totalCount = 0;

            await connection.QueryAsync<PropertyDto, BrokerContactDto, int, PropertyDto>(
                string.Format(sql, whereClause),
                (property, broker, total) =>
                {
                    totalCount = total;
                    if (!propertyDict.TryGetValue(property.Id, out var propertyEntry))
                    {
                        propertyEntry = property;
                        propertyEntry.Broker = broker;
                        propertyDict.Add(property.Id, propertyEntry);
                    }
                    return propertyEntry;
                },
                parameters,
                splitOn: "BrokerId,TotalCount");

            // Load images for filtered properties
            var propertyIds = propertyDict.Keys.ToList();
            if (propertyIds.Any())
            {
                var imagesSql = @"SELECT PropertyId, ImageUrl FROM PropertyImages 
                                WHERE PropertyId IN @PropertyIds 
                                ORDER BY PropertyId, DisplayOrder";
                
                var images = await connection.QueryAsync<(int PropertyId, string ImageUrl)>(
                    imagesSql, new { PropertyIds = propertyIds });
                
                foreach (var imageGroup in images.GroupBy(i => i.PropertyId))
                {
                    if (propertyDict.TryGetValue(imageGroup.Key, out var property))
                    {
                        property.ImageUrls = imageGroup.Select(i => i.ImageUrl).ToList();
                    }
                }
            }

            return new PaginatedResult<PropertyDto>(
                Items: propertyDict.Values,
                Page: filters.Page,
                PageSize: filters.PageSize,
                TotalCount: totalCount
            );
        }
    }
}
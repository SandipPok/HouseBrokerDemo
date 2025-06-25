# House Broker Application

A comprehensive RESTful API for managing property listings with user authentication, advanced search capabilities, and broker-seeker differentiation.

## Features

- **JWT Authentication** with role-based authorization (Broker/Seeker)
- **Complete Property CRUD Operations** with image support
- **Advanced Search & Filtering** by location, price, and property type
- **Broker Contact Information** included in property responses
- **Pagination Support** for efficient data handling
- **Clean Architecture** with proper separation of concerns
- **Comprehensive Unit Testing** with high coverage
- **API Documentation** with Swagger/OpenAPI
- **Health Checks** for database and JWT configuration

## Setup Instructions

### Prerequisites

- .NET 9 SDK
- SQL Server (LocalDB, Express, or Full)
- Visual Studio 2022 or VS Code

### 1. Database Setup

#### Option A: SQL Server LocalDB (Recommended for Development)

```sql
-- LocalDB connection string (already configured in appsettings.Development.json)
-- No additional setup required
```

#### Option B: SQL Server Express/Full

1. Create a new database:

```sql
CREATE DATABASE HouseBrokerDB;
```

2. Update the connection string in `HouseBroker.Presentation/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=HouseBrokerDB;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

### 2. JWT Configuration

Update the JWT settings in `HouseBroker.Presentation/appsettings.json`:

```json
{
  "JwtSettings": {
    "Key": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "HouseBrokerApp",
    "Audience": "HouseBrokerApp",
    "ExpiryInHours": 24
  }
}
```

### 3. Running the Application

1. **Restore packages and build:**

```bash
dotnet restore
dotnet build
```

2. **Initialize the database:**
   The application will automatically create the required tables on first run.

3. **Run the application:**

```bash
cd HouseBroker.Presentation
dotnet run
```

4. **Access the application:**

- API: `https://localhost:5001` or `http://localhost:5000`
- Swagger UI: `https://localhost:5001/swagger`
- Health Checks: `https://localhost:5001/health`

### 4. API Usage Examples

#### Register a Broker

```bash
curl -X POST "https://localhost:5001/api/Auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.broker@example.com",
    "password": "SecurePassword123!",
    "role": "Broker"
  }'
```

#### Login and Get Token

```bash
curl -X POST "https://localhost:5001/api/Auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john.broker@example.com",
    "password": "SecurePassword123!"
  }'
```

#### Create a Property (Broker Only)

```bash
curl -X POST "https://localhost:5001/api/Properties" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "type": "House",
    "location": {
      "address": "123 Main St",
      "neighborhood": "Downtown",
      "city": "Springfield"
    },
    "price": {
      "amount": 350000,
      "currency": "USD"
    },
    "description": "Beautiful 3-bedroom house in downtown",
    "features": "3 bedrooms, 2 bathrooms, garage, garden",
    "imageUrls": [
      "https://example.com/image1.jpg",
      "https://example.com/image2.jpg"
    ]
  }'
```

#### Search Properties

```bash
curl "https://localhost:5001/api/Properties/search?location=Downtown&minPrice=200000&maxPrice=500000&propertyType=House&page=1&pageSize=10"
```

## Running Tests

Execute the comprehensive test suite:

```bash
# Run all tests
dotnet test

# Run tests with coverage (requires coverlet.collector package)
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test HouseBroker.Tests/HouseBroker.Tests.csproj

# Run tests with detailed output
dotnet test --logger:"console;verbosity=detailed"
```

### Test Coverage

The project includes comprehensive unit tests covering:

- **Service Layer Tests**: Business logic validation
- **Controller Tests**: API endpoint behavior
- **Repository Tests**: Data access operations
- **Authentication Tests**: Security and authorization
- **Edge Cases**: Error handling and validation

## Project Structure

```
HouseBroker/
├── HouseBroker.Domain/           # Domain entities, value objects, exceptions
│   ├── Entities/                 # Core business entities (Property, User)
│   ├── ValueObjects/            # Value objects (Location, Money)
│   ├── Enums/                   # Domain enumerations
│   └── Exceptions/              # Domain-specific exceptions
├── HouseBroker.Application/      # Application services and DTOs
│   ├── Services/                # Business logic services
│   ├── Interfaces/              # Service contracts
│   └── Dtos/                   # Data transfer objects
├── HouseBroker.Infrastructure/   # Data access and external services
│   ├── Data/                   # Database context and repositories
│   ├── Security/               # Authentication and authorization
│   └── TypeHandlers/           # Dapper type mappings
├── HouseBroker.Presentation/    # Web API controllers and configuration
│   ├── Controllers/            # API endpoints
│   ├── Middleware/             # Custom middleware
│   └── HealthChecks/           # Application health monitoring
└── HouseBroker.Tests/           # Comprehensive test suite
    ├── Services/               # Service layer tests
    ├── Controllers/            # Controller tests
    └── Repositories/           # Data access tests
```

## API Documentation

After running the application, comprehensive API documentation is available at:

- **Swagger UI**: [https://localhost:5001/swagger](https://localhost:5001/swagger)
- **OpenAPI Spec**: [https://localhost:5001/swagger/v1/swagger.json](https://localhost:5001/swagger/v1/swagger.json)

## Health Checks

Monitor application health at:

- **Health Status**: [https://localhost:5001/health](https://localhost:5001/health)

Health checks monitor:

- Database connectivity
- JWT configuration validity

## Architecture Principles

This application follows **Clean Architecture** principles:

1. **Dependency Inversion**: Higher-level modules don't depend on lower-level modules
2. **Separation of Concerns**: Each layer has a specific responsibility
3. **Testability**: Comprehensive unit testing with mocking
4. **Maintainability**: Clear structure and well-documented code

## Security Features

- **JWT Authentication** with secure token generation
- **Role-based Authorization** (Broker/Seeker differentiation)
- **Password Hashing** using BCrypt
- **Input Validation** with comprehensive DTOs
- **Security Headers** for API protection

## Database Schema

### Users Table

- `Id` (Primary Key)
- `FirstName`, `LastName`, `Email` (Contact Info)
- `PasswordHash` (Secure password storage)
- `Role` (Broker/Seeker differentiation)
- `CreatedDate` (Audit trail)

### Properties Table

- `Id` (Primary Key)
- `PropertyType`, `Location`, `Price` (Core attributes)
- `Description`, `Features` (Detailed information)
- `BrokerId` (Foreign Key to Users)
- `CreatedDate` (Audit trail)

### PropertyImages Table

- `Id` (Primary Key)
- `PropertyId` (Foreign Key to Properties)
- `ImageUrl` (Image storage)
- `DisplayOrder` (Image ordering)

## Troubleshooting

### Common Issues

1. **Database Connection Issues**

   - Verify SQL Server is running
   - Check connection string in appsettings.json
   - Ensure database exists

2. **JWT Token Issues**

   - Verify JWT configuration in appsettings.json
   - Ensure the secret key is at least 32 characters
   - Check token expiration

3. **Authorization Issues**
   - Ensure proper role assignment (Broker/Seeker)
   - Verify JWT token includes correct claims
   - Check authorization policies

### Getting Help

For issues or questions:

1. Check the comprehensive test suite for usage examples
2. Review the API documentation at `/swagger`
3. Examine the health checks at `/health` for system status

---

**Note**: This application demonstrates enterprise-level .NET development practices including Clean Architecture, comprehensive testing, security best practices, and thorough documentation.

- `/health`

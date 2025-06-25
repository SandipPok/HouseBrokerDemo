# House Broker Application

## Setup Instructions

### Prerequisites
- .NET 9 SDK
- SQL Server

### 1. Database Setup
Create a SQL Server database and run the initialization script:

```sql
CREATE DATABASE HouseBrokerDB;
```

## Running the Application

1. Update the connection string in `HouseBroker.Presentation/appsettings.json` to point to your SQL Server instance.
2. Build and run the application:
3. The API will be available at `https://localhost:5001` (or the port specified in your launch settings).

## API Documentation

After running the application, navigate to the Swagger UI for interactive API documentation:

- [https://localhost:5001/swagger](https://localhost:5001/swagger)

## Running Tests

To run unit tests:

1. Open a terminal in the solution root directory.
2. Run the following command:
```bash
   dotnet test
```
This will build the solution and execute all tests in the `HouseBroker.Tests` project.



## Project Structure

- `HouseBroker.Presentation`: API controllers and startup configuration
- `HouseBroker.Application`: Business logic and services
- `HouseBroker.Infrastructure`: Data access and repositories
- `HouseBroker.Tests`: Unit and integration tests

## Health Checks

The application exposes health check endpoints for database and JWT configuration:

- `/health`

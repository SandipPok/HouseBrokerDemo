# House Broker API Documentation

**Version:** v1  
**OpenAPI Version:** 3.0.4  
**Authentication:** Bearer JWT Token (`Authorization: Bearer {token}`)

---

## Authentication

### POST `/api/Auth/register`

**Register a new user (Broker or Seeker)**

**Request Body:**

```json
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "user@example.com",
  "password": "YourSecurePassword123",
  "role": "Broker"
}
```

**Response Codes:**

- `200 OK` - Returns user details (without password)
- `400 Bad Request` – Returns validation errors

**Example Response:**

```json
{
  "id": 1,
  "email": "user@example.com",
  "role": "Broker",
  "firstName": "John",
  "lastName": "Doe"
}
```

---

### POST `/api/Auth/login`

**Authenticate and receive JWT token**

**Request Body:**

```json
{
  "email": "user@example.com",
  "password": "YourSecurePassword123"
}
```

**Response Codes:**

- `200 OK` - Returns JWT token
- `401 Unauthorized` – Invalid credentials

**Example Response:**

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

---

## Property Management

### GET `/api/Properties`

**Get all properties with broker contact details**

**Authorization:** None (Public endpoint)

**Response:** Array of PropertyDto objects

**Example Response:**

```json
[
  {
    "id": 1,
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
    "description": "Beautiful 3-bedroom house",
    "features": "3 bedrooms, 2 bathrooms, garage",
    "createdDate": "2025-06-25T10:00:00Z",
    "imageUrls": [
      "https://example.com/image1.jpg",
      "https://example.com/image2.jpg"
    ],
    "broker": {
      "id": 1,
      "firstName": "John",
      "lastName": "Doe",
      "email": "john.broker@example.com",
      "fullName": "John Doe"
    }
  }
]
```

---

### GET `/api/Properties/{id}`

**Get property by ID with broker contact details**

**Authorization:** None (Public endpoint)

**Parameters:**

- `id` (path, required): Property ID

**Response Codes:**

- `200 OK` - Returns PropertyDto
- `404 Not Found` - Property not found

---

### POST `/api/Properties`

**Create new property listing**

**Authorization:** Required - Broker role only

**Request Body (CreatePropertyDto):**

```json
{
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
}
```

**Property Types:** `House`, `Apartment`, `Condo`, `Townhouse`, `Commercial`

**Response Codes:**

- `200 OK` - Property created successfully
- `400 Bad Request` - Validation errors
- `401 Unauthorized` - Invalid or missing token
- `403 Forbidden` - Not a broker

---

### PUT `/api/Properties/{id}`

**Update existing property**

**Authorization:** Required - Broker role only (must be property owner)

**Parameters:**

- `id` (path, required): Property ID

**Request Body (UpdatePropertyDto):**

```json
{
  "type": "Apartment",
  "location": {
    "address": "456 Oak Ave",
    "neighborhood": "Uptown",
    "city": "Springfield"
  },
  "price": {
    "amount": 275000,
    "currency": "USD"
  },
  "description": "Updated: Spacious apartment",
  "features": "2 bedrooms, 1 bathroom, balcony",
  "imageUrls": ["https://example.com/newimage1.jpg"]
}
```

**Response Codes:**

- `204 No Content` - Successfully updated
- `400 Bad Request` - Validation errors
- `401 Unauthorized` - Invalid or missing token
- `403 Forbidden` - Not authorized (not property owner)
- `404 Not Found` - Property not found

---

### DELETE `/api/Properties/{id}`

**Delete a property**

**Authorization:** Required - Broker role only (must be property owner)

**Parameters:**

- `id` (path, required): Property ID

**Response Codes:**

- `204 No Content` - Successfully deleted
- `401 Unauthorized` - Invalid or missing token
- `403 Forbidden` - Not authorized (not property owner)
- `404 Not Found` - Property not found

---

## Property Search

### GET `/api/Properties/search`

**Search properties with advanced filters and broker contact details**

**Authorization:** None (Public endpoint)

**Query Parameters:**

- `location` (optional): Filter by location (partial match)
- `minPrice` (optional): Minimum price filter
- `maxPrice` (optional): Maximum price filter
- `propertyType` (optional): Property type filter
- `page` (optional, default: 1): Page number for pagination
- `pageSize` (optional, default: 20): Items per page

**Example Request:**

```
GET /api/Properties/search?location=Downtown&minPrice=200000&maxPrice=500000&propertyType=House&page=1&pageSize=10
```

**Response (PaginatedResult<PropertyDto>):**

```json
{
  "items": [
    {
      "id": 1,
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
      "description": "Beautiful 3-bedroom house",
      "features": "3 bedrooms, 2 bathrooms, garage",
      "createdDate": "2025-06-25T10:00:00Z",
      "imageUrls": [
        "https://example.com/image1.jpg",
        "https://example.com/image2.jpg"
      ],
      "broker": {
        "id": 1,
        "firstName": "John",
        "lastName": "Doe",
        "email": "john.broker@example.com",
        "fullName": "John Doe"
      }
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 1,
  "totalPages": 1,
  "hasPreviousPage": false,
  "hasNextPage": false
}
```

---

## Data Transfer Objects

### PropertyDto

```json
{
  "id": "integer",
  "type": "string (PropertyType enum)",
  "location": "Location object",
  "price": "Money object",
  "description": "string (nullable)",
  "features": "string (nullable)",
  "createdDate": "datetime",
  "imageUrls": "array of strings",
  "broker": "BrokerContactDto object"
}
```

### CreatePropertyDto

```json
{
  "type": "string (PropertyType enum, required)",
  "location": "Location object (required)",
  "price": "Money object (required)",
  "description": "string (optional, max 1000 chars)",
  "features": "string (optional, max 500 chars)",
  "imageUrls": "array of strings (optional)"
}
```

### UpdatePropertyDto

```json
{
  "type": "string (PropertyType enum, required)",
  "location": "Location object (required)",
  "price": "Money object (required)",
  "description": "string (optional, max 1000 chars)",
  "features": "string (optional, max 500 chars)",
  "imageUrls": "array of strings (optional)"
}
```

### BrokerContactDto

```json
{
  "id": "integer",
  "firstName": "string",
  "lastName": "string",
  "email": "string",
  "fullName": "string (computed property)"
}
```

### Location (Value Object)

```json
{
  "address": "string",
  "neighborhood": "string",
  "city": "string"
}
```

### Money (Value Object)

```json
{
  "amount": "decimal",
  "currency": "string"
}
```

### PaginatedResult<T>

```json
{
  "items": "array of T",
  "page": "integer",
  "pageSize": "integer",
  "totalCount": "integer",
  "totalPages": "integer",
  "hasPreviousPage": "boolean",
  "hasNextPage": "boolean"
}
```

---

## Error Handling

### Common Error Response Format

```json
{
  "message": "Error description",
  "details": "Additional error details (if available)"
}
```

### HTTP Status Codes

- `200 OK` - Request successful
- `201 Created` - Resource created successfully
- `204 No Content` - Request successful, no content returned
- `400 Bad Request` - Invalid request data or validation errors
- `401 Unauthorized` - Authentication required or token invalid
- `403 Forbidden` - Access denied (insufficient permissions)
- `404 Not Found` - Resource not found
- `500 Internal Server Error` - Server error

---

## Authentication Flow

1. **Register** as Broker or Seeker using `/api/Auth/register`
2. **Login** using `/api/Auth/login` to receive JWT token
3. **Include token** in Authorization header: `Bearer {token}`
4. **Access protected endpoints** with valid token

### Role-Based Access:

- **Public**: Property search, view properties
- **Seeker**: All public endpoints + user profile management
- **Broker**: All Seeker permissions + property CRUD operations

---

## Additional Features

### Image Management

- Properties support multiple image URLs
- Images are stored as URLs (external storage)
- Display order maintained for consistent presentation

### Advanced Search

- Partial text matching for location
- Price range filtering
- Property type filtering
- Pagination support for large result sets

### Broker Contact Integration

- All property responses include broker contact details
- Direct communication facilitation
- Professional contact information display

### Health Monitoring

- Database connectivity: `/health`
- JWT configuration validation
- Application status monitoring
  **Get all properties**

**Response:**

- `200 OK` – Returns list of `Property` objects

---

### POST `/api/Properties`

**Create a new property (Broker only)**

**Request Body:**

```json
{
  "type": "Apartment",
  "location": {
    "street": "Suganda Marga",
    "city": "Kathmandu",
    "postalCode": "44600"
  },
  "price": {
    "amount": 20000.45,
    "currency": "USD"
  },
  "description": "Beautiful apartment",
  "features": "Garden, Balcony",
  "brokerId": 3
}
```

**Response Codes:**

- `201 Created`
- `400 Bad Request`
- `403 Forbidden`

---

### GET `/api/Properties/{id}`

**Get a specific property by ID**

**Path Parameter:**

- `id` (integer)

**Response Codes:**

- `200 OK`
- `404 Not Found` – Returns `ProblemDetails`

---

### PUT `/api/Properties/{id}`

**Update property by ID (Broker only)**

**Path Parameter:**

- `id` (integer)

**Request Body:** Same as POST `/api/Properties`

**Response Codes:**

- `204 No Content`
- `403 Forbidden`
- `404 Not Found`

---

### DELETE `/api/Properties/{id}`

**Delete a property (Broker only)**

**Path Parameter:**

- `id` (integer)

**Response Codes:**

- `204 No Content`
- `403 Forbidden`
- `404 Not Found`

---

### GET `/api/Properties/search`

**Search properties with filters**

**Query Parameters:**

- `Location` (string)
- `MinPrice` (number)
- `MaxPrice` (number)
- `PropertyType` (string): One of `"Apartment"`, `"House"`, `"Condo"`, `"Townhouse"`, `"Land"`, `"Commercial"`
- `Page` (int)
- `PageSize` (int)

**Response:**

- `200 OK` – Returns `PropertyPaginatedResult`

---

## Schemas

### `RegisterDto`

- `firstName`: string (required)
- `lastName`: string (required)
- `email`: string (email, required)
- `password`: string (min 8 characters)
- `role`: `"Broker"` or `"Seeker"` (required)

### `LoginDto`

- `email`: string
- `password`: string

### `Property`

- `id`: int
- `type`: string (enum: `"Apartment"`, `"House"`, `"Condo"`, `"Townhouse"`, `"Land"`, `"Commercial"`)
- `location`: `Location`
- `price`: `Money`
- `description`: string
- `features`: string
- `brokerId`: int
- `createdDate`: date-time

### `Location`

- `street`: string
- `city`: string
- `postalCode`: string

### `Money`

- `amount`: number
- `currency`: string

### `PropertyPaginatedResult`

- `items`: array of `Property`
- `page`: int
- `pageSize`: int
- `totalCount`: int
- `totalPages`: int

### `ProblemDetails`

Standard error response for bad requests:

- `type`, `title`, `status`, `detail`, `instance`

---

## Security Scheme

All authenticated endpoints use:

```
Authorization: Bearer {your_jwt_token}
```

Defined in:

```json
"securitySchemes": {
  "Bearer": {
    "type": "http",
    "scheme": "Bearer",
    "description": "JWT Authorization header"
  }
}
```

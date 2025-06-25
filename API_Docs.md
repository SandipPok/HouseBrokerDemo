# 🏡 House Broker API Documentation

**Version:** v1  
**OpenAPI Version:** 3.0.4  
**Authentication:** Bearer JWT Token (`Authorization: Bearer {token}`)

---

## 🔐 Authentication

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
- `200 OK`
- `400 Bad Request` – Returns `ProblemDetails`

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
- `200 OK`
- `401 Unauthorized` – Returns `ProblemDetails`

---

## 🏠 Property Management

### GET `/api/Properties`
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

## 📦 Schemas

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

## 🔐 Security Scheme

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

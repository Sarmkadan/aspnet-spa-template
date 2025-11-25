// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# API Reference

Complete reference for all REST API endpoints provided by the aspnet-spa-template.

## Table of Contents
- [Base URL](#base-url)
- [Authentication](#authentication)
- [Response Format](#response-format)
- [Error Handling](#error-handling)
- [Products API](#products-api)
- [Orders API](#orders-api)
- [Users API](#users-api)
- [Health API](#health-api)

## Base URL

```
https://localhost:7001/api
```

For production deployments, replace `localhost:7001` with your domain.

## Authentication

Some endpoints require authentication via Bearer token:

```
Authorization: Bearer <token>
```

Include this header in requests to protected endpoints.

## Response Format

### Success Response

All successful responses follow this format:

```json
{
  "success": true,
  "data": {
    "id": "uuid",
    "name": "Example"
  },
  "message": "Operation successful",
  "timestamp": "2025-01-15T10:30:00Z"
}
```

### Error Response

```json
{
  "success": false,
  "error": {
    "code": "PRODUCT_NOT_FOUND",
    "message": "The requested product does not exist",
    "details": "Product with ID 123 not found"
  },
  "timestamp": "2025-01-15T10:30:00Z"
}
```

## Error Handling

### HTTP Status Codes

- **200 OK** - Successful GET, PUT request
- **201 Created** - Successful POST request
- **204 No Content** - Successful DELETE request
- **400 Bad Request** - Invalid input or validation error
- **401 Unauthorized** - Missing or invalid authentication
- **403 Forbidden** - Authenticated but not authorized
- **404 Not Found** - Resource does not exist
- **422 Unprocessable Entity** - Validation error on entity
- **429 Too Many Requests** - Rate limit exceeded
- **500 Internal Server Error** - Server error
- **503 Service Unavailable** - Server maintenance

---

## Products API

### List All Products

**Endpoint:** `GET /products`

**Description:** Retrieve paginated list of all products.

**Query Parameters:**
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| pageNumber | int | 1 | Page number (starts at 1) |
| pageSize | int | 10 | Items per page (max 100) |
| category | string | - | Filter by category |
| search | string | - | Search product name |

**Example Request:**
```bash
curl -X GET "https://localhost:7001/api/products?pageNumber=1&pageSize=10" \
  -H "Content-Type: application/json"
```

**Success Response:** 200 OK
```json
{
  "success": true,
  "data": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "name": "Laptop",
      "description": "High-performance laptop",
      "price": 999.99,
      "category": "Electronics",
      "stock": 5,
      "createdAt": "2025-01-15T10:30:00Z"
    },
    {
      "id": "550e8400-e29b-41d4-a716-446655440001",
      "name": "Mouse",
      "description": "Wireless mouse",
      "price": 29.99,
      "category": "Accessories",
      "stock": 50,
      "createdAt": "2025-01-15T10:30:00Z"
    }
  ],
  "message": "Products retrieved successfully",
  "timestamp": "2025-01-15T10:30:00Z"
}
```

### Get Product by ID

**Endpoint:** `GET /products/{id}`

**Description:** Retrieve a specific product by ID.

**URL Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| id | UUID | Product ID |

**Example Request:**
```bash
curl -X GET "https://localhost:7001/api/products/550e8400-e29b-41d4-a716-446655440000" \
  -H "Content-Type: application/json"
```

**Success Response:** 200 OK
```json
{
  "success": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "Laptop",
    "description": "High-performance laptop",
    "price": 999.99,
    "category": "Electronics",
    "stock": 5,
    "createdAt": "2025-01-15T10:30:00Z",
    "updatedAt": "2025-01-15T10:30:00Z"
  },
  "message": "Product retrieved successfully"
}
```

**Error Response:** 404 Not Found
```json
{
  "success": false,
  "error": {
    "code": "PRODUCT_NOT_FOUND",
    "message": "Product not found"
  }
}
```

### Create Product

**Endpoint:** `POST /products`

**Description:** Create a new product. Requires admin role.

**Headers:**
```
Content-Type: application/json
Authorization: Bearer <admin_token>
```

**Request Body:**
```json
{
  "name": "New Product",
  "description": "Product description",
  "price": 99.99,
  "category": "Electronics",
  "stock": 10
}
```

**Validation Rules:**
- `name` - Required, max 200 characters
- `price` - Required, must be > 0
- `category` - Required, must be valid category
- `stock` - Required, must be >= 0

**Example Request:**
```bash
curl -X POST "https://localhost:7001/api/products" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIs..." \
  -d '{
    "name": "USB Cable",
    "description": "Type-C USB cable",
    "price": 15.99,
    "category": "Accessories",
    "stock": 100
  }'
```

**Success Response:** 201 Created
```json
{
  "success": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440002",
    "name": "USB Cable",
    "price": 15.99,
    "category": "Accessories"
  },
  "message": "Product created successfully"
}
```

**Error Response:** 400 Bad Request
```json
{
  "success": false,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Validation failed",
    "details": {
      "price": "Price must be greater than 0"
    }
  }
}
```

### Update Product

**Endpoint:** `PUT /products/{id}`

**Description:** Update an existing product. Requires admin role.

**Headers:**
```
Content-Type: application/json
Authorization: Bearer <admin_token>
```

**URL Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| id | UUID | Product ID |

**Request Body:**
```json
{
  "name": "Updated Name",
  "price": 79.99,
  "stock": 8
}
```

**Example Request:**
```bash
curl -X PUT "https://localhost:7001/api/products/550e8400-e29b-41d4-a716-446655440002" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIs..." \
  -d '{
    "price": 19.99,
    "stock": 75
  }'
```

**Success Response:** 200 OK
```json
{
  "success": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440002",
    "name": "USB Cable",
    "price": 19.99,
    "stock": 75
  },
  "message": "Product updated successfully"
}
```

### Delete Product

**Endpoint:** `DELETE /products/{id}`

**Description:** Delete a product. Requires admin role.

**Headers:**
```
Authorization: Bearer <admin_token>
```

**Example Request:**
```bash
curl -X DELETE "https://localhost:7001/api/products/550e8400-e29b-41d4-a716-446655440002" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIs..."
```

**Success Response:** 204 No Content
```
(Empty response body)
```

---

## Orders API

### List All Orders

**Endpoint:** `GET /orders`

**Description:** Retrieve paginated list of orders.

**Query Parameters:**
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| pageNumber | int | 1 | Page number |
| pageSize | int | 10 | Items per page |
| status | string | - | Filter by status |

**Statuses:** `pending`, `processing`, `shipped`, `delivered`, `cancelled`

**Example Request:**
```bash
curl -X GET "https://localhost:7001/api/orders?pageNumber=1&pageSize=10&status=pending"
```

### Create Order

**Endpoint:** `POST /orders`

**Description:** Create a new order.

**Headers:**
```
Content-Type: application/json
Authorization: Bearer <token>
```

**Request Body:**
```json
{
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "items": [
    {
      "productId": "550e8400-e29b-41d4-a716-446655440001",
      "quantity": 2
    }
  ]
}
```

**Example Request:**
```bash
curl -X POST "https://localhost:7001/api/orders" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "items": [
      {
        "productId": "550e8400-e29b-41d4-a716-446655440001",
        "quantity": 1
      }
    ]
  }'
```

**Success Response:** 201 Created
```json
{
  "success": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440003",
    "orderNumber": "ORD-2025-00001",
    "status": "pending",
    "total": 999.99,
    "createdAt": "2025-01-15T10:30:00Z"
  },
  "message": "Order created successfully"
}
```

### Get Order by ID

**Endpoint:** `GET /orders/{id}`

**Description:** Retrieve order details.

**Example Request:**
```bash
curl -X GET "https://localhost:7001/api/orders/550e8400-e29b-41d4-a716-446655440003"
```

### Update Order Status

**Endpoint:** `PUT /orders/{id}/status`

**Description:** Update order status. Requires admin role.

**Request Body:**
```json
{
  "status": "shipped",
  "notes": "Order has been shipped"
}
```

**Allowed Status Transitions:**
- `pending` → `processing` → `shipped` → `delivered`
- Any status → `cancelled`

---

## Users API

### List All Users

**Endpoint:** `GET /users`

**Description:** List all users. Requires admin role.

**Query Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| pageNumber | int | Page number |
| pageSize | int | Items per page |

### Create User

**Endpoint:** `POST /users`

**Description:** Create a new user account.

**Request Body:**
```json
{
  "email": "user@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "phone": "+1234567890"
}
```

**Validation Rules:**
- `email` - Required, valid email format, unique
- `firstName` - Required, max 100 characters
- `lastName` - Required, max 100 characters
- `phone` - Optional, valid format

**Example Request:**
```bash
curl -X POST "https://localhost:7001/api/users" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "phone": "+1234567890"
  }'
```

### Get User by ID

**Endpoint:** `GET /users/{id}`

**Description:** Retrieve user details. Requires authentication.

**Example Request:**
```bash
curl -X GET "https://localhost:7001/api/users/550e8400-e29b-41d4-a716-446655440000" \
  -H "Authorization: Bearer <token>"
```

### Update User

**Endpoint:** `PUT /users/{id}`

**Description:** Update user details. User can only update their own profile.

**Request Body:**
```json
{
  "firstName": "Jane",
  "phone": "+0987654321"
}
```

---

## Health API

### Health Check

**Endpoint:** `GET /health`

**Description:** Application health status. No authentication required.

**Example Request:**
```bash
curl -X GET "https://localhost:7001/api/health"
```

**Success Response:** 200 OK
```json
{
  "status": "healthy",
  "version": "1.0.0",
  "timestamp": "2025-01-15T10:30:00Z",
  "uptime": "2d 5h 30m",
  "checks": {
    "database": "healthy",
    "cache": "healthy",
    "external_api": "healthy"
  }
}
```

---

## Rate Limiting

The API implements rate limiting to prevent abuse:

- **Default**: 100 requests per minute per IP
- **Response**: 429 Too Many Requests
- **Headers**: `X-RateLimit-Limit`, `X-RateLimit-Remaining`

---

## Best Practices

### 1. Always Check Response Status

```javascript
async function apiRequest(endpoint, options = {}) {
  const response = await fetch(endpoint, {
    headers: {
      'Content-Type': 'application/json',
      ...options.headers
    },
    ...options
  });
  
  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.message);
  }
  
  return response.json();
}
```

### 2. Include Bearer Token

```javascript
const headers = {
  'Authorization': `Bearer ${localStorage.getItem('token')}`,
  'Content-Type': 'application/json'
};

const response = await fetch('/api/users', { headers });
```

### 3. Handle Pagination

```javascript
async function getAllProducts() {
  let page = 1;
  let allProducts = [];
  
  while (true) {
    const data = await apiRequest(
      `/api/products?pageNumber=${page}&pageSize=50`
    );
    
    allProducts = allProducts.concat(data.data);
    
    if (data.data.length < 50) break;
    page++;
  }
  
  return allProducts;
}
```

---

For implementation examples, see the `examples/` directory.

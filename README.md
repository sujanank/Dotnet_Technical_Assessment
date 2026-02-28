# OpenSenseMap API - Dotnet Technical Assessment v1.0

A .NET 8 Web API application that integrates with OpenSenseMap external APIs, demonstrating SOLID principles, comprehensive error handling, and maintainable code architecture.

## Assessment Requirements

This solution implements five REST API endpoints that interact with OpenSenseMap:

1. **RegisterUser** (POST) - Register new users
2. **Login** (POST) - Authenticate users and cache tokens
3. **NewSenseBox** (POST) - Create sense boxes (requires authentication)
4. **GetSenseBoxById** (GET) - Retrieve sense box details
5. **Logout** (POST) - Logout user with Bearer token to clear cached token

## Architecture & Design

### Clean Architecture

The application follows clean architecture principles with clear separation of concerns:

- **Controllers Layer**: Handles HTTP requests/responses and routing
- **Services Layer**: Contains business logic and external API integration
- **Models Layer**: Defines data transfer objects and domain models
- **Middleware Layer**: Implements cross-cutting concerns

### Design Patterns

1. **Dependency Injection**: Used throughout for loose coupling and testability
2. **Repository Pattern**: Service layer abstracts data access logic
3. **Middleware Pattern**: Global exception handling and request processing
4. **Factory Pattern**: JSON serialization options configuration
5. **Caching Pattern**: In-memory token storage for performance

### SOLID Principles Implementation

- **Single Responsibility**: Each class has one clear purpose (Controllers handle HTTP, Services handle business logic, Middleware handles exceptions)
- **Open/Closed**: Extensible through interfaces without modifying existing code
- **Liskov Substitution**: All implementations follow their interface contracts
- **Interface Segregation**: Focused interfaces (IOpenSenseMapService, ITokenCacheService)
- **Dependency Inversion**: Dependencies injected through abstractions, not concrete implementations



### Project Structure

```
OpenSenseMapAPI/
├── Controllers/
│   ├── UsersController.cs          # User management endpoints
│   └── BoxesController.cs          # Sensor box endpoints
├── Services/
│   ├── IOpenSenseMapService.cs     # Service interface
│   ├── OpenSenseMapService.cs      # API integration implementation
│   ├── ITokenCacheService.cs       # Token cache interface
│   └── TokenCacheService.cs        # Token cache implementation
├── Models/
│   ├── RegisterRequest.cs          # Registration request model
│   ├── RegisterResponse.cs         # Registration response model
│   ├── LoginRequest.cs             # Login request model
│   ├── LoginResponse.cs            # Login response model
│   ├── NewSenseBoxRequest.cs       # Create box request model
│   ├── CreateBoxResponse.cs        # Create box response model
│   ├── GetSenseBoxResponse.cs      # Get box response model
│   └── ApiResponse.cs              # Generic API response wrapper
├── Middleware/
│   └── GlobalExceptionHandlerMiddleware.cs  # Exception handling
├── Exceptions/
│   └── OpenSenseMapException.cs    # Custom exception type
├── Program.cs                       # Application entry point
└── appsettings.json                # Configuration

OpenSenseMapAPI.Tests/
├── Services/
│   └── OpenSenseMapServiceTests.cs # Service layer tests
└── Controllers/
    ├── UsersControllerTests.cs     # User controller tests
    └── BoxesControllerTests.cs     # Box controller tests
```

## Technologies Stack

- **.NET 8.0**: Latest LTS version of .NET
- **ASP.NET Core Web API**: RESTful API framework
- **System.Text.Json**: High-performance JSON serialization
- **Serilog**: Structured logging framework
- **Swagger/OpenAPI**: API documentation and testing
- **xUnit**: Unit testing framework
- **Moq**: Mocking framework for tests
- **Microsoft.Extensions.Caching.Memory**: In-memory caching

## Prerequisites

- .NET 8 SDK or later
- Visual Studio 2022 / VS Code
- Windows/Linux

## Installation & Setup

### 1. Download the Project from repository

```bash
cd OpenSenseMapAPI
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Build the Solution

```bash
dotnet build
```

### 4. Run the Application

```bash
cd OpenSenseMapAPI
dotnet run
```

The API will start at:
- HTTP: `http://localhost:5000`

### 5. Access Swagger UI

Open your browser and navigate to:
```
http://localhost:5000/index.html
```

The Swagger UI will be displayed automatically, showing all available endpoints with interactive documentation.

#### Using the Logout Endpoint in Swagger

The logout endpoint requires a Bearer token in the Authorization header. In Swagger UI:

1. First, call the `/api/users/login` endpoint to get your token
2. For the `/api/users/logout` endpoint, you'll see a lock icon next to it
3. Click the lock icon and enter your token (without the "Bearer" prefix)
4. Click "Authorize" to save the token
5. Now you can execute the logout endpoint with the token automatically included in the header

Alternatively, you can directly enter the full Bearer token in the Authorization parameter field:
```
Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

## Running Tests

### Run All Tests

```bash
dotnet test
```

### Run Tests with Detailed Output

```bash
dotnet test --verbosity detailed
```


## API Endpoints

### 1. Register User
**POST** `/api/users/register`

**Request Body:**
```json
{
  "name": "Assessment01",
  "email": "Assessment01@test.com",
  "password": "12345678"
}
```

**Response:**
```json
{
  "code": "created",
  "message": "Successfully registered new user.",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "data": {}
}
```

### 2. Login
**POST** `/api/users/login`

**Request Body:**
```json
{
  "email": "Assessment01@test.com",
  "password": "12345678"
}
```

**Response:**
```json
{
  "code": "Authorized",
  "message": "Successfully signed in",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "refresh-token-value",
  "data": {}
}
```

### 3. Create New Sense Box
**POST** `/api/boxes`

**Request Body:**
```json
{
  "_id": "507f1f77bcf86cd799439011",
  "createdAt": "2026-02-28T06:22:04.143Z",
  "exposure": "indoor",
  "model": "homeV2Wifi",
  "name": "My Home Sensor",
  "updatedAt": "2026-02-28T06:22:04.143Z",
  "currentLocation": {
    "timestamp": "2026-02-28T06:22:04.136Z",
    "coordinates": [51.5074, -0.1278, 10],
    "type": "Point"
  },
  "sensors": [...],
  "access_token": "box-access-token"
}
```

**Note:** User must be logged in first. Token is automatically retrieved from cache using email.

### 4. Get Sense Box by ID
**GET** `/api/boxes/{senseBoxId}`

**Example:**
```
GET /api/boxes/507f1f77bcf86cd799439011

**Response:**
```json
{
  "_id": "507f1f77bcf86cd799439011",
  "createdAt": "2026-02-28T06:22:04.143Z",
  "exposure": "outdoor",
  "grouptag": [],
  "image": "507f1f77bcf86cd799439011.png",
  "currentLocation": {
    "coordinates": [7.64568, 51.962372],
    "timestamp": "2026-02-28T06:22:04.143Z",
    "type": "Point"
  },
  "name": "My Sensor Box",
  "sensors": [
    {
      "_id": "sensor-id",
      "title": "Temperature",
      "unit": "°C",
      "sensorType": "HDC1080",
      "icon": "osem-thermometer",
      "lastMeasurement": {
        "value": "22.5",
        "createdAt": "2026-02-28T06:22:04.143Z"
      }
    }
  ],
  "updatedAt": "2026-02-28T06:22:04.143Z"
}
``````

### 5. Logout
**POST** `/api/users/logout`

**Headers:**
```
Authorization: Bearer {your-token-here}
```

**Example:**
```
POST /api/users/logout
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response:**
```json
{
  "success": true,
  "message": "Logout successful",
  "data": {
    "message": "Logged out successfully"
  },
  "errors": []
}
```

**Note:** The Authorization header with Bearer token is required. The token is used to identify the user and clear both the token cache and reverse mapping.

## Authentication Flow

1. **Register** a new user (optional if user exists)
2. **Login** to receive authentication token (stored in cache)
3. **Create Sense Box** using cached token
4. **Logout** with Bearer token to clear cached token

## Logging

Serilog is configured to log to:
- **Console** - Real-time log output
- **File** - `Logs/log-{Date}.txt` with daily rolling

Log levels:
- Information: Normal operations
- Warning: Non-critical issues
- Error: Exceptions and failures

## Error Handling

Global exception handling middleware catches all exceptions and returns standardized error responses:

```json
{
  "success": false,
  "message": "Error description",
  "data": null,
  "errors": ["Detailed error message"]
}
```

**Exception Types Handled:**
- `OpenSenseMapException` - Custom API errors with specific status codes
- `UnauthorizedAccessException` - Returns 401
- `ArgumentException` / `ArgumentNullException` - Returns 400
- `ValidationException` - Returns 400
- Generic exceptions - Returns 500

## Testing Coverage

The test suite includes:

- **Controller Tests**: Validate HTTP request/response handling
- **Service Tests**: Test business logic and external API integration
- **Middleware Tests**: Verify exception handling behavior
- **Token Cache Tests**: Ensure proper token storage and retrieval

**Total Test Cases:** 40+ comprehensive unit tests covering:
- Token cache functionality
- Multi-user token isolation
- Token update scenarios


## Code Quality Features

- **Data Validation** - Model validation with data annotations
- **Structured Logging** - Serilog with contextual information
- **Dependency Injection** - Proper DI container usage
- **Async/Await** - All I/O operations are asynchronous
- **Error Handling** - Global middleware with specific exception types
- **Clean Architecture** - Separation of concerns
- **Testability** - Interfaces and mocking support
- **API Documentation** - Swagger with XML comments

## Deployment

### Build for Production

```bash
dotnet publish -c Release -o ./publish
```

### Run Published Application

```bash
cd publish
dotnet OpenSenseMapAPI.dll
```



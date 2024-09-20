
# Contract Management System - Documentation

## Overview

This project is a **Contract Management System** for companies, built with **ASP.NET Core**. It allows businesses to manage contracts with suppliers, clients, and partners. The main features include:

- **JWT Authentication**: Protects API endpoints, ensuring that only authenticated users can perform sensitive operations.
- **Automatic Contract Renewal**: Contracts can be automatically renewed when they are about to expire.
- **Contract CRUD**: Create, read, update, and delete contracts via API.
- **Dashboard and Reports**: Supports contract viewing by status, date, and renewal periods.
- **Swagger UI**: Provides a graphical interface for testing and interacting with the API.

## Features

- **JWT Authentication**: Users can log in and receive a JWT token that allows access to protected API endpoints.
- **Automatic Renewal**: Contracts with the auto-renewal option enabled will be automatically renewed when they are about to expire.
- **CRUD Operations for Contracts**: Allows creating, reading, updating, and deleting contracts via API.
- **Scheduled Tasks with Hangfire**: Uses Hangfire to automatically renew contracts daily.
- **Protected Endpoints with `[Authorize]`**: Only authenticated users can access protected API endpoints.
- **Swagger Integration**: Allows you to view and test the API via Swagger.

## Requirements

- **.NET 6 or later**
- **SQL Server** (or LocalDB for development)
- **Visual Studio 2022** or any compatible .NET IDE
- **NuGet Packages**:
  - `Microsoft.EntityFrameworkCore`
  - `Microsoft.EntityFrameworkCore.SqlServer`
  - `Microsoft.EntityFrameworkCore.Tools`
  - `Microsoft.AspNetCore.Authentication.JwtBearer`
  - `FluentValidation.AspNetCore`
  - `Hangfire`
  - `Swashbuckle.AspNetCore` (for Swagger)

## Installation and Configuration

### 1. Clone the Repository

```bash
git clone https://github.com/Almeida96/ContractManagement.git
```

### 2. Configure the Connection String

In the `appsettings.json` file, configure the connection string for your SQL Server database:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\mssqllocaldb;Database=ContractManagementDb;Trusted_Connection=True;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### 3. Install Dependencies

Run the following command to restore the **NuGet** packages:

```bash
dotnet restore
```

### 4. Apply Migrations and Update the Database

Run the following command to apply the migrations and create the database:

```bash
dotnet ef database update
```

### 5. Run the Application

After configuration, you can run the application with:

```bash
dotnet run
```

The API will be available at `http://localhost:{PORT}`.

## How JWT Authentication Works

The system uses **JWT (JSON Web Token)** for authentication. Here's how it works:

1. **User Login**: The user provides their credentials (username and password) to the `/api/Auth/login` endpoint. If the credentials are valid, a **JWT token** is generated and returned in the response.

2. **Token Usage**: The token must be included in the **Authorization header** of all subsequent requests to protected endpoints. The format is:

   ```
   Authorization: Bearer {your_jwt_token}
   ```

3. **Token Validation**: The server validates the token on each request. If the token is valid and not expired, the request is processed. Otherwise, a **401 Unauthorized** error is returned.

4. **Token Expiration**: JWT tokens have an expiration time (`exp` claim). If the token is used after its expiration time, it will be considered invalid, and you will need to log in again to obtain a new token.

### Example of Token Usage:

- **Login Request**:

```json
{
  "username": "admin",
  "password": "password"
}
```

- **Using the Token in Subsequent Requests**:

Once you have the token, include it in the headers as follows:

```bash
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6ImFkbWluIiwibmJmIjoxNzI2ODU3MTAwLCJleHAiOjE3MjY4NjQzMDAsImlhdCI6MTcyNjg1NzEwMH0.rm9LytMbAsWS7I2iH0D51NmN9wNR9phvfgSpJUr--Vs
```

#### Example cURL Request to Create a Contract:

```bash
curl -X POST   'https://localhost:7219/api/Contratos'   -H 'accept: text/plain'   -H 'Authorization: Bearer {your_jwt_token}'   -H 'Content-Type: application/json'   -d '{
  "id": 0,
  "empresa": "Amazon",
  "parteEnvolvida": "Contrato x",
  "dataInicio": "2024-09-20",
  "dataFim": "2025-09-20",
  "valor": 5000,
  "periodicidade": "mensal",
  "status": "ativo",
  "documento": "contrato_123.pdf",
  "renovacaoAutomatica": true
}'
```

Ensure you replace `{your_jwt_token}` with the actual token you received after logging in.

### Important Notes:

- The token has a limited lifetime. After it expires, you will need to log in again to get a new token.
- The token should be passed in the **Authorization** header for all requests to protected routes.

## API Documentation

### Authentication (JWT)

To access protected endpoints, you first need to log in and obtain a JWT token.

- **POST /api/auth/login**

  Send a JSON object with the user's credentials:

  ```json
  {
    "username": "admin",
    "password": "password"
  }
  ```

  **Response**:

  ```json
  {
    "token": "your_jwt_token"
  }
  ```

- Use the JWT token returned to authorize your requests in Swagger and for protected endpoints. Click **Authorize** in the Swagger UI and input the token in the format:

  ```
  Bearer {your_jwt_token}
  ```

## Contributing

Contributions are welcome! Follow the steps below to contribute:

1. **Fork** this repository.
2. Create a branch for your feature (`git checkout -b my-feature`).
3. Commit your changes (`git commit -am 'Add new feature'`).
4. Push to the branch (`git push origin my-feature`).
5. Open a **Pull Request**.

---

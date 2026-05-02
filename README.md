# Custom Identity Server
Production-ready ASP.NET Identity Server built from scratch, without third-party auth providers.

#### Core Features
Registration, login, logout, token refresh, email confirmation, password & email change, secure code generation

#### Token Management
- Short-lived JWT access tokens with configurable lifetime
- Rotating refresh tokens in httpOnly cookies, new pair issued on every refresh
- Automatic revocation on rotation
- Token family tracking (stolen token detection)
- Role-based claims embedded in JWT

#### Session & Security
- Stateless microservice auth with zero roundtrips
- Multi-device session control
- Brute-force protection and rate limiting
- Account lockout with unlock functionality
- Bcrypt-hashed passwords
- Configurable per-environment secrets

#### Architecture & Quality
- Vertical Slice Architecture + Minimal API
- Dapper + PostgreSQL (no EF overhead)
- Every endpoint covered by integration tests with real DB via Testcontainers
- Docker-ready
- CI/CD pipeline

## 👷 Frameworks, Libraries and Technologies

- [.NET 10](https://github.com/dotnet/core)
- [C#](https://github.com/dotnet/csharplang)
- [ASP.NET Core](https://github.com/dotnet/aspnetcore)
- [Dapper](https://github.com/DapperLib/Dapper)
- [PostgreSQL](https://github.com/postgres)
- [FluentValidation](https://github.com/FluentValidation)
- [xUnit](https://github.com/xunit/xunit)
- [Testcontainers](https://github.com/testcontainers)
- [Scalar](https://github.com/ScalaR/ScalaR)
- [Docker](https://github.com/docker)


## 🐳 List of docker containers

- **api.app** - container for all application layers

- **api.database** - postgresql database container



## 🖨️ Scalar UI documentation (local dev access)

        http://localhost:8080/scalar  

## 🩺 How to run tests

*Allows you to run all integration and unit tests.*

   ```sh
 dotnet test  # donet SKD is required
```

## 🚜 How to run the server

1. Build and start Docker images based on the configuration defined in the docker-compose.yml

   ```sh
    make up  # docker-compose up -d --build
   ```

2. Stop and remove containers
   ```sh
    make down  # docker-compose down
   ```
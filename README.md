# Contract Management API

An ASP.NET Core API for a shared contract register: authenticated CRUD, validated dates and amounts, and optional annual renewal jobs. The original Portuguese route and field names remain supported.

## Run locally

Install the .NET 8 SDK. SQLite is the default; its database is created on first startup. Configure one local demo account and a signing key before launching.

PowerShell:

```powershell
$env:Auth__Key = [Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
$env:Auth__Username = "admin"
$env:Auth__Password = Read-Host "Local demo password (12 to 128 characters)"
dotnet run --project ContractManagement --launch-profile http
```

On Bash, use `export Auth__Key="$(openssl rand -base64 32)"`, `export Auth__Username=admin`, and `read -rs Auth__Password; export Auth__Password` before the same run command.

The API listens on `http://localhost:5012`; development OpenAPI is at `/swagger`. Startup fails if the signing key is shorter than 32 UTF-8 bytes or the account configuration is missing. Never commit these values.

## API

First request `POST /api/auth/login` with `{"username":"admin","password":"your configured password"}`. The response contains `token` and `expiresAt`. Send `Authorization: Bearer TOKEN` on contract requests.

| Method and route | Result |
| --- | --- |
| `GET /api/contratos?offset=0&limit=50` | Contracts ordered by ID; limit 1–200 |
| `GET /api/contratos/{id}` | One contract, or 404 |
| `POST /api/contratos` | 201 and a Location header; omit the ID |
| `PUT /api/contratos/{id}` | Replace a contract; body ID must match the route |
| `DELETE /api/contratos/{id}` | 204, or 404 |

Example creation body:

```json
{
  "empresa": "Example Ltd",
  "parteEnvolvida": "Supplier",
  "dataInicio": "2026-01-01",
  "dataFim": "2027-01-01",
  "valor": 1200.50,
  "periodicidade": "anual",
  "status": "ativo",
  "documento": "",
  "renovacaoAutomatica": true
}
```

Dates are returned as `yyyy-MM-dd`; legacy `dd-MM-yyyy` input is also accepted. The end must follow the start. Amounts must be positive, with at most two decimal places and a maximum of 9,999,999,999.99. Status is `ativo`, `cancelado` or `encerrado`. Invalid bodies return 400; missing or invalid tokens return 401. Login has a shared limit of ten attempts per minute and returns 429 above it.

## Storage and scheduled renewals

The default `ContractManagement/contracts.db` is a local SQLite database, configured by `ConnectionStrings__Sqlite`. Back it up before changing schema. SQLite uses `EnsureCreated` for a new demo database; it does not apply the historical SQL Server migrations or upgrade an existing SQLite schema.

For SQL Server, set `Database__UseSqlite=false` and `ConnectionStrings__DefaultConnection`, then apply the checked-in migrations with the .NET EF 8 tool:

```sh
dotnet ef database update --project ContractManagement
```

Set `Jobs__Enabled=true` only for SQL Server. Hangfire runs one worker server and schedules the renewal job daily at 09:00 UTC. No public Hangfire dashboard is exposed.

Renewal selects automatic contracts with status `ativo` whose end date falls between today and seven days ahead, inclusive. It moves the start to the previous end and adds one year to the end. Cancelled and already expired contracts are skipped. Re-running the job leaves the new term unchanged. `periodicidade` records billing cadence; this demo's renewal term is always one year.

## Tests

```sh
dotnet test ContractManagement.Tests/ContractManagement.Tests.csproj
```

HTTP integration tests use temporary SQLite databases. They check issued-token access, CRUD, invalid contracts, pagination and renewal eligibility/idempotence. SQL Server deployment and Hangfire scheduling require a separate configured SQL Server instance and are not covered by the SQLite tests.

## Scope

This is a single-account demonstration. Every authenticated request sees the same register. There is no tenant isolation, user registration, document upload, approval workflow or reporting dashboard. `documento` is a text reference, not uploaded file storage. Authentication and contract data need a managed identity provider, durable secret storage, HTTPS and a deployment-specific backup policy before multi-user use.

[Contribution notes](CONTRIBUTING.md) · [Changes](CHANGELOG.md)

# Contributing

Use .NET 8 and run `dotnet test ContractManagement.Tests/ContractManagement.Tests.csproj` before opening a change. Add an HTTP regression test when changing validation or authorization.

Keep Portuguese API field names compatible. Document a schema migration separately for SQL Server and SQLite; do not silently replace a user's database. Use disposable data for examples. Configuration secrets belong in environment variables or a secret store.

Tests use SQLite and do not demonstrate that SQL Server/Hangfire deployment works. Describe that validation separately when changing scheduled jobs.

# Changes

## Unreleased

- Use one configured JWT signing key for login and validation, with issuer, audience, algorithm and expiry checks.
- Replace fixed demo credentials with startup-validated configuration and bound login attempts.
- Add a local SQLite path and keep SQL Server/Hangfire optional.
- Return ISO dates while accepting the legacy date input format.
- Bound list queries and validate contract statuses, amounts and field lengths.
- Skip cancelled and expired contracts during renewal; remove the duplicate Hangfire server and exposed dashboard.
- Add temporary-database HTTP integration tests and document the actual API scope.

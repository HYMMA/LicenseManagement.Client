# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.3.0] - 2026-05-15

Addresses findings from `docs/AUDIT-2026-05-14.md` after the companion server-side fixes in
`docs/WEBAPP-FIXES-2026-05-15.md`.

### Added

- `Idempotency-Key` support on every `Create*` method. New overloads accept a stable key; supplying it makes a retried POST replay the original response instead of creating a duplicate. (C-2, M-4)
- `LicenseManagementException.CorrelationId` carries the server's `X-Correlation-Id`, surfaced into the exception message for support escalation. (M-5)
- `WebhookOptions.MaxBodyBytes` (default 1 MB) caps inbound webhook bodies, with `413 Payload Too Large` returned before the body is read. (H-5)
- `WebhookOptionsValidator` rejects misconfigured tolerance and body size at startup. (M-10)
- HTTP requests now carry a `User-Agent: LicenseManagement.Client/<version>` header so server logs can attribute traffic to the SDK version. (L-3)
- New regression tests cover idempotency-key forwarding, the rotate-code flow, the webhook signature fixes (Unix-seconds + ISO fallback + raw-byte FixedTimeEquals), and problem+json error parsing.

### Changed

- **`ResetReceiptCodeAsync` is now atomic.** It calls the server's new `POST /receipt/{id}/rotate-code` endpoint, which performs the void + create inside a single `TransactionScope`. A failed retry can no longer leave the buyer with a voided receipt and no replacement. (C-1)
- **Webhook signatures are compared as raw HMAC bytes** using `CryptographicOperations.FixedTimeEquals` on .NET 6+, with a constant-time byte comparator on netstandard2.0. The previous hex-string comparator is gone. (H-1)
- **Webhook timestamps accept Unix seconds first** (Stripe / GitHub convention) and fall back to ISO-8601. ISO strings without an explicit offset are parsed with `DateTimeStyles.AssumeUniversal` so a server clock in local time can no longer slip past replay protection. The webhook filter also reads the legacy `X-Webhook-Timestamp-ISO` header. (C-3)
- **API error messages now include method + URL + RFC 7807 `detail`.** When the webapp returns `application/problem+json`, the SDK extracts the `detail` (or `title`) field into the exception message. (H-3)
- **`HttpClient` configuration moved out of the constructor** into `AddLicenseManagementClient(...)` / `AddHttpClient<>` so `IHttpClientFactory` can pool sockets correctly and caller-supplied delegates are not overwritten. (M-1)
- **Cancellation tokens flow through `EnsureSuccessAsync`** so error-body reads are cancellable on .NET 6+. (H-2)
- **All `Get*By*` lookups now throw `LicenseManagementException` on 404** for a single contract. `GetWebhookAsync` and `GetWebhookDeliveryAsync` continue to return `null` on 404 by explicit design; XML docs flag this distinction. (M-8)
- The `WebhookSignatureExtensions.IsValidWebhook` extension now accepts an optional tolerance argument instead of always using the 5-minute default. (L-5)
- Removed all null-forgiving `!` operators on JSON deserialization; empty/null bodies on `Create*` now throw a typed exception that names the request. (H-4)
- Public classes are now `sealed`. (M-2)
- All files use file-scoped namespaces. (L-7)
- `WebhookSignatureValidator.ValidateTimestamp` is now `internal`. (L-8)
- Redundant `[JsonPropertyName]` attributes removed; the camelCase `JsonNamingPolicy` is the single source of truth. (L-1)
- `ConfigureAwait(false)` added throughout library code. (L-2)

### Fixed

- The optional `LicenseManagementException(message, statusCode, innerException, …)` constructor now also accepts `responseContent` and `correlationId`. (L-4)

### Audit findings not applied

- **H-6** (test project on `net10.0`) — the audit assumed net10 was unreleased; it GA'd as LTS in November 2025. The test project stays on `net10.0`.

## [1.2.1] - 2026-05-13

### Fixed

- `GetComputersAsync`, `GetProductsAsync`, `GetWebhooksAsync`, and `GetWebhookDeliveriesAsync` no longer throw `System.Text.Json.JsonException` ("The input does not contain any JSON tokens") when the server returns HTTP 204 No Content or HTTP 200 with an empty body. They now return an empty sequence.
- The same empty-body guard was applied to the single-item `GetLicenseAsync`, `GetReceiptAsync`, `GetProductAsync`, `GetComputerAsync`, and `GetWebhookAsync` methods, which now return `null` consistently on 204 or 200-with-empty-body responses.
- Deserialization is centralized in new `ReadJsonOrDefaultAsync<T>` / `ReadJsonOrEmptyAsync<T>` helpers so future endpoints inherit the guard automatically.

## [1.2.0] - 2025-12-17

### Added

- `GetReceiptsAsync` method to retrieve all receipts for a buyer/product combination.

## [1.1.0] - 2025-12-17

### Added

- `ResetReceiptCodeAsync` method for resetting receipt codes. This voids the existing receipt and creates a new one with a fresh code while preserving the original buyer email, product, expiration date, and quantity.

## [1.0.0] - 2024-12-11

### Added

- Initial release of LicenseManagement.Client SDK
- License management operations (Get, Create, Update)
- Receipt management operations (Get, Create, Update, GenerateCode)
- Product management operations (Get, GetAll, Create)
- Computer management operations (Get, Register, GetAll)
- Signing key retrieval
- Webhook management with full CRUD support
- Webhook signature validation for secure event handling
- ASP.NET Core integration with `EnsureIsFromLicenseManagementAttribute`
- Dependency injection extensions for easy setup
- Multi-targeting support: .NET Standard 2.0, .NET 6.0, .NET 8.0

# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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

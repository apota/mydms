# Shared Core Libraries

This directory contains shared core libraries and components that are used across different modules of the Dealership Management System. These components provide common functionality and help maintain consistency throughout the application.

## Contents

- `/models` - Shared domain models
- `/interfaces` - Common interfaces
- `/exceptions` - Custom exception types
- `/utils` - Utility classes and helper functions
- `/extensions` - Extension methods
- `/services` - Common services (logging, caching, etc.)
- `/validation` - Validation rules and validators

## Naming Conventions

- All shared libraries should be in the `DMS.Shared` namespace
- Class names should be clear and descriptive
- Follow standard C# naming conventions throughout

## Usage Guidelines

When adding new shared components:

1. Ensure the component is truly reusable across multiple modules
2. Keep dependencies minimal to avoid circular references
3. Write comprehensive unit tests for all shared code
4. Document public APIs with XML comments
5. Avoid breaking changes to existing interfaces

## Key Abstractions

The following key abstractions are defined in this directory:

- `IEntity` - Base interface for all domain entities
- `IRepository<T>` - Generic repository interface
- `IUnitOfWork` - Unit of work pattern interface
- `ILogger<T>` - Logging abstraction
- `IValidator<T>` - Validation abstraction

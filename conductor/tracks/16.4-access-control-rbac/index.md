# Track 16.4: Access Control & RBAC

## Status
- [x] Specification: `spec.md`
- [x] Implementation Plan: `plan.md`
- [x] Implementation: Completed

## Objectives
Implement a robust Role-Based Access Control (RBAC) system to distinguish between 'Admin' and 'Reader' roles.

## Context
Following the completion of Track 16.3 (Identity Expansion), we now need to enforce authorization rules across the backend and frontend.

## Index
- [Specification](./spec.md)
- [Implementation Plan](./plan.md)
- [Metadata](./metadata.json)

## Execution Summary
The implementation successfully enforced authorization using ASP.NET Core Identity roles.
- **Backend**: Restricted Article CRUD to 'Admin' and implemented ownership checks for 'Reader' comments via `ICurrentUserService`.
- **Frontend**: Integrated `PermissionGate` component and updated `AuthContext` to handle JWT role claims, ensuring a reactive UI.
- **Security**: Refactored `DbInitializer` to use environment variables for admin seeding.
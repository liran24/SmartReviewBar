# ARCHITECTURE — SMART STICKY REVIEWER

This document explains the Clean Architecture boundaries, the dependency injection composition root, and the extensible design for **review providers** and **paid features**.

## Clean Architecture boundaries

### `SmartStickyReviewer.Domain` (innermost)

**Rules**

- Contains **Entities**, **Value Objects**, **Enums**, and **Interfaces** only.
- Contains **no framework references** (no ASP.NET Core, no MongoDB).
- Contains **no service registration code**.

**What lives here**

- `Entities/SiteConfiguration.cs`: the per-site configuration aggregate.
- `Interfaces/IReviewProvider.cs`: provider abstraction (extensible).
- `Interfaces/IFeaturePolicy.cs`: feature flag abstraction.
- `Interfaces/ISiteConfigurationRepository.cs`: persistence abstraction.

### `SmartStickyReviewer.Application` (business orchestration)

**Rules**

- Depends only on `Domain`.
- Contains use cases, orchestration, and business rules.
- Contains **no service registration code**.

**What lives here**

- Use cases:
  - `GetAdminConfigurationUseCase`
  - `SaveAdminConfigurationUseCase`
  - `GetWidgetDataUseCase`
- Provider selection:
  - `ReviewProviderSelector` selects the provider by calling `IReviewProvider.CanHandle(...)`.
- Plan → features mapping:
  - `PlanFeatureMatrix` is **data-driven** and avoids branching on plan (no `if(plan == X)`).

### `SmartStickyReviewer.Infrastructure` (outer adapters)

**Rules**

- Depends on `Domain` + `Application`.
- Implements repositories/policies/providers.
- Contains **NO service registration code**.

**What lives here**

- MongoDB repository:
  - `MongoSiteConfigurationRepository` uses `MongoDB.Driver`.
- Feature policy implementation:
  - `PlanBasedFeaturePolicy` uses the stored site plan and `PlanFeatureMatrix`.
- Review providers:
  - `ManualReviewProvider`
  - `JudgeMeReviewProvider` (present for extensibility; **explicitly does not call Judge.me** per requirements)
- Notification placeholder:
  - `EmailStoreOwnerNotifier` (no-op placeholder)

### `SmartStickyReviewer.Api` (composition root + delivery)

**Rules**

- Contains controllers, DTOs, and HTTP pipeline.
- **All DI registrations happen in one place only:** `Program.cs`.

## Dependency Injection composition root

The **only** place where services are registered is:

- `src/SmartStickyReviewer.Api/Program.cs`

`Domain`, `Application`, and `Infrastructure` contain **zero** DI code.

## Review provider design (extensible)

### Abstraction

`IReviewProvider` is defined in `Domain`:

- `bool CanHandle(ReviewContext context)`
- `ReviewResult GetReview(ReviewContext context)`

### Provider selection

Selection is generic and provider-agnostic:

- The Application layer creates a `ReviewContext` including `DesiredProvider`
- `ReviewProviderSelector` chooses the first provider where `CanHandle(context)` is `true`

No provider-specific selection logic is allowed outside provider classes; selection is purely via `CanHandle`.

### How to add a new review provider

1. Create a class implementing `IReviewProvider` (place it in `Infrastructure/Providers`)
2. Implement:
   - `CanHandle` (typically check `context.DesiredProvider`)
   - `GetReview` (fetch/compute review data)
3. Register it in one place:
   - `src/SmartStickyReviewer.Api/Program.cs`

Example registration pattern:

- `builder.Services.AddSingleton<IReviewProvider, MyNewProvider>();`

## Fallback strategy

When the primary provider fails (throws or returns failure), the Application use case applies:

1. **Manual rating if available**
2. **Fallback text if configured AND `ManualFallbackText` is enabled**
3. **If `EmailNotificationOnFailure` is enabled → notify store owner (placeholder)**
4. Otherwise, the site widget **fails silently** (`ShouldRender = false`)

## Feature flags & plans (no `if(plan == X)`)

### Plans

- `Free`
- `Pro`
- `Premium`

### Features

- `MultipleReviewProviders`
- `ManualFallbackText`
- `EmailNotificationOnFailure`
- `AdvancedStyling`

### Plan → features mapping

The mapping is owned by `PlanFeatureMatrix` (Application layer) and is used by the feature policy implementation.

Current mapping:

| Plan | Enabled features |
|------|------------------|
| Free | *(none)* |
| Pro | MultipleReviewProviders, ManualFallbackText |
| Premium | MultipleReviewProviders, ManualFallbackText, EmailNotificationOnFailure, AdvancedStyling |

## How paid features are enforced

Paid features are enforced **in the Application layer** during save and read operations:

- `SaveAdminConfigurationUseCase` strips/normalizes settings that the site is not entitled to.
  - Example: if `AdvancedStyling` is disabled, custom colors are reset to defaults.
  - Example: if `ManualFallbackText` is disabled, `FallbackText` is cleared.
- `GetWidgetDataUseCase` applies feature gates during rendering.
  - Example: if `MultipleReviewProviders` is disabled, the primary provider is forced to Manual.

The admin widget can still attempt to submit premium fields; the backend remains the source of truth.

## How to add a new paid feature

1. Add a new enum value in `Domain/Enums/Feature.cs`
2. Add it to `PlanFeatureMatrix` in `Application/Services/PlanFeatureMatrix.cs`
3. Enforce the feature in an Application use case
   - Save-time enforcement: sanitize config and return warnings
   - Read-time enforcement: gate runtime behavior (site widget output)
4. Expose the feature in the Admin API response
   - `GetAdminConfigurationUseCase` already enumerates all known features (update the list if needed)

## Notes on “no external APIs”

- No Wix API calls
- No OAuth
- No Judge.me API calls

The `JudgeMeReviewProvider` exists to demonstrate provider extensibility and to trigger the fallback strategy. It always fails by design.


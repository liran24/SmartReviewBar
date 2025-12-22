# Smart Sticky Reviewer - Architecture Documentation

This document describes the architecture of Smart Sticky Reviewer, a .NET 8 application built following Clean Architecture principles.

## Table of Contents

1. [Overview](#overview)
2. [Clean Architecture Layers](#clean-architecture-layers)
3. [Dependency Injection](#dependency-injection)
4. [Review Provider System](#review-provider-system)
5. [Feature Policy System](#feature-policy-system)
6. [Fallback Strategy](#fallback-strategy)
7. [Data Flow](#data-flow)
8. [Extension Points](#extension-points)

## Overview

Smart Sticky Reviewer is designed with the following principles:

- **Clean Architecture**: Clear separation of concerns with dependency rules
- **SOLID Principles**: Each component has a single responsibility
- **Dependency Inversion**: High-level modules don't depend on low-level modules
- **Interface Segregation**: Small, focused interfaces
- **Open/Closed Principle**: Easy to extend without modifying existing code

```
┌─────────────────────────────────────────────────────────────┐
│                         API Layer                           │
│              (Controllers, DTOs, Program.cs)                │
│                    [DI COMPOSITION ROOT]                    │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    Infrastructure Layer                      │
│        (MongoDB Repos, Providers, Policies)                  │
│              [Implements Domain Interfaces]                  │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    Application Layer                         │
│            (Use Cases, Services, Business Logic)             │
│                [Orchestrates Domain Objects]                 │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                      Domain Layer                            │
│         (Entities, Value Objects, Interfaces)                │
│                   [Core Business Rules]                      │
└─────────────────────────────────────────────────────────────┘
```

## Clean Architecture Layers

### Domain Layer (`SmartStickyReviewer.Domain`)

The innermost layer containing core business logic. Has no dependencies on external frameworks.

**Contains:**
- **Entities**: `SiteConfiguration`, `ManualReview`, `ProviderFailureLog`
- **Value Objects**: `ReviewContext`, `ReviewResult`, `FallbackConfiguration`, `StickyBarStyle`
- **Enums**: `Plan`, `Feature`, `ReviewProviderType`
- **Interfaces**: Repository contracts, Provider contracts, Policy contracts

**Key Rules:**
- No framework dependencies (no ASP.NET, no MongoDB)
- Contains only business logic and contracts
- Other layers depend on this layer, never the reverse

```csharp
// Domain defines interfaces, not implementations
public interface IReviewProvider
{
    ReviewProviderType ProviderType { get; }
    string ProviderName { get; }
    bool CanHandle(ReviewContext context);
    Task<ReviewResult> GetReviewAsync(ReviewContext context, CancellationToken ct);
}
```

### Application Layer (`SmartStickyReviewer.Application`)

Contains use cases and business rules orchestration. Depends only on Domain.

**Contains:**
- **Use Cases**: `GetReviewUseCase`, `SaveSiteConfigurationUseCase`, etc.
- **Services**: `ReviewProviderSelector`, `FallbackService`
- **Service Interfaces**: Internal application services

**Key Rules:**
- Depends only on Domain layer
- Contains NO dependency injection code
- Orchestrates domain objects and interfaces
- Implements application-specific business rules

```csharp
// Application uses Domain interfaces, doesn't know implementations
public sealed class GetReviewUseCase
{
    private readonly ISiteConfigurationRepository _configRepo;
    private readonly IReviewProviderSelector _providerSelector;
    
    public GetReviewUseCase(
        ISiteConfigurationRepository configRepo,  // Injected
        IReviewProviderSelector providerSelector) // Injected
    {
        _configRepo = configRepo;
        _providerSelector = providerSelector;
    }
}
```

### Infrastructure Layer (`SmartStickyReviewer.Infrastructure`)

Implements interfaces defined in Domain. Contains external dependencies.

**Contains:**
- **Persistence**: MongoDB repositories
- **Providers**: `JudgeMeReviewProvider`, `ManualReviewProvider`, `EmailNotificationProvider`
- **Policies**: `PlanBasedFeaturePolicy`

**Key Rules:**
- Implements Domain interfaces
- Contains external framework dependencies (MongoDB.Driver)
- Contains NO dependency injection registration code
- All services receive dependencies via constructor injection

```csharp
// Infrastructure implements Domain interfaces
public sealed class SiteConfigurationRepository : ISiteConfigurationRepository
{
    private readonly IMongoCollection<SiteConfiguration> _collection;

    public SiteConfigurationRepository(IMongoDatabase database) // Injected
    {
        _collection = database.GetCollection<SiteConfiguration>("site_configurations");
    }
}
```

### API Layer (`SmartStickyReviewer.Api`)

Entry point and **COMPOSITION ROOT** for dependency injection.

**Contains:**
- **Controllers**: HTTP endpoints
- **DTOs**: Data transfer objects
- **Program.cs**: The ONLY place where DI registrations occur

**Key Rules:**
- This is the ONLY layer that knows about all other layers
- Contains ALL dependency injection registrations
- Maps between DTOs and domain objects
- Handles HTTP concerns

## Dependency Injection

### Composition Root Pattern

**All DI registrations are in `Program.cs`**. This is a critical architectural decision.

```csharp
// Program.cs - THE ONLY PLACE FOR DI REGISTRATIONS

// =============================================================================
// COMPOSITION ROOT
// =============================================================================

// MongoDB
builder.Services.AddSingleton<IMongoClient>(sp => new MongoClient(connectionString));
builder.Services.AddSingleton<IMongoDatabase>(sp => 
    sp.GetRequiredService<IMongoClient>().GetDatabase(databaseName));

// Repositories
builder.Services.AddScoped<ISiteConfigurationRepository, SiteConfigurationRepository>();
builder.Services.AddScoped<IManualReviewRepository, ManualReviewRepository>();

// Policies
builder.Services.AddSingleton<IFeaturePolicy, PlanBasedFeaturePolicy>();

// Providers
builder.Services.AddScoped<JudgeMeReviewProvider>();
builder.Services.AddScoped<ManualReviewProvider>();
builder.Services.AddScoped<IEnumerable<IReviewProvider>>(sp => new List<IReviewProvider>
{
    sp.GetRequiredService<JudgeMeReviewProvider>(),
    sp.GetRequiredService<ManualReviewProvider>()
});

// Application Services
builder.Services.AddScoped<IReviewProviderSelector, ReviewProviderSelector>();
builder.Services.AddScoped<IFallbackService, FallbackService>();

// Use Cases
builder.Services.AddScoped<GetReviewUseCase>();
builder.Services.AddScoped<SaveSiteConfigurationUseCase>();
```

### Why This Pattern?

1. **Single Point of Truth**: All wiring in one place
2. **Testability**: Easy to mock dependencies in tests
3. **Flexibility**: Change implementations without touching business logic
4. **Clean Layers**: Domain/Application/Infrastructure stay pure

## Review Provider System

### Extensible Provider Design

The system supports multiple review providers through the Strategy pattern.

```
┌─────────────────────────────────────────────────────────────┐
│                  IReviewProvider                             │
│  + ProviderType: ReviewProviderType                          │
│  + ProviderName: string                                      │
│  + CanHandle(context): bool                                  │
│  + GetReviewAsync(context): Task<ReviewResult>               │
└─────────────────────────────────────────────────────────────┘
                              ▲
                              │
           ┌──────────────────┼──────────────────┐
           │                  │                  │
┌──────────────────┐ ┌──────────────────┐ ┌──────────────────┐
│ JudgeMeProvider  │ │ ManualProvider   │ │ FutureProvider   │
└──────────────────┘ └──────────────────┘ └──────────────────┘
```

### Provider Selection

The `ReviewProviderSelector` uses Chain of Responsibility:

```csharp
public IReviewProvider? SelectProvider(ReviewContext context)
{
    // 1. Try preferred provider first
    var preferred = _providers
        .FirstOrDefault(p => p.ProviderType == context.PreferredProvider 
                          && p.CanHandle(context));
    
    if (preferred != null) return preferred;
    
    // 2. Fall back to any available provider
    return _providers.FirstOrDefault(p => p.CanHandle(context));
}
```

### Adding New Providers

1. Create class implementing `IReviewProvider`
2. Add enum value to `ReviewProviderType`
3. Register in `Program.cs`

No changes to business logic required!

## Feature Policy System

### Plan-Based Features

Features are resolved dynamically based on subscription plan.

```csharp
// NO if (plan == Plan.Pro) in business logic!

// Instead, use the policy:
if (_featurePolicy.IsFeatureEnabled(Feature.EmailNotificationOnFailure, config.Plan))
{
    await SendNotificationAsync();
}
```

### Feature Mapping

```csharp
private static readonly Dictionary<Feature, Plan> FeaturePlanMapping = new()
{
    // Pro Features
    { Feature.MultipleReviewProviders, Plan.Pro },
    { Feature.ManualFallbackText, Plan.Pro },
    
    // Premium Features
    { Feature.EmailNotificationOnFailure, Plan.Premium },
    { Feature.AdvancedStyling, Plan.Premium }
};
```

### Why This Approach?

1. **No Hard-coded Plan Checks**: Business logic doesn't know about plans
2. **Easy to Modify**: Change feature availability without touching code
3. **Testable**: Mock the policy for testing
4. **Future-Proof**: Add new plans without changing existing code

## Fallback Strategy

When the primary provider fails, the system follows this fallback chain:

```
┌─────────────────────────────────────────────────────────────┐
│                   PRIMARY PROVIDER                           │
│                   (e.g., Judge.me)                           │
└─────────────────────────────────────────────────────────────┘
                              │
                         FAILURE
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│              STEP 1: Log Failure                             │
│         (Always happens for monitoring)                      │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│       STEP 2: Try Manual Review for Product                  │
│    (If UseManualRatingFallback is enabled)                   │
└─────────────────────────────────────────────────────────────┘
                              │
                         NOT FOUND
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│       STEP 3: Try Configured Manual Rating                   │
│      (If ManualRating is configured)                         │
└─────────────────────────────────────────────────────────────┘
                              │
                         NOT CONFIGURED
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│          STEP 4: Try Fallback Text                           │
│   (If enabled AND ManualFallbackText feature available)      │
└─────────────────────────────────────────────────────────────┘
                              │
                         NOT AVAILABLE
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│         STEP 5: Send Notification (if enabled)               │
│   (If EmailNotificationOnFailure feature available)          │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│              STEP 6: Return Failure                          │
│           (Widget fails silently on frontend)                │
└─────────────────────────────────────────────────────────────┘
```

## Data Flow

### Getting a Review

```
User Request
     │
     ▼
┌─────────────┐
│  Controller │ ◄── Receives HTTP request
└─────────────┘
     │
     ▼
┌─────────────┐
│  Use Case   │ ◄── Orchestrates the flow
└─────────────┘
     │
     ├─────────────────────────────────────┐
     ▼                                     │
┌─────────────┐                            │
│  Config     │ ◄── Fetch site config      │
│  Repository │                            │
└─────────────┘                            │
     │                                     │
     ▼                                     │
┌─────────────┐                            │
│  Provider   │ ◄── Select appropriate     │
│  Selector   │     provider               │
└─────────────┘                            │
     │                                     │
     ▼                                     │
┌─────────────┐                            │
│  Review     │ ◄── Fetch review data      │
│  Provider   │                            │
└─────────────┘                            │
     │                                     │
     │ (on failure)                        │
     ▼                                     │
┌─────────────┐                            │
│  Fallback   │ ◄── Try fallback options   │
│  Service    │                            │
└─────────────┘                            │
     │                                     │
     ▼                                     ▼
┌─────────────────────────────────────────────┐
│                  Response                    │
└─────────────────────────────────────────────┘
```

## Extension Points

### 1. New Review Provider

**Location:** `Infrastructure/Providers/`

```csharp
public sealed class NewProvider : IReviewProvider
{
    public ReviewProviderType ProviderType => ReviewProviderType.New;
    public string ProviderName => "New Provider";
    
    public bool CanHandle(ReviewContext context) => true;
    
    public async Task<ReviewResult> GetReviewAsync(
        ReviewContext context,
        CancellationToken ct = default)
    {
        // Implementation
    }
}
```

### 2. New Feature

**Location:** `Domain/Enums/Feature.cs` and `Infrastructure/Policies/`

```csharp
// In Feature.cs
public enum Feature
{
    // ... existing
    NewFeature
}

// In PlanBasedFeaturePolicy.cs
{ Feature.NewFeature, Plan.Pro }
```

### 3. New Repository

**Location:** `Domain/Interfaces/Repositories/` and `Infrastructure/Persistence/`

```csharp
// In Domain
public interface INewRepository
{
    Task<Entity?> GetAsync(string id, CancellationToken ct);
}

// In Infrastructure
public sealed class NewRepository : INewRepository
{
    private readonly IMongoCollection<Entity> _collection;
    
    public NewRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<Entity>("entities");
    }
}
```

### 4. New Notification Provider

**Location:** `Infrastructure/Providers/`

```csharp
public sealed class SlackNotificationProvider : INotificationProvider
{
    public async Task<bool> SendFailureNotificationAsync(
        string recipientEmail,
        string siteId,
        string productId,
        string providerName,
        string errorMessage,
        CancellationToken ct = default)
    {
        // Send to Slack instead of email
    }
}
```

## Testing Strategy

### Unit Tests Focus

1. **Application Layer Logic**
   - Use case orchestration
   - Provider selection
   - Fallback logic

2. **Feature Policy Behavior**
   - Plan-based access
   - Feature resolution

3. **No Infrastructure Tests**
   - Mock all repositories
   - Mock all external providers
   - No MongoDB in tests

### Test Pattern (AAA)

```csharp
[Fact]
public async Task GetReview_WhenProviderSucceeds_ReturnsReview()
{
    // Arrange
    var mockRepo = new Mock<ISiteConfigurationRepository>();
    var mockProvider = new Mock<IReviewProvider>();
    // ... setup mocks
    
    // Act
    var result = await _useCase.ExecuteAsync(request);
    
    // Assert
    result.Success.Should().BeTrue();
    result.Rating.Should().Be(4.5m);
}
```

## Summary

Smart Sticky Reviewer demonstrates Clean Architecture with:

- **Clear Layer Boundaries**: Each layer has specific responsibilities
- **Dependency Rule**: Dependencies point inward (toward Domain)
- **Single DI Root**: All wiring in Program.cs
- **Extensible Design**: Add providers/features without modifying existing code
- **Testability**: Mock any dependency for isolated testing

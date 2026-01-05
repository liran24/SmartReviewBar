# Smart Sticky Reviewer

A Wix App backend and widgets for displaying product reviews in a sticky bar format. Built with Clean Architecture principles and .NET 8.

## Features

- **Sticky Review Bar**: Display product ratings prominently on your site
- **Multiple Review Providers**: Support for Judge.me and Manual reviews
- **Fallback System**: Graceful degradation when primary provider fails
- **Plan-Based Features**: Free, Pro, and Premium subscription tiers
- **Admin Dashboard**: Configure all settings through an intuitive UI
- **Extensible Design**: Easy to add new review providers

## Tech Stack

### Backend
- .NET 8
- ASP.NET Core Web API
- MongoDB Atlas (Free Tier M0)
- MongoDB.Driver
- xUnit + FluentAssertions + Moq

### Frontend Widgets
- Vanilla HTML, CSS, JavaScript
- No external dependencies

## Solution Structure

```
SmartStickyReviewer/
├── src/
│   ├── SmartStickyReviewer.Domain/         # Core business entities and interfaces
│   ├── SmartStickyReviewer.Application/    # Use cases and business logic
│   ├── SmartStickyReviewer.Infrastructure/ # MongoDB, providers, policies
│   └── SmartStickyReviewer.Api/            # ASP.NET Core Web API (DI Root)
├── tests/
│   └── SmartStickyReviewer.Tests/          # Unit tests
├── widgets/
│   ├── admin-widget/                       # Admin configuration dashboard
│   └── site-widget/                        # Frontend display widget
└── SmartStickyReviewer.sln
```

## Getting Started

### Prerequisites

- .NET 8 SDK
- MongoDB Atlas account (or local MongoDB)

### Configuration

1. Update `appsettings.json` with your MongoDB connection string:

```json
{
  "MongoDB": {
    "ConnectionString": "mongodb+srv://<username>:<password>@<cluster>.mongodb.net/",
    "DatabaseName": "SmartStickyReviewer"
  }
}
```

### Running the API

```bash
cd src/SmartStickyReviewer.Api
dotnet run
```

The API will be available at `http://localhost:5000` (or the configured port).

### Running Tests

```bash
dotnet test
```

## Deployment (Tailway/Railway)

This application is configured for deployment on Tailway, Railway, and similar PaaS platforms.

### Using Docker (Recommended)

The included `Dockerfile` provides a multi-stage build:

```bash
# Build the image
docker build -t smart-sticky-reviewer .

# Run locally (testing)
docker run -p 8080:8080 \
  -e MongoDB__ConnectionString="your-mongodb-connection-string" \
  -e MongoDB__DatabaseName="SmartStickyReviewer" \
  smart-sticky-reviewer
```

### Using Nixpacks

The `nixpacks.toml` configuration enables automatic builds on platforms that support Nixpacks.

### Environment Variables

Configure these environment variables in your deployment platform:

| Variable | Description | Required |
|----------|-------------|----------|
| `PORT` | The port the application listens on (auto-set by platform) | Auto |
| `MongoDB__ConnectionString` | MongoDB Atlas connection string | Yes |
| `MongoDB__DatabaseName` | Database name (default: SmartStickyReviewer) | No |
| `ASPNETCORE_ENVIRONMENT` | Environment (Production/Development) | No |

### Tailway Deployment Steps

1. Connect your repository to Tailway
2. Set the environment variables in Tailway dashboard
3. Deploy - Tailway will automatically detect the Dockerfile or use Nixpacks
4. The application will start on the assigned PORT

### Using the Widgets

#### Admin Widget
Open `widgets/admin-widget/index.html` in a browser to access the configuration dashboard.

#### Site Widget

Include the embed script on your website:

```html
<script>
  window.SmartStickyReviewerConfig = {
    siteId: 'your-site-id',
    productId: 'your-product-id',
    apiUrl: 'https://your-api-domain.com/api'
  };
</script>
<script src="https://your-cdn.com/embed.js" async></script>
```

## API Endpoints

### Configuration

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/configuration/{siteId}` | Get site configuration |
| POST | `/api/configuration` | Save site configuration |
| GET | `/api/configuration/plans` | Get available plans |
| GET | `/api/configuration/providers` | Get available providers |

### Reviews

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/reviews?siteId=X&productId=Y` | Get review for product |
| POST | `/api/reviews/manual` | Save manual review |

### Health

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/health` | Health check |

## Subscription Plans

| Feature | Free | Pro | Premium |
|---------|------|-----|---------|
| Basic Review Display | ✓ | ✓ | ✓ |
| Multiple Review Providers | ✗ | ✓ | ✓ |
| Manual Fallback Text | ✗ | ✓ | ✓ |
| Email Notifications | ✗ | ✗ | ✓ |
| Advanced Styling | ✗ | ✗ | ✓ |

## Adding a New Review Provider

1. Create a new class implementing `IReviewProvider` in Infrastructure:

```csharp
public sealed class MyCustomProvider : IReviewProvider
{
    public ReviewProviderType ProviderType => ReviewProviderType.Custom;
    public string ProviderName => "My Custom Provider";

    public bool CanHandle(ReviewContext context) => true;

    public async Task<ReviewResult> GetReviewAsync(
        ReviewContext context,
        CancellationToken cancellationToken = default)
    {
        // Implement your logic
        return ReviewResult.Successful(4.5m, 100, "Great!", ProviderName);
    }
}
```

2. Add the new provider type to the `ReviewProviderType` enum in Domain.

3. Register the provider in `Program.cs`:

```csharp
builder.Services.AddScoped<MyCustomProvider>();

builder.Services.AddScoped<IEnumerable<IReviewProvider>>(sp =>
{
    return new List<IReviewProvider>
    {
        sp.GetRequiredService<JudgeMeReviewProvider>(),
        sp.GetRequiredService<ManualReviewProvider>(),
        sp.GetRequiredService<MyCustomProvider>() // Add here
    };
});
```

## Adding a New Paid Feature

1. Add the feature to the `Feature` enum in Domain:

```csharp
public enum Feature
{
    MultipleReviewProviders,
    ManualFallbackText,
    EmailNotificationOnFailure,
    AdvancedStyling,
    MyNewFeature  // Add here
}
```

2. Map the feature to a plan in `PlanBasedFeaturePolicy`:

```csharp
private static readonly Dictionary<Feature, Plan> FeaturePlanMapping = new()
{
    { Feature.MultipleReviewProviders, Plan.Pro },
    { Feature.ManualFallbackText, Plan.Pro },
    { Feature.EmailNotificationOnFailure, Plan.Premium },
    { Feature.AdvancedStyling, Plan.Premium },
    { Feature.MyNewFeature, Plan.Pro }  // Add here
};
```

3. Use the feature in your business logic:

```csharp
if (_featurePolicy.IsFeatureEnabled(Feature.MyNewFeature, config.Plan))
{
    // Feature is enabled
}
```

## Architecture

See [ARCHITECTURE.md](ARCHITECTURE.md) for detailed architecture documentation.

## License

MIT

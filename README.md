# SMART STICKY REVIEWER

Brand-new Wix App backend + widgets (admin + site) built from scratch with **.NET 8**, **ASP.NET Core Web API**, **MongoDB Atlas (M0)**, and **vanilla HTML/CSS/JS**.

## What this project does

SMART STICKY REVIEWER renders a small sticky “review bar” on a site using a **pluggable review provider** model:

- The **Admin Widget** saves per-site configuration (plan, provider, fallback behavior, styling).
- The **Site Widget** fetches that configuration, resolves the provider, applies fallback rules, and renders (or fails silently).

## Repository layout

```
/src
  SmartStickyReviewer.Domain
  SmartStickyReviewer.Application
  SmartStickyReviewer.Infrastructure
  SmartStickyReviewer.Api
/tests
  SmartStickyReviewer.Tests
/widgets
  /admin-widget
  /site-widget
SmartStickyReviewer.sln
README.md
ARCHITECTURE.md
```

## Prerequisites

- .NET SDK 8
- MongoDB Atlas connection string (or local MongoDB for development)

## Configure MongoDB

Edit:

- `src/SmartStickyReviewer.Api/appsettings.Development.json`

Example:

```json
{
  "Mongo": {
    "ConnectionString": "mongodb+srv://<user>:<pass>@<cluster>/<db>?retryWrites=true&w=majority",
    "DatabaseName": "smart-sticky-reviewer-dev",
    "SiteConfigurationsCollectionName": "site_configurations"
  }
}
```

## Run the API

From repo root:

```bash
dotnet run --project src/SmartStickyReviewer.Api
```

Swagger (dev): `https://localhost:7051/swagger` or `http://localhost:5260/swagger`

Health: `GET /health`

## API endpoints

- `GET /api/admin/sites/{siteId}/config`
- `PUT /api/admin/sites/{siteId}/config`
- `GET /api/widget/sites/{siteId}?productId=...`

## Run unit tests

```bash
dotnet test
```

## Widgets (vanilla)

### Admin widget

Open `widgets/admin-widget/index.html` in a browser and set:

- **API base URL** (default `http://localhost:5260`)
- **Site ID**

Then load / save configuration.

### Site widget

Use `widgets/site-widget/index.html` for a demo embed, or copy the script include:

```html
<script
  src="./embed.js"
  data-api-base="http://localhost:5260"
  data-site-id="demo-site"
  defer>
</script>
```

The widget will call `GET /api/widget/sites/{siteId}` and render a sticky bar or do nothing.

## Extensibility quickstart

- Add a new provider by implementing `SmartStickyReviewer.Domain.Interfaces.IReviewProvider`
- Register it in **one place only**: `src/SmartStickyReviewer.Api/Program.cs`

For the full architectural rules and how-to guides, see `ARCHITECTURE.md`.
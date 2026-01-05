using MongoDB.Driver;
using SmartStickyReviewer.Application.Services;
using SmartStickyReviewer.Application.UseCases.Configuration;
using SmartStickyReviewer.Application.UseCases.Reviews;
using SmartStickyReviewer.Domain.Interfaces.Policies;
using SmartStickyReviewer.Domain.Interfaces.Providers;
using SmartStickyReviewer.Domain.Interfaces.Repositories;
using SmartStickyReviewer.Infrastructure.Persistence;
using SmartStickyReviewer.Infrastructure.Policies;
using SmartStickyReviewer.Infrastructure.Providers;

// =============================================================================
// SMART STICKY REVIEWER - COMPOSITION ROOT
// =============================================================================
// This is the ONLY place where Dependency Injection registrations are allowed.
// Domain, Application, and Infrastructure layers contain NO DI code.
// All services use constructor injection.
// =============================================================================

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------------------------------
// TAILWAY/RAILWAY DEPLOYMENT SUPPORT
// -----------------------------------------------------------------------------
// PaaS platforms like Tailway inject the PORT environment variable
// The application must listen on this port for successful deployment

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://*:{port}");

// -----------------------------------------------------------------------------
// CONFIGURATION
// -----------------------------------------------------------------------------

var mongoSettings = new MongoDbSettings
{
    ConnectionString = builder.Configuration.GetValue<string>("MongoDB:ConnectionString")
        ?? "mongodb://localhost:27017",
    DatabaseName = builder.Configuration.GetValue<string>("MongoDB:DatabaseName")
        ?? "SmartStickyReviewer"
};

// -----------------------------------------------------------------------------
// MONGODB SETUP
// -----------------------------------------------------------------------------

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    return new MongoClient(mongoSettings.ConnectionString);
});

builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(mongoSettings.DatabaseName);
});

// -----------------------------------------------------------------------------
// REPOSITORIES (Infrastructure -> Domain interfaces)
// -----------------------------------------------------------------------------

builder.Services.AddScoped<ISiteConfigurationRepository, SiteConfigurationRepository>();
builder.Services.AddScoped<IManualReviewRepository, ManualReviewRepository>();
builder.Services.AddScoped<IProviderFailureLogRepository, ProviderFailureLogRepository>();

// -----------------------------------------------------------------------------
// POLICIES (Infrastructure -> Domain interfaces)
// -----------------------------------------------------------------------------

builder.Services.AddSingleton<IFeaturePolicy, PlanBasedFeaturePolicy>();

// -----------------------------------------------------------------------------
// NOTIFICATION PROVIDERS (Infrastructure -> Domain interfaces)
// -----------------------------------------------------------------------------

builder.Services.AddSingleton<INotificationProvider, EmailNotificationProvider>();

// -----------------------------------------------------------------------------
// REVIEW PROVIDERS (Infrastructure -> Domain interfaces)
// Extensible design: Add new providers here
// -----------------------------------------------------------------------------

// Register individual providers for injection
builder.Services.AddScoped<JudgeMeReviewProvider>();
builder.Services.AddScoped<ManualReviewProvider>();

// Register all providers as IEnumerable<IReviewProvider> for provider selector
builder.Services.AddScoped<IEnumerable<IReviewProvider>>(sp =>
{
    var providers = new List<IReviewProvider>
    {
        sp.GetRequiredService<JudgeMeReviewProvider>(),
        sp.GetRequiredService<ManualReviewProvider>()
    };
    return providers;
});

// -----------------------------------------------------------------------------
// APPLICATION SERVICES
// -----------------------------------------------------------------------------

builder.Services.AddScoped<IReviewProviderSelector, ReviewProviderSelector>();
builder.Services.AddScoped<IFallbackService, FallbackService>();

// -----------------------------------------------------------------------------
// USE CASES (Application layer)
// -----------------------------------------------------------------------------

builder.Services.AddScoped<GetReviewUseCase>();
builder.Services.AddScoped<GetSiteConfigurationUseCase>();
builder.Services.AddScoped<SaveSiteConfigurationUseCase>();
builder.Services.AddScoped<SaveManualReviewUseCase>();

// -----------------------------------------------------------------------------
// ASP.NET CORE SERVICES
// -----------------------------------------------------------------------------

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Smart Sticky Reviewer API",
        Version = "v1",
        Description = "API for managing sticky review bars on Wix sites"
    });
});

// CORS for widget access
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWidgets", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// -----------------------------------------------------------------------------
// BUILD APPLICATION
// -----------------------------------------------------------------------------

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable Swagger in production for API documentation (optional, can be removed for security)
if (!app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Smart Sticky Reviewer API v1");
        c.RoutePrefix = "swagger";
    });
}

// Skip HTTPS redirection in containerized environments (handled by reverse proxy)
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors("AllowWidgets");
app.UseAuthorization();
app.MapControllers();

// Serve static files for widgets
app.UseStaticFiles();

app.Run();

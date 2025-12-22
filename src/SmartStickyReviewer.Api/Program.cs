using MongoDB.Driver;
using SmartStickyReviewer.Application.Services;
using SmartStickyReviewer.Application.UseCases;
using SmartStickyReviewer.Domain.Interfaces;
using SmartStickyReviewer.Infrastructure.Mongo;
using SmartStickyReviewer.Infrastructure.Notifications;
using SmartStickyReviewer.Infrastructure.Policies;
using SmartStickyReviewer.Infrastructure.Providers;

var builder = WebApplication.CreateBuilder(args);

// ============================
// Dependency Injection (ONLY)
// Composition Root: Program.cs
// ============================

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod()
    );
});

var mongoOptions = builder.Configuration.GetSection("Mongo").Get<MongoOptions>() ?? new MongoOptions();
builder.Services.AddSingleton(mongoOptions);
builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoOptions.ConnectionString));

builder.Services.AddSingleton<ISiteConfigurationRepository, MongoSiteConfigurationRepository>();
builder.Services.AddSingleton<PlanFeatureMatrix>();
builder.Services.AddSingleton<IFeaturePolicy, PlanBasedFeaturePolicy>();
builder.Services.AddSingleton<IStoreOwnerNotifier, EmailStoreOwnerNotifier>();

builder.Services.AddSingleton<IReviewProvider, ManualReviewProvider>();
builder.Services.AddSingleton<IReviewProvider, JudgeMeReviewProvider>();
builder.Services.AddSingleton<ReviewProviderSelector>();

builder.Services.AddScoped<GetAdminConfigurationUseCase>();
builder.Services.AddScoped<SaveAdminConfigurationUseCase>();
builder.Services.AddScoped<GetWidgetDataUseCase>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseHttpsRedirection();
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();

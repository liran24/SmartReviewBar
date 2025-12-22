namespace SmartStickyReviewer.Infrastructure.Mongo;

public sealed class MongoOptions
{
    public string ConnectionString { get; init; } = string.Empty;
    public string DatabaseName { get; init; } = "smart-sticky-reviewer";
    public string SiteConfigurationsCollectionName { get; init; } = "site_configurations";
}


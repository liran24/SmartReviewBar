namespace SmartStickyReviewer.Infrastructure.Persistence;

/// <summary>
/// MongoDB connection settings
/// </summary>
public sealed class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = "SmartStickyReviewer";
}

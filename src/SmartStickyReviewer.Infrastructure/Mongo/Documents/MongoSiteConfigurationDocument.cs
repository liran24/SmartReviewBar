using MongoDB.Bson.Serialization.Attributes;
using SmartStickyReviewer.Domain.Enums;

namespace SmartStickyReviewer.Infrastructure.Mongo.Documents;

public sealed class MongoSiteConfigurationDocument
{
    [BsonId]
    public string SiteId { get; set; } = string.Empty;

    public Plan Plan { get; set; } = Plan.Free;

    public ReviewProviderKind PrimaryProvider { get; set; } = ReviewProviderKind.Manual;

    public decimal? ManualRating { get; set; }
    public string? ManualText { get; set; }

    public string? FallbackText { get; set; }

    public string BackgroundColorHex { get; set; } = "#111827";
    public string TextColorHex { get; set; } = "#F9FAFB";
    public string AccentColorHex { get; set; } = "#F59E0B";

    public string? StoreOwnerEmail { get; set; }
}


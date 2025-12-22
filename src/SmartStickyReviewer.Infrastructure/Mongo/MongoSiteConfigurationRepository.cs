using MongoDB.Driver;
using SmartStickyReviewer.Domain.Entities;
using SmartStickyReviewer.Domain.Interfaces;
using SmartStickyReviewer.Domain.ValueObjects;
using SmartStickyReviewer.Infrastructure.Mongo.Documents;

namespace SmartStickyReviewer.Infrastructure.Mongo;

public sealed class MongoSiteConfigurationRepository : ISiteConfigurationRepository
{
    private readonly IMongoCollection<MongoSiteConfigurationDocument> _collection;

    public MongoSiteConfigurationRepository(IMongoClient client, MongoOptions options)
    {
        if (client is null) throw new ArgumentNullException(nameof(client));
        if (options is null) throw new ArgumentNullException(nameof(options));

        var db = client.GetDatabase(options.DatabaseName);
        _collection = db.GetCollection<MongoSiteConfigurationDocument>(options.SiteConfigurationsCollectionName);
    }

    public async Task<SiteConfiguration?> GetAsync(SiteId siteId, CancellationToken ct)
    {
        var doc = await _collection.Find(x => x.SiteId == siteId.Value).FirstOrDefaultAsync(ct);
        return doc is null ? null : MapToDomain(doc);
    }

    public async Task UpsertAsync(SiteConfiguration configuration, CancellationToken ct)
    {
        var doc = MapToDocument(configuration);
        await _collection.ReplaceOneAsync(
            filter: x => x.SiteId == doc.SiteId,
            replacement: doc,
            options: new ReplaceOptions { IsUpsert = true },
            cancellationToken: ct
        );
    }

    private static SiteConfiguration MapToDomain(MongoSiteConfigurationDocument doc)
    {
        var manual = doc.ManualRating.HasValue
            ? new ManualReview(new StarRating(doc.ManualRating.Value), doc.ManualText)
            : null;

        var style = new StickyStyle(doc.BackgroundColorHex, doc.TextColorHex, doc.AccentColorHex);

        return new SiteConfiguration(
            siteId: new SiteId(doc.SiteId),
            plan: doc.Plan,
            primaryProvider: doc.PrimaryProvider,
            manualReview: manual,
            fallbackText: doc.FallbackText,
            style: style,
            storeOwnerEmail: doc.StoreOwnerEmail
        );
    }

    private static MongoSiteConfigurationDocument MapToDocument(SiteConfiguration configuration)
    {
        return new MongoSiteConfigurationDocument
        {
            SiteId = configuration.SiteId.Value,
            Plan = configuration.Plan,
            PrimaryProvider = configuration.PrimaryProvider,
            ManualRating = configuration.ManualReview?.Rating.Value,
            ManualText = configuration.ManualReview?.Text,
            FallbackText = configuration.FallbackText,
            BackgroundColorHex = configuration.Style.BackgroundColorHex,
            TextColorHex = configuration.Style.TextColorHex,
            AccentColorHex = configuration.Style.AccentColorHex,
            StoreOwnerEmail = configuration.StoreOwnerEmail
        };
    }
}


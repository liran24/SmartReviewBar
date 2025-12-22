using MongoDB.Driver;
using SmartStickyReviewer.Domain.Entities;
using SmartStickyReviewer.Domain.Interfaces.Repositories;

namespace SmartStickyReviewer.Infrastructure.Persistence;

/// <summary>
/// MongoDB implementation of site configuration repository
/// </summary>
public sealed class SiteConfigurationRepository : ISiteConfigurationRepository
{
    private readonly IMongoCollection<SiteConfiguration> _collection;

    public SiteConfigurationRepository(IMongoDatabase database)
    {
        if (database == null)
            throw new ArgumentNullException(nameof(database));

        _collection = database.GetCollection<SiteConfiguration>("site_configurations");

        // Ensure index on SiteId for fast lookups
        var indexKeysDefinition = Builders<SiteConfiguration>.IndexKeys.Ascending(x => x.SiteId);
        _collection.Indexes.CreateOne(new CreateIndexModel<SiteConfiguration>(
            indexKeysDefinition,
            new CreateIndexOptions { Unique = true }));
    }

    public async Task<SiteConfiguration?> GetBySiteIdAsync(
        string siteId,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<SiteConfiguration>.Filter.Eq(x => x.SiteId, siteId);
        return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<SiteConfiguration> CreateAsync(
        SiteConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        await _collection.InsertOneAsync(configuration, cancellationToken: cancellationToken);
        return configuration;
    }

    public async Task<SiteConfiguration> UpdateAsync(
        SiteConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        var filter = Builders<SiteConfiguration>.Filter.Eq(x => x.SiteId, configuration.SiteId);
        await _collection.ReplaceOneAsync(filter, configuration, cancellationToken: cancellationToken);
        return configuration;
    }

    public async Task<bool> DeleteAsync(string siteId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<SiteConfiguration>.Filter.Eq(x => x.SiteId, siteId);
        var result = await _collection.DeleteOneAsync(filter, cancellationToken);
        return result.DeletedCount > 0;
    }

    public async Task<bool> ExistsAsync(string siteId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<SiteConfiguration>.Filter.Eq(x => x.SiteId, siteId);
        var count = await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        return count > 0;
    }
}

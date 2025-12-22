using MongoDB.Driver;
using SmartStickyReviewer.Domain.Entities;
using SmartStickyReviewer.Domain.Interfaces.Repositories;

namespace SmartStickyReviewer.Infrastructure.Persistence;

/// <summary>
/// MongoDB implementation of provider failure log repository
/// </summary>
public sealed class ProviderFailureLogRepository : IProviderFailureLogRepository
{
    private readonly IMongoCollection<ProviderFailureLog> _collection;

    public ProviderFailureLogRepository(IMongoDatabase database)
    {
        if (database == null)
            throw new ArgumentNullException(nameof(database));

        _collection = database.GetCollection<ProviderFailureLog>("provider_failure_logs");

        // Create indexes
        var siteIdIndex = Builders<ProviderFailureLog>.IndexKeys.Ascending(x => x.SiteId);
        _collection.Indexes.CreateOne(new CreateIndexModel<ProviderFailureLog>(siteIdIndex));

        var occurredAtIndex = Builders<ProviderFailureLog>.IndexKeys.Descending(x => x.OccurredAt);
        _collection.Indexes.CreateOne(new CreateIndexModel<ProviderFailureLog>(occurredAtIndex));

        var notificationIndex = Builders<ProviderFailureLog>.IndexKeys.Ascending(x => x.NotificationSent);
        _collection.Indexes.CreateOne(new CreateIndexModel<ProviderFailureLog>(notificationIndex));
    }

    public async Task<ProviderFailureLog> CreateAsync(
        ProviderFailureLog log,
        CancellationToken cancellationToken = default)
    {
        if (log == null)
            throw new ArgumentNullException(nameof(log));

        await _collection.InsertOneAsync(log, cancellationToken: cancellationToken);
        return log;
    }

    public async Task<IEnumerable<ProviderFailureLog>> GetBySiteIdAsync(
        string siteId,
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<ProviderFailureLog>.Filter.Eq(x => x.SiteId, siteId);
        var sort = Builders<ProviderFailureLog>.Sort.Descending(x => x.OccurredAt);

        return await _collection
            .Find(filter)
            .Sort(sort)
            .Limit(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ProviderFailureLog>> GetUnnotifiedAsync(
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<ProviderFailureLog>.Filter.Eq(x => x.NotificationSent, false);
        return await _collection.Find(filter).ToListAsync(cancellationToken);
    }

    public async Task MarkNotifiedAsync(string id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<ProviderFailureLog>.Filter.Eq(x => x.Id, id);
        var update = Builders<ProviderFailureLog>.Update.Set(x => x.NotificationSent, true);
        await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
    }
}

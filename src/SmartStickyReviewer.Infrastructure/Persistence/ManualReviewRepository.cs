using MongoDB.Driver;
using SmartStickyReviewer.Domain.Entities;
using SmartStickyReviewer.Domain.Interfaces.Repositories;

namespace SmartStickyReviewer.Infrastructure.Persistence;

/// <summary>
/// MongoDB implementation of manual review repository
/// </summary>
public sealed class ManualReviewRepository : IManualReviewRepository
{
    private readonly IMongoCollection<ManualReview> _collection;

    public ManualReviewRepository(IMongoDatabase database)
    {
        if (database == null)
            throw new ArgumentNullException(nameof(database));

        _collection = database.GetCollection<ManualReview>("manual_reviews");

        // Ensure compound index on SiteId + ProductId
        var indexKeysDefinition = Builders<ManualReview>.IndexKeys
            .Ascending(x => x.SiteId)
            .Ascending(x => x.ProductId);

        _collection.Indexes.CreateOne(new CreateIndexModel<ManualReview>(
            indexKeysDefinition,
            new CreateIndexOptions { Unique = true }));
    }

    public async Task<ManualReview?> GetAsync(
        string siteId,
        string productId,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<ManualReview>.Filter.And(
            Builders<ManualReview>.Filter.Eq(x => x.SiteId, siteId),
            Builders<ManualReview>.Filter.Eq(x => x.ProductId, productId));

        return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<ManualReview>> GetBySiteIdAsync(
        string siteId,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<ManualReview>.Filter.Eq(x => x.SiteId, siteId);
        return await _collection.Find(filter).ToListAsync(cancellationToken);
    }

    public async Task<ManualReview> CreateAsync(
        ManualReview review,
        CancellationToken cancellationToken = default)
    {
        if (review == null)
            throw new ArgumentNullException(nameof(review));

        await _collection.InsertOneAsync(review, cancellationToken: cancellationToken);
        return review;
    }

    public async Task<ManualReview> UpdateAsync(
        ManualReview review,
        CancellationToken cancellationToken = default)
    {
        if (review == null)
            throw new ArgumentNullException(nameof(review));

        var filter = Builders<ManualReview>.Filter.And(
            Builders<ManualReview>.Filter.Eq(x => x.SiteId, review.SiteId),
            Builders<ManualReview>.Filter.Eq(x => x.ProductId, review.ProductId));

        await _collection.ReplaceOneAsync(filter, review, cancellationToken: cancellationToken);
        return review;
    }

    public async Task<bool> DeleteAsync(
        string siteId,
        string productId,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<ManualReview>.Filter.And(
            Builders<ManualReview>.Filter.Eq(x => x.SiteId, siteId),
            Builders<ManualReview>.Filter.Eq(x => x.ProductId, productId));

        var result = await _collection.DeleteOneAsync(filter, cancellationToken);
        return result.DeletedCount > 0;
    }
}

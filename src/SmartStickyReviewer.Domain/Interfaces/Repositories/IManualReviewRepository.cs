using SmartStickyReviewer.Domain.Entities;

namespace SmartStickyReviewer.Domain.Interfaces.Repositories;

/// <summary>
/// Repository for manual review persistence
/// </summary>
public interface IManualReviewRepository
{
    Task<ManualReview?> GetAsync(string siteId, string productId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ManualReview>> GetBySiteIdAsync(string siteId, CancellationToken cancellationToken = default);
    Task<ManualReview> CreateAsync(ManualReview review, CancellationToken cancellationToken = default);
    Task<ManualReview> UpdateAsync(ManualReview review, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string siteId, string productId, CancellationToken cancellationToken = default);
}

using SmartStickyReviewer.Domain.Enums;
using SmartStickyReviewer.Domain.ValueObjects;

namespace SmartStickyReviewer.Domain.Interfaces.Providers;

/// <summary>
/// Interface for review data providers (extensible design)
/// </summary>
public interface IReviewProvider
{
    /// <summary>
    /// Gets the provider type identifier
    /// </summary>
    ReviewProviderType ProviderType { get; }

    /// <summary>
    /// Gets the display name of the provider
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Determines if this provider can handle the given context
    /// </summary>
    bool CanHandle(ReviewContext context);

    /// <summary>
    /// Fetches review data for the given context
    /// </summary>
    Task<ReviewResult> GetReviewAsync(ReviewContext context, CancellationToken cancellationToken = default);
}

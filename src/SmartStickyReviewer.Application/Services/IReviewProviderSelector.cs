using SmartStickyReviewer.Domain.Interfaces.Providers;
using SmartStickyReviewer.Domain.ValueObjects;

namespace SmartStickyReviewer.Application.Services;

/// <summary>
/// Service for selecting the appropriate review provider based on context
/// </summary>
public interface IReviewProviderSelector
{
    /// <summary>
    /// Selects the appropriate provider for the given context
    /// </summary>
    IReviewProvider? SelectProvider(ReviewContext context);

    /// <summary>
    /// Gets all available providers
    /// </summary>
    IEnumerable<IReviewProvider> GetAllProviders();
}

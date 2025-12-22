using SmartStickyReviewer.Domain.Enums;

namespace SmartStickyReviewer.Domain.ValueObjects;

/// <summary>
/// Context information needed to resolve and fetch a review
/// </summary>
public sealed class ReviewContext
{
    public string SiteId { get; }
    public string ProductId { get; }
    public ReviewProviderType PreferredProvider { get; }
    public Plan CurrentPlan { get; }

    public ReviewContext(
        string siteId,
        string productId,
        ReviewProviderType preferredProvider,
        Plan currentPlan)
    {
        if (string.IsNullOrWhiteSpace(siteId))
            throw new ArgumentException("Site ID cannot be empty", nameof(siteId));

        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException("Product ID cannot be empty", nameof(productId));

        SiteId = siteId;
        ProductId = productId;
        PreferredProvider = preferredProvider;
        CurrentPlan = currentPlan;
    }
}

using SmartStickyReviewer.Domain.Enums;
using SmartStickyReviewer.Domain.Interfaces.Providers;
using SmartStickyReviewer.Domain.Interfaces.Repositories;
using SmartStickyReviewer.Domain.ValueObjects;

namespace SmartStickyReviewer.Infrastructure.Providers;

/// <summary>
/// Manual review provider - uses manually entered review data
/// </summary>
public sealed class ManualReviewProvider : IReviewProvider
{
    private readonly IManualReviewRepository _repository;

    public ManualReviewProvider(IManualReviewRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public ReviewProviderType ProviderType => ReviewProviderType.Manual;

    public string ProviderName => "Manual";

    public bool CanHandle(ReviewContext context)
    {
        if (context == null)
            return false;

        // Manual provider can always handle if there's a review for the product
        return true;
    }

    public async Task<ReviewResult> GetReviewAsync(
        ReviewContext context,
        CancellationToken cancellationToken = default)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        var review = await _repository.GetAsync(
            context.SiteId,
            context.ProductId,
            cancellationToken);

        if (review == null)
        {
            return ReviewResult.Failed(
                $"No manual review found for product {context.ProductId}",
                ProviderName);
        }

        return ReviewResult.Successful(
            rating: review.Rating,
            reviewCount: review.ReviewCount,
            displayText: review.DisplayText,
            providerName: ProviderName);
    }
}

using SmartStickyReviewer.Domain.Enums;
using SmartStickyReviewer.Domain.Interfaces;
using SmartStickyReviewer.Domain.ValueObjects;

namespace SmartStickyReviewer.Infrastructure.Providers;

public sealed class ManualReviewProvider : IReviewProvider
{
    public bool CanHandle(ReviewContext context)
    {
        if (context is null) throw new ArgumentNullException(nameof(context));
        return context.DesiredProvider == ReviewProviderKind.Manual;
    }

    public ReviewResult GetReview(ReviewContext context)
    {
        if (context is null) throw new ArgumentNullException(nameof(context));

        if (context.ManualReview is null)
        {
            return ReviewResult.Failure(nameof(ManualReviewProvider), "Manual review is not configured.");
        }

        return ReviewResult.Success(context.ManualReview.Rating, context.ManualReview.Text, nameof(ManualReviewProvider));
    }
}


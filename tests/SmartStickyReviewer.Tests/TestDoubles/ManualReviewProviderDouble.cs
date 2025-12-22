using SmartStickyReviewer.Domain.Enums;
using SmartStickyReviewer.Domain.Interfaces;
using SmartStickyReviewer.Domain.ValueObjects;

namespace SmartStickyReviewer.Tests.TestDoubles;

public sealed class ManualReviewProviderDouble : IReviewProvider
{
    public bool CanHandle(ReviewContext context) => context.DesiredProvider == ReviewProviderKind.Manual;

    public ReviewResult GetReview(ReviewContext context)
    {
        if (context.ManualReview is null)
        {
            return ReviewResult.Failure(nameof(ManualReviewProviderDouble), "Manual review is not configured.");
        }

        return ReviewResult.Success(context.ManualReview.Rating, context.ManualReview.Text, "ManualReviewProvider");
    }
}


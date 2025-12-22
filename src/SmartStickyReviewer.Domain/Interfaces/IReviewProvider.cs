using SmartStickyReviewer.Domain.ValueObjects;

namespace SmartStickyReviewer.Domain.Interfaces;

public interface IReviewProvider
{
    bool CanHandle(ReviewContext context);
    ReviewResult GetReview(ReviewContext context);
}


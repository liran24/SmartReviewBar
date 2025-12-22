using SmartStickyReviewer.Domain.Enums;
using SmartStickyReviewer.Domain.Interfaces;
using SmartStickyReviewer.Domain.ValueObjects;

namespace SmartStickyReviewer.Infrastructure.Providers;

public sealed class JudgeMeReviewProvider : IReviewProvider
{
    public bool CanHandle(ReviewContext context)
    {
        if (context is null) throw new ArgumentNullException(nameof(context));
        return context.DesiredProvider == ReviewProviderKind.JudgeMe;
    }

    public ReviewResult GetReview(ReviewContext context)
    {
        if (context is null) throw new ArgumentNullException(nameof(context));

        // Explicitly no external Judge.me API usage is allowed by the project requirements.
        // This provider exists to demonstrate extensibility and failure/fallback behavior.
        throw new InvalidOperationException("Judge.me API usage is disabled in this project. Configure Manual review or fallback behavior.");
    }
}


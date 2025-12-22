using SmartStickyReviewer.Domain.Enums;
using SmartStickyReviewer.Domain.Interfaces;
using SmartStickyReviewer.Domain.ValueObjects;

namespace SmartStickyReviewer.Tests.TestDoubles;

public sealed class ThrowingReviewProvider : IReviewProvider
{
    private readonly ReviewProviderKind _kind;
    private readonly string _message;

    public ThrowingReviewProvider(ReviewProviderKind kind, string message = "boom")
    {
        _kind = kind;
        _message = message;
    }

    public bool CanHandle(ReviewContext context) => context.DesiredProvider == _kind;

    public ReviewResult GetReview(ReviewContext context) => throw new InvalidOperationException(_message);
}


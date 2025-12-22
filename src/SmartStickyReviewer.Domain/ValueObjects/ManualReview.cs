namespace SmartStickyReviewer.Domain.ValueObjects;

public sealed record ManualReview(
    StarRating Rating,
    string? Text
);


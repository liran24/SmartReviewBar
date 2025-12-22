namespace SmartStickyReviewer.Domain.ValueObjects;

public sealed record ReviewResult(
    bool IsSuccess,
    StarRating? Rating,
    string? Text,
    string ProviderName,
    string? FailureReason
)
{
    public static ReviewResult Success(StarRating rating, string? text, string providerName) =>
        new(true, rating, text, providerName, null);

    public static ReviewResult Failure(string providerName, string failureReason) =>
        new(false, null, null, providerName, failureReason);
}


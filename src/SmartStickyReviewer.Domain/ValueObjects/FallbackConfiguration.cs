namespace SmartStickyReviewer.Domain.ValueObjects;

/// <summary>
/// Configuration for fallback behavior when primary provider fails
/// </summary>
public sealed class FallbackConfiguration
{
    public bool UseManualRatingFallback { get; }
    public decimal? ManualRating { get; }
    public int? ManualReviewCount { get; }
    public string? FallbackText { get; }
    public bool NotifyOnFailure { get; }
    public string? NotificationEmail { get; }

    public FallbackConfiguration(
        bool useManualRatingFallback = false,
        decimal? manualRating = null,
        int? manualReviewCount = null,
        string? fallbackText = null,
        bool notifyOnFailure = false,
        string? notificationEmail = null)
    {
        UseManualRatingFallback = useManualRatingFallback;
        ManualRating = manualRating;
        ManualReviewCount = manualReviewCount;
        FallbackText = fallbackText;
        NotifyOnFailure = notifyOnFailure;
        NotificationEmail = notificationEmail;
    }

    public static FallbackConfiguration Default => new();

    public bool HasManualRating => UseManualRatingFallback && ManualRating.HasValue;
    public bool HasFallbackText => !string.IsNullOrWhiteSpace(FallbackText);
}

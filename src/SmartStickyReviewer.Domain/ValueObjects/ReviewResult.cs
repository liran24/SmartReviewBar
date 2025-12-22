namespace SmartStickyReviewer.Domain.ValueObjects;

/// <summary>
/// Result of a review provider request
/// </summary>
public sealed class ReviewResult
{
    public bool Success { get; }
    public decimal Rating { get; }
    public int ReviewCount { get; }
    public string DisplayText { get; }
    public string ProviderName { get; }
    public string? ErrorMessage { get; }
    public bool IsFallback { get; }

    private ReviewResult(
        bool success,
        decimal rating,
        int reviewCount,
        string displayText,
        string providerName,
        string? errorMessage,
        bool isFallback)
    {
        Success = success;
        Rating = rating;
        ReviewCount = reviewCount;
        DisplayText = displayText;
        ProviderName = providerName;
        ErrorMessage = errorMessage;
        IsFallback = isFallback;
    }

    public static ReviewResult Successful(
        decimal rating,
        int reviewCount,
        string displayText,
        string providerName,
        bool isFallback = false)
    {
        return new ReviewResult(
            success: true,
            rating: rating,
            reviewCount: reviewCount,
            displayText: displayText,
            providerName: providerName,
            errorMessage: null,
            isFallback: isFallback);
    }

    public static ReviewResult Failed(string errorMessage, string providerName)
    {
        return new ReviewResult(
            success: false,
            rating: 0,
            reviewCount: 0,
            displayText: string.Empty,
            providerName: providerName,
            errorMessage: errorMessage,
            isFallback: false);
    }
}

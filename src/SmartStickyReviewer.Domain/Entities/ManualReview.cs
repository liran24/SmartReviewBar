namespace SmartStickyReviewer.Domain.Entities;

/// <summary>
/// A manually entered review for a product
/// </summary>
public sealed class ManualReview
{
    public string Id { get; private set; }
    public string SiteId { get; private set; }
    public string ProductId { get; private set; }
    public decimal Rating { get; private set; }
    public int ReviewCount { get; private set; }
    public string DisplayText { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // For MongoDB deserialization
    private ManualReview()
    {
        Id = string.Empty;
        SiteId = string.Empty;
        ProductId = string.Empty;
        DisplayText = string.Empty;
    }

    public ManualReview(
        string siteId,
        string productId,
        decimal rating,
        int reviewCount,
        string displayText)
    {
        if (string.IsNullOrWhiteSpace(siteId))
            throw new ArgumentException("Site ID cannot be empty", nameof(siteId));

        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException("Product ID cannot be empty", nameof(productId));

        if (rating < 0 || rating > 5)
            throw new ArgumentOutOfRangeException(nameof(rating), "Rating must be between 0 and 5");

        if (reviewCount < 0)
            throw new ArgumentOutOfRangeException(nameof(reviewCount), "Review count cannot be negative");

        Id = Guid.NewGuid().ToString();
        SiteId = siteId;
        ProductId = productId;
        Rating = rating;
        ReviewCount = reviewCount;
        DisplayText = displayText ?? string.Empty;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(decimal rating, int reviewCount, string displayText)
    {
        if (rating < 0 || rating > 5)
            throw new ArgumentOutOfRangeException(nameof(rating), "Rating must be between 0 and 5");

        if (reviewCount < 0)
            throw new ArgumentOutOfRangeException(nameof(reviewCount), "Review count cannot be negative");

        Rating = rating;
        ReviewCount = reviewCount;
        DisplayText = displayText ?? string.Empty;
        UpdatedAt = DateTime.UtcNow;
    }
}

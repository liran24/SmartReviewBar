using SmartStickyReviewer.Domain.Enums;

namespace SmartStickyReviewer.Domain.Entities;

/// <summary>
/// Log entry for provider failures (for monitoring and notifications)
/// </summary>
public sealed class ProviderFailureLog
{
    public string Id { get; private set; }
    public string SiteId { get; private set; }
    public string ProductId { get; private set; }
    public ReviewProviderType ProviderType { get; private set; }
    public string ErrorMessage { get; private set; }
    public bool NotificationSent { get; private set; }
    public DateTime OccurredAt { get; private set; }

    // For MongoDB deserialization
    private ProviderFailureLog()
    {
        Id = string.Empty;
        SiteId = string.Empty;
        ProductId = string.Empty;
        ErrorMessage = string.Empty;
    }

    public ProviderFailureLog(
        string siteId,
        string productId,
        ReviewProviderType providerType,
        string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(siteId))
            throw new ArgumentException("Site ID cannot be empty", nameof(siteId));

        Id = Guid.NewGuid().ToString();
        SiteId = siteId;
        ProductId = productId;
        ProviderType = providerType;
        ErrorMessage = errorMessage ?? string.Empty;
        NotificationSent = false;
        OccurredAt = DateTime.UtcNow;
    }

    public void MarkNotificationSent()
    {
        NotificationSent = true;
    }
}

using SmartStickyReviewer.Domain.Enums;
using SmartStickyReviewer.Domain.ValueObjects;

namespace SmartStickyReviewer.Domain.Entities;

/// <summary>
/// Configuration for a site using the Smart Sticky Reviewer
/// </summary>
public sealed class SiteConfiguration
{
    public string Id { get; private set; }
    public string SiteId { get; private set; }
    public Plan Plan { get; private set; }
    public ReviewProviderType PrimaryProvider { get; private set; }
    public FallbackConfiguration FallbackConfig { get; private set; }
    public StickyBarStyle Style { get; private set; }
    public bool IsEnabled { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // For MongoDB deserialization
    private SiteConfiguration()
    {
        Id = string.Empty;
        SiteId = string.Empty;
        FallbackConfig = FallbackConfiguration.Default;
        Style = StickyBarStyle.Default;
    }

    public SiteConfiguration(
        string siteId,
        Plan plan = Plan.Free,
        ReviewProviderType primaryProvider = ReviewProviderType.JudgeMe,
        FallbackConfiguration? fallbackConfig = null,
        StickyBarStyle? style = null,
        bool isEnabled = true)
    {
        if (string.IsNullOrWhiteSpace(siteId))
            throw new ArgumentException("Site ID cannot be empty", nameof(siteId));

        Id = Guid.NewGuid().ToString();
        SiteId = siteId;
        Plan = plan;
        PrimaryProvider = primaryProvider;
        FallbackConfig = fallbackConfig ?? FallbackConfiguration.Default;
        Style = style ?? StickyBarStyle.Default;
        IsEnabled = isEnabled;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePlan(Plan newPlan)
    {
        Plan = newPlan;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePrimaryProvider(ReviewProviderType provider)
    {
        PrimaryProvider = provider;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateFallbackConfiguration(FallbackConfiguration config)
    {
        FallbackConfig = config ?? throw new ArgumentNullException(nameof(config));
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateStyle(StickyBarStyle style)
    {
        Style = style ?? throw new ArgumentNullException(nameof(style));
        UpdatedAt = DateTime.UtcNow;
    }

    public void Enable()
    {
        IsEnabled = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Disable()
    {
        IsEnabled = false;
        UpdatedAt = DateTime.UtcNow;
    }
}

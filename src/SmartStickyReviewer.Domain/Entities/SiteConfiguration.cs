using SmartStickyReviewer.Domain.Enums;
using SmartStickyReviewer.Domain.ValueObjects;

namespace SmartStickyReviewer.Domain.Entities;

public sealed class SiteConfiguration
{
    public SiteId SiteId { get; }
    public Plan Plan { get; private set; }

    public ReviewProviderKind PrimaryProvider { get; private set; }

    public ManualReview? ManualReview { get; private set; }

    /// <summary>
    /// Paid feature: ManualFallbackText
    /// </summary>
    public string? FallbackText { get; private set; }

    /// <summary>
    /// Paid feature: AdvancedStyling
    /// </summary>
    public StickyStyle Style { get; private set; }

    public string? StoreOwnerEmail { get; private set; }

    public SiteConfiguration(
        SiteId siteId,
        Plan plan,
        ReviewProviderKind primaryProvider,
        ManualReview? manualReview,
        string? fallbackText,
        StickyStyle? style,
        string? storeOwnerEmail)
    {
        SiteId = siteId;
        Plan = plan;
        PrimaryProvider = primaryProvider;
        ManualReview = manualReview;
        FallbackText = fallbackText;
        Style = style ?? StickyStyle.Default;
        StoreOwnerEmail = storeOwnerEmail;
    }

    public static SiteConfiguration CreateDefault(SiteId siteId) =>
        new(siteId, Plan.Free, ReviewProviderKind.Manual, null, null, StickyStyle.Default, null);

    public void UpdatePlan(Plan plan) => Plan = plan;

    public void UpdatePrimaryProvider(ReviewProviderKind primaryProvider) => PrimaryProvider = primaryProvider;

    public void UpdateManualReview(ManualReview? manualReview) => ManualReview = manualReview;

    public void UpdateFallbackText(string? fallbackText) => FallbackText = fallbackText;

    public void UpdateStyle(StickyStyle style) => Style = style ?? StickyStyle.Default;

    public void UpdateStoreOwnerEmail(string? email) => StoreOwnerEmail = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
}


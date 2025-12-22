using SmartStickyReviewer.Domain.Enums;
using SmartStickyReviewer.Domain.ValueObjects;

namespace SmartStickyReviewer.Domain.Interfaces;

public interface IFeaturePolicy
{
    Task<bool> IsEnabledAsync(SiteId siteId, Feature feature, CancellationToken ct);
}


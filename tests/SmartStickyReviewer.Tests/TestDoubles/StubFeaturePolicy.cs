using SmartStickyReviewer.Domain.Enums;
using SmartStickyReviewer.Domain.Interfaces;
using SmartStickyReviewer.Domain.ValueObjects;

namespace SmartStickyReviewer.Tests.TestDoubles;

public sealed class StubFeaturePolicy : IFeaturePolicy
{
    private readonly Func<Feature, bool> _resolver;

    public StubFeaturePolicy(Func<Feature, bool> resolver)
    {
        _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
    }

    public Task<bool> IsEnabledAsync(SiteId siteId, Feature feature, CancellationToken ct)
    {
        _ = siteId;
        _ = ct;
        return Task.FromResult(_resolver(feature));
    }
}


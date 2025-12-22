using SmartStickyReviewer.Domain.Entities;
using SmartStickyReviewer.Domain.Interfaces;
using SmartStickyReviewer.Domain.ValueObjects;

namespace SmartStickyReviewer.Tests.TestDoubles;

public sealed class InMemorySiteConfigurationRepository : ISiteConfigurationRepository
{
    private readonly Dictionary<string, SiteConfiguration> _store = new(StringComparer.Ordinal);

    public Task<SiteConfiguration?> GetAsync(SiteId siteId, CancellationToken ct)
    {
        _ = ct;
        _store.TryGetValue(siteId.Value, out var value);
        return Task.FromResult<SiteConfiguration?>(value);
    }

    public Task UpsertAsync(SiteConfiguration configuration, CancellationToken ct)
    {
        _ = ct;
        _store[configuration.SiteId.Value] = configuration;
        return Task.CompletedTask;
    }
}


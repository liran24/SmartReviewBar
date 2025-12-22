using SmartStickyReviewer.Domain.Interfaces;
using SmartStickyReviewer.Domain.ValueObjects;

namespace SmartStickyReviewer.Tests.TestDoubles;

public sealed class SpyNotifier : IStoreOwnerNotifier
{
    public int CallCount { get; private set; }
    public (SiteId SiteId, string? Email, string Message)? LastCall { get; private set; }

    public Task NotifyProviderFailureAsync(SiteId siteId, string? storeOwnerEmail, string message, CancellationToken ct)
    {
        _ = ct;
        CallCount++;
        LastCall = (siteId, storeOwnerEmail, message);
        return Task.CompletedTask;
    }
}


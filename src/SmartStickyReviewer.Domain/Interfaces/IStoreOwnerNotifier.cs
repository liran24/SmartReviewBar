using SmartStickyReviewer.Domain.ValueObjects;

namespace SmartStickyReviewer.Domain.Interfaces;

public interface IStoreOwnerNotifier
{
    Task NotifyProviderFailureAsync(SiteId siteId, string? storeOwnerEmail, string message, CancellationToken ct);
}


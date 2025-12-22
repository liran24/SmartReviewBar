using SmartStickyReviewer.Domain.Interfaces;
using SmartStickyReviewer.Domain.ValueObjects;

namespace SmartStickyReviewer.Infrastructure.Notifications;

public sealed class EmailStoreOwnerNotifier : IStoreOwnerNotifier
{
    public Task NotifyProviderFailureAsync(SiteId siteId, string? storeOwnerEmail, string message, CancellationToken ct)
    {
        // Placeholder only: no paid email provider integration in this project.
        // Intentionally does nothing besides completing successfully.
        _ = siteId;
        _ = storeOwnerEmail;
        _ = message;
        _ = ct;
        return Task.CompletedTask;
    }
}


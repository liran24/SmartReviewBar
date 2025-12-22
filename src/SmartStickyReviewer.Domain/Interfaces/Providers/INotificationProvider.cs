namespace SmartStickyReviewer.Domain.Interfaces.Providers;

/// <summary>
/// Interface for notification delivery (email, etc.)
/// </summary>
public interface INotificationProvider
{
    /// <summary>
    /// Sends a notification about provider failure
    /// </summary>
    Task<bool> SendFailureNotificationAsync(
        string recipientEmail,
        string siteId,
        string productId,
        string providerName,
        string errorMessage,
        CancellationToken cancellationToken = default);
}

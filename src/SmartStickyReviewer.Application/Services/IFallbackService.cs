using SmartStickyReviewer.Domain.Entities;
using SmartStickyReviewer.Domain.ValueObjects;

namespace SmartStickyReviewer.Application.Services;

/// <summary>
/// Service for handling fallback logic when primary provider fails
/// </summary>
public interface IFallbackService
{
    /// <summary>
    /// Attempts to get a fallback review result
    /// </summary>
    Task<ReviewResult?> GetFallbackAsync(
        SiteConfiguration config,
        string productId,
        string failedProviderName,
        string errorMessage,
        CancellationToken cancellationToken = default);
}

using SmartStickyReviewer.Domain.Entities;
using SmartStickyReviewer.Domain.Enums;
using SmartStickyReviewer.Domain.Interfaces.Policies;
using SmartStickyReviewer.Domain.Interfaces.Providers;
using SmartStickyReviewer.Domain.Interfaces.Repositories;
using SmartStickyReviewer.Domain.ValueObjects;

namespace SmartStickyReviewer.Application.Services;

/// <summary>
/// Implementation of fallback logic with feature policy checks
/// </summary>
public sealed class FallbackService : IFallbackService
{
    private readonly IManualReviewRepository _manualReviewRepository;
    private readonly IProviderFailureLogRepository _failureLogRepository;
    private readonly INotificationProvider _notificationProvider;
    private readonly IFeaturePolicy _featurePolicy;

    public FallbackService(
        IManualReviewRepository manualReviewRepository,
        IProviderFailureLogRepository failureLogRepository,
        INotificationProvider notificationProvider,
        IFeaturePolicy featurePolicy)
    {
        _manualReviewRepository = manualReviewRepository ?? throw new ArgumentNullException(nameof(manualReviewRepository));
        _failureLogRepository = failureLogRepository ?? throw new ArgumentNullException(nameof(failureLogRepository));
        _notificationProvider = notificationProvider ?? throw new ArgumentNullException(nameof(notificationProvider));
        _featurePolicy = featurePolicy ?? throw new ArgumentNullException(nameof(featurePolicy));
    }

    public async Task<ReviewResult?> GetFallbackAsync(
        SiteConfiguration config,
        string productId,
        string failedProviderName,
        string errorMessage,
        CancellationToken cancellationToken = default)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config));

        // Log the failure
        var failureLog = new ProviderFailureLog(
            config.SiteId,
            productId,
            config.PrimaryProvider,
            errorMessage);

        await _failureLogRepository.CreateAsync(failureLog, cancellationToken);

        // Try manual rating fallback first
        if (config.FallbackConfig.UseManualRatingFallback)
        {
            var manualReview = await _manualReviewRepository.GetAsync(
                config.SiteId,
                productId,
                cancellationToken);

            if (manualReview != null)
            {
                await HandleNotificationAsync(config, productId, failedProviderName, errorMessage, cancellationToken);

                return ReviewResult.Successful(
                    manualReview.Rating,
                    manualReview.ReviewCount,
                    manualReview.DisplayText,
                    "Manual",
                    isFallback: true);
            }

            // Use configured manual rating if available
            if (config.FallbackConfig.HasManualRating)
            {
                await HandleNotificationAsync(config, productId, failedProviderName, errorMessage, cancellationToken);

                return ReviewResult.Successful(
                    config.FallbackConfig.ManualRating!.Value,
                    config.FallbackConfig.ManualReviewCount ?? 0,
                    config.FallbackConfig.FallbackText ?? "Based on customer feedback",
                    "Manual Fallback",
                    isFallback: true);
            }
        }

        // Try fallback text (requires ManualFallbackText feature)
        if (config.FallbackConfig.HasFallbackText &&
            _featurePolicy.IsFeatureEnabled(Feature.ManualFallbackText, config.Plan))
        {
            await HandleNotificationAsync(config, productId, failedProviderName, errorMessage, cancellationToken);

            return ReviewResult.Successful(
                0,
                0,
                config.FallbackConfig.FallbackText!,
                "Fallback Text",
                isFallback: true);
        }

        // Handle notification even if no fallback is available
        await HandleNotificationAsync(config, productId, failedProviderName, errorMessage, cancellationToken);

        return null;
    }

    private async Task HandleNotificationAsync(
        SiteConfiguration config,
        string productId,
        string providerName,
        string errorMessage,
        CancellationToken cancellationToken)
    {
        // Check if notification feature is enabled and configured
        if (!config.FallbackConfig.NotifyOnFailure ||
            string.IsNullOrWhiteSpace(config.FallbackConfig.NotificationEmail))
        {
            return;
        }

        // Check if EmailNotificationOnFailure feature is enabled for the plan
        if (!_featurePolicy.IsFeatureEnabled(Feature.EmailNotificationOnFailure, config.Plan))
        {
            return;
        }

        await _notificationProvider.SendFailureNotificationAsync(
            config.FallbackConfig.NotificationEmail,
            config.SiteId,
            productId,
            providerName,
            errorMessage,
            cancellationToken);
    }
}

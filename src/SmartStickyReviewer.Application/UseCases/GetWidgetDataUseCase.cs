using SmartStickyReviewer.Application.Models;
using SmartStickyReviewer.Application.Services;
using SmartStickyReviewer.Domain.Entities;
using SmartStickyReviewer.Domain.Enums;
using SmartStickyReviewer.Domain.Interfaces;
using SmartStickyReviewer.Domain.ValueObjects;

namespace SmartStickyReviewer.Application.UseCases;

public sealed class GetWidgetDataUseCase
{
    private readonly ISiteConfigurationRepository _repository;
    private readonly IFeaturePolicy _featurePolicy;
    private readonly ReviewProviderSelector _selector;
    private readonly IStoreOwnerNotifier _notifier;

    public GetWidgetDataUseCase(
        ISiteConfigurationRepository repository,
        IFeaturePolicy featurePolicy,
        ReviewProviderSelector selector,
        IStoreOwnerNotifier notifier)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _featurePolicy = featurePolicy ?? throw new ArgumentNullException(nameof(featurePolicy));
        _selector = selector ?? throw new ArgumentNullException(nameof(selector));
        _notifier = notifier ?? throw new ArgumentNullException(nameof(notifier));
    }

    public async Task<WidgetData> ExecuteAsync(string siteId, string? productId, CancellationToken ct)
    {
        var id = new SiteId(siteId);
        var config = await _repository.GetAsync(id, ct) ?? SiteConfiguration.CreateDefault(id);

        var requestedProvider = config.PrimaryProvider;
        if (requestedProvider != ReviewProviderKind.Manual)
        {
            var canUse = await _featurePolicy.IsEnabledAsync(id, Feature.MultipleReviewProviders, ct);
            if (!canUse)
            {
                requestedProvider = ReviewProviderKind.Manual;
            }
        }

        var primaryContext = new ReviewContext(id, productId, requestedProvider, config.ManualReview);
        var provider = _selector.Select(primaryContext);

        ReviewResult primaryResult;
        try
        {
            primaryResult = provider is null
                ? ReviewResult.Failure("NoProvider", "No provider could handle the request.")
                : provider.GetReview(primaryContext);
        }
        catch (Exception ex)
        {
            primaryResult = ReviewResult.Failure(provider?.GetType().Name ?? "UnknownProvider", ex.Message);
        }

        if (primaryResult.IsSuccess)
        {
            return ToWidgetData(true, primaryResult, config, canUseAdvancedStyling: await _featurePolicy.IsEnabledAsync(id, Feature.AdvancedStyling, ct));
        }

        // Fallback strategy:
        // 1) manual rating if available
        if (config.ManualReview is not null)
        {
            var manual = ReviewResult.Success(config.ManualReview.Rating, config.ManualReview.Text, "ManualReviewProvider");
            return ToWidgetData(true, manual, config, canUseAdvancedStyling: await _featurePolicy.IsEnabledAsync(id, Feature.AdvancedStyling, ct));
        }

        // 2) fallback text if configured AND feature enabled
        var canUseFallbackText = await _featurePolicy.IsEnabledAsync(id, Feature.ManualFallbackText, ct);
        if (canUseFallbackText && !string.IsNullOrWhiteSpace(config.FallbackText))
        {
            var fallback = new ReviewResult(
                IsSuccess: true,
                Rating: null,
                Text: config.FallbackText,
                ProviderName: "FallbackText",
                FailureReason: null
            );
            return ToWidgetData(true, fallback, config, canUseAdvancedStyling: await _featurePolicy.IsEnabledAsync(id, Feature.AdvancedStyling, ct));
        }

        // 3) notify store owner (placeholder) if enabled, then fail silently
        if (await _featurePolicy.IsEnabledAsync(id, Feature.EmailNotificationOnFailure, ct))
        {
            await _notifier.NotifyProviderFailureAsync(id, config.StoreOwnerEmail, $"Primary provider failed: {primaryResult.FailureReason}", ct);
        }

        return ToWidgetData(false, primaryResult, config, canUseAdvancedStyling: await _featurePolicy.IsEnabledAsync(id, Feature.AdvancedStyling, ct));
    }

    private static WidgetData ToWidgetData(bool shouldRender, ReviewResult result, SiteConfiguration config, bool canUseAdvancedStyling)
    {
        var style = canUseAdvancedStyling ? config.Style : StickyStyle.Default;

        return new WidgetData(
            ShouldRender: shouldRender,
            Rating: result.Rating,
            Text: result.Text,
            ProviderName: result.ProviderName,
            BackgroundColorHex: style.BackgroundColorHex,
            TextColorHex: style.TextColorHex,
            AccentColorHex: style.AccentColorHex
        );
    }
}


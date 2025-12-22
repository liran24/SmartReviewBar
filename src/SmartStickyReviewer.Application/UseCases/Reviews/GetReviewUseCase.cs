using SmartStickyReviewer.Application.Services;
using SmartStickyReviewer.Domain.Enums;
using SmartStickyReviewer.Domain.Interfaces.Policies;
using SmartStickyReviewer.Domain.Interfaces.Repositories;
using SmartStickyReviewer.Domain.ValueObjects;

namespace SmartStickyReviewer.Application.UseCases.Reviews;

/// <summary>
/// Request for getting a review
/// </summary>
public sealed class GetReviewRequest
{
    public string SiteId { get; }
    public string ProductId { get; }

    public GetReviewRequest(string siteId, string productId)
    {
        SiteId = siteId;
        ProductId = productId;
    }
}

/// <summary>
/// Response for getting a review
/// </summary>
public sealed class GetReviewResponse
{
    public bool Success { get; }
    public decimal Rating { get; }
    public int ReviewCount { get; }
    public string DisplayText { get; }
    public string ProviderName { get; }
    public bool IsFallback { get; }
    public StickyBarStyle Style { get; }
    public string? ErrorMessage { get; }
    public bool IsEnabled { get; }

    private GetReviewResponse(
        bool success,
        decimal rating,
        int reviewCount,
        string displayText,
        string providerName,
        bool isFallback,
        StickyBarStyle style,
        string? errorMessage,
        bool isEnabled)
    {
        Success = success;
        Rating = rating;
        ReviewCount = reviewCount;
        DisplayText = displayText;
        ProviderName = providerName;
        IsFallback = isFallback;
        Style = style;
        ErrorMessage = errorMessage;
        IsEnabled = isEnabled;
    }

    public static GetReviewResponse FromResult(ReviewResult result, StickyBarStyle style, bool isEnabled = true)
    {
        return new GetReviewResponse(
            result.Success,
            result.Rating,
            result.ReviewCount,
            result.DisplayText,
            result.ProviderName,
            result.IsFallback,
            style,
            result.ErrorMessage,
            isEnabled);
    }

    public static GetReviewResponse Disabled()
    {
        return new GetReviewResponse(
            success: false,
            rating: 0,
            reviewCount: 0,
            displayText: string.Empty,
            providerName: string.Empty,
            isFallback: false,
            style: StickyBarStyle.Default,
            errorMessage: "Widget is disabled",
            isEnabled: false);
    }

    public static GetReviewResponse NotConfigured()
    {
        return new GetReviewResponse(
            success: false,
            rating: 0,
            reviewCount: 0,
            displayText: string.Empty,
            providerName: string.Empty,
            isFallback: false,
            style: StickyBarStyle.Default,
            errorMessage: "Site not configured",
            isEnabled: false);
    }

    public static GetReviewResponse Failed(string errorMessage, StickyBarStyle style)
    {
        return new GetReviewResponse(
            success: false,
            rating: 0,
            reviewCount: 0,
            displayText: string.Empty,
            providerName: string.Empty,
            isFallback: false,
            style: style,
            errorMessage: errorMessage,
            isEnabled: true);
    }
}

/// <summary>
/// Use case for fetching review data with fallback handling
/// </summary>
public sealed class GetReviewUseCase
{
    private readonly ISiteConfigurationRepository _configRepository;
    private readonly IReviewProviderSelector _providerSelector;
    private readonly IFallbackService _fallbackService;
    private readonly IFeaturePolicy _featurePolicy;

    public GetReviewUseCase(
        ISiteConfigurationRepository configRepository,
        IReviewProviderSelector providerSelector,
        IFallbackService fallbackService,
        IFeaturePolicy featurePolicy)
    {
        _configRepository = configRepository ?? throw new ArgumentNullException(nameof(configRepository));
        _providerSelector = providerSelector ?? throw new ArgumentNullException(nameof(providerSelector));
        _fallbackService = fallbackService ?? throw new ArgumentNullException(nameof(fallbackService));
        _featurePolicy = featurePolicy ?? throw new ArgumentNullException(nameof(featurePolicy));
    }

    public async Task<GetReviewResponse> ExecuteAsync(
        GetReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        // Get site configuration
        var config = await _configRepository.GetBySiteIdAsync(request.SiteId, cancellationToken);

        if (config == null)
            return GetReviewResponse.NotConfigured();

        if (!config.IsEnabled)
            return GetReviewResponse.Disabled();

        // Build review context
        var context = new ReviewContext(
            request.SiteId,
            request.ProductId,
            config.PrimaryProvider,
            config.Plan);

        // Select provider
        var provider = _providerSelector.SelectProvider(context);

        if (provider == null)
        {
            return GetReviewResponse.Failed("No suitable provider available", config.Style);
        }

        // Try to get review from provider
        var result = await provider.GetReviewAsync(context, cancellationToken);

        if (result.Success)
        {
            return GetReviewResponse.FromResult(result, config.Style);
        }

        // Provider failed, try fallback
        var fallbackResult = await _fallbackService.GetFallbackAsync(
            config,
            request.ProductId,
            provider.ProviderName,
            result.ErrorMessage ?? "Unknown error",
            cancellationToken);

        if (fallbackResult != null)
        {
            return GetReviewResponse.FromResult(fallbackResult, config.Style);
        }

        return GetReviewResponse.Failed(result.ErrorMessage ?? "Review unavailable", config.Style);
    }
}

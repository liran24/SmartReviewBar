using SmartStickyReviewer.Domain.Entities;
using SmartStickyReviewer.Domain.Enums;
using SmartStickyReviewer.Domain.Interfaces.Policies;
using SmartStickyReviewer.Domain.Interfaces.Repositories;
using SmartStickyReviewer.Domain.ValueObjects;

namespace SmartStickyReviewer.Application.UseCases.Configuration;

/// <summary>
/// Request for saving site configuration
/// </summary>
public sealed class SaveSiteConfigurationRequest
{
    public string SiteId { get; }
    public Plan Plan { get; }
    public ReviewProviderType PrimaryProvider { get; }
    public FallbackConfiguration FallbackConfig { get; }
    public StickyBarStyle Style { get; }
    public bool IsEnabled { get; }

    public SaveSiteConfigurationRequest(
        string siteId,
        Plan plan,
        ReviewProviderType primaryProvider,
        FallbackConfiguration fallbackConfig,
        StickyBarStyle style,
        bool isEnabled)
    {
        SiteId = siteId;
        Plan = plan;
        PrimaryProvider = primaryProvider;
        FallbackConfig = fallbackConfig;
        Style = style;
        IsEnabled = isEnabled;
    }
}

/// <summary>
/// Response for saving site configuration
/// </summary>
public sealed class SaveSiteConfigurationResponse
{
    public bool Success { get; }
    public string? SiteId { get; }
    public bool IsNew { get; }
    public IEnumerable<Feature>? EnabledFeatures { get; }
    public string? ErrorMessage { get; }

    private SaveSiteConfigurationResponse(
        bool success,
        string? siteId,
        bool isNew,
        IEnumerable<Feature>? enabledFeatures,
        string? errorMessage)
    {
        Success = success;
        SiteId = siteId;
        IsNew = isNew;
        EnabledFeatures = enabledFeatures;
        ErrorMessage = errorMessage;
    }

    public static SaveSiteConfigurationResponse Created(
        string siteId,
        IEnumerable<Feature> enabledFeatures)
    {
        return new SaveSiteConfigurationResponse(true, siteId, true, enabledFeatures, null);
    }

    public static SaveSiteConfigurationResponse Updated(
        string siteId,
        IEnumerable<Feature> enabledFeatures)
    {
        return new SaveSiteConfigurationResponse(true, siteId, false, enabledFeatures, null);
    }

    public static SaveSiteConfigurationResponse Failed(string errorMessage)
    {
        return new SaveSiteConfigurationResponse(false, null, false, null, errorMessage);
    }
}

/// <summary>
/// Use case for saving (create or update) site configuration
/// </summary>
public sealed class SaveSiteConfigurationUseCase
{
    private readonly ISiteConfigurationRepository _repository;
    private readonly IFeaturePolicy _featurePolicy;

    public SaveSiteConfigurationUseCase(
        ISiteConfigurationRepository repository,
        IFeaturePolicy featurePolicy)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _featurePolicy = featurePolicy ?? throw new ArgumentNullException(nameof(featurePolicy));
    }

    public async Task<SaveSiteConfigurationResponse> ExecuteAsync(
        SaveSiteConfigurationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.SiteId))
            return SaveSiteConfigurationResponse.Failed("Site ID is required");

        var enabledFeatures = _featurePolicy.GetEnabledFeatures(request.Plan);

        // Check for existing configuration
        var existing = await _repository.GetBySiteIdAsync(request.SiteId, cancellationToken);

        if (existing == null)
        {
            // Create new configuration
            var config = new SiteConfiguration(
                request.SiteId,
                request.Plan,
                request.PrimaryProvider,
                request.FallbackConfig,
                request.Style,
                request.IsEnabled);

            await _repository.CreateAsync(config, cancellationToken);

            return SaveSiteConfigurationResponse.Created(request.SiteId, enabledFeatures);
        }

        // Update existing configuration
        existing.UpdatePlan(request.Plan);
        existing.UpdatePrimaryProvider(request.PrimaryProvider);
        existing.UpdateFallbackConfiguration(request.FallbackConfig);
        existing.UpdateStyle(request.Style);

        if (request.IsEnabled)
            existing.Enable();
        else
            existing.Disable();

        await _repository.UpdateAsync(existing, cancellationToken);

        return SaveSiteConfigurationResponse.Updated(request.SiteId, enabledFeatures);
    }
}

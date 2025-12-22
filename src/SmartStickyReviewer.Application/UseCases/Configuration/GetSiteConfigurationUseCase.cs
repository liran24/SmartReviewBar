using SmartStickyReviewer.Domain.Entities;
using SmartStickyReviewer.Domain.Enums;
using SmartStickyReviewer.Domain.Interfaces.Policies;
using SmartStickyReviewer.Domain.Interfaces.Repositories;
using SmartStickyReviewer.Domain.ValueObjects;

namespace SmartStickyReviewer.Application.UseCases.Configuration;

/// <summary>
/// Request for getting site configuration
/// </summary>
public sealed class GetSiteConfigurationRequest
{
    public string SiteId { get; }

    public GetSiteConfigurationRequest(string siteId)
    {
        SiteId = siteId;
    }
}

/// <summary>
/// Response for getting site configuration
/// </summary>
public sealed class GetSiteConfigurationResponse
{
    public bool Found { get; }
    public string? SiteId { get; }
    public Plan? Plan { get; }
    public ReviewProviderType? PrimaryProvider { get; }
    public FallbackConfiguration? FallbackConfig { get; }
    public StickyBarStyle? Style { get; }
    public bool? IsEnabled { get; }
    public IEnumerable<Feature>? EnabledFeatures { get; }
    public DateTime? CreatedAt { get; }
    public DateTime? UpdatedAt { get; }

    private GetSiteConfigurationResponse(
        bool found,
        SiteConfiguration? config,
        IEnumerable<Feature>? enabledFeatures)
    {
        Found = found;
        if (config != null)
        {
            SiteId = config.SiteId;
            Plan = config.Plan;
            PrimaryProvider = config.PrimaryProvider;
            FallbackConfig = config.FallbackConfig;
            Style = config.Style;
            IsEnabled = config.IsEnabled;
            EnabledFeatures = enabledFeatures;
            CreatedAt = config.CreatedAt;
            UpdatedAt = config.UpdatedAt;
        }
    }

    public static GetSiteConfigurationResponse FromConfig(
        SiteConfiguration config,
        IEnumerable<Feature> enabledFeatures)
    {
        return new GetSiteConfigurationResponse(true, config, enabledFeatures);
    }

    public static GetSiteConfigurationResponse NotFound()
    {
        return new GetSiteConfigurationResponse(false, null, null);
    }
}

/// <summary>
/// Use case for retrieving site configuration
/// </summary>
public sealed class GetSiteConfigurationUseCase
{
    private readonly ISiteConfigurationRepository _repository;
    private readonly IFeaturePolicy _featurePolicy;

    public GetSiteConfigurationUseCase(
        ISiteConfigurationRepository repository,
        IFeaturePolicy featurePolicy)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _featurePolicy = featurePolicy ?? throw new ArgumentNullException(nameof(featurePolicy));
    }

    public async Task<GetSiteConfigurationResponse> ExecuteAsync(
        GetSiteConfigurationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var config = await _repository.GetBySiteIdAsync(request.SiteId, cancellationToken);

        if (config == null)
            return GetSiteConfigurationResponse.NotFound();

        var enabledFeatures = _featurePolicy.GetEnabledFeatures(config.Plan);

        return GetSiteConfigurationResponse.FromConfig(config, enabledFeatures);
    }
}

using SmartStickyReviewer.Application.Models;
using SmartStickyReviewer.Domain.Entities;
using SmartStickyReviewer.Domain.Enums;
using SmartStickyReviewer.Domain.Interfaces;
using SmartStickyReviewer.Domain.ValueObjects;

namespace SmartStickyReviewer.Application.UseCases;

public sealed class GetAdminConfigurationUseCase
{
    private static readonly Feature[] AllFeatures =
    [
        Feature.MultipleReviewProviders,
        Feature.ManualFallbackText,
        Feature.EmailNotificationOnFailure,
        Feature.AdvancedStyling
    ];

    private readonly ISiteConfigurationRepository _repository;
    private readonly IFeaturePolicy _featurePolicy;

    public GetAdminConfigurationUseCase(ISiteConfigurationRepository repository, IFeaturePolicy featurePolicy)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _featurePolicy = featurePolicy ?? throw new ArgumentNullException(nameof(featurePolicy));
    }

    public async Task<AdminConfigurationSnapshot> ExecuteAsync(string siteId, CancellationToken ct)
    {
        var id = new SiteId(siteId);
        var config = await _repository.GetAsync(id, ct) ?? SiteConfiguration.CreateDefault(id);

        var availability = new Dictionary<Feature, bool>();
        foreach (var feature in AllFeatures)
        {
            availability[feature] = await _featurePolicy.IsEnabledAsync(id, feature, ct);
        }

        return new AdminConfigurationSnapshot(config, availability);
    }
}


using SmartStickyReviewer.Domain.Entities;
using SmartStickyReviewer.Domain.Enums;
using SmartStickyReviewer.Domain.Interfaces;
using SmartStickyReviewer.Domain.ValueObjects;
using SmartStickyReviewer.Application.Services;

namespace SmartStickyReviewer.Infrastructure.Policies;

public sealed class PlanBasedFeaturePolicy : IFeaturePolicy
{
    private readonly ISiteConfigurationRepository _repository;
    private readonly PlanFeatureMatrix _matrix;

    public PlanBasedFeaturePolicy(ISiteConfigurationRepository repository, PlanFeatureMatrix matrix)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _matrix = matrix ?? throw new ArgumentNullException(nameof(matrix));
    }

    public async Task<bool> IsEnabledAsync(SiteId siteId, Feature feature, CancellationToken ct)
    {
        var config = await _repository.GetAsync(siteId, ct) ?? SiteConfiguration.CreateDefault(siteId);
        return _matrix.IsEnabled(config.Plan, feature);
    }
}


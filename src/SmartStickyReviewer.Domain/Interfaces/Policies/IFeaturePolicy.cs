using SmartStickyReviewer.Domain.Enums;

namespace SmartStickyReviewer.Domain.Interfaces.Policies;

/// <summary>
/// Policy interface for feature flag resolution based on subscription plan
/// </summary>
public interface IFeaturePolicy
{
    /// <summary>
    /// Checks if a feature is enabled for the given plan
    /// </summary>
    bool IsFeatureEnabled(Feature feature, Plan plan);

    /// <summary>
    /// Gets all features enabled for a given plan
    /// </summary>
    IEnumerable<Feature> GetEnabledFeatures(Plan plan);

    /// <summary>
    /// Gets the minimum plan required for a feature
    /// </summary>
    Plan GetMinimumPlanForFeature(Feature feature);
}

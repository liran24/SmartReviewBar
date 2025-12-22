using SmartStickyReviewer.Domain.Enums;

namespace SmartStickyReviewer.Application.Services;

/// <summary>
/// Data-driven mapping of plans to features.
/// This avoids branching on plan types in business logic (no "if(plan == X)").
/// </summary>
public sealed class PlanFeatureMatrix
{
    private readonly IReadOnlyDictionary<Plan, ISet<Feature>> _featuresByPlan = new Dictionary<Plan, ISet<Feature>>
    {
        [Plan.Free] = new HashSet<Feature>(),
        [Plan.Pro] = new HashSet<Feature>
        {
            Feature.MultipleReviewProviders,
            Feature.ManualFallbackText
        },
        [Plan.Premium] = new HashSet<Feature>
        {
            Feature.MultipleReviewProviders,
            Feature.ManualFallbackText,
            Feature.EmailNotificationOnFailure,
            Feature.AdvancedStyling
        }
    };

    public IReadOnlyCollection<Feature> GetEnabledFeatures(Plan plan) =>
        _featuresByPlan.TryGetValue(plan, out var features)
            ? features.ToArray()
            : Array.Empty<Feature>();

    public bool IsEnabled(Plan plan, Feature feature) =>
        _featuresByPlan.TryGetValue(plan, out var features) && features.Contains(feature);
}


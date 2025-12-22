using SmartStickyReviewer.Domain.Enums;
using SmartStickyReviewer.Domain.Interfaces.Policies;

namespace SmartStickyReviewer.Infrastructure.Policies;

/// <summary>
/// Feature policy based on subscription plan
/// Features are resolved dynamically per plan - no if(plan == X) in business logic
/// </summary>
public sealed class PlanBasedFeaturePolicy : IFeaturePolicy
{
    // Feature to minimum plan mapping
    private static readonly Dictionary<Feature, Plan> FeaturePlanMapping = new()
    {
        // Free plan features (available to all)
        // No features require Free plan minimum - all plans have it

        // Pro plan features
        { Feature.MultipleReviewProviders, Plan.Pro },
        { Feature.ManualFallbackText, Plan.Pro },

        // Premium plan features
        { Feature.EmailNotificationOnFailure, Plan.Premium },
        { Feature.AdvancedStyling, Plan.Premium }
    };

    public bool IsFeatureEnabled(Feature feature, Plan plan)
    {
        if (!FeaturePlanMapping.TryGetValue(feature, out var minimumPlan))
        {
            // Feature not in mapping means it's available to all
            return true;
        }

        // Plan enum values are ordered: Free(0) < Pro(1) < Premium(2)
        return plan >= minimumPlan;
    }

    public IEnumerable<Feature> GetEnabledFeatures(Plan plan)
    {
        return Enum.GetValues<Feature>()
            .Where(feature => IsFeatureEnabled(feature, plan));
    }

    public Plan GetMinimumPlanForFeature(Feature feature)
    {
        if (FeaturePlanMapping.TryGetValue(feature, out var minimumPlan))
        {
            return minimumPlan;
        }

        // Feature not in mapping means it's available to all (Free plan)
        return Plan.Free;
    }
}

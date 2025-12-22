using FluentAssertions;
using SmartStickyReviewer.Domain.Enums;
using SmartStickyReviewer.Infrastructure.Policies;

namespace SmartStickyReviewer.Tests.Infrastructure;

/// <summary>
/// Unit tests for PlanBasedFeaturePolicy
/// </summary>
public sealed class PlanBasedFeaturePolicyTests
{
    private readonly PlanBasedFeaturePolicy _policy;

    public PlanBasedFeaturePolicyTests()
    {
        _policy = new PlanBasedFeaturePolicy();
    }

    // ==========================================================================
    // Free Plan Tests
    // ==========================================================================

    [Fact]
    public void IsFeatureEnabled_FreePlan_MultipleReviewProviders_ReturnsFalse()
    {
        // Act
        var result = _policy.IsFeatureEnabled(Feature.MultipleReviewProviders, Plan.Free);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsFeatureEnabled_FreePlan_ManualFallbackText_ReturnsFalse()
    {
        // Act
        var result = _policy.IsFeatureEnabled(Feature.ManualFallbackText, Plan.Free);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsFeatureEnabled_FreePlan_EmailNotification_ReturnsFalse()
    {
        // Act
        var result = _policy.IsFeatureEnabled(Feature.EmailNotificationOnFailure, Plan.Free);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsFeatureEnabled_FreePlan_AdvancedStyling_ReturnsFalse()
    {
        // Act
        var result = _policy.IsFeatureEnabled(Feature.AdvancedStyling, Plan.Free);

        // Assert
        result.Should().BeFalse();
    }

    // ==========================================================================
    // Pro Plan Tests
    // ==========================================================================

    [Fact]
    public void IsFeatureEnabled_ProPlan_MultipleReviewProviders_ReturnsTrue()
    {
        // Act
        var result = _policy.IsFeatureEnabled(Feature.MultipleReviewProviders, Plan.Pro);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsFeatureEnabled_ProPlan_ManualFallbackText_ReturnsTrue()
    {
        // Act
        var result = _policy.IsFeatureEnabled(Feature.ManualFallbackText, Plan.Pro);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsFeatureEnabled_ProPlan_EmailNotification_ReturnsFalse()
    {
        // Act
        var result = _policy.IsFeatureEnabled(Feature.EmailNotificationOnFailure, Plan.Pro);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsFeatureEnabled_ProPlan_AdvancedStyling_ReturnsFalse()
    {
        // Act
        var result = _policy.IsFeatureEnabled(Feature.AdvancedStyling, Plan.Pro);

        // Assert
        result.Should().BeFalse();
    }

    // ==========================================================================
    // Premium Plan Tests
    // ==========================================================================

    [Fact]
    public void IsFeatureEnabled_PremiumPlan_AllFeatures_ReturnsTrue()
    {
        // Arrange
        var features = Enum.GetValues<Feature>();

        // Act & Assert
        foreach (var feature in features)
        {
            var result = _policy.IsFeatureEnabled(feature, Plan.Premium);
            result.Should().BeTrue($"Premium plan should have access to {feature}");
        }
    }

    // ==========================================================================
    // GetEnabledFeatures Tests
    // ==========================================================================

    [Fact]
    public void GetEnabledFeatures_FreePlan_ReturnsEmptyList()
    {
        // Act
        var features = _policy.GetEnabledFeatures(Plan.Free);

        // Assert
        features.Should().BeEmpty();
    }

    [Fact]
    public void GetEnabledFeatures_ProPlan_ReturnsProFeatures()
    {
        // Act
        var features = _policy.GetEnabledFeatures(Plan.Pro).ToList();

        // Assert
        features.Should().Contain(Feature.MultipleReviewProviders);
        features.Should().Contain(Feature.ManualFallbackText);
        features.Should().NotContain(Feature.EmailNotificationOnFailure);
        features.Should().NotContain(Feature.AdvancedStyling);
    }

    [Fact]
    public void GetEnabledFeatures_PremiumPlan_ReturnsAllFeatures()
    {
        // Act
        var features = _policy.GetEnabledFeatures(Plan.Premium).ToList();

        // Assert
        features.Should().HaveCount(4);
        features.Should().Contain(Feature.MultipleReviewProviders);
        features.Should().Contain(Feature.ManualFallbackText);
        features.Should().Contain(Feature.EmailNotificationOnFailure);
        features.Should().Contain(Feature.AdvancedStyling);
    }

    // ==========================================================================
    // GetMinimumPlanForFeature Tests
    // ==========================================================================

    [Fact]
    public void GetMinimumPlanForFeature_MultipleReviewProviders_ReturnsPro()
    {
        // Act
        var minPlan = _policy.GetMinimumPlanForFeature(Feature.MultipleReviewProviders);

        // Assert
        minPlan.Should().Be(Plan.Pro);
    }

    [Fact]
    public void GetMinimumPlanForFeature_ManualFallbackText_ReturnsPro()
    {
        // Act
        var minPlan = _policy.GetMinimumPlanForFeature(Feature.ManualFallbackText);

        // Assert
        minPlan.Should().Be(Plan.Pro);
    }

    [Fact]
    public void GetMinimumPlanForFeature_EmailNotification_ReturnsPremium()
    {
        // Act
        var minPlan = _policy.GetMinimumPlanForFeature(Feature.EmailNotificationOnFailure);

        // Assert
        minPlan.Should().Be(Plan.Premium);
    }

    [Fact]
    public void GetMinimumPlanForFeature_AdvancedStyling_ReturnsPremium()
    {
        // Act
        var minPlan = _policy.GetMinimumPlanForFeature(Feature.AdvancedStyling);

        // Assert
        minPlan.Should().Be(Plan.Premium);
    }

    // ==========================================================================
    // Plan Hierarchy Tests
    // ==========================================================================

    [Theory]
    [InlineData(Plan.Free, false)]
    [InlineData(Plan.Pro, true)]
    [InlineData(Plan.Premium, true)]
    public void IsFeatureEnabled_RespectsPlanHierarchy(Plan currentPlan, bool expected)
    {
        // Arrange
        // MultipleReviewProviders requires Pro plan
        var feature = Feature.MultipleReviewProviders;

        // Act
        var result = _policy.IsFeatureEnabled(feature, currentPlan);

        // Assert
        result.Should().Be(expected);
    }
}

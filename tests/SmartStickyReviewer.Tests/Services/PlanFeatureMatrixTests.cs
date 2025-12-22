using FluentAssertions;
using SmartStickyReviewer.Application.Services;
using SmartStickyReviewer.Domain.Enums;

namespace SmartStickyReviewer.Tests.Services;

public sealed class PlanFeatureMatrixTests
{
    [Fact]
    public void IsEnabled_FreePlan_HasNoPaidFeatures()
    {
        // Arrange
        var matrix = new PlanFeatureMatrix();

        // Act
        var enabled = matrix.GetEnabledFeatures(Plan.Free);

        // Assert
        enabled.Should().BeEmpty();
        matrix.IsEnabled(Plan.Free, Feature.MultipleReviewProviders).Should().BeFalse();
        matrix.IsEnabled(Plan.Free, Feature.ManualFallbackText).Should().BeFalse();
        matrix.IsEnabled(Plan.Free, Feature.EmailNotificationOnFailure).Should().BeFalse();
        matrix.IsEnabled(Plan.Free, Feature.AdvancedStyling).Should().BeFalse();
    }

    [Fact]
    public void IsEnabled_ProPlan_EnablesMultipleProvidersAndFallbackText()
    {
        // Arrange
        var matrix = new PlanFeatureMatrix();

        // Act / Assert
        matrix.IsEnabled(Plan.Pro, Feature.MultipleReviewProviders).Should().BeTrue();
        matrix.IsEnabled(Plan.Pro, Feature.ManualFallbackText).Should().BeTrue();
        matrix.IsEnabled(Plan.Pro, Feature.EmailNotificationOnFailure).Should().BeFalse();
        matrix.IsEnabled(Plan.Pro, Feature.AdvancedStyling).Should().BeFalse();
    }

    [Fact]
    public void IsEnabled_PremiumPlan_EnablesAllFeatures()
    {
        // Arrange
        var matrix = new PlanFeatureMatrix();

        // Act / Assert
        matrix.IsEnabled(Plan.Premium, Feature.MultipleReviewProviders).Should().BeTrue();
        matrix.IsEnabled(Plan.Premium, Feature.ManualFallbackText).Should().BeTrue();
        matrix.IsEnabled(Plan.Premium, Feature.EmailNotificationOnFailure).Should().BeTrue();
        matrix.IsEnabled(Plan.Premium, Feature.AdvancedStyling).Should().BeTrue();
    }
}


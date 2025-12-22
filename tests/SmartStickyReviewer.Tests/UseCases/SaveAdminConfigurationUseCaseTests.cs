using FluentAssertions;
using SmartStickyReviewer.Application.Models;
using SmartStickyReviewer.Application.UseCases;
using SmartStickyReviewer.Domain.Enums;
using SmartStickyReviewer.Domain.ValueObjects;
using SmartStickyReviewer.Tests.TestDoubles;

namespace SmartStickyReviewer.Tests.UseCases;

public sealed class SaveAdminConfigurationUseCaseTests
{
    [Fact]
    public async Task Execute_WhenMultipleProvidersDisabled_ForcesManualPrimaryProvider()
    {
        // Arrange
        var repo = new InMemorySiteConfigurationRepository();
        var featurePolicy = new StubFeaturePolicy(feature => feature != Feature.MultipleReviewProviders);
        var useCase = new SaveAdminConfigurationUseCase(repo, featurePolicy);

        var cmd = new SaveAdminConfigurationCommand(
            SiteId: "site-1",
            Plan: Plan.Free,
            PrimaryProvider: ReviewProviderKind.JudgeMe,
            ManualRating: 4.2m,
            ManualText: "Great!",
            FallbackText: null,
            StoreOwnerEmail: null,
            BackgroundColorHex: null,
            TextColorHex: null,
            AccentColorHex: null
        );

        // Act
        var result = await useCase.ExecuteAsync(cmd, CancellationToken.None);

        // Assert
        result.Configuration.PrimaryProvider.Should().Be(ReviewProviderKind.Manual);
        result.Warnings.Should().Contain(w => w.Contains("forced to Manual"));
    }

    [Fact]
    public async Task Execute_WhenFallbackTextFeatureDisabled_ClearsFallbackText()
    {
        // Arrange
        var repo = new InMemorySiteConfigurationRepository();
        var featurePolicy = new StubFeaturePolicy(feature => feature != Feature.ManualFallbackText);
        var useCase = new SaveAdminConfigurationUseCase(repo, featurePolicy);

        var cmd = new SaveAdminConfigurationCommand(
            SiteId: "site-1",
            Plan: Plan.Free,
            PrimaryProvider: ReviewProviderKind.Manual,
            ManualRating: null,
            ManualText: null,
            FallbackText: "Fallback text!",
            StoreOwnerEmail: null,
            BackgroundColorHex: null,
            TextColorHex: null,
            AccentColorHex: null
        );

        // Act
        var result = await useCase.ExecuteAsync(cmd, CancellationToken.None);

        // Assert
        result.Configuration.FallbackText.Should().BeNull();
        result.Warnings.Should().Contain(w => w.Contains("fallback text was cleared"));
    }

    [Fact]
    public async Task Execute_WhenAdvancedStylingDisabled_ResetsStyleToDefault()
    {
        // Arrange
        var repo = new InMemorySiteConfigurationRepository();
        var featurePolicy = new StubFeaturePolicy(feature => feature != Feature.AdvancedStyling);
        var useCase = new SaveAdminConfigurationUseCase(repo, featurePolicy);

        var cmd = new SaveAdminConfigurationCommand(
            SiteId: "site-1",
            Plan: Plan.Free,
            PrimaryProvider: ReviewProviderKind.Manual,
            ManualRating: null,
            ManualText: null,
            FallbackText: null,
            StoreOwnerEmail: null,
            BackgroundColorHex: "#000000",
            TextColorHex: "#ffffff",
            AccentColorHex: "#ff0000"
        );

        // Act
        var result = await useCase.ExecuteAsync(cmd, CancellationToken.None);

        // Assert
        result.Configuration.Style.Should().Be(StickyStyle.Default);
        result.Warnings.Should().Contain(w => w.Contains("reset to default"));
    }
}


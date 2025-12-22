using FluentAssertions;
using Moq;
using SmartStickyReviewer.Application.UseCases.Configuration;
using SmartStickyReviewer.Domain.Entities;
using SmartStickyReviewer.Domain.Enums;
using SmartStickyReviewer.Domain.Interfaces.Policies;
using SmartStickyReviewer.Domain.Interfaces.Repositories;
using SmartStickyReviewer.Domain.ValueObjects;

namespace SmartStickyReviewer.Tests.Application;

/// <summary>
/// Unit tests for SaveSiteConfigurationUseCase
/// </summary>
public sealed class SaveSiteConfigurationUseCaseTests
{
    private readonly Mock<ISiteConfigurationRepository> _repository;
    private readonly Mock<IFeaturePolicy> _featurePolicy;
    private readonly SaveSiteConfigurationUseCase _useCase;

    public SaveSiteConfigurationUseCaseTests()
    {
        _repository = new Mock<ISiteConfigurationRepository>();
        _featurePolicy = new Mock<IFeaturePolicy>();

        _featurePolicy.Setup(f => f.GetEnabledFeatures(It.IsAny<Plan>()))
            .Returns(new List<Feature> { Feature.MultipleReviewProviders });

        _useCase = new SaveSiteConfigurationUseCase(
            _repository.Object,
            _featurePolicy.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNewSite_CreatesConfiguration()
    {
        // Arrange
        var request = new SaveSiteConfigurationRequest(
            "site1",
            Plan.Pro,
            ReviewProviderType.JudgeMe,
            FallbackConfiguration.Default,
            StickyBarStyle.Default,
            true);

        _repository.Setup(r => r.GetBySiteIdAsync("site1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((SiteConfiguration?)null);

        _repository.Setup(r => r.CreateAsync(It.IsAny<SiteConfiguration>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SiteConfiguration c, CancellationToken _) => c);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.IsNew.Should().BeTrue();
        result.SiteId.Should().Be("site1");

        _repository.Verify(r => r.CreateAsync(
            It.Is<SiteConfiguration>(c => c.SiteId == "site1" && c.Plan == Plan.Pro),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenExistingSite_UpdatesConfiguration()
    {
        // Arrange
        var existingConfig = new SiteConfiguration("site1", Plan.Free);

        var request = new SaveSiteConfigurationRequest(
            "site1",
            Plan.Pro,
            ReviewProviderType.Manual,
            FallbackConfiguration.Default,
            StickyBarStyle.Default,
            true);

        _repository.Setup(r => r.GetBySiteIdAsync("site1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingConfig);

        _repository.Setup(r => r.UpdateAsync(It.IsAny<SiteConfiguration>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SiteConfiguration c, CancellationToken _) => c);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.IsNew.Should().BeFalse();
        result.SiteId.Should().Be("site1");

        _repository.Verify(r => r.UpdateAsync(
            It.Is<SiteConfiguration>(c =>
                c.SiteId == "site1" &&
                c.Plan == Plan.Pro &&
                c.PrimaryProvider == ReviewProviderType.Manual),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptySiteId_ReturnsFailed()
    {
        // Arrange
        var request = new SaveSiteConfigurationRequest(
            "",
            Plan.Free,
            ReviewProviderType.JudgeMe,
            FallbackConfiguration.Default,
            StickyBarStyle.Default,
            true);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Site ID is required");
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEnabledFeatures()
    {
        // Arrange
        var request = new SaveSiteConfigurationRequest(
            "site1",
            Plan.Pro,
            ReviewProviderType.JudgeMe,
            FallbackConfiguration.Default,
            StickyBarStyle.Default,
            true);

        _repository.Setup(r => r.GetBySiteIdAsync("site1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((SiteConfiguration?)null);

        _repository.Setup(r => r.CreateAsync(It.IsAny<SiteConfiguration>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SiteConfiguration c, CancellationToken _) => c);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.EnabledFeatures.Should().NotBeNull();
        result.EnabledFeatures.Should().Contain(Feature.MultipleReviewProviders);
    }

    [Fact]
    public async Task ExecuteAsync_WhenDisabled_DisablesWidget()
    {
        // Arrange
        var existingConfig = new SiteConfiguration("site1", Plan.Free, isEnabled: true);

        var request = new SaveSiteConfigurationRequest(
            "site1",
            Plan.Free,
            ReviewProviderType.JudgeMe,
            FallbackConfiguration.Default,
            StickyBarStyle.Default,
            isEnabled: false);

        _repository.Setup(r => r.GetBySiteIdAsync("site1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingConfig);

        _repository.Setup(r => r.UpdateAsync(It.IsAny<SiteConfiguration>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SiteConfiguration c, CancellationToken _) => c);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        _repository.Verify(r => r.UpdateAsync(
            It.Is<SiteConfiguration>(c => c.IsEnabled == false),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = async () => await _useCase.ExecuteAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}

using FluentAssertions;
using Moq;
using SmartStickyReviewer.Application.Services;
using SmartStickyReviewer.Application.UseCases.Reviews;
using SmartStickyReviewer.Domain.Entities;
using SmartStickyReviewer.Domain.Enums;
using SmartStickyReviewer.Domain.Interfaces.Policies;
using SmartStickyReviewer.Domain.Interfaces.Providers;
using SmartStickyReviewer.Domain.Interfaces.Repositories;
using SmartStickyReviewer.Domain.ValueObjects;

namespace SmartStickyReviewer.Tests.Application;

/// <summary>
/// Unit tests for GetReviewUseCase
/// </summary>
public sealed class GetReviewUseCaseTests
{
    private readonly Mock<ISiteConfigurationRepository> _configRepo;
    private readonly Mock<IReviewProviderSelector> _providerSelector;
    private readonly Mock<IFallbackService> _fallbackService;
    private readonly Mock<IFeaturePolicy> _featurePolicy;
    private readonly Mock<IReviewProvider> _reviewProvider;
    private readonly GetReviewUseCase _useCase;

    public GetReviewUseCaseTests()
    {
        _configRepo = new Mock<ISiteConfigurationRepository>();
        _providerSelector = new Mock<IReviewProviderSelector>();
        _fallbackService = new Mock<IFallbackService>();
        _featurePolicy = new Mock<IFeaturePolicy>();
        _reviewProvider = new Mock<IReviewProvider>();

        _reviewProvider.Setup(p => p.ProviderName).Returns("Judge.me");

        _useCase = new GetReviewUseCase(
            _configRepo.Object,
            _providerSelector.Object,
            _fallbackService.Object,
            _featurePolicy.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSiteNotConfigured_ReturnsNotConfigured()
    {
        // Arrange
        var request = new GetReviewRequest("site1", "product1");
        _configRepo.Setup(r => r.GetBySiteIdAsync("site1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((SiteConfiguration?)null);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not configured");
    }

    [Fact]
    public async Task ExecuteAsync_WhenWidgetDisabled_ReturnsDisabled()
    {
        // Arrange
        var request = new GetReviewRequest("site1", "product1");
        var config = new SiteConfiguration("site1", isEnabled: false);

        _configRepo.Setup(r => r.GetBySiteIdAsync("site1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(config);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.IsEnabled.Should().BeFalse();
        result.ErrorMessage.Should().Contain("disabled");
    }

    [Fact]
    public async Task ExecuteAsync_WhenProviderSucceeds_ReturnsReview()
    {
        // Arrange
        var request = new GetReviewRequest("site1", "product1");
        var config = new SiteConfiguration("site1");

        _configRepo.Setup(r => r.GetBySiteIdAsync("site1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(config);

        _providerSelector.Setup(s => s.SelectProvider(It.IsAny<ReviewContext>()))
            .Returns(_reviewProvider.Object);

        var reviewResult = ReviewResult.Successful(4.5m, 100, "Great product!", "Judge.me");
        _reviewProvider.Setup(p => p.GetReviewAsync(It.IsAny<ReviewContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reviewResult);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.Rating.Should().Be(4.5m);
        result.ReviewCount.Should().Be(100);
        result.DisplayText.Should().Be("Great product!");
        result.ProviderName.Should().Be("Judge.me");
        result.IsFallback.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_WhenProviderFailsAndFallbackSucceeds_ReturnsFallback()
    {
        // Arrange
        var request = new GetReviewRequest("site1", "product1");
        var config = new SiteConfiguration("site1");

        _configRepo.Setup(r => r.GetBySiteIdAsync("site1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(config);

        _providerSelector.Setup(s => s.SelectProvider(It.IsAny<ReviewContext>()))
            .Returns(_reviewProvider.Object);

        var failedResult = ReviewResult.Failed("API Error", "Judge.me");
        _reviewProvider.Setup(p => p.GetReviewAsync(It.IsAny<ReviewContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(failedResult);

        var fallbackResult = ReviewResult.Successful(4.0m, 50, "Fallback text", "Manual", isFallback: true);
        _fallbackService.Setup(f => f.GetFallbackAsync(
            config, "product1", "Judge.me", "API Error", It.IsAny<CancellationToken>()))
            .ReturnsAsync(fallbackResult);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.Rating.Should().Be(4.0m);
        result.IsFallback.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoProviderAvailable_ReturnsFailed()
    {
        // Arrange
        var request = new GetReviewRequest("site1", "product1");
        var config = new SiteConfiguration("site1");

        _configRepo.Setup(r => r.GetBySiteIdAsync("site1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(config);

        _providerSelector.Setup(s => s.SelectProvider(It.IsAny<ReviewContext>()))
            .Returns((IReviewProvider?)null);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("No suitable provider");
    }

    [Fact]
    public async Task ExecuteAsync_WhenProviderAndFallbackFail_ReturnsFailed()
    {
        // Arrange
        var request = new GetReviewRequest("site1", "product1");
        var config = new SiteConfiguration("site1");

        _configRepo.Setup(r => r.GetBySiteIdAsync("site1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(config);

        _providerSelector.Setup(s => s.SelectProvider(It.IsAny<ReviewContext>()))
            .Returns(_reviewProvider.Object);

        var failedResult = ReviewResult.Failed("API Error", "Judge.me");
        _reviewProvider.Setup(p => p.GetReviewAsync(It.IsAny<ReviewContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(failedResult);

        _fallbackService.Setup(f => f.GetFallbackAsync(
            config, "product1", "Judge.me", "API Error", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ReviewResult?)null);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("API Error");
    }

    [Fact]
    public async Task ExecuteAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = async () => await _useCase.ExecuteAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}

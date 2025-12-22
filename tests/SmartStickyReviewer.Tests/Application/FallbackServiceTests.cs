using FluentAssertions;
using Moq;
using SmartStickyReviewer.Application.Services;
using SmartStickyReviewer.Domain.Entities;
using SmartStickyReviewer.Domain.Enums;
using SmartStickyReviewer.Domain.Interfaces.Policies;
using SmartStickyReviewer.Domain.Interfaces.Providers;
using SmartStickyReviewer.Domain.Interfaces.Repositories;
using SmartStickyReviewer.Domain.ValueObjects;

namespace SmartStickyReviewer.Tests.Application;

/// <summary>
/// Unit tests for FallbackService
/// </summary>
public sealed class FallbackServiceTests
{
    private readonly Mock<IManualReviewRepository> _manualReviewRepo;
    private readonly Mock<IProviderFailureLogRepository> _failureLogRepo;
    private readonly Mock<INotificationProvider> _notificationProvider;
    private readonly Mock<IFeaturePolicy> _featurePolicy;
    private readonly FallbackService _fallbackService;

    public FallbackServiceTests()
    {
        _manualReviewRepo = new Mock<IManualReviewRepository>();
        _failureLogRepo = new Mock<IProviderFailureLogRepository>();
        _notificationProvider = new Mock<INotificationProvider>();
        _featurePolicy = new Mock<IFeaturePolicy>();

        _fallbackService = new FallbackService(
            _manualReviewRepo.Object,
            _failureLogRepo.Object,
            _notificationProvider.Object,
            _featurePolicy.Object);
    }

    [Fact]
    public async Task GetFallbackAsync_WhenManualReviewExists_ReturnsManualReview()
    {
        // Arrange
        var fallbackConfig = new FallbackConfiguration(useManualRatingFallback: true);
        var config = new SiteConfiguration("site1", Plan.Pro, fallbackConfig: fallbackConfig);
        var manualReview = new ManualReview("site1", "product1", 4.5m, 100, "Great product!");

        _manualReviewRepo.Setup(r => r.GetAsync("site1", "product1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(manualReview);

        // Act
        var result = await _fallbackService.GetFallbackAsync(
            config, "product1", "Judge.me", "API Error");

        // Assert
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Rating.Should().Be(4.5m);
        result.ReviewCount.Should().Be(100);
        result.IsFallback.Should().BeTrue();
        result.ProviderName.Should().Be("Manual");
    }

    [Fact]
    public async Task GetFallbackAsync_WhenManualRatingConfigured_ReturnsConfiguredRating()
    {
        // Arrange
        var fallbackConfig = new FallbackConfiguration(
            useManualRatingFallback: true,
            manualRating: 4.0m,
            manualReviewCount: 50,
            fallbackText: "Based on customer feedback");
        var config = new SiteConfiguration("site1", Plan.Pro, fallbackConfig: fallbackConfig);

        _manualReviewRepo.Setup(r => r.GetAsync("site1", "product1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ManualReview?)null);

        // Act
        var result = await _fallbackService.GetFallbackAsync(
            config, "product1", "Judge.me", "API Error");

        // Assert
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Rating.Should().Be(4.0m);
        result.ReviewCount.Should().Be(50);
        result.DisplayText.Should().Be("Based on customer feedback");
        result.IsFallback.Should().BeTrue();
    }

    [Fact]
    public async Task GetFallbackAsync_WhenFallbackTextEnabledForPlan_ReturnsFallbackText()
    {
        // Arrange
        var fallbackConfig = new FallbackConfiguration(
            useManualRatingFallback: false,
            fallbackText: "Trusted by customers");
        var config = new SiteConfiguration("site1", Plan.Pro, fallbackConfig: fallbackConfig);

        _featurePolicy.Setup(f => f.IsFeatureEnabled(Feature.ManualFallbackText, Plan.Pro))
            .Returns(true);

        // Act
        var result = await _fallbackService.GetFallbackAsync(
            config, "product1", "Judge.me", "API Error");

        // Assert
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.DisplayText.Should().Be("Trusted by customers");
        result.IsFallback.Should().BeTrue();
    }

    [Fact]
    public async Task GetFallbackAsync_WhenFallbackTextNotEnabledForPlan_ReturnsNull()
    {
        // Arrange
        var fallbackConfig = new FallbackConfiguration(
            useManualRatingFallback: false,
            fallbackText: "Trusted by customers");
        var config = new SiteConfiguration("site1", Plan.Free, fallbackConfig: fallbackConfig);

        _featurePolicy.Setup(f => f.IsFeatureEnabled(Feature.ManualFallbackText, Plan.Free))
            .Returns(false);

        // Act
        var result = await _fallbackService.GetFallbackAsync(
            config, "product1", "Judge.me", "API Error");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetFallbackAsync_LogsFailure()
    {
        // Arrange
        var fallbackConfig = new FallbackConfiguration(useManualRatingFallback: false);
        var config = new SiteConfiguration("site1", Plan.Free, fallbackConfig: fallbackConfig);

        // Act
        await _fallbackService.GetFallbackAsync(
            config, "product1", "Judge.me", "API Error");

        // Assert
        _failureLogRepo.Verify(
            r => r.CreateAsync(
                It.Is<ProviderFailureLog>(log =>
                    log.SiteId == "site1" &&
                    log.ProductId == "product1" &&
                    log.ErrorMessage == "API Error"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetFallbackAsync_WhenNotificationEnabledAndPremiumPlan_SendsNotification()
    {
        // Arrange
        var fallbackConfig = new FallbackConfiguration(
            useManualRatingFallback: false,
            notifyOnFailure: true,
            notificationEmail: "owner@example.com");
        var config = new SiteConfiguration("site1", Plan.Premium, fallbackConfig: fallbackConfig);

        _featurePolicy.Setup(f => f.IsFeatureEnabled(Feature.EmailNotificationOnFailure, Plan.Premium))
            .Returns(true);
        _notificationProvider.Setup(n => n.SendFailureNotificationAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _fallbackService.GetFallbackAsync(
            config, "product1", "Judge.me", "API Error");

        // Assert
        _notificationProvider.Verify(
            n => n.SendFailureNotificationAsync(
                "owner@example.com", "site1", "product1", "Judge.me", "API Error",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetFallbackAsync_WhenNotificationNotEnabledForPlan_DoesNotSendNotification()
    {
        // Arrange
        var fallbackConfig = new FallbackConfiguration(
            useManualRatingFallback: false,
            notifyOnFailure: true,
            notificationEmail: "owner@example.com");
        var config = new SiteConfiguration("site1", Plan.Free, fallbackConfig: fallbackConfig);

        _featurePolicy.Setup(f => f.IsFeatureEnabled(Feature.EmailNotificationOnFailure, Plan.Free))
            .Returns(false);

        // Act
        await _fallbackService.GetFallbackAsync(
            config, "product1", "Judge.me", "API Error");

        // Assert
        _notificationProvider.Verify(
            n => n.SendFailureNotificationAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetFallbackAsync_WithNullConfig_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = async () => await _fallbackService.GetFallbackAsync(
            null!, "product1", "Judge.me", "Error");
        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}

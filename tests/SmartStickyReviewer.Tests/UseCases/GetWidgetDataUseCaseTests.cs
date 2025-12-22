using FluentAssertions;
using SmartStickyReviewer.Application.Services;
using SmartStickyReviewer.Application.UseCases;
using SmartStickyReviewer.Domain.Entities;
using SmartStickyReviewer.Domain.Enums;
using SmartStickyReviewer.Domain.Interfaces;
using SmartStickyReviewer.Domain.ValueObjects;
using SmartStickyReviewer.Tests.TestDoubles;

namespace SmartStickyReviewer.Tests.UseCases;

public sealed class GetWidgetDataUseCaseTests
{
    [Fact]
    public async Task Execute_WhenPrimaryProviderThrows_UsesManualRatingFallback()
    {
        // Arrange
        var repo = new InMemorySiteConfigurationRepository();
        var siteId = new SiteId("site-1");
        var config = new SiteConfiguration(
            siteId,
            plan: Plan.Premium,
            primaryProvider: ReviewProviderKind.JudgeMe,
            manualReview: new ManualReview(new StarRating(4.6m), "Loved it"),
            fallbackText: null,
            style: StickyStyle.Default,
            storeOwnerEmail: "owner@example.com"
        );
        await repo.UpsertAsync(config, CancellationToken.None);

        var featurePolicy = new StubFeaturePolicy(feature => feature switch
        {
            Feature.MultipleReviewProviders => true,
            Feature.ManualFallbackText => false,
            Feature.EmailNotificationOnFailure => false,
            Feature.AdvancedStyling => false,
            _ => false
        });

        var selector = new ReviewProviderSelector(new SmartStickyReviewer.Domain.Interfaces.IReviewProvider[]
        {
            new ThrowingReviewProvider(ReviewProviderKind.JudgeMe, "primary failed"),
            new ManualReviewProviderDouble()
        });

        var notifier = new SpyNotifier();
        var useCase = new GetWidgetDataUseCase(repo, featurePolicy, selector, notifier);

        // Act
        var widget = await useCase.ExecuteAsync(siteId.Value, productId: null, CancellationToken.None);

        // Assert
        widget.ShouldRender.Should().BeTrue();
        widget.Rating!.Value.Value.Should().Be(4.6m);
        widget.Text.Should().Be("Loved it");
        widget.ProviderName.Should().Be("ManualReviewProvider");
        notifier.CallCount.Should().Be(0);
    }

    [Fact]
    public async Task Execute_WhenPrimaryProviderThrows_UsesFallbackTextWhenEnabled()
    {
        // Arrange
        var repo = new InMemorySiteConfigurationRepository();
        var siteId = new SiteId("site-1");
        var config = new SiteConfiguration(
            siteId,
            plan: Plan.Pro,
            primaryProvider: ReviewProviderKind.JudgeMe,
            manualReview: null,
            fallbackText: "We are loved by thousands of customers.",
            style: StickyStyle.Default,
            storeOwnerEmail: null
        );
        await repo.UpsertAsync(config, CancellationToken.None);

        var featurePolicy = new StubFeaturePolicy(feature => feature switch
        {
            Feature.MultipleReviewProviders => true,
            Feature.ManualFallbackText => true,
            Feature.EmailNotificationOnFailure => false,
            Feature.AdvancedStyling => false,
            _ => false
        });

        var selector = new ReviewProviderSelector(new SmartStickyReviewer.Domain.Interfaces.IReviewProvider[]
        {
            new ThrowingReviewProvider(ReviewProviderKind.JudgeMe),
            new ManualReviewProviderDouble()
        });

        var notifier = new SpyNotifier();
        var useCase = new GetWidgetDataUseCase(repo, featurePolicy, selector, notifier);

        // Act
        var widget = await useCase.ExecuteAsync(siteId.Value, productId: null, CancellationToken.None);

        // Assert
        widget.ShouldRender.Should().BeTrue();
        widget.Rating.Should().BeNull();
        widget.Text.Should().Be(config.FallbackText);
        widget.ProviderName.Should().Be("FallbackText");
        notifier.CallCount.Should().Be(0);
    }

    [Fact]
    public async Task Execute_WhenAllFallbacksUnavailable_AndEmailEnabled_NotifiesAndFailsSilently()
    {
        // Arrange
        var repo = new InMemorySiteConfigurationRepository();
        var siteId = new SiteId("site-1");
        var config = new SiteConfiguration(
            siteId,
            plan: Plan.Premium,
            primaryProvider: ReviewProviderKind.JudgeMe,
            manualReview: null,
            fallbackText: null,
            style: StickyStyle.Default,
            storeOwnerEmail: "owner@example.com"
        );
        await repo.UpsertAsync(config, CancellationToken.None);

        var featurePolicy = new StubFeaturePolicy(feature => feature switch
        {
            Feature.MultipleReviewProviders => true,
            Feature.ManualFallbackText => false,
            Feature.EmailNotificationOnFailure => true,
            Feature.AdvancedStyling => false,
            _ => false
        });

        var selector = new ReviewProviderSelector(new SmartStickyReviewer.Domain.Interfaces.IReviewProvider[]
        {
            new ThrowingReviewProvider(ReviewProviderKind.JudgeMe, "primary failed"),
            new ManualReviewProviderDouble()
        });

        var notifier = new SpyNotifier();
        var useCase = new GetWidgetDataUseCase(repo, featurePolicy, selector, notifier);

        // Act
        var widget = await useCase.ExecuteAsync(siteId.Value, productId: null, CancellationToken.None);

        // Assert
        widget.ShouldRender.Should().BeFalse();
        notifier.CallCount.Should().Be(1);
        notifier.LastCall!.Value.Email.Should().Be("owner@example.com");
        notifier.LastCall!.Value.Message.Should().Contain("primary failed");
    }

    [Fact]
    public async Task Execute_WhenMultipleProvidersDisabled_ForcesManualPrimaryProvider()
    {
        // Arrange
        var repo = new InMemorySiteConfigurationRepository();
        var siteId = new SiteId("site-1");
        var config = new SiteConfiguration(
            siteId,
            plan: Plan.Free,
            primaryProvider: ReviewProviderKind.JudgeMe,
            manualReview: new ManualReview(new StarRating(4.9m), "Superb"),
            fallbackText: null,
            style: StickyStyle.Default,
            storeOwnerEmail: null
        );
        await repo.UpsertAsync(config, CancellationToken.None);

        var featurePolicy = new StubFeaturePolicy(feature => feature switch
        {
            Feature.MultipleReviewProviders => false,
            Feature.ManualFallbackText => false,
            Feature.EmailNotificationOnFailure => false,
            Feature.AdvancedStyling => false,
            _ => false
        });

        var selector = new ReviewProviderSelector(new IReviewProvider[]
        {
            new ThrowingReviewProvider(ReviewProviderKind.JudgeMe),
            new ManualReviewProviderDouble()
        });

        var notifier = new SpyNotifier();
        var useCase = new GetWidgetDataUseCase(repo, featurePolicy, selector, notifier);

        // Act
        var widget = await useCase.ExecuteAsync(siteId.Value, productId: null, CancellationToken.None);

        // Assert
        widget.ShouldRender.Should().BeTrue();
        widget.ProviderName.Should().Be("ManualReviewProvider");
        widget.Rating!.Value.Value.Should().Be(4.9m);
    }
}


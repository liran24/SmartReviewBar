using FluentAssertions;
using Moq;
using SmartStickyReviewer.Application.Services;
using SmartStickyReviewer.Domain.Enums;
using SmartStickyReviewer.Domain.Interfaces.Providers;
using SmartStickyReviewer.Domain.ValueObjects;

namespace SmartStickyReviewer.Tests.Application;

/// <summary>
/// Unit tests for ReviewProviderSelector
/// </summary>
public sealed class ReviewProviderSelectorTests
{
    private readonly Mock<IReviewProvider> _judgeMeProvider;
    private readonly Mock<IReviewProvider> _manualProvider;
    private readonly ReviewProviderSelector _selector;

    public ReviewProviderSelectorTests()
    {
        _judgeMeProvider = new Mock<IReviewProvider>();
        _judgeMeProvider.Setup(p => p.ProviderType).Returns(ReviewProviderType.JudgeMe);
        _judgeMeProvider.Setup(p => p.ProviderName).Returns("Judge.me");

        _manualProvider = new Mock<IReviewProvider>();
        _manualProvider.Setup(p => p.ProviderType).Returns(ReviewProviderType.Manual);
        _manualProvider.Setup(p => p.ProviderName).Returns("Manual");

        var providers = new List<IReviewProvider>
        {
            _judgeMeProvider.Object,
            _manualProvider.Object
        };

        _selector = new ReviewProviderSelector(providers);
    }

    [Fact]
    public void SelectProvider_WhenPreferredProviderCanHandle_ReturnsPreferredProvider()
    {
        // Arrange
        var context = new ReviewContext("site1", "product1", ReviewProviderType.JudgeMe, Plan.Free);
        _judgeMeProvider.Setup(p => p.CanHandle(context)).Returns(true);
        _manualProvider.Setup(p => p.CanHandle(context)).Returns(true);

        // Act
        var result = _selector.SelectProvider(context);

        // Assert
        result.Should().NotBeNull();
        result!.ProviderType.Should().Be(ReviewProviderType.JudgeMe);
    }

    [Fact]
    public void SelectProvider_WhenPreferredProviderCannotHandle_ReturnsFallbackProvider()
    {
        // Arrange
        var context = new ReviewContext("site1", "product1", ReviewProviderType.JudgeMe, Plan.Free);
        _judgeMeProvider.Setup(p => p.CanHandle(context)).Returns(false);
        _manualProvider.Setup(p => p.CanHandle(context)).Returns(true);

        // Act
        var result = _selector.SelectProvider(context);

        // Assert
        result.Should().NotBeNull();
        result!.ProviderType.Should().Be(ReviewProviderType.Manual);
    }

    [Fact]
    public void SelectProvider_WhenNoProviderCanHandle_ReturnsNull()
    {
        // Arrange
        var context = new ReviewContext("site1", "product1", ReviewProviderType.JudgeMe, Plan.Free);
        _judgeMeProvider.Setup(p => p.CanHandle(context)).Returns(false);
        _manualProvider.Setup(p => p.CanHandle(context)).Returns(false);

        // Act
        var result = _selector.SelectProvider(context);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void SelectProvider_WhenManualPreferred_ReturnsManualProvider()
    {
        // Arrange
        var context = new ReviewContext("site1", "product1", ReviewProviderType.Manual, Plan.Pro);
        _judgeMeProvider.Setup(p => p.CanHandle(context)).Returns(true);
        _manualProvider.Setup(p => p.CanHandle(context)).Returns(true);

        // Act
        var result = _selector.SelectProvider(context);

        // Assert
        result.Should().NotBeNull();
        result!.ProviderType.Should().Be(ReviewProviderType.Manual);
    }

    [Fact]
    public void SelectProvider_WithNullContext_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        var act = () => _selector.SelectProvider(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetAllProviders_ReturnsAllRegisteredProviders()
    {
        // Act
        var providers = _selector.GetAllProviders();

        // Assert
        providers.Should().HaveCount(2);
        providers.Should().Contain(p => p.ProviderType == ReviewProviderType.JudgeMe);
        providers.Should().Contain(p => p.ProviderType == ReviewProviderType.Manual);
    }

    [Fact]
    public void Constructor_WithNullProviders_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        var act = () => new ReviewProviderSelector(null!);
        act.Should().Throw<ArgumentNullException>();
    }
}

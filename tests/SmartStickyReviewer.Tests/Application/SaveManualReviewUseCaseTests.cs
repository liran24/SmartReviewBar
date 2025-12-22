using FluentAssertions;
using Moq;
using SmartStickyReviewer.Application.UseCases.Configuration;
using SmartStickyReviewer.Domain.Entities;
using SmartStickyReviewer.Domain.Interfaces.Repositories;

namespace SmartStickyReviewer.Tests.Application;

/// <summary>
/// Unit tests for SaveManualReviewUseCase
/// </summary>
public sealed class SaveManualReviewUseCaseTests
{
    private readonly Mock<IManualReviewRepository> _repository;
    private readonly SaveManualReviewUseCase _useCase;

    public SaveManualReviewUseCaseTests()
    {
        _repository = new Mock<IManualReviewRepository>();
        _useCase = new SaveManualReviewUseCase(_repository.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNewReview_CreatesReview()
    {
        // Arrange
        var request = new SaveManualReviewRequest(
            "site1", "product1", 4.5m, 100, "Great product!");

        _repository.Setup(r => r.GetAsync("site1", "product1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ManualReview?)null);

        _repository.Setup(r => r.CreateAsync(It.IsAny<ManualReview>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ManualReview r, CancellationToken _) => r);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.IsNew.Should().BeTrue();

        _repository.Verify(r => r.CreateAsync(
            It.Is<ManualReview>(m =>
                m.SiteId == "site1" &&
                m.ProductId == "product1" &&
                m.Rating == 4.5m &&
                m.ReviewCount == 100),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenExistingReview_UpdatesReview()
    {
        // Arrange
        var existingReview = new ManualReview("site1", "product1", 3.0m, 50, "Old text");

        var request = new SaveManualReviewRequest(
            "site1", "product1", 4.5m, 100, "Updated text");

        _repository.Setup(r => r.GetAsync("site1", "product1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingReview);

        _repository.Setup(r => r.UpdateAsync(It.IsAny<ManualReview>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ManualReview r, CancellationToken _) => r);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.IsNew.Should().BeFalse();

        _repository.Verify(r => r.UpdateAsync(
            It.Is<ManualReview>(m =>
                m.Rating == 4.5m &&
                m.ReviewCount == 100 &&
                m.DisplayText == "Updated text"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptySiteId_ReturnsFailed()
    {
        // Arrange
        var request = new SaveManualReviewRequest("", "product1", 4.5m, 100, "Text");

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Site ID is required");
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyProductId_ReturnsFailed()
    {
        // Arrange
        var request = new SaveManualReviewRequest("site1", "", 4.5m, 100, "Text");

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Product ID is required");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(6)]
    public async Task ExecuteAsync_WithInvalidRating_ReturnsFailed(decimal rating)
    {
        // Arrange
        var request = new SaveManualReviewRequest("site1", "product1", rating, 100, "Text");

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Rating must be between 0 and 5");
    }

    [Fact]
    public async Task ExecuteAsync_WithNegativeReviewCount_ReturnsFailed()
    {
        // Arrange
        var request = new SaveManualReviewRequest("site1", "product1", 4.5m, -1, "Text");

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Review count cannot be negative");
    }

    [Fact]
    public async Task ExecuteAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = async () => await _useCase.ExecuteAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}

using FluentAssertions;
using SmartStickyReviewer.Application.Services;
using SmartStickyReviewer.Domain.Enums;
using SmartStickyReviewer.Domain.Interfaces;
using SmartStickyReviewer.Domain.ValueObjects;

namespace SmartStickyReviewer.Tests.Services;

public sealed class ReviewProviderSelectorTests
{
    private sealed class KindProvider : IReviewProvider
    {
        private readonly ReviewProviderKind _kind;
        private readonly string _name;

        public KindProvider(ReviewProviderKind kind, string name)
        {
            _kind = kind;
            _name = name;
        }

        public bool CanHandle(ReviewContext context) => context.DesiredProvider == _kind;

        public ReviewResult GetReview(ReviewContext context) =>
            ReviewResult.Success(new StarRating(5m), $"from {_name}", _name);
    }

    [Fact]
    public void Select_WhenProviderCanHandle_ReturnsFirstMatchingProvider()
    {
        // Arrange
        var manual = new KindProvider(ReviewProviderKind.Manual, "Manual");
        var judge = new KindProvider(ReviewProviderKind.JudgeMe, "JudgeMe");
        var selector = new ReviewProviderSelector(new IReviewProvider[] { judge, manual });

        var context = new ReviewContext(new SiteId("site-1"), ProductId: null, DesiredProvider: ReviewProviderKind.Manual, ManualReview: null);

        // Act
        var selected = selector.Select(context);

        // Assert
        selected.Should().BeSameAs(manual);
    }
}


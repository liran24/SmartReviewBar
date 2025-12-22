using SmartStickyReviewer.Domain.Entities;
using SmartStickyReviewer.Domain.Interfaces.Repositories;

namespace SmartStickyReviewer.Application.UseCases.Configuration;

/// <summary>
/// Request for saving a manual review
/// </summary>
public sealed class SaveManualReviewRequest
{
    public string SiteId { get; }
    public string ProductId { get; }
    public decimal Rating { get; }
    public int ReviewCount { get; }
    public string DisplayText { get; }

    public SaveManualReviewRequest(
        string siteId,
        string productId,
        decimal rating,
        int reviewCount,
        string displayText)
    {
        SiteId = siteId;
        ProductId = productId;
        Rating = rating;
        ReviewCount = reviewCount;
        DisplayText = displayText;
    }
}

/// <summary>
/// Response for saving a manual review
/// </summary>
public sealed class SaveManualReviewResponse
{
    public bool Success { get; }
    public bool IsNew { get; }
    public string? ErrorMessage { get; }

    private SaveManualReviewResponse(bool success, bool isNew, string? errorMessage)
    {
        Success = success;
        IsNew = isNew;
        ErrorMessage = errorMessage;
    }

    public static SaveManualReviewResponse Created() => new(true, true, null);
    public static SaveManualReviewResponse Updated() => new(true, false, null);
    public static SaveManualReviewResponse Failed(string errorMessage) => new(false, false, errorMessage);
}

/// <summary>
/// Use case for saving manual reviews
/// </summary>
public sealed class SaveManualReviewUseCase
{
    private readonly IManualReviewRepository _repository;

    public SaveManualReviewUseCase(IManualReviewRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<SaveManualReviewResponse> ExecuteAsync(
        SaveManualReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.SiteId))
            return SaveManualReviewResponse.Failed("Site ID is required");

        if (string.IsNullOrWhiteSpace(request.ProductId))
            return SaveManualReviewResponse.Failed("Product ID is required");

        if (request.Rating < 0 || request.Rating > 5)
            return SaveManualReviewResponse.Failed("Rating must be between 0 and 5");

        if (request.ReviewCount < 0)
            return SaveManualReviewResponse.Failed("Review count cannot be negative");

        var existing = await _repository.GetAsync(request.SiteId, request.ProductId, cancellationToken);

        if (existing == null)
        {
            var review = new ManualReview(
                request.SiteId,
                request.ProductId,
                request.Rating,
                request.ReviewCount,
                request.DisplayText);

            await _repository.CreateAsync(review, cancellationToken);

            return SaveManualReviewResponse.Created();
        }

        existing.Update(request.Rating, request.ReviewCount, request.DisplayText);
        await _repository.UpdateAsync(existing, cancellationToken);

        return SaveManualReviewResponse.Updated();
    }
}

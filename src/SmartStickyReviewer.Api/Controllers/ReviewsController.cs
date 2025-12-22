using Microsoft.AspNetCore.Mvc;
using SmartStickyReviewer.Api.DTOs;
using SmartStickyReviewer.Application.UseCases.Configuration;
using SmartStickyReviewer.Application.UseCases.Reviews;

namespace SmartStickyReviewer.Api.Controllers;

/// <summary>
/// Controller for review operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class ReviewsController : ControllerBase
{
    private readonly GetReviewUseCase _getReviewUseCase;
    private readonly SaveManualReviewUseCase _saveManualReviewUseCase;

    public ReviewsController(
        GetReviewUseCase getReviewUseCase,
        SaveManualReviewUseCase saveManualReviewUseCase)
    {
        _getReviewUseCase = getReviewUseCase;
        _saveManualReviewUseCase = saveManualReviewUseCase;
    }

    /// <summary>
    /// Get review for a product (used by site widget)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ReviewResponseDto>> GetReview(
        [FromQuery] string siteId,
        [FromQuery] string productId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(siteId) || string.IsNullOrWhiteSpace(productId))
        {
            return BadRequest(new ReviewResponseDto
            {
                Success = false,
                ErrorMessage = "SiteId and ProductId are required"
            });
        }

        var request = new GetReviewRequest(siteId, productId);
        var response = await _getReviewUseCase.ExecuteAsync(request, cancellationToken);

        var dto = new ReviewResponseDto
        {
            Success = response.Success,
            Rating = response.Rating,
            ReviewCount = response.ReviewCount,
            DisplayText = response.DisplayText,
            ProviderName = response.ProviderName,
            IsFallback = response.IsFallback,
            Style = new StyleConfigDto
            {
                BackgroundColor = response.Style.BackgroundColor,
                TextColor = response.Style.TextColor,
                StarColor = response.Style.StarColor,
                Position = response.Style.Position,
                FontSize = response.Style.FontSize,
                ShowReviewCount = response.Style.ShowReviewCount,
                ShowStars = response.Style.ShowStars
            },
            ErrorMessage = response.ErrorMessage,
            IsEnabled = response.IsEnabled
        };

        return Ok(dto);
    }

    /// <summary>
    /// Save a manual review for a product
    /// </summary>
    [HttpPost("manual")]
    public async Task<ActionResult<SaveManualReviewResponseDto>> SaveManualReview(
        [FromBody] SaveManualReviewRequestDto dto,
        CancellationToken cancellationToken)
    {
        var request = new SaveManualReviewRequest(
            dto.SiteId,
            dto.ProductId,
            dto.Rating,
            dto.ReviewCount,
            dto.DisplayText);

        var response = await _saveManualReviewUseCase.ExecuteAsync(request, cancellationToken);

        var responseDto = new SaveManualReviewResponseDto
        {
            Success = response.Success,
            IsNew = response.IsNew,
            ErrorMessage = response.ErrorMessage
        };

        if (!response.Success)
        {
            return BadRequest(responseDto);
        }

        return Ok(responseDto);
    }
}

namespace SmartStickyReviewer.Api.DTOs;

/// <summary>
/// DTO for review request
/// </summary>
public sealed class GetReviewRequestDto
{
    public string SiteId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
}

/// <summary>
/// DTO for review response
/// </summary>
public sealed class ReviewResponseDto
{
    public bool Success { get; set; }
    public decimal Rating { get; set; }
    public int ReviewCount { get; set; }
    public string DisplayText { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public bool IsFallback { get; set; }
    public StyleConfigDto Style { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public bool IsEnabled { get; set; }
}

/// <summary>
/// DTO for manual review request
/// </summary>
public sealed class SaveManualReviewRequestDto
{
    public string SiteId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public int ReviewCount { get; set; }
    public string DisplayText { get; set; } = string.Empty;
}

/// <summary>
/// DTO for manual review response
/// </summary>
public sealed class SaveManualReviewResponseDto
{
    public bool Success { get; set; }
    public bool IsNew { get; set; }
    public string? ErrorMessage { get; set; }
}

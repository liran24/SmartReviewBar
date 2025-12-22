using SmartStickyReviewer.Domain.Enums;

namespace SmartStickyReviewer.Api.DTOs;

/// <summary>
/// DTO for site configuration request
/// </summary>
public sealed class SaveConfigurationRequestDto
{
    public string SiteId { get; set; } = string.Empty;
    public int Plan { get; set; }
    public int PrimaryProvider { get; set; }
    public FallbackConfigDto FallbackConfig { get; set; } = new();
    public StyleConfigDto Style { get; set; } = new();
    public bool IsEnabled { get; set; } = true;
}

/// <summary>
/// DTO for fallback configuration
/// </summary>
public sealed class FallbackConfigDto
{
    public bool UseManualRatingFallback { get; set; }
    public decimal? ManualRating { get; set; }
    public int? ManualReviewCount { get; set; }
    public string? FallbackText { get; set; }
    public bool NotifyOnFailure { get; set; }
    public string? NotificationEmail { get; set; }
}

/// <summary>
/// DTO for style configuration
/// </summary>
public sealed class StyleConfigDto
{
    public string BackgroundColor { get; set; } = "#ffffff";
    public string TextColor { get; set; } = "#333333";
    public string StarColor { get; set; } = "#ffc107";
    public string Position { get; set; } = "bottom";
    public int FontSize { get; set; } = 14;
    public bool ShowReviewCount { get; set; } = true;
    public bool ShowStars { get; set; } = true;
}

/// <summary>
/// DTO for configuration response
/// </summary>
public sealed class ConfigurationResponseDto
{
    public bool Found { get; set; }
    public string? SiteId { get; set; }
    public int? Plan { get; set; }
    public string? PlanName { get; set; }
    public int? PrimaryProvider { get; set; }
    public string? PrimaryProviderName { get; set; }
    public FallbackConfigDto? FallbackConfig { get; set; }
    public StyleConfigDto? Style { get; set; }
    public bool? IsEnabled { get; set; }
    public List<FeatureDto>? Features { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for feature information
/// </summary>
public sealed class FeatureDto
{
    public string Name { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public string MinimumPlan { get; set; } = string.Empty;
}

/// <summary>
/// DTO for save configuration response
/// </summary>
public sealed class SaveConfigurationResponseDto
{
    public bool Success { get; set; }
    public string? SiteId { get; set; }
    public bool IsNew { get; set; }
    public List<string>? EnabledFeatures { get; set; }
    public string? ErrorMessage { get; set; }
}

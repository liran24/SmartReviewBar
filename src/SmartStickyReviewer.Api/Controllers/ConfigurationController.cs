using Microsoft.AspNetCore.Mvc;
using SmartStickyReviewer.Api.DTOs;
using SmartStickyReviewer.Application.UseCases.Configuration;
using SmartStickyReviewer.Domain.Enums;
using SmartStickyReviewer.Domain.Interfaces.Policies;
using SmartStickyReviewer.Domain.ValueObjects;

namespace SmartStickyReviewer.Api.Controllers;

/// <summary>
/// Controller for site configuration management
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class ConfigurationController : ControllerBase
{
    private readonly GetSiteConfigurationUseCase _getConfigUseCase;
    private readonly SaveSiteConfigurationUseCase _saveConfigUseCase;
    private readonly IFeaturePolicy _featurePolicy;

    public ConfigurationController(
        GetSiteConfigurationUseCase getConfigUseCase,
        SaveSiteConfigurationUseCase saveConfigUseCase,
        IFeaturePolicy featurePolicy)
    {
        _getConfigUseCase = getConfigUseCase;
        _saveConfigUseCase = saveConfigUseCase;
        _featurePolicy = featurePolicy;
    }

    /// <summary>
    /// Get site configuration
    /// </summary>
    [HttpGet("{siteId}")]
    public async Task<ActionResult<ConfigurationResponseDto>> GetConfiguration(
        string siteId,
        CancellationToken cancellationToken)
    {
        var request = new GetSiteConfigurationRequest(siteId);
        var response = await _getConfigUseCase.ExecuteAsync(request, cancellationToken);

        if (!response.Found)
        {
            return Ok(new ConfigurationResponseDto { Found = false });
        }

        var dto = new ConfigurationResponseDto
        {
            Found = true,
            SiteId = response.SiteId,
            Plan = (int?)response.Plan,
            PlanName = response.Plan?.ToString(),
            PrimaryProvider = (int?)response.PrimaryProvider,
            PrimaryProviderName = response.PrimaryProvider?.ToString(),
            FallbackConfig = response.FallbackConfig != null
                ? new FallbackConfigDto
                {
                    UseManualRatingFallback = response.FallbackConfig.UseManualRatingFallback,
                    ManualRating = response.FallbackConfig.ManualRating,
                    ManualReviewCount = response.FallbackConfig.ManualReviewCount,
                    FallbackText = response.FallbackConfig.FallbackText,
                    NotifyOnFailure = response.FallbackConfig.NotifyOnFailure,
                    NotificationEmail = response.FallbackConfig.NotificationEmail
                }
                : null,
            Style = response.Style != null
                ? new StyleConfigDto
                {
                    BackgroundColor = response.Style.BackgroundColor,
                    TextColor = response.Style.TextColor,
                    StarColor = response.Style.StarColor,
                    Position = response.Style.Position,
                    FontSize = response.Style.FontSize,
                    ShowReviewCount = response.Style.ShowReviewCount,
                    ShowStars = response.Style.ShowStars
                }
                : null,
            IsEnabled = response.IsEnabled,
            Features = GetAllFeatures(response.Plan ?? Plan.Free),
            CreatedAt = response.CreatedAt,
            UpdatedAt = response.UpdatedAt
        };

        return Ok(dto);
    }

    /// <summary>
    /// Save site configuration
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<SaveConfigurationResponseDto>> SaveConfiguration(
        [FromBody] SaveConfigurationRequestDto dto,
        CancellationToken cancellationToken)
    {
        var fallbackConfig = new FallbackConfiguration(
            dto.FallbackConfig.UseManualRatingFallback,
            dto.FallbackConfig.ManualRating,
            dto.FallbackConfig.ManualReviewCount,
            dto.FallbackConfig.FallbackText,
            dto.FallbackConfig.NotifyOnFailure,
            dto.FallbackConfig.NotificationEmail);

        var style = new StickyBarStyle(
            dto.Style.BackgroundColor,
            dto.Style.TextColor,
            dto.Style.StarColor,
            dto.Style.Position,
            dto.Style.FontSize,
            dto.Style.ShowReviewCount,
            dto.Style.ShowStars);

        var request = new SaveSiteConfigurationRequest(
            dto.SiteId,
            (Plan)dto.Plan,
            (ReviewProviderType)dto.PrimaryProvider,
            fallbackConfig,
            style,
            dto.IsEnabled);

        var response = await _saveConfigUseCase.ExecuteAsync(request, cancellationToken);

        var responseDto = new SaveConfigurationResponseDto
        {
            Success = response.Success,
            SiteId = response.SiteId,
            IsNew = response.IsNew,
            EnabledFeatures = response.EnabledFeatures?.Select(f => f.ToString()).ToList(),
            ErrorMessage = response.ErrorMessage
        };

        if (!response.Success)
        {
            return BadRequest(responseDto);
        }

        return Ok(responseDto);
    }

    /// <summary>
    /// Get available plans and features
    /// </summary>
    [HttpGet("plans")]
    public ActionResult<object> GetPlans()
    {
        var plans = Enum.GetValues<Plan>()
            .Select(plan => new
            {
                Id = (int)plan,
                Name = plan.ToString(),
                Features = GetAllFeatures(plan)
            })
            .ToList();

        return Ok(plans);
    }

    /// <summary>
    /// Get available review providers
    /// </summary>
    [HttpGet("providers")]
    public ActionResult<object> GetProviders()
    {
        var providers = Enum.GetValues<ReviewProviderType>()
            .Select(provider => new
            {
                Id = (int)provider,
                Name = provider.ToString()
            })
            .ToList();

        return Ok(providers);
    }

    private List<FeatureDto> GetAllFeatures(Plan currentPlan)
    {
        return Enum.GetValues<Feature>()
            .Select(feature => new FeatureDto
            {
                Name = feature.ToString(),
                IsEnabled = _featurePolicy.IsFeatureEnabled(feature, currentPlan),
                MinimumPlan = _featurePolicy.GetMinimumPlanForFeature(feature).ToString()
            })
            .ToList();
    }
}

using Microsoft.AspNetCore.Mvc;
using SmartStickyReviewer.Api.DTOs;
using SmartStickyReviewer.Application.Models;
using SmartStickyReviewer.Application.UseCases;

namespace SmartStickyReviewer.Api.Controllers;

[ApiController]
[Route("api/admin/sites/{siteId}/config")]
public sealed class AdminConfigController : ControllerBase
{
    private readonly GetAdminConfigurationUseCase _getUseCase;
    private readonly SaveAdminConfigurationUseCase _saveUseCase;

    public AdminConfigController(GetAdminConfigurationUseCase getUseCase, SaveAdminConfigurationUseCase saveUseCase)
    {
        _getUseCase = getUseCase ?? throw new ArgumentNullException(nameof(getUseCase));
        _saveUseCase = saveUseCase ?? throw new ArgumentNullException(nameof(saveUseCase));
    }

    [HttpGet]
    public async Task<ActionResult<AdminConfigResponseDto>> GetAsync([FromRoute] string siteId, CancellationToken ct)
    {
        var snapshot = await _getUseCase.ExecuteAsync(siteId, ct);
        var dto = new AdminConfigResponseDto(
            SiteId: snapshot.Configuration.SiteId.Value,
            Configuration: Map(snapshot.Configuration),
            FeatureAvailability: snapshot.FeatureAvailability
        );

        return Ok(dto);
    }

    [HttpPut]
    public async Task<ActionResult<SaveAdminConfigResponseDto>> SaveAsync(
        [FromRoute] string siteId,
        [FromBody] SaveAdminConfigRequestDto request,
        CancellationToken ct)
    {
        var command = new SaveAdminConfigurationCommand(
            SiteId: siteId,
            Plan: request.Plan,
            PrimaryProvider: request.PrimaryProvider,
            ManualRating: request.ManualRating,
            ManualText: request.ManualText,
            FallbackText: request.FallbackText,
            StoreOwnerEmail: request.StoreOwnerEmail,
            BackgroundColorHex: request.BackgroundColorHex,
            TextColorHex: request.TextColorHex,
            AccentColorHex: request.AccentColorHex
        );

        var result = await _saveUseCase.ExecuteAsync(command, ct);
        var dto = new SaveAdminConfigResponseDto(
            SiteId: result.Configuration.SiteId.Value,
            Configuration: Map(result.Configuration),
            Warnings: result.Warnings
        );

        return Ok(dto);
    }

    private static AdminConfigDto Map(Domain.Entities.SiteConfiguration config)
    {
        return new AdminConfigDto(
            Plan: config.Plan,
            PrimaryProvider: config.PrimaryProvider,
            ManualRating: config.ManualReview?.Rating.Value,
            ManualText: config.ManualReview?.Text,
            FallbackText: config.FallbackText,
            StoreOwnerEmail: config.StoreOwnerEmail,
            BackgroundColorHex: config.Style.BackgroundColorHex,
            TextColorHex: config.Style.TextColorHex,
            AccentColorHex: config.Style.AccentColorHex
        );
    }
}


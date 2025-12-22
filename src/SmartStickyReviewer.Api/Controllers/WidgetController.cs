using Microsoft.AspNetCore.Mvc;
using SmartStickyReviewer.Api.DTOs;
using SmartStickyReviewer.Application.UseCases;

namespace SmartStickyReviewer.Api.Controllers;

[ApiController]
[Route("api/widget/sites/{siteId}")]
public sealed class WidgetController : ControllerBase
{
    private readonly GetWidgetDataUseCase _useCase;

    public WidgetController(GetWidgetDataUseCase useCase)
    {
        _useCase = useCase ?? throw new ArgumentNullException(nameof(useCase));
    }

    [HttpGet]
    public async Task<ActionResult<WidgetResponseDto>> GetAsync(
        [FromRoute] string siteId,
        [FromQuery] string? productId,
        CancellationToken ct)
    {
        var data = await _useCase.ExecuteAsync(siteId, productId, ct);

        var dto = new WidgetResponseDto(
            ShouldRender: data.ShouldRender,
            Rating: data.Rating?.Value,
            Text: data.Text,
            ProviderName: data.ProviderName,
            BackgroundColorHex: data.BackgroundColorHex ?? "#111827",
            TextColorHex: data.TextColorHex ?? "#F9FAFB",
            AccentColorHex: data.AccentColorHex ?? "#F59E0B"
        );

        return Ok(dto);
    }
}


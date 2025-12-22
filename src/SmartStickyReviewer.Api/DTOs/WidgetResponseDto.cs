namespace SmartStickyReviewer.Api.DTOs;

public sealed record WidgetResponseDto(
    bool ShouldRender,
    decimal? Rating,
    string? Text,
    string ProviderName,
    string BackgroundColorHex,
    string TextColorHex,
    string AccentColorHex
);


using SmartStickyReviewer.Domain.ValueObjects;

namespace SmartStickyReviewer.Application.Models;

public sealed record WidgetData(
    bool ShouldRender,
    StarRating? Rating,
    string? Text,
    string ProviderName,
    string? BackgroundColorHex,
    string? TextColorHex,
    string? AccentColorHex
);


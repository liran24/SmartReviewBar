using SmartStickyReviewer.Domain.Enums;

namespace SmartStickyReviewer.Api.DTOs;

public sealed record AdminConfigDto(
    Plan Plan,
    ReviewProviderKind PrimaryProvider,
    decimal? ManualRating,
    string? ManualText,
    string? FallbackText,
    string? StoreOwnerEmail,
    string BackgroundColorHex,
    string TextColorHex,
    string AccentColorHex
);


using SmartStickyReviewer.Domain.Enums;

namespace SmartStickyReviewer.Application.Models;

public sealed record SaveAdminConfigurationCommand(
    string SiteId,
    Plan Plan,
    ReviewProviderKind PrimaryProvider,
    decimal? ManualRating,
    string? ManualText,
    string? FallbackText,
    string? StoreOwnerEmail,
    string? BackgroundColorHex,
    string? TextColorHex,
    string? AccentColorHex
);


namespace SmartStickyReviewer.Api.DTOs;

public sealed record SaveAdminConfigResponseDto(
    string SiteId,
    AdminConfigDto Configuration,
    IReadOnlyList<string> Warnings
);


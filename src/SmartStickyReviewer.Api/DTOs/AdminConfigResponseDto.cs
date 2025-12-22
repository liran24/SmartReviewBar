using SmartStickyReviewer.Domain.Enums;

namespace SmartStickyReviewer.Api.DTOs;

public sealed record AdminConfigResponseDto(
    string SiteId,
    AdminConfigDto Configuration,
    IReadOnlyDictionary<Feature, bool> FeatureAvailability
);


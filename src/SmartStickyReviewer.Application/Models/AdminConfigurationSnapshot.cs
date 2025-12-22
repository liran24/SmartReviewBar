using SmartStickyReviewer.Domain.Entities;
using SmartStickyReviewer.Domain.Enums;

namespace SmartStickyReviewer.Application.Models;

public sealed record AdminConfigurationSnapshot(
    SiteConfiguration Configuration,
    IReadOnlyDictionary<Feature, bool> FeatureAvailability
);


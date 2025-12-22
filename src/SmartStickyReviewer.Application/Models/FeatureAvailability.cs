using SmartStickyReviewer.Domain.Enums;

namespace SmartStickyReviewer.Application.Models;

public sealed record FeatureAvailability(
    Feature Feature,
    bool IsEnabled
);


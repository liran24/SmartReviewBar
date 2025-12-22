using SmartStickyReviewer.Domain.Enums;

namespace SmartStickyReviewer.Domain.ValueObjects;

public sealed record ReviewContext(
    SiteId SiteId,
    string? ProductId,
    ReviewProviderKind DesiredProvider,
    ManualReview? ManualReview
);


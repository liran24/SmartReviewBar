using SmartStickyReviewer.Domain.Entities;

namespace SmartStickyReviewer.Application.Models;

public sealed record SaveAdminConfigurationResult(
    SiteConfiguration Configuration,
    IReadOnlyList<string> Warnings
);


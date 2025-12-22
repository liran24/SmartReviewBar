using SmartStickyReviewer.Application.Models;
using SmartStickyReviewer.Domain.Entities;
using SmartStickyReviewer.Domain.Enums;
using SmartStickyReviewer.Domain.Interfaces;
using SmartStickyReviewer.Domain.ValueObjects;

namespace SmartStickyReviewer.Application.UseCases;

public sealed class SaveAdminConfigurationUseCase
{
    private readonly ISiteConfigurationRepository _repository;
    private readonly IFeaturePolicy _featurePolicy;

    public SaveAdminConfigurationUseCase(ISiteConfigurationRepository repository, IFeaturePolicy featurePolicy)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _featurePolicy = featurePolicy ?? throw new ArgumentNullException(nameof(featurePolicy));
    }

    public async Task<SaveAdminConfigurationResult> ExecuteAsync(SaveAdminConfigurationCommand command, CancellationToken ct)
    {
        if (command is null) throw new ArgumentNullException(nameof(command));

        var warnings = new List<string>();
        var siteId = new SiteId(command.SiteId);

        var config = await _repository.GetAsync(siteId, ct) ?? SiteConfiguration.CreateDefault(siteId);
        config.UpdatePlan(command.Plan);
        config.UpdateStoreOwnerEmail(command.StoreOwnerEmail);

        var requestedPrimary = command.PrimaryProvider;

        var manualReview = command.ManualRating.HasValue
            ? new ManualReview(new StarRating(command.ManualRating.Value), string.IsNullOrWhiteSpace(command.ManualText) ? null : command.ManualText.Trim())
            : null;
        config.UpdateManualReview(manualReview);

        config.UpdatePrimaryProvider(requestedPrimary);
        config.UpdateFallbackText(string.IsNullOrWhiteSpace(command.FallbackText) ? null : command.FallbackText.Trim());

        var requestedStyle = new StickyStyle(
            string.IsNullOrWhiteSpace(command.BackgroundColorHex) ? StickyStyle.Default.BackgroundColorHex : command.BackgroundColorHex.Trim(),
            string.IsNullOrWhiteSpace(command.TextColorHex) ? StickyStyle.Default.TextColorHex : command.TextColorHex.Trim(),
            string.IsNullOrWhiteSpace(command.AccentColorHex) ? StickyStyle.Default.AccentColorHex : command.AccentColorHex.Trim()
        );
        config.UpdateStyle(requestedStyle);

        // Enforce feature-gated settings (no "if(plan == X)" anywhere; only dynamic feature checks).
        if (!await _featurePolicy.IsEnabledAsync(siteId, Feature.MultipleReviewProviders, ct))
        {
            if (config.PrimaryProvider != ReviewProviderKind.Manual)
            {
                warnings.Add("Multiple providers are not enabled for this site; primary provider was forced to Manual.");
            }
            config.UpdatePrimaryProvider(ReviewProviderKind.Manual);
        }

        if (!await _featurePolicy.IsEnabledAsync(siteId, Feature.ManualFallbackText, ct))
        {
            if (!string.IsNullOrWhiteSpace(config.FallbackText))
            {
                warnings.Add("Manual fallback text is not enabled for this site; fallback text was cleared.");
            }
            config.UpdateFallbackText(null);
        }

        if (!await _featurePolicy.IsEnabledAsync(siteId, Feature.AdvancedStyling, ct))
        {
            if (config.Style != StickyStyle.Default)
            {
                warnings.Add("Advanced styling is not enabled for this site; styling was reset to default.");
            }
            config.UpdateStyle(StickyStyle.Default);
        }

        await _repository.UpsertAsync(config, ct);
        return new SaveAdminConfigurationResult(config, warnings);
    }
}


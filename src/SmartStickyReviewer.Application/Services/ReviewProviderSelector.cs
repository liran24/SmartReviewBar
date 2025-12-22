using SmartStickyReviewer.Domain.Interfaces.Providers;
using SmartStickyReviewer.Domain.ValueObjects;

namespace SmartStickyReviewer.Application.Services;

/// <summary>
/// Implementation of provider selection logic
/// </summary>
public sealed class ReviewProviderSelector : IReviewProviderSelector
{
    private readonly IEnumerable<IReviewProvider> _providers;

    public ReviewProviderSelector(IEnumerable<IReviewProvider> providers)
    {
        _providers = providers ?? throw new ArgumentNullException(nameof(providers));
    }

    public IReviewProvider? SelectProvider(ReviewContext context)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        // First, try to find the preferred provider
        var preferredProvider = _providers
            .FirstOrDefault(p => p.ProviderType == context.PreferredProvider && p.CanHandle(context));

        if (preferredProvider != null)
            return preferredProvider;

        // If preferred provider is not available, find any that can handle
        return _providers.FirstOrDefault(p => p.CanHandle(context));
    }

    public IEnumerable<IReviewProvider> GetAllProviders()
    {
        return _providers;
    }
}

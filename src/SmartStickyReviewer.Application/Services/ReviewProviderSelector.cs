using SmartStickyReviewer.Domain.Interfaces;
using SmartStickyReviewer.Domain.ValueObjects;

namespace SmartStickyReviewer.Application.Services;

public sealed class ReviewProviderSelector
{
    private readonly IReadOnlyList<IReviewProvider> _providers;

    public ReviewProviderSelector(IEnumerable<IReviewProvider> providers)
    {
        _providers = (providers ?? throw new ArgumentNullException(nameof(providers))).ToList();
    }

    public IReviewProvider? Select(ReviewContext context)
    {
        if (context is null) throw new ArgumentNullException(nameof(context));
        return _providers.FirstOrDefault(p => p.CanHandle(context));
    }
}


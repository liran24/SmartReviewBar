namespace SmartStickyReviewer.Domain.ValueObjects;

public readonly record struct SiteId
{
    public string Value { get; }

    public SiteId(string value)
    {
        value = (value ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("SiteId cannot be empty.", nameof(value));
        }

        Value = value;
    }

    public override string ToString() => Value;
}


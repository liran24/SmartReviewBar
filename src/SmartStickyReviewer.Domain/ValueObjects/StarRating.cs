namespace SmartStickyReviewer.Domain.ValueObjects;

public readonly record struct StarRating
{
    public decimal Value { get; }

    public StarRating(decimal value)
    {
        if (value < 0m || value > 5m)
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, "Star rating must be between 0.0 and 5.0.");
        }

        Value = decimal.Round(value, 2, MidpointRounding.AwayFromZero);
    }

    public override string ToString() => Value.ToString("0.##");
}


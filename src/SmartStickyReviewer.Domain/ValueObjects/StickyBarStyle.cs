namespace SmartStickyReviewer.Domain.ValueObjects;

/// <summary>
/// Styling configuration for the sticky bar widget
/// </summary>
public sealed class StickyBarStyle
{
    public string BackgroundColor { get; }
    public string TextColor { get; }
    public string StarColor { get; }
    public string Position { get; }
    public int FontSize { get; }
    public bool ShowReviewCount { get; }
    public bool ShowStars { get; }

    public StickyBarStyle(
        string backgroundColor = "#ffffff",
        string textColor = "#333333",
        string starColor = "#ffc107",
        string position = "bottom",
        int fontSize = 14,
        bool showReviewCount = true,
        bool showStars = true)
    {
        BackgroundColor = backgroundColor;
        TextColor = textColor;
        StarColor = starColor;
        Position = position;
        FontSize = fontSize;
        ShowReviewCount = showReviewCount;
        ShowStars = showStars;
    }

    public static StickyBarStyle Default => new();
}

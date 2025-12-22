namespace SmartStickyReviewer.Domain.ValueObjects;

public sealed record StickyStyle(
    string BackgroundColorHex,
    string TextColorHex,
    string AccentColorHex
)
{
    public static StickyStyle Default => new("#111827", "#F9FAFB", "#F59E0B");
}


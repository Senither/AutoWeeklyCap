using AutoWeeklyCap.UI.Helpers;

namespace AutoWeeklyCap.Enums;

public enum ColorTheme
{
    // Warm & Energetic Primaries
    Red,
    Orange,
    Amber,
    Yellow,

    // Fresh & Natural Primaries
    Lime,
    Green,
    Emerald,
    Teal,
    Cyan,

    // Cool & Professional Primaries
    Sky,
    Blue,
    Indigo,
    Violet,
    Purple,
    Fuchsia,
    Pink,
    Rose,
}

public static class ColorThemeExtensions
{
    extension(ColorTheme theme)
    {
        public string GetName()
        {
            return theme.ToString();
        }

        public Vector4 GetPrimaryColor()
        {
            return theme switch
            {
                ColorTheme.Red => ColorUtils.HexToVector("#ef4444"),
                ColorTheme.Orange => ColorUtils.HexToVector("#f97316"),
                ColorTheme.Amber => ColorUtils.HexToVector("#f59e0b"),
                ColorTheme.Yellow => ColorUtils.HexToVector("#eab308"),
                ColorTheme.Lime => ColorUtils.HexToVector("#84cc16"),
                ColorTheme.Green => ColorUtils.HexToVector("#22c55e"),
                ColorTheme.Emerald => ColorUtils.HexToVector("#10b981"),
                ColorTheme.Teal => ColorUtils.HexToVector("#14b8a6"),
                ColorTheme.Cyan => ColorUtils.HexToVector("#06b6d4"),
                ColorTheme.Sky => ColorUtils.HexToVector("#0ea5e9"),
                ColorTheme.Blue => ColorUtils.HexToVector("#3b82f6"),
                ColorTheme.Indigo => ColorUtils.HexToVector("#6366f1"),
                ColorTheme.Violet => ColorUtils.HexToVector("#8b5cf6"),
                ColorTheme.Purple => ColorUtils.HexToVector("#a855f7"),
                ColorTheme.Fuchsia => ColorUtils.HexToVector("#d946ef"),
                ColorTheme.Pink => ColorUtils.HexToVector("#ec4899"),
                ColorTheme.Rose => ColorUtils.HexToVector("#f43f5e"),
                _ => throw new ArgumentOutOfRangeException(nameof(theme), theme, null)
            };
        }

        public ColorTheme GetPreviousTheme()
        {
            var values = Enum.GetValues(typeof(ColorTheme)).Cast<ColorTheme>().ToArray();
            var currentIndex = Array.IndexOf(values, theme);
            var previousIndex = (currentIndex - 1 + values.Length) % values.Length;

            return values[previousIndex];
        }

        public ColorTheme GetNextTheme()
        {
            var values = Enum.GetValues(typeof(ColorTheme)).Cast<ColorTheme>().ToArray();
            var currentIndex = Array.IndexOf(values, theme);
            var nextIndex = (currentIndex + 1) % values.Length;

            return values[nextIndex];
        }
    }
}

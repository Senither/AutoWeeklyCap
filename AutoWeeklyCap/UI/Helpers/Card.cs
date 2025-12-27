using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;

namespace AutoWeeklyCap.UI.Helpers;

public static class Card
{
    internal const float Rounding = 1f;
    internal const float BorderSize = 1f;

    internal static readonly Vector2 TitlePadding = new(10, 6);
    internal static readonly Vector2 ContentPadding = new(10, 10);

    internal static readonly uint ColorBg = ColorUtils.HexToUInt(0xAA, 0xAA, 0xAA, 0.2f);
    internal static readonly uint BorderBg = ColorUtils.HexToUInt(0x5A, 0x5A, 0x59);

    public static void Draw(string title, Action bodyContent)
    {
        using var id = ImRaii.PushId(title);
        using var color = ImRaii.PushColor(ImGuiCol.ChildBg, new Vector4(0.05f, 0.05f, 0.05f, 0.2f));

        var drawList = ImGui.GetWindowDrawList();
        drawList.ChannelsSplit(2);
        drawList.ChannelsSetCurrent(1);

        ImGui.BeginGroup();

        var cardMin = ImGui.GetCursorScreenPos();
        var width = ImGui.GetContentRegionAvail().X;

        var titleSize = ImGui.CalcTextSize(title);
        var titleBarHeight = titleSize.Y + (TitlePadding.Y * 2);

        ImGui.Dummy(new Vector2(width, titleBarHeight));

        ImGui.SetCursorScreenPos(cardMin + TitlePadding);
        ImGui.TextUnformatted(title);

        ImGui.SetCursorScreenPos(cardMin + ContentPadding with { Y = titleBarHeight + ContentPadding.Y });
        ImGui.BeginGroup();

        bodyContent();

        ImGui.EndGroup();
        ImGui.Dummy(ContentPadding with { X = 0 });

        ImGui.EndGroup();

        var cardMax = ImGui.GetItemRectMax();
        var cardBgU32 = ImGui.ColorConvertFloat4ToU32(ImGui.GetStyle().Colors[(int)ImGuiCol.ChildBg]);

        drawList.ChannelsSetCurrent(0);

        drawList.AddRectFilled(cardMin, cardMax, cardBgU32, Rounding, ImDrawFlags.RoundCornersBottom);
        drawList.AddRectFilled(
            cardMin, cardMax with { Y = cardMin.Y + titleBarHeight },
            ColorBg, Rounding,
            ImDrawFlags.RoundCornersNone
        );

        drawList.AddRect(
            cardMin, cardMax,
            BorderBg, Rounding,
            ImDrawFlags.RoundCornersBottom,
            BorderSize
        );

        drawList.ChannelsSetCurrent(1);
        drawList.ChannelsMerge();

        ImGui.Spacing();
    }
}

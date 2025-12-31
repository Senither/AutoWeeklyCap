using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;

namespace AutoWeeklyCap.UI.Helpers;

public static class Card
{
    internal const float Rounding = 1f;
    internal const float BorderSize = 1f;

    internal static readonly Vector2 TitlePadding = new(10, 6);
    internal static readonly Vector2 ContentPadding = new(10, 10);

    private static readonly Dictionary<uint, bool> OpenStateById = new();

    public static void Draw(string title, Action bodyContent, bool collapsible = true, bool defaultOpen = false)
    {
        DrawWithColors(
            title,
            bodyContent,
            ColorUtils.HexToUInt(0xAA, 0xAA, 0xAA, 0.2f),
            ColorUtils.HexToUInt(0x5A, 0x5A, 0x59),
            collapsible,
            defaultOpen
        );
    }

    public static void DrawWarning(string title, Action bodyContent, bool collapsible = true, bool defaultOpen = false)
    {
        DrawWithColors(
            title,
            bodyContent,
            ColorUtils.HexToUInt(0xFF, 0xC6, 0x3C, 0.3f),
            ColorUtils.HexToUInt(0xAB, 0x8E, 0x1B, 0.8f),
            collapsible,
            defaultOpen
        );
    }

    public static void DrawDanger(string title, Action bodyContent, bool collapsible = true, bool defaultOpen = false)
    {
        DrawWithColors(
            title,
            bodyContent,
            ColorUtils.HexToUInt(0xFF, 0x3C, 0x3C, 0.3f),
            ColorUtils.HexToUInt(0xAB, 0x1B, 0x1B, 0.8f),
            collapsible,
            defaultOpen
        );
    }

    public static void DrawWithColors(
        string title,
        Action bodyContent,
        uint backgroundColor,
        uint borderColor,
        bool collapsible = true,
        bool defaultOpen = false)
    {
        using var id = ImRaii.PushId(title);
        using var color = ImRaii.PushColor(ImGuiCol.ChildBg, new Vector4(0.05f, 0.05f, 0.05f, 0.2f));

        var stateId = ImGui.GetID("###card-open-state");
        var isOpen = true;
        if (collapsible)
        {
            if (!OpenStateById.TryGetValue(stateId, out isOpen))
            {
                isOpen = defaultOpen;
                OpenStateById[stateId] = defaultOpen;
            }
        }

        var drawList = ImGui.GetWindowDrawList();
        drawList.ChannelsSplit(2);
        drawList.ChannelsSetCurrent(1);

        ImGui.BeginGroup();

        var cardMin = ImGui.GetCursorScreenPos();
        var width = ImGui.GetContentRegionAvail().X;

        var titleLineHeight = ImGui.GetTextLineHeight();
        var titleBarHeight = titleLineHeight + (TitlePadding.Y * 2);

        ImGui.Dummy(new Vector2(width, titleBarHeight));

        if (collapsible)
        {
            ImGui.SetCursorScreenPos(cardMin);
            if (ImGui.InvisibleButton("###toggle-card-open", new Vector2(width, titleBarHeight)))
            {
                isOpen = !isOpen;
                OpenStateById[stateId] = isOpen;
            }
        }

        ImGui.SetCursorScreenPos(cardMin + new Vector2((titleBarHeight - titleLineHeight) / 2f));
        
        if (collapsible)
        {
            var icon = isOpen ? FontAwesomeIcon.ChevronDown : FontAwesomeIcon.ChevronRight;
            using (ImRaii.PushFont(UiBuilder.IconFont))
            {
                ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudGrey2);
                ImGui.TextUnformatted(icon.ToIconString());
                ImGui.PopStyleColor();
            }

            ImGui.SameLine(0f, 6f);
        }

        ImGui.TextUnformatted(title);

        if (!collapsible || isOpen)
        {
            ImGui.SetCursorScreenPos(cardMin + ContentPadding with { Y = titleBarHeight + ContentPadding.Y });
            ImGui.BeginGroup();

            bodyContent();

            ImGui.EndGroup();
            ImGui.Dummy(ContentPadding with { X = 0 });
        }

        ImGui.EndGroup();

        var cardMax = ImGui.GetItemRectMax();
        var cardBgU32 = ImGui.ColorConvertFloat4ToU32(ImGui.GetStyle().Colors[(int)ImGuiCol.ChildBg]);

        drawList.ChannelsSetCurrent(0);

        drawList.AddRectFilled(cardMin, cardMax, cardBgU32, Rounding, ImDrawFlags.RoundCornersBottom);
        drawList.AddRectFilled(
            cardMin, cardMax with { Y = cardMin.Y + titleBarHeight },
            backgroundColor, Rounding,
            ImDrawFlags.RoundCornersNone
        );

        drawList.AddRect(
            cardMin, cardMax,
            borderColor, Rounding,
            ImDrawFlags.RoundCornersBottom,
            BorderSize
        );

        drawList.ChannelsSetCurrent(1);
        drawList.ChannelsMerge();

        ImGui.Spacing();
    }
}

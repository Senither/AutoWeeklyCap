using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;

namespace AutoWeeklyCap.UI.Helpers;

public static class Card
{
    internal const float Rounding = 1f;
    internal const float BorderSize = 1f;

    internal static readonly Vector2 TitlePadding = new(10, 6);
    internal static readonly Vector2 ContentPadding = new(10, 10);

    internal static readonly Vector2 SubtleTitlePadding = new(8, 4);
    internal static readonly Vector2 SubtleContentPadding = new(8, 8);

    internal static readonly Vector4 DefaultChildBg = new(0.05f, 0.05f, 0.05f, 0.2f);
    internal static readonly Vector4 SubtleChildBg = new(0.05f, 0.05f, 0.05f, 0.12f);

    private readonly record struct CardContext(uint BorderColor, Vector2 ContentPadding, float ParentPaddingX, bool ForceOpenDescendants);

    private readonly record struct CardBackgroundDraw(
        Vector2 Min,
        Vector2 Max,
        float TitleBarHeight,
        uint CardBgColor,
        uint TitleBarColor,
        uint BorderColor
    );

    private static readonly Stack<CardContext> ContextStack = new();
    private static int ChannelSplitDepth = 0;
    private static List<CardBackgroundDraw>? PendingBackgrounds = null;

    private static readonly Dictionary<uint, bool> OpenStateById = new();

    public static void Draw(string title, Action bodyContent, bool collapsible = true, bool defaultOpen = false, string? id = null)
    {
        DrawWithColors(
            title,
            bodyContent,
            ColorUtils.HexToUInt(0xAA, 0xAA, 0xAA, 0.2f),
            ColorUtils.HexToUInt(0x5A, 0x5A, 0x59),
            collapsible,
            defaultOpen,
            id
        );
    }

    public static void DrawSubtle(string title, Action bodyContent, bool collapsible = true, bool defaultOpen = false, string? id = null)
    {
        DrawCore(
            title,
            bodyContent,
            ColorUtils.HexToUInt(0xAA, 0xAA, 0xAA, 0.12f),
            ColorUtils.HexToUInt(0x5A, 0x5A, 0x59, 0.45f),
            SubtleChildBg,
            SubtleTitlePadding,
            SubtleContentPadding,
            collapsible,
            defaultOpen,
            id
        );
    }

    public static void DrawWarning(string title, Action bodyContent, bool collapsible = true, bool defaultOpen = false, string? id = null)
    {
        DrawWithColors(
            title,
            bodyContent,
            ColorUtils.HexToUInt(0xFF, 0xC6, 0x3C, 0.3f),
            ColorUtils.HexToUInt(0xAB, 0x8E, 0x1B, 0.8f),
            collapsible,
            defaultOpen,
            id
        );
    }

    public static void DrawDanger(string title, Action bodyContent, bool collapsible = true, bool defaultOpen = false, string? id = null)
    {
        DrawWithColors(
            title,
            bodyContent,
            ColorUtils.HexToUInt(0xFF, 0x3C, 0x3C, 0.3f),
            ColorUtils.HexToUInt(0xAB, 0x1B, 0x1B, 0.8f),
            collapsible,
            defaultOpen,
            id
        );
    }

    public static void DrawWithColors(
        string title,
        Action bodyContent,
        uint backgroundColor,
        uint borderColor,
        bool collapsible = true,
        bool defaultOpen = false,
        string? id = null
    )
    {
        DrawCore(
            title,
            bodyContent,
            backgroundColor,
            borderColor,
            DefaultChildBg,
            TitlePadding,
            ContentPadding,
            collapsible,
            defaultOpen,
            id
        );
    }

    private static void DrawCore(
        string title,
        Action bodyContent,
        uint backgroundColor,
        uint borderColor,
        Vector4 childBg,
        Vector2 titlePadding,
        Vector2 contentPadding,
        bool collapsible,
        bool defaultOpen,
        string? idOverride
    )
    {
        var (visibleTitle, idFromTitle) = SplitVisibleAndId(title);
        var stableId = !string.IsNullOrWhiteSpace(idOverride) ? idOverride : idFromTitle;
        if (string.IsNullOrWhiteSpace(stableId))
            stableId = title;

        var parentPaddingX = ContextStack.Count > 0 ? ContextStack.Peek().ContentPadding.X : 0f;
        var parentForceOpenDescendants = ContextStack.Count > 0 && ContextStack.Peek().ForceOpenDescendants;
        ContextStack.Push(new CardContext(borderColor, contentPadding, parentPaddingX, parentForceOpenDescendants));

        var drawList = ImGui.GetWindowDrawList();
        var ownsChannelSplit = ChannelSplitDepth == 0;
        if (ownsChannelSplit) {
            drawList.ChannelsSplit(2);
            PendingBackgrounds = [];
        }

        ChannelSplitDepth++;

        var bgIndex = -1;
        var bgRecorded = false;

        try {
            using var id = ImRaii.PushId(stableId);
            using var color = ImRaii.PushColor(ImGuiCol.ChildBg, childBg);

            var stateId = ImGui.GetID("###card-open-state");
            var isOpen = true;
            if (collapsible) {
                if (!OpenStateById.TryGetValue(stateId, out isOpen)) {
                    isOpen = defaultOpen;
                    OpenStateById[stateId] = defaultOpen;
                }

                if (parentForceOpenDescendants) {
                    isOpen = true;
                    OpenStateById[stateId] = true;
                }
            }

            drawList.ChannelsSetCurrent(1);

            ImGui.BeginGroup();

            var cardMin = ImGui.GetCursorScreenPos();
            var width = Math.Max(0f, ImGui.GetContentRegionAvail().X - parentPaddingX);

            var titleLineHeight = ImGui.GetTextLineHeight();
            var titleBarHeight = titleLineHeight + (titlePadding.Y * 2);

            bgIndex = PendingBackgrounds?.Count ?? -1;
            PendingBackgrounds?.Add(default);

            ImGui.Dummy(new Vector2(width, titleBarHeight));

            var forceOpenDescendants = parentForceOpenDescendants;

            if (collapsible) {
                ImGui.SetCursorScreenPos(cardMin);
                if (ImGui.InvisibleButton("###toggle-card-open", new Vector2(width, titleBarHeight))) {
                    isOpen = !isOpen;
                    OpenStateById[stateId] = isOpen;

                    if (ImGui.GetIO().KeyCtrl && isOpen)
                        forceOpenDescendants = true;
                }
            }

            if (forceOpenDescendants != parentForceOpenDescendants) {
                var current = ContextStack.Pop();
                ContextStack.Push(current with { ForceOpenDescendants = forceOpenDescendants });
            }

            ImGui.SetCursorScreenPos(cardMin + new Vector2(titlePadding.X, (titleBarHeight - titleLineHeight) / 2f));

            if (collapsible) {
                var icon = isOpen ? FontAwesomeIcon.ChevronDown : FontAwesomeIcon.ChevronRight;
                using (ImRaii.PushFont(UiBuilder.IconFont)) {
                    ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudGrey2);
                    ImGui.TextUnformatted(icon.ToIconString());
                    ImGui.PopStyleColor();
                }

                ImGui.SameLine(0f, 6f);
            }

            ImGui.TextUnformatted(visibleTitle);

            if (!collapsible || isOpen) {
                ImGui.SetCursorScreenPos(cardMin + contentPadding with { Y = titleBarHeight + contentPadding.Y });
                ImGui.BeginGroup();

                bodyContent();

                ImGui.EndGroup();
                ImGui.Dummy(contentPadding with { X = 0f });
            }

            ImGui.EndGroup();

            var cardMax = ImGui.GetItemRectMax();
            var cardBgU32 = ImGui.ColorConvertFloat4ToU32(ImGui.GetStyle().Colors[(int)ImGuiCol.ChildBg]);

            if (PendingBackgrounds != null && bgIndex >= 0) {
                PendingBackgrounds[bgIndex] = new CardBackgroundDraw(
                    cardMin,
                    cardMax,
                    titleBarHeight,
                    cardBgU32,
                    backgroundColor,
                    borderColor
                );
                bgRecorded = true;
            }

            ImGui.Spacing();
        }
        finally {
            if (!bgRecorded && PendingBackgrounds != null && bgIndex >= 0 && bgIndex < PendingBackgrounds.Count) {
                PendingBackgrounds.RemoveAt(bgIndex);
            }

            ChannelSplitDepth = Math.Max(0, ChannelSplitDepth - 1);

            if (ownsChannelSplit) {
                FlushPendingBackgrounds(drawList);
                drawList.ChannelsSetCurrent(1);
                drawList.ChannelsMerge();
                PendingBackgrounds = null;
            }

            ContextStack.Pop();
        }
    }

    private static void FlushPendingBackgrounds(ImDrawListPtr drawList)
    {
        if (PendingBackgrounds == null || PendingBackgrounds.Count == 0)
            return;

        drawList.ChannelsSetCurrent(0);

        foreach (var bg in PendingBackgrounds) {
            drawList.AddRectFilled(bg.Min, bg.Max, bg.CardBgColor, Rounding, ImDrawFlags.RoundCornersBottom);
            drawList.AddRectFilled(
                bg.Min,
                bg.Max with { Y = bg.Min.Y + bg.TitleBarHeight },
                bg.TitleBarColor,
                Rounding,
                ImDrawFlags.RoundCornersNone
            );

            drawList.AddRect(bg.Min, bg.Max, bg.BorderColor, Rounding, ImDrawFlags.RoundCornersBottom, BorderSize);
        }
    }

    private static (string Visible, string Id) SplitVisibleAndId(string title)
    {
        var idx = title.IndexOf("###", StringComparison.Ordinal);
        if (idx < 0)
            return (title, title);

        var visible = title[..idx];
        var id = title[(idx + 3)..];
        if (string.IsNullOrWhiteSpace(id))
            id = title;

        return (visible, id);
    }

    public static void Separator() => Separator(null);

    internal static bool TryGetContentRightBoundX(out float rightBoundX)
    {
        rightBoundX = 0f;

        if (ContextStack.Count == 0)
            return false;

        var context = ContextStack.Peek();
        rightBoundX = ImGui.GetCursorScreenPos().X + Math.Max(0f, ImGui.GetContentRegionAvail().X - context.ParentPaddingX - context.ContentPadding.X);

        return true;
    }

    private static void Separator(uint? borderColor)
    {
        var drawList = ImGui.GetWindowDrawList();

        if (ContextStack.Count == 0)
            throw new NullReferenceException("expected Card#Separator to be called inside a card body");

        var context = ContextStack.Peek();
        var color = borderColor ?? context.BorderColor;

        var cursor = ImGui.GetCursorScreenPos();
        var width = Math.Max(0f, ImGui.GetContentRegionAvail().X - context.ParentPaddingX);

        var paddingY = ImGui.GetStyle().ItemSpacing.Y * 1.5f;
        var lineY = cursor.Y + paddingY;

        drawList.AddLine(
            new Vector2(cursor.X - context.ContentPadding.X, lineY),
            new Vector2(cursor.X + width, lineY),
            color, BorderSize
        );

        ImGui.Dummy(new Vector2(0f, paddingY * 2f));
    }
}

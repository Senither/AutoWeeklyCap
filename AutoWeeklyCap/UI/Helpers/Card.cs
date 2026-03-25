using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;

namespace AutoWeeklyCap.UI.Helpers;

public static class Card
{
    private const float Rounding = 8f;
    private const float BorderSize = 1f;

    private static readonly Vector2 TitlePadding = new(10, 6);
    private static readonly Vector2 ContentPadding = new(10, 10);

    private static readonly Vector2 SubtleTitlePadding = new(8, 4);
    private static readonly Vector2 SubtleContentPadding = new(8, 8);

    private static readonly Vector4 DefaultChildBg = Theme.BackgroundDefault with { W = 0.35f };
    private static readonly Vector4 SubtleChildBg = Theme.BackgroundDark with { W = 0.35f };

    private static int _channelSplitDepth = 0;
    private static readonly Stack<CardContext> ContextStack = new();
    private static List<CardBackgroundDraw>? _pendingBackgrounds = null;
    private static readonly Dictionary<uint, bool> OpenStateById = new();

    // ReSharper disable once MemberHidesStaticFromOuterClass
    private readonly record struct CardContext(Vector4 BorderColor, Vector2 ContentPadding, float ParentPaddingX, bool ForceOpenDescendants);

    private readonly record struct CardBackgroundDraw(
        Vector2 Min,
        Vector2 Max,
        float TitleBarHeight,
        Vector4 CardBgColor,
        Vector4 TitleBarColor,
        Vector4 BorderColor,
        bool DrawTitleDivider
    );

    internal static void Draw(string title, Action bodyContent, bool collapsible = true, bool defaultOpen = false, string? id = null)
    {
        DrawWithColors(
            title,
            bodyContent,
            Theme.BackgroundDefault,
            Theme.BorderDefault,
            collapsible,
            defaultOpen,
            id
        );
    }

    internal static void DrawSubtle(string title, Action bodyContent, bool collapsible = true, bool defaultOpen = false, string? id = null)
    {
        DrawCore(
            title,
            bodyContent,
            Theme.BackgroundDark,
            Theme.BorderDark,
            SubtleChildBg,
            SubtleTitlePadding,
            SubtleContentPadding,
            collapsible,
            defaultOpen,
            id
        );
    }

    internal static void DrawWarning(string title, Action bodyContent, bool collapsible = true, bool defaultOpen = false, string? id = null)
    {
        DrawWithColors(
            title,
            bodyContent,
            Theme.BackgroundWarning,
            Theme.BorderWarning,
            collapsible,
            defaultOpen,
            id
        );
    }

    internal static void DrawDanger(string title, Action bodyContent, bool collapsible = true, bool defaultOpen = false, string? id = null)
    {
        DrawWithColors(
            title,
            bodyContent,
            Theme.BackgroundDanger,
            Theme.BorderDanger,
            collapsible,
            defaultOpen,
            id
        );
    }

    internal static void DrawWithColors(
        string title,
        Action bodyContent,
        Vector4 titleBackgroundColor,
        Vector4 borderColor,
        bool collapsible = true,
        bool defaultOpen = false,
        string? id = null
    )
    {
        DrawCore(
            title,
            bodyContent,
            titleBackgroundColor,
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
        Vector4 titleBackgroundColor,
        Vector4 borderColor,
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
        if (string.IsNullOrWhiteSpace(stableId)) {
            stableId = title;
        }

        var parentPaddingX = ContextStack.Count > 0 ? ContextStack.Peek().ContentPadding.X : 0f;
        var parentForceOpenDescendants = ContextStack.Count > 0 && ContextStack.Peek().ForceOpenDescendants;
        ContextStack.Push(new CardContext(borderColor, contentPadding, parentPaddingX, parentForceOpenDescendants));

        var drawList = ImGui.GetWindowDrawList();
        var ownsChannelSplit = _channelSplitDepth == 0;
        if (ownsChannelSplit) {
            drawList.ChannelsSplit(2);
            _pendingBackgrounds = [];
        }

        _channelSplitDepth++;

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

            bgIndex = _pendingBackgrounds?.Count ?? -1;
            _pendingBackgrounds?.Add(default);

            ImGui.Dummy(new Vector2(width, titleBarHeight));

            var forceOpenDescendants = parentForceOpenDescendants;

            if (collapsible) {
                ImGui.SetCursorScreenPos(cardMin);
                if (ImGui.InvisibleButton("###toggle-card-open", new Vector2(width, titleBarHeight))) {
                    isOpen = !isOpen;
                    OpenStateById[stateId] = isOpen;

                    if (ImGui.GetIO().KeyCtrl && isOpen) {
                        forceOpenDescendants = true;
                    }
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
                    ImGui.PushStyleColor(ImGuiCol.Text, Theme.TextMuted);
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
            var cardBgColor = ImGui.GetStyle().Colors[(int)ImGuiCol.ChildBg];

            if (_pendingBackgrounds != null && bgIndex >= 0) {
                _pendingBackgrounds[bgIndex] = new CardBackgroundDraw(
                    cardMin,
                    cardMax,
                    titleBarHeight,
                    cardBgColor,
                    titleBackgroundColor,
                    borderColor,
                    !collapsible || isOpen
                );
                bgRecorded = true;
            }

            ImGui.Spacing();
        } finally {
            if (!bgRecorded && _pendingBackgrounds != null && bgIndex >= 0 && bgIndex < _pendingBackgrounds.Count) {
                _pendingBackgrounds.RemoveAt(bgIndex);
            }

            _channelSplitDepth = Math.Max(0, _channelSplitDepth - 1);

            if (ownsChannelSplit) {
                FlushPendingBackgrounds(drawList);
                drawList.ChannelsSetCurrent(1);
                drawList.ChannelsMerge();
                _pendingBackgrounds = null;
            }

            ContextStack.Pop();
        }
    }

    private static void FlushPendingBackgrounds(ImDrawListPtr drawList)
    {
        if (_pendingBackgrounds == null || _pendingBackgrounds.Count == 0) {
            return;
        }

        drawList.ChannelsSetCurrent(0);

        foreach (var bg in _pendingBackgrounds) {
            var titleBarRounding = bg.DrawTitleDivider ? ImDrawFlags.RoundCornersTop : ImDrawFlags.RoundCornersAll;

            drawList.AddRectFilled(bg.Min, bg.Max, ImGui.ColorConvertFloat4ToU32(bg.CardBgColor), Rounding, ImDrawFlags.RoundCornersAll);
            drawList.AddRectFilled(
                bg.Min,
                bg.Max with { Y = bg.Min.Y + bg.TitleBarHeight },
                ImGui.ColorConvertFloat4ToU32(bg.TitleBarColor),
                Rounding,
                titleBarRounding
            );

            if (bg.DrawTitleDivider) {
                var titleDividerY = bg.Min.Y + bg.TitleBarHeight;
                drawList.AddLine(
                    new Vector2(bg.Min.X, titleDividerY),
                    new Vector2(bg.Max.X, titleDividerY),
                    ImGui.ColorConvertFloat4ToU32(bg.BorderColor),
                    BorderSize
                );
            }

            drawList.AddRect(bg.Min, bg.Max, ImGui.ColorConvertFloat4ToU32(bg.BorderColor), Rounding, ImDrawFlags.RoundCornersAll, BorderSize);
        }
    }

    private static (string Visible, string Id) SplitVisibleAndId(string title)
    {
        var idx = title.IndexOf("###", StringComparison.Ordinal);
        if (idx < 0) {
            return (title, title);
        }

        var visible = title[..idx];
        var id = title[(idx + 3)..];
        if (string.IsNullOrWhiteSpace(id)) {
            id = title;
        }

        return (visible, id);
    }

    public static void Separator()
    {
        Separator(null);
    }

    internal static bool TryGetContentRightBoundX(out float rightBoundX)
    {
        rightBoundX = 0f;

        if (ContextStack.Count == 0) {
            return false;
        }

        var context = ContextStack.Peek();
        rightBoundX = ImGui.GetCursorScreenPos().X + Math.Max(0f, ImGui.GetContentRegionAvail().X - context.ParentPaddingX - context.ContentPadding.X);

        return true;
    }

    private static void Separator(Vector4? borderColor)
    {
        var drawList = ImGui.GetWindowDrawList();

        if (ContextStack.Count == 0) {
            throw new NullReferenceException("expected Card#Separator to be called inside a card body");
        }

        var context = ContextStack.Peek();
        var color = borderColor ?? context.BorderColor;

        var cursor = ImGui.GetCursorScreenPos();
        var width = Math.Max(0f, ImGui.GetContentRegionAvail().X - context.ParentPaddingX);

        var paddingY = ImGui.GetStyle().ItemSpacing.Y * 1.5f;
        var lineY = cursor.Y + paddingY;

        drawList.AddLine(
            new Vector2(cursor.X - context.ContentPadding.X, lineY),
            new Vector2(cursor.X + width, lineY),
            ImGui.ColorConvertFloat4ToU32(color), BorderSize
        );

        ImGui.Dummy(new Vector2(0f, paddingY * 2f));
    }
}

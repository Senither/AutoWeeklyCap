using AutoWeeklyCap.Config;
using AutoWeeklyCap.UI.Helpers;

using Dalamud.Interface;

using System.Globalization;

using Dalamud.Interface.Utility.Raii;

namespace AutoWeeklyCap.UI.ControlPanelWindow;

public static class StatisticsUi
{
    private const float MinCardWidth = 160f;
    private const float CardHeight = 78f;

    internal static void Draw()
    {
        List<(string Character, CharacterOptions Options, CharacterMetrics Metrics)> characterEntries = GetCharacterEntries();

        DrawCombinedHighlights(characterEntries);

        if (characterEntries.Count == 0) {
            Card.DrawSubtle("Character Statistics", () =>
            {
                ImGui.TextWrapped("No character metrics are available yet. Start running duties to begin collecting statistics.");
            }, collapsible: false, defaultOpen: true, id: "statistics-character-empty");

            return;
        }

        ImGui.Spacing();

        DrawCharacterCards(characterEntries);
    }

    private static List<(string Character, CharacterOptions Options, CharacterMetrics Metrics)> GetCharacterEntries()
    {
        var entries = new List<(string Character, CharacterOptions Options, CharacterMetrics Metrics)>();

        foreach (var character in AWC.Config.GetSortedCharacters()) {
            if (!AWC.Config.Characters.TryGetValue(character, out var options)) {
                continue;
            }

            entries.Add((character, options, options.Metrics));
        }

        return entries;
    }

    private static void DrawCombinedHighlights(List<(string Character, CharacterOptions Options, CharacterMetrics Metrics)> characterEntries)
    {
        AggregateMetrics totals = Aggregate(characterEntries);

        HighlightCard[] cards =
        [
            new("Runs Completed", FormatNumber(totals.RunsCompleted), FontAwesomeIcon.FlagCheckered),
            new("Time In Runs", FormatDuration(totals.TimeSpentInRuns), FontAwesomeIcon.Clock),
            new("Tomes Collected", FormatNumber(totals.TomestonesCollected), FontAwesomeIcon.Coins),
            new("Tomes Spent", FormatNumber(totals.TomestonesSpent), FontAwesomeIcon.Trophy),
            new("Repairs", FormatNumber(totals.RepairsCompleted), FontAwesomeIcon.Hammer),
            new("Materia Extracted", FormatNumber(totals.MateriaExtracted), FontAwesomeIcon.Gem),
            new("Deliverables Handed In", FormatNumber(totals.DeliverableItemsHandedIn), FontAwesomeIcon.MailBulk),
            new("Retains Collected", FormatNumber(totals.RetainersCollected), FontAwesomeIcon.Bell)
        ];

        var availableWidth = Math.Max(0f, ImGui.GetContentRegionAvail().X);
        var spacing = ImGui.GetStyle().ItemSpacing.X;

        var cardsPerRow = Math.Clamp((int)((availableWidth + spacing) / (MinCardWidth + spacing)), 1, cards.Length) / 2 * 2;
        if (cardsPerRow == 6) {
            cardsPerRow = 4;
        }

        var cardWidth = Math.Max(MinCardWidth, (availableWidth - ((cardsPerRow - 1) * spacing)) / cardsPerRow);

        for (var i = 0; i < cards.Length; i++) {
            DrawHighlightCard(
                card: cards[i],
                index: i,
                size: new Vector2(cardWidth, CardHeight)
            );

            var hasAnotherInRow = (i + 1) % cardsPerRow != 0 && i + 1 < cards.Length;
            if (hasAnotherInRow) {
                ImGui.SameLine();
            } else {
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + spacing);
            }
        }
    }

    private static void DrawHighlightCard(HighlightCard card, int index, Vector2 size)
    {
        var background = Theme.BackgroundDefault with { W = 0.8f };
        using var bgColor = ImRaii.PushColor(ImGuiCol.ChildBg, background);
        using var borderColor = ImRaii.PushColor(ImGuiCol.Border, Theme.BorderDefault);

        ImGui.BeginChild($"stats-highlight-{index}", size, true);

        using (ImRaii.PushFont(UiBuilder.IconFont)) {
            ImGui.PushStyleColor(ImGuiCol.Text, Theme.TextPrimary);
            ImGui.TextUnformatted(card.Icon.ToIconString());
            ImGui.PopStyleColor();
        }

        ImGui.SameLine(0f, 8f);
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 4);
        ImGui.AlignTextToFramePadding();
        ImGui.PushStyleColor(ImGuiCol.Text, Theme.TextMuted);
        ImGui.TextUnformatted(card.Title);
        ImGui.PopStyleColor();

        ImGui.Spacing();

        using (Theme.BigFont?.Push()) {
            ImGui.PushStyleColor(ImGuiCol.Text, Theme.TextDefault);
            ImGui.TextUnformatted(card.Value);
            ImGui.PopStyleColor();
        }

        ImGui.EndChild();
    }

    private static void DrawCharacterCards(List<(string Character, CharacterOptions Options, CharacterMetrics Metrics)> characterEntries)
    {
        foreach (var (characterName, _, metrics) in characterEntries) {
            Card.Draw(
                title: $"{characterName}",
                bodyContent: () => DrawCharacterMetrics(metrics),
                defaultOpen: false,
                id: $"character-metrics-{characterName}"
            );
        }
    }

    private static void DrawCharacterMetrics(CharacterMetrics metrics)
    {
        DrawMetricLine("Runs completed", FormatNumber(metrics.RunsCompleted));
        DrawMetricLine("Time spent in runs", FormatDuration(metrics.TimeSpentInRuns));
        DrawMetricLine("Uncapped tomestones collected", FormatNumber(metrics.UncappedAcquiredTomestoneCollected));
        DrawMetricLine("Weekly limited tomestones collected", FormatNumber(metrics.WeeklyAcquiredLimitedTomestoneCollected));
        DrawMetricLine("Weekly limited tomestones spent", FormatNumber(metrics.WeeklyAcquiredLimitedTomestoneSpent));

        Card.Separator();

        DrawMetricLine("Repairs completed", FormatNumber(metrics.RepairsCompleted));
        DrawMetricLine("Gil spent on repairs", FormatGil(metrics.GilSpentOnRepairs));
        DrawMetricLine("Dark matter spent on repairs", FormatNumber(metrics.DarkMatterSpentOnRepairs));

        Card.Separator();

        DrawMetricLine("Materia extracted", FormatNumber(metrics.MateriaExtracted));
        DrawMetricLine("Retainers collected", FormatNumber(metrics.RetainersCollected));
        DrawMetricLine("Deliverable items handed in", FormatNumber(metrics.DeliverableItemsHandedIn));
        DrawMetricLine("Gil spent on teleportation", FormatGil(metrics.GilSpentOnTeleportationFees));
    }

    private static void DrawMetricLine(string label, string value)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, Theme.TextMuted);
        ImGui.TextUnformatted($"{label}:");
        ImGui.PopStyleColor();

        ImGui.SameLine();
        ImGui.TextUnformatted(value);
    }

    private static AggregateMetrics Aggregate(List<(string Character, CharacterOptions Options, CharacterMetrics Metrics)> characterEntries)
    {
        var totalRuns = 0u;
        var totalTimeSpentInRuns = 0ul;
        var totalTomestonesCollected = 0u;
        var totalTomestonesSpent = 0u;
        var totalRepairsCompleted = 0u;
        var totalMateriaExtracted = 0u;
        var totalDeliverableItemsHandedIn = 0u;
        var totalRetainersCollected = 0u;

        foreach (var metrics in characterEntries.Select(entry => entry.Metrics)) {
            totalRuns += metrics.RunsCompleted;
            totalTimeSpentInRuns += metrics.TimeSpentInRuns;
            totalTomestonesCollected += metrics.UncappedAcquiredTomestoneCollected;
            totalTomestonesCollected += metrics.WeeklyAcquiredLimitedTomestoneCollected;
            totalTomestonesSpent += metrics.WeeklyAcquiredLimitedTomestoneSpent;
            totalRepairsCompleted += metrics.RepairsCompleted;
            totalMateriaExtracted += metrics.MateriaExtracted;
            totalDeliverableItemsHandedIn += metrics.DeliverableItemsHandedIn;
            totalRetainersCollected += metrics.RetainersCollected;
        }

        return new AggregateMetrics(
            RunsCompleted: totalRuns,
            TimeSpentInRuns: totalTimeSpentInRuns,
            TomestonesCollected: totalTomestonesCollected,
            TomestonesSpent: totalTomestonesSpent,
            RepairsCompleted: totalRepairsCompleted,
            MateriaExtracted: totalMateriaExtracted,
            DeliverableItemsHandedIn: totalDeliverableItemsHandedIn,
            RetainersCollected: totalRetainersCollected
        );
    }

    private static string FormatDuration(ulong totalSeconds)
    {
        TimeSpan time = TimeSpan.FromSeconds(totalSeconds);

        return time switch
        {
            { TotalDays: >= 1 } => $"{(int)time.TotalDays}d {time.Hours}h {time.Minutes}m",
            _ => $"{time.Hours}h {time.Minutes}m {time.Seconds}s"
        };
    }

    private static string FormatGil(uint value)
    {
        return $"{FormatNumber(value)} gil";
    }

    private static string FormatNumber(uint value)
    {
        return value.ToString("N0", CultureInfo.InvariantCulture);
    }

    private readonly record struct HighlightCard(string Title, string Value, FontAwesomeIcon Icon);

    private readonly record struct AggregateMetrics(
        uint RunsCompleted,
        ulong TimeSpentInRuns,
        uint TomestonesCollected,
        uint TomestonesSpent,
        uint RepairsCompleted,
        uint MateriaExtracted,
        uint DeliverableItemsHandedIn,
        uint RetainersCollected
    );
}

using AutoWeeklyCap.Config;
using AutoWeeklyCap.UI.Helpers;

using Dalamud.Interface;

namespace AutoWeeklyCap.UI.MainWindow;

internal static class CharactersTabUi
{
    private const int TomesPerRun = 50;
    private const int DefaultRunSeconds = 24 * 60;

    internal static void Draw()
    {
        var charactersEnabled = 0;
        var totalTomesCollected = 0;
        var weeklyTomeLimit = CurrencyHelper.GetLimitedTomestoneWeeklyLimit();
        var totalEtaSeconds = 0.0;

        foreach (var character in AWC.Config.GetSortedCharacters()) {
            var option = AWC.Config.Characters[character];
            if (option.IsHidden()) {
                continue;
            }

            var characterTomes = AWC.Config.GetWeeklyTomes(character);

            if (option.IsEnabled()) {
                totalTomesCollected += characterTomes;

                var remainingTomes = Math.Max(0, weeklyTomeLimit - characterTomes);
                var runsNeeded = (int)Math.Ceiling(remainingTomes / (double)TomesPerRun);
                var averageSeconds = DefaultRunSeconds;

                if (option.LastDutyDurationsSeconds.Count > 0)
                    // Adding 30 seconds to the timer to account for waiting time outside
                    // the instance, AutoRetainer, repairs, extracting, etc
                {
                    averageSeconds = (int)option.LastDutyDurationsSeconds.Average() + 30;
                }

                totalEtaSeconds += runsNeeded * averageSeconds;
            }

            if (option.IsEnabled()) {
                charactersEnabled++;
            }

            ImGui.PushID(character);

            CharacterElements.DrawCharacterStatusIcon(character, option);
            CharacterElements.DrawCharacterRelogIcon(character, true);
            CharacterElements.DrawCharacterSettingsIcon(character, true);

            DrawCharacterDetails(character, option, characterTomes, weeklyTomeLimit);

            ImGui.PopID();
        }

        ImGuiEx.LineCentered(
            "TomestoneCap",
            () => ImGuiEx.Text($"Weekly tomestone cap is at {totalTomesCollected}/{weeklyTomeLimit * charactersEnabled}")
        );

        var time = TimeSpan.FromSeconds(totalEtaSeconds);
        if (AWC.Runner.CurrentDutyStartUtc != null) {
            time -= DateTime.UtcNow - AWC.Runner.CurrentDutyStartUtc.Value;
        }

        var etaText = time switch
        {
            { TotalDays: >= 1 } => $"{(int)time.TotalDays}d {time.Hours}h {time.Minutes}m",
            _ => $"{time.Hours}h {time.Minutes}m {time.Seconds}s"
        };

        ImGuiEx.LineCentered(
            "TomestoneEta",
            time.TotalSeconds > 0D
                ? () => ImGuiEx.Text($"Estimated time to cap {etaText}")
                : () => ImGui.TextColored(
                    ColorUtils.HexToUInt(0xFF, 0xFF, 0xFF, 0.45f),
                    "All your characters are tome capped"
                )
        );
    }

    private static void DrawCharacterDetails(string character, CharacterOptions options, int tomes, int weeklyLimit)
    {
        ImGui.SameLine(0f, 4f);

        var cursorPos = ImGui.GetCursorPos();
        ImGui.ProgressBar(0, new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetFrameHeight()), "");
        ImGui.SameLine();

        cursorPos.X += 8;
        ImGui.SetCursorPos(cursorPos);

        var characterText = character;
        if (options.PreferredJob != PlayerJob.None) {
            characterText += $"  ({options.PreferredJob.GetName()})";
        }

        ImGui.TextWrapped(characterText);

        if (options.HasOverrideSettingsEnabled()) {
            ImGui.SameLine();
            ImGuiEx.IconWithText(Theme.TextPrimary, FontAwesomeIcon.Flask, "");
        }

        ImGui.SameLine(ImGui.GetContentRegionAvail().X - 64 + ImGui.GetStyle().ItemSpacing.X);

        ImGui.TextUnformatted($"{tomes}/{weeklyLimit}");
    }
}

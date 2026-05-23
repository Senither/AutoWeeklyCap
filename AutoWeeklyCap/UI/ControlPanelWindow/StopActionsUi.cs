using AutoWeeklyCap.Config;
using AutoWeeklyCap.UI.Helpers;

using Dalamud.Interface;

using ECommons.Configuration;

namespace AutoWeeklyCap.UI.ControlPanelWindow;

public static class StopActionsUi
{
    public static void Draw()
    {
        Card.Draw("Stop Actions", () =>
        {
            ImGui.TextWrapped("Select what should happen when all characters have been tomestone capped.");

            Card.Separator();

            ImGui.Spacing();
            ImGui.Spacing();

            foreach (StopAction action in StopActionExtensions.GetOrderedStopActions()) {
                if (ImGui.RadioButton(action.GetName(), AWC.Config.StopAction == action)) {
                    AWC.Config.StopAction = action;
                }

                var tooltip = action.GetTooltip();
                if (tooltip != null) {
                    InformationTooltip.Draw(tooltip);
                }
            }
        }, collapsible: false);

        List<Action> elements = GetOptionsDrawElements();
        if (elements.Count > 0) {
            Card.Draw(AWC.Config.StopAction.GetName() + " Options", () =>
            {
                foreach (var element in elements) {
                    element.Invoke();
                }
            }, collapsible: false);
        } else {
            ImGui.Spacing();
            ImGui.Spacing();

            Disabled.Draw(() => ImGui.TextWrapped(
                "Selecting certain options will show additional options here, allowing you to further customize the behaviour of the actions."
            ));
        }
    }

    private static List<Action> GetOptionsDrawElements()
    {
        return AWC.Config.StopAction switch
        {
            StopAction.SwitchCharacter => [DrawCharacterSwitch],
            StopAction.StartUnlimitedRuns => [DrawCharacterSwitch],
            StopAction.LevelJobs => [DrawLevelJobElements],
            _ => []
        };
    }

    private static void DrawCharacterSwitch()
    {
        ImGui.TextWrapped("Preferred Character");

        var preview = AWC.Config.Characters.ContainsKey(AWC.Config.CharacterForSwap)
            ? AWC.Config.CharacterForSwap
            : "Not selected";

        if (!ImGui.BeginCombo($"###character-selector", preview)) {
            return;
        }

        foreach (var character in AWC.Config.GetSortedCharacters()) {
            if (ImGui.Selectable(character, AWC.Config.CharacterForSwap == character)) {
                AWC.Config.CharacterForSwap = character;
            }
        }

        ImGui.EndCombo();
    }

    private static void DrawLevelJobElements()
    {
        ImGui.TextWrapped(
            "Configure which jobs should be leveled when all characters are tome capped, only " +
            $"jobs that are between level 15 and {Constants.CurrentMaxLevel - 1} will be displayed."
        );

        ImGui.Spacing();

        ImGui.TextWrapped("How it works");
        InformationTooltip.Draw(() =>
        {
            ImGui.Text($"The runner will switch to your character and job that have priority to be leveled, it will");
            ImGui.Text($"then queue for the highest level duty that's available with that jobs current item level.");
            ImGui.Text($"It will continue to do this until manually stopped, or the job reaches level {Constants.CurrentMaxLevel} and");
            ImGui.Text($"it switches to another character or job. If ");
            StatusText.Draw(SkipCutsceneIPC.IsEnabled, "SkipCutscene");
            ImGui.Text(" is enabled, the MSQ duties will");
            ImGui.Text("be run between level 50 - 55 to help speed up leveling.");
        });

        Card.Separator();

        var useStylistForGearUpgrades = AWC.Config.LevelJobs.UseStylistForGearUpgrades;
        if (ImGui.Checkbox("Use Stylist for gear upgrades", ref useStylistForGearUpgrades)) {
            AWC.Config.LevelJobs.UseStylistForGearUpgrades = useStylistForGearUpgrades;
            EzConfig.Save();
        }

        InformationTooltip.Draw(() =>
        {
            ImGui.Text("When both the option and ");
            StatusText.Draw(StylistIPC.IsEnabled, "Stylist");
            ImGui.Text(" is enabled, it will be used");
            ImGui.Text("to automatically equip gear upgrades found in the armory");
            ImGui.Text("and inventory, both before and after a dungeon run.");
            ImGui.Text("");
            ImGui.Text("If this options is not used, it's recommended to enable");
            ImGui.Text("the gear upgrade option within ");
            StatusText.Draw(AutoDutyIPC.IsEnabled, "AutoDuty");
            ImGui.Text(" instead.");
        });

        ImGui.Spacing();
        var useCharacterOrder = AWC.Config.LevelJobs.UseCharacterOrder;
        if (ImGui.RadioButton("All characters", useCharacterOrder)) {
            AWC.Config.LevelJobs.UseCharacterOrder = true;
            EzConfig.Save();
        }

        InformationTooltip.Draw(() =>
        {
            ImGui.Text("This will go through each individual character in the same order");
            ImGui.Text("as the runner, each character can have different orders for");
            ImGui.Text("jobs that should be leveled.");
        });

        if (ImGui.RadioButton("Selected character", !useCharacterOrder)) {
            AWC.Config.LevelJobs.UseCharacterOrder = false;
            EzConfig.Save();
        }

        InformationTooltip.Draw(() =>
        {
            ImGui.Text("This will siwtch to your selected character and level all the enabled jobs,");
            ImGui.Text("if all the selected jobs are max level the runner will stop itself instead.");
        });

        var sortedCharacters = AWC.Config.GetSortedCharacters();
        if (sortedCharacters.Count == 0) {
            ImGui.Spacing();
            Disabled.Draw(() => ImGui.TextWrapped("No characters are registered yet."));
            return;
        }

        if (!AWC.Config.LevelJobs.UseCharacterOrder) {
            DrawLevelJobsSingleCharacterSelector(sortedCharacters);
        }

        var charactersToRender = AWC.Config.LevelJobs.UseCharacterOrder
            ? sortedCharacters
            : [AWC.Config.LevelJobs.SelectedCharacter];

        Card.Separator();

        foreach (var character in charactersToRender) {
            if (!AWC.Config.Characters.ContainsKey(character)) {
                continue;
            }

            var options = AWC.Config.GetOrRegisterCharacterOptions(character);
            if (options == null) {
                continue;
            }

            var eligibleJobLevels = options.JobLevels
                .Where(entry => entry.Key != PlayerJob.None && LevelingHelper.IsWithingLevelingThreshold(entry.Value))
                .ToDictionary(entry => entry.Key, entry => entry.Value);

            var entries = GetOrCreateCharacterJobEntries(character, eligibleJobLevels);

            Card.Draw(
                character,
                () => DrawCharacterLevelingJobs(character, eligibleJobLevels, entries),
                defaultOpen: true,
                id: $"level-jobs-card-{character}"
            );
        }
    }

    private static void DrawLevelJobsSingleCharacterSelector(List<string> sortedCharacters)
    {
        ImGui.Spacing();
        ImGui.TextWrapped("Character to level");

        if (!sortedCharacters.Contains(AWC.Config.LevelJobs.SelectedCharacter)) {
            AWC.Config.LevelJobs.SelectedCharacter = sortedCharacters[0];
            EzConfig.Save();
        }

        var preview = AWC.Config.LevelJobs.SelectedCharacter;
        if (!ImGui.BeginCombo("###level-jobs-character-selector", preview)) {
            return;
        }

        foreach (var character in sortedCharacters) {
            if (!ImGui.Selectable(character, AWC.Config.LevelJobs.SelectedCharacter == character)) {
                continue;
            }

            AWC.Config.LevelJobs.SelectedCharacter = character;
            EzConfig.Save();
        }

        ImGui.EndCombo();
    }

    private static void DrawCharacterLevelingJobs(string character, Dictionary<PlayerJob, int> eligibleJobLevels, List<LevelJobEntry> entries)
    {
        if (entries.Count == 0) {
            Disabled.Draw(() => ImGui.TextWrapped($"No eligible jobs found (only jobs between level 15 and {Constants.CurrentMaxLevel - 1} are shown)."));
            return;
        }

        for (var index = 0; index < entries.Count; index++) {
            var entry = entries[index];
            if (!eligibleJobLevels.TryGetValue(entry.Job, out var level)) {
                continue;
            }

            ImGui.PushID($"{character}-job-{entry.Job}");

            var enabled = entry.Enabled;
            if (ImGui.Checkbox("##enabled", ref enabled)) {
                entry.Enabled = enabled;
                EzConfig.Save();
            }

            ImGui.SameLine(0f, 4f);
            Disabled.Draw(index == 0, () =>
            {
                if (ImGuiEx.IconButton(FontAwesomeIcon.ArrowUp)) {
                    MoveCharacterJob(character, index, -1);
                }
            });

            ImGui.SameLine(0f, 4f);
            Disabled.Draw(index == entries.Count - 1, () =>
            {
                if (ImGuiEx.IconButton(FontAwesomeIcon.ArrowDown)) {
                    MoveCharacterJob(character, index, 1);
                }
            });

            ImGui.SameLine(0f, 6f);

            var cursorPos = ImGui.GetCursorPos();

            ImGui.Text($"{entry.Job.GetName()}");
            ImGui.SetCursorPos(cursorPos with { X = cursorPos.X + 32, Y = cursorPos.Y + 3 });
            ImGui.Text($"(Lvl {level})");

            ImGui.PopID();
        }
    }

    private static List<LevelJobEntry> GetOrCreateCharacterJobEntries(string character, Dictionary<PlayerJob, int> eligibleJobLevels)
    {
        if (!AWC.Config.LevelJobs.CharacterJobs.TryGetValue(character, out var entries)) {
            entries = [];
            AWC.Config.LevelJobs.CharacterJobs[character] = entries;
        }

        var changed = false;

        var distinctEntries = entries
            .GroupBy(entry => entry.Job)
            .Select(group => group.First())
            .Where(entry => entry.Job != PlayerJob.None)
            .ToList();

        if (distinctEntries.Count != entries.Count) {
            entries = distinctEntries;
            AWC.Config.LevelJobs.CharacterJobs[character] = entries;
            changed = true;
        }

        var eligibleJobs = eligibleJobLevels.Keys.ToHashSet();
        var filteredEntries = entries
            .Where(entry => eligibleJobs.Contains(entry.Job))
            .ToList();

        if (filteredEntries.Count != entries.Count) {
            entries = filteredEntries;
            AWC.Config.LevelJobs.CharacterJobs[character] = entries;
            changed = true;
        }

        var existingJobs = entries
            .Select(entry => entry.Job)
            .ToHashSet();

        foreach (var job in eligibleJobs.OrderBy(job => job.GetName())) {
            if (existingJobs.Contains(job)) {
                continue;
            }

            entries.Add(new LevelJobEntry { Job = job, Enabled = true });
            changed = true;
        }

        if (changed) {
            EzConfig.Save();
        }

        return entries;
    }

    private static void MoveCharacterJob(string character, int index, int direction)
    {
        if (!AWC.Config.LevelJobs.CharacterJobs.TryGetValue(character, out var entries)) {
            return;
        }

        var targetIndex = index + direction;
        if (targetIndex < 0 || targetIndex >= entries.Count) {
            return;
        }

        (entries[index], entries[targetIndex]) = (entries[targetIndex], entries[index]);
        EzConfig.Save();
    }
}

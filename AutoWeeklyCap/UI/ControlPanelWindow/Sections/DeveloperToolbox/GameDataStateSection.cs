using System.Globalization;

using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;

namespace AutoWeeklyCap.UI.ControlPanelWindow.Sections.DeveloperToolbox;

internal static class GameDataStateSection
{
    internal static void Draw()
    {
        CopyableText(
            $"Player:     {PlayerHelper.GetFullCharacterName() ?? "<unknown>"} [id: {Player.CID}]",
            "player ID",
            () => $"{Player.CID}"
        );

        CopyableText(
            $"Position:  T:{Player.Territory.RowId} P:{Player.Position}",
            "position",
            () => $"{Player.Position.X.ToString(CultureInfo.InvariantCulture)}f, {Player.Position.Y.ToString(CultureInfo.InvariantCulture)}f, {Player.Position.Z.ToString(CultureInfo.InvariantCulture)}f"
        );

        string taget = "<no target>";
        uint targetId = 0;

        try {
            unsafe {
                GameObject* t = TargetSystem.Instance()->Target;
                float distance = Vector3.Distance(Player.Position, t->Position);

                targetId = t->BaseId;
                taget = $"{t->GetName()} [id: {t->BaseId}, disc: {distance}]";
            }
        } catch (Exception) {
            // ignored
        }

        CopyableText($"Target:     {taget}", "target ID", () => $"{targetId}u");

        ImGui.Text("State:       ");
        StateText(() => PlayerHelper.IsReady, "Ready");
        StateText(() => PlayerHelper.IsValid, "Valid");
        StateText(() => PlayerHelper.IsOccupied, "Occupied");
        StateText(() => PlayerHelper.IsJumping, "Jumping");
        StateText(() => PlayerHelper.IsMoving, "Moving");
        StateText(() => PlayerHelper.IsCasting, "Casting", false);
    }

    private static void CopyableText(string text, string propertyName, Func<string> copy)
    {
        ImGui.Text(text);

        if (ImGui.IsItemClicked()) {
            ImGui.SetClipboardText(copy());
        }

        if (ImGui.IsItemHovered()) {
            ImGuiEx.Tooltip($"Click to copy {propertyName} to clipboard");
        }
    }

    private static void StateText(Func<bool> value, string text, bool seperator = true)
    {
        ImGui.SameLine(0, 0);

        try {
            ImGui.TextColored(value() ? Theme.TextSuccess : Theme.TextDanger, text);
        } catch (Exception) {
            ImGui.TextColored(Theme.TextWarning, text);
        }

        if (!seperator) {
            return;
        }

        ImGui.SameLine(0, 0);
        ImGui.Text(" | ");
    }
}

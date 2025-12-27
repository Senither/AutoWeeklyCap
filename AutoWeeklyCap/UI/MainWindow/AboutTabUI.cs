using Dalamud.Bindings.ImGui;
using Lumina.Excel.Sheets;

namespace AutoWeeklyCap.UI.MainWindow;

internal static class AboutTabUi
{
    internal static void Draw()
    {
        ImGui.Text("About section");
        ImGui.Text("More information is coming soon...");
        ImGui.Text("Also... The plugin status is: " + AutoWeeklyCap.Runner.GetStatus());
        ImGui.Text("and the selected duty is: ");
        ImGui.SameLine(0f, 0f);

        if (AutoWeeklyCap.DataManager.GetExcelSheet<TerritoryType>()
                  .TryGetRow(AutoWeeklyCap.Config.ZoneId, out var territoryRow))
        {
            ImGui.Text(
                $"{territoryRow.PlaceName.Value.Name} ({AutoWeeklyCap.Config.ZoneId})");
        }
        else
        {
            ImGui.Text("invalid Zone ID");
        }
    }
}

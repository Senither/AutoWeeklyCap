using System;
using System.Collections.Generic;
using Dalamud.Bindings.ImGui;

namespace AutoWeeklyCap.UI.MainWindow;

internal static class AboutTabUi
{
    private static readonly Dictionary<string, string> Content = new()
    {
        { "Author:", "Senither" },
        { "Discord:", "@senither" },
        { "Version:", AutoWeeklyCap.Version },
        { "Credits:", "Tuffic for the original idea" }
    };

    internal static void Draw()
    {
        var labelWidth = 0f;
        foreach (var label in Content.Keys)
            labelWidth = MathF.Max(labelWidth, ImGui.CalcTextSize(label).X);

        labelWidth += ImGui.GetStyle().CellPadding.X * 2f;

        if (ImGui.BeginTable("##about-table-awc", 2))
        {
            ImGui.TableSetupColumn("Field", ImGuiTableColumnFlags.WidthFixed, labelWidth);
            ImGui.TableSetupColumn("Value", ImGuiTableColumnFlags.WidthStretch);

            foreach (var (label, value) in Content)
            {
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.TextUnformatted(label);
                ImGui.TableSetColumnIndex(1);
                ImGui.TextWrapped(value);
            }

            ImGui.EndTable();
        }
    }
}

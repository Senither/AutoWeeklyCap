namespace AutoWeeklyCap.UI.MainWindow;

internal static class AboutTabUi
{
    internal static void Draw()
    {
        ImGuiHelpers.ScaledDummy(5f);
        ImGuiEx.TextCentered($"{AWC.Name} v{AWC.Version}");
        ImGuiHelpers.ScaledDummy(1f);

        ImGuiEx.TextCentered("Developed and published by Senither");
        ImGuiEx.TextCentered("Original idea by Tuffic");
        ImGuiEx.TextCentered("Additional ideas by Naru, Myuri & Yoite");

        ImGuiHelpers.ScaledDummy(5f);

        ImGuiEx.LineCentered(() =>
        {
            if (ImGui.Button("Plugin List"))
            {
                ImGui.SetClipboardText("https://dalamud-plugins.senither.com");
                Notify.Success("Link copied to clipboard");
            }

            ImGui.SameLine();
            if (ImGui.Button("Plugin Repository"))
            {
                ImGui.SetClipboardText("https://dalamud-plugins.senither.com/plugin/AutoWeeklyCap.json");
                Notify.Success("Link copied to clipboard");
            }
        });
    }
}

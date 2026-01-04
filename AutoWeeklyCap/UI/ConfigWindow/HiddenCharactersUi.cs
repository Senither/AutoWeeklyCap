using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using ECommons.ImGuiMethods;

namespace AutoWeeklyCap.UI.ConfigWindow;

public static class HiddenCharactersUi
{
    public static void Draw()
    {
        ImGui.TextWrapped("Hidden characters don't show in the character list, and are ignored for tomestone runs.");

        ImGui.Spacing();
        ImGui.Spacing();

        foreach (var (characterAndWorld, options) in AutoWeeklyCap.Config.Characters)
        {
            if (!options.IsHidden())
                continue;

            if (ImGuiEx.IconButton(FontAwesomeIcon.Eye, "###show-hidden-character" + characterAndWorld))
            {
                options.Hidden = false;
            }

            ImGui.SameLine();
            ImGui.TextWrapped(characterAndWorld);
        }
    }
}

using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;

namespace AutoWeeklyCap.UI.Helpers;

public static class ItemIcon
{
    public static void Draw(uint iconId, bool itemHq = false, float multiplier = 1f)
    {
        if (!Svc.Texture.GetFromGameIcon(new GameIconLookup(iconId, itemHq)).TryGetWrap(out IDalamudTextureWrap? wrap, out _)) {
            return;
        }

        ImGui.Image(wrap.Handle, new Vector2(ImGui.GetFrameHeight() * multiplier));
        ImGui.SameLine();
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + ImGui.GetStyle().FramePadding.X);
    }
}

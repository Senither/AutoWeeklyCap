using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;

namespace AutoWeeklyCap.UI.Helpers;

public static class ItemIcon
{
    public static void Draw(uint iconId)
    {
        if (!Svc.Texture.GetFromGameIcon(new GameIconLookup(iconId)).TryGetWrap(out IDalamudTextureWrap? wrap, out _)) {
            return;
        }

        ImGui.Image(wrap.Handle, new Vector2(ImGui.GetFrameHeight()));
        ImGui.SameLine();
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + ImGui.GetStyle().FramePadding.X);
    }
}

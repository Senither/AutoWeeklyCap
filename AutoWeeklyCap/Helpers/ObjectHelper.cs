using Dalamud.Game.ClientState.Objects.Types;

using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;

using ObjectKind = Dalamud.Game.ClientState.Objects.Enums.ObjectKind;

namespace AutoWeeklyCap.Helpers;

public static class ObjectHelper
{
    internal static unsafe void InteractWithObject(IGameObject? gameObject, bool face = true)
    {
        try {
            if (gameObject is not { IsTargetable: true }) {
                return;
            }

            GameObject* gameObjectPointer = (GameObject*)gameObject.Address;
            TargetSystem.Instance()->InteractWithObject(gameObjectPointer, false);
        } catch (Exception ex) {
            Svc.Log.Info($"InteractWithObject: Exception: {ex}");
        }
    }

    internal static IGameObject? FindGameObject(uint id, Vector3 position)
    {
        try {
            IGameObject? closest = null;
            var closestDistance = float.MaxValue;

            foreach (var obj in Svc.Objects) {
                if (obj.ObjectKind is not (ObjectKind.EventNpc or ObjectKind.BattleNpc or ObjectKind.EventObj)) {
                    continue;
                }

                if (obj.BaseId != id) {
                    continue;
                }

                var d = Vector3.Distance(Player.Position, obj.Position);
                if (!(d < closestDistance)) {
                    continue;
                }

                closest = obj;
                closestDistance = d;
            }

            if (closest != null) {
                return closest;
            }

            // Fallback: nearest object around the expected mender location.
            foreach (var obj in Svc.Objects) {
                if (obj.ObjectKind is not (ObjectKind.EventNpc or ObjectKind.BattleNpc or ObjectKind.EventObj)) {
                    continue;
                }

                var d = Vector3.Distance(position, obj.Position);
                if (d <= 6f) {
                    return obj;
                }
            }
        } catch (Exception) {
            // ignored
        }

        return null;
    }
}

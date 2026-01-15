using System;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;

namespace AutoWeeklyCap.Helpers;

public class ObjectHelper
{
    internal static unsafe void InteractWithObject(IGameObject? gameObject, bool face = true)
    {
        try
        {
            if (gameObject is not { IsTargetable: true })
                return;

            GameObject* gameObjectPointer = (GameObject*)gameObject.Address;
            TargetSystem.Instance()->InteractWithObject(gameObjectPointer, false);
        }
        catch (Exception ex)
        {
            Svc.Log.Info($"InteractWithObject: Exception: {ex}");
        }
    }
}

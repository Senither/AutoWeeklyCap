using System;
using System.Numerics;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using ECommons.ExcelServices;
using ECommons.GameHelpers;
using ObjectKind = Dalamud.Game.ClientState.Objects.Enums.ObjectKind;

namespace AutoWeeklyCap.Helpers;

public class RepairNPCHelper
{
    public static bool Repair() => Repair(AutoWeeklyCap.Config.RepairPercentage);

    public static bool Repair(uint percent)
    {
        if (InventoryHelper.GetItemsNeedingRepairCount(percent) == 0)
            return false;

        if (!AutoWeeklyCap.PlayerState.IsLoaded || !Player.Available)
            return false;

        if (Svc.Condition[ConditionFlag.BetweenAreas] || Svc.Condition[ConditionFlag.BetweenAreas51])
            return false;

        //...

        return true;
    }

    private static uint RepairVendorTerritoryType()
    {
        return PlayerHelper.GetGrandCompanyTerritoryType(PlayerHelper.GetGrandCompany());
    }

    private static Vector3 RepairVendorLocation => PlayerHelper.GetGrandCompany() switch
    {
        GrandCompany.Maelstrom => new Vector3(17.715698f, 40.200005f, 3.9520264f),
        GrandCompany.TwinAdder => new Vector3(24.826416f, -8f, 93.18677f),
        _ => new Vector3(32.85266f, 6.999999f, -81.31531f),
    };

    private static uint RepairVendorDataId => PlayerHelper.GetGrandCompany() switch
    {
        GrandCompany.Maelstrom => 1003251u,
        GrandCompany.TwinAdder => 1000394u,
        _ => 1004416u,
    };

    private static IGameObject? RepairVendorGameObject()
    {
        try
        {
            var wantedDataId = RepairVendorDataId;
            IGameObject? closest = null;
            var closestDistance = float.MaxValue;

            foreach (var obj in Svc.Objects)
            {
                if (obj.ObjectKind is not (ObjectKind.EventNpc or ObjectKind.BattleNpc or ObjectKind.EventObj))
                    continue;

                if (obj.BaseId != wantedDataId)
                    continue;

                var d = Vector3.Distance(Player.Position, obj.Position);
                if (d < closestDistance)
                {
                    closest = obj;
                    closestDistance = d;
                }
            }

            if (closest != null)
                return closest;

            // Fallback: nearest object around the expected mender location.
            foreach (var obj in Svc.Objects)
            {
                if (obj == null)
                    continue;

                if (obj.ObjectKind is not (ObjectKind.EventNpc or ObjectKind.BattleNpc or ObjectKind.EventObj))
                    continue;

                var d = Vector3.Distance(RepairVendorLocation, obj.Position);
                if (d <= 6f)
                    return obj;
            }
        }
        catch (Exception)
        {
            // ignored
        }

        return null;
    }
}

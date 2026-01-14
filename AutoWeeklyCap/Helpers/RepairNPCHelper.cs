using System.Numerics;
using ECommons;
using ECommons.DalamudServices;
using ECommons.ExcelServices;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace AutoWeeklyCap.Helpers;

public class RepairNPCHelper
{
    public static bool Repair() => Repair(AutoWeeklyCap.Config.RepairPercentage);

    public static bool Repair(uint percent)
    {
        if (InventoryHelper.GetItemsNeedingRepairCount(percent) == 0)
            return false;

        // TODO - 1: Check if repair NPC is within reach
        // TODO - 1.1: If yes, skip to step 5
        // TODO - 1.2: If no, go to step 2
        // TODO - 2: Check that the player is idle (not jumping, casting, etc)
        // TODO - 3: Start VNavMesh to move to the grand company repair NPC
        // TODO - 4: Wait for VNavMesh to finish navigating to the NPC
        // TODO - 5: Interact with the repair NPC
        // TODO - 6: Click "Repair All" button
        // TODO - 7: Confirm the repair of all the items
        // TODO - 8: Close the repair window


        // AutoWeeklyCap.TaskManager.Enqueue(() =>
        // {
        //     if (Svc.ClientState.TerritoryType == RepairVendorTerritoryType())
        //         return true;
        //
        //     return false;
        // }, "npc repair: going to territory");

        return false;
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
}

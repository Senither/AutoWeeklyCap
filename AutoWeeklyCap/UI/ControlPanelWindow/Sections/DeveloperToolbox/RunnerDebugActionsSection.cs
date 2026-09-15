using AutoWeeklyCap.UI.Helpers;

namespace AutoWeeklyCap.UI.ControlPanelWindow.Sections.DeveloperToolbox;

internal static class RunnerDebugActionsSection
{
    internal static void Draw()
    {
        DebugButton.Draw("Extract", ActionInstance.Extract, false);
        DebugButton.Draw("Self Repair", ActionInstance.SelfRepair);
        DebugButton.Draw("NPC Repair", ActionInstance.NpcRepair);
        DebugButton.Draw("Spend Tomestones", ActionInstance.SpendTomestone);
        DebugButton.Draw("Use Food", ActionInstance.UseFood);
        DebugButton.Draw("Buy Food", ActionInstance.BuyFood);

        DebugButton.Draw("Equip Gear Upgrades", ActionInstance.EquipGearUpgrade, false);
        DebugButton.Draw("Buy Leveling Gear Upgrades", ActionInstance.BuyLevelingUpgrade);
        DebugButton.Draw("Move items to Saddlebag", ActionInstance.MoveInventoryItemsToSaddlebag);

        DebugButton.Draw("Learn TT cards", ActionInstance.LearnTripleTriadCard, false);
        DebugButton.Draw("Sell TT cards", ActionInstance.SellTripleTriadCard);
        DebugButton.Draw("Sell Worthless Items", ActionInstance.SellWorthlessItems);

        DebugButton.Draw("Return to Homeworld", ActionInstance.Homeworld, false);
        DebugButton.Draw("Deliveroo", ActionInstance.Deliveroo);
        DebugButton.Draw("Notification", ActionInstance.Notification);

        Card.Separator();

        DebugButton.Draw("Enter preferred safezone", ActionInstance.Safezone, false);
        DebugButton.Draw("Enter GC Inn", ActionInstance.EnterGrandCompanyInn);
        DebugButton.Draw("Leave GC Inn", ActionInstance.LeaveGrandCompanyInn);

        DebugButton.Draw("Enter Private House", ActionInstance.EnterPrivateHouse, false);
        DebugButton.Draw("Enter Apartment", ActionInstance.EnterApartment);
        DebugButton.Draw("Enter FC House", ActionInstance.EnterFcHouse);
    }
}

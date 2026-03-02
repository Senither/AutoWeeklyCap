using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoWeeklyCap.Runner.Actions;

public class ExtractAction : BaseAction
{
    protected override string Name => nameof(ExtractAction);
    protected override string[] AddonsToClose { get; } = ["MaterializeDialog", "Materialize", "SelectYesno", "SelectString"];

    protected override bool Run(params object[] args)
    {
        if (!QuestManager.IsQuestComplete(66174))
        {
            LogInfo("Stopping materia extraction, reason: player has not completed quest 66174 (Forging the Spirit)");
            return false;
        }

        var stoppingCategory = AWC.Config.ExtractAll ? 6 : 0;
        var currentCategory = 0;
        var switchCategory = false;

        using var title = TitleManager.RegisterTitle(BitmapFontIcon.ElementEarth, "Extracting materia");

        Enqueue(() =>
        {
            if (!EzThrottler.Throttle("Extract", 250))
                return false;

            if (currentCategory > stoppingCategory)
                return true;

            try
            {
                unsafe
                {
                    if (InventoryManager.Instance()->GetEmptySlotsInBag() < 1)
                    {
                        LogInfo("Stopping materia extraction, reason: no items slot left");
                        return true;
                    }

                    if (PlayerHelper.IsOccupied)
                        return false;

                    if (GenericHelpers.TryGetAddonByName("MaterializeDialog", out AtkUnitBase* addonMaterializeDialog) &&
                        GenericHelpers.IsAddonReady(addonMaterializeDialog))
                    {
                        LogDebug("Confirming MaterializeDialog");
                        new AddonMaster.MaterializeDialog(addonMaterializeDialog).Materialize();
                        return false;
                    }

                    if (!GenericHelpers.TryGetAddonByName("Materialize", out AtkUnitBase* addonMaterialize))
                    {
                        ActionManager.Instance()->UseAction(ActionType.GeneralAction, 14);
                        return false;
                    }

                    if (!GenericHelpers.IsAddonReady(addonMaterialize))
                        return false;

                    AtkComponentList* list = addonMaterialize->GetNodeById(12)->GetAsAtkComponentList();
                    if (list == null)
                        return false;

                    AtkTextNode* spiritbondTextNode = list->UldManager.NodeList[2]->GetComponent()->GetTextNodeById(5)->GetAsAtkTextNode();
                    AtkTextNode* categoryTextNode = addonMaterialize->GetNodeById(4)->GetAsAtkComponentDropdownList()->UldManager.NodeList[1]
                        ->GetAsAtkComponentCheckBox()->GetTextNodeById(3)->GetAsAtkTextNode();

                    if (spiritbondTextNode == null || categoryTextNode == null)
                        return false;

                    if (switchCategory)
                    {
                        LogDebug($"Switching to Category: {currentCategory}");
                        AddonHelper.FireCallBack(addonMaterialize, false, 1, currentCategory);
                        switchCategory = false;
                        return false;
                    }

                    if (spiritbondTextNode->NodeText.ToString().Replace(" ", string.Empty) == "100%")
                    {
                        LogDebug("Extracting Materia");
                        AddonHelper.FireCallBack(addonMaterialize, true, 2, 0);
                    }
                    else
                    {
                        currentCategory++;
                        switchCategory = true;
                    }

                    return false;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return true;
        }, "extracting materia", 180_000); // 3 minutes

        Enqueue(() =>
        {
            try
            {
                unsafe
                {
                    if (GenericHelpers.TryGetAddonByName("Materialize", out AtkUnitBase* addonMaterialize))
                    {
                        ActionManager.Instance()->UseAction(ActionType.GeneralAction, 14);
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return true;
        }, "closing window");

        return true;
    }
}

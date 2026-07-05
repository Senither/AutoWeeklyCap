using AutoWeeklyCap.Contracts.Runner;

using FFXIVClientStructs.FFXIV.Client.Game;

namespace AutoWeeklyCap.Runner.Actions;

public class UseFoodAction : BaseAction
{
    protected override string Name => nameof(UseFoodAction);

    private const uint OrangeJuice = 4745;

    protected override bool Run(params object[] args)
    {
        if (HasFoodBuffStatus()) {
            return false;
        }

        var itemCount = InventoryHelper.GetItemCount(OrangeJuice);
        if (itemCount == 0) {
            if (!QuestManager.IsQuestComplete(65970)) {
                LogInfo("Stopping food usage, reason: player has not completed quest 65970 (It Could Happen to You)");
                return false;
            }

            // TODO: Buy more orange juice
            return false;
        }

        using var title = TitleManager.RegisterTitle(BitmapFontIcon.OrangeDiamond, "Use Food");

        Enqueue(() =>
        {
            if (!EzThrottler.Throttle("UseItemUntilStatus", 250)) {
                return false;
            }

            if (HasFoodBuffStatus()) {
                return true;
            }

            if (!PlayerHelper.IsReady || PlayerHelper.IsCasting || PlayerHelper.IsAnimationLocked) {
                return false;
            }

            InventoryHelper.UseItem(OrangeJuice);

            return false;
        }, "use food");

        EnqueueDelay(50);
        Enqueue(() => PlayerHelper.IsReady && !PlayerHelper.IsAnimationLocked, "wait for player");

        return true;
    }

    private static bool HasFoodBuffStatus()
    {
        return PlayerHelper.HasStatus(48, 1200);
    }
}

using AutoWeeklyCap.Contracts.Runner;

namespace AutoWeeklyCap.Runner.Actions;

public class UseFoodAction : BaseAction
{
    protected override string Name => nameof(UseFoodAction);

    protected override bool Run(params object[] args)
    {
        if (HasFoodBuffStatus()) {
            return false;
        }

        var itemCount = InventoryHelper.GetItemCount(Constants.LevelingFoodItemId);
        if (itemCount == 0 && !ActionInstance.BuyFood.Invoke()) {
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

            InventoryHelper.UseItem(Constants.LevelingFoodItemId);

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

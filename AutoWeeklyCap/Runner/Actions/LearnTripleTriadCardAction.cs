using AutoWeeklyCap.Contracts.Runner;

using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace AutoWeeklyCap.Runner.Actions;

public class LearnTripleTriadCardAction : BaseAction
{
    protected override string Name => nameof(LearnTripleTriadCardAction);

    protected override bool Run(params object[] args)
    {
        List<InventoryItem> cards = GetUnlearnedTripleTriadCards();
        if (cards.Count == 0) {
            return false;
        }

        using var title = TitleManager.RegisterTitle(BitmapFontIcon.BlueStar, "Learning TT cards");

        Enqueue(() =>
        {
            if (!EzThrottler.Throttle("LearningTripleTriadCards", 250)) {
                return false;
            }

            if (PlayerHelper.IsOccupied || PlayerHelper.IsCasting) {
                return false;
            }

            if (cards.Count == 0) {
                return true;
            }

            InventoryItem card = cards[0];
            cards.RemoveAt(0);

            InventoryHelper.UseItem(card.ItemId);

            return false;
        }, "learning triple triad cards");

        return false;
    }

    private static List<InventoryItem> GetUnlearnedTripleTriadCards()
    {
        return InventoryHelper.GetTripleTriadCardsInInventory().Where(item =>
        {
            try {
                if (InventoryHelper.TryGetSheetItemFromItemId(item.ItemId, out var sheetItem)) {
                    unsafe {
                        return !UIState.Instance()->IsTripleTriadCardUnlocked((ushort)sheetItem.AdditionalData.RowId);
                    }
                }
            } catch (Exception) {
                // ignored
            }

            return false;
        }).ToList();
    }
}

using AutoWeeklyCap.Config;
using AutoWeeklyCap.Contracts.Runner;

using ECommons.UIHelpers.AtkReaderImplementations;

using FFXIVClientStructs.FFXIV.Client.Game;

// ReSharper disable InconsistentNaming

namespace AutoWeeklyCap.Runner.Actions;

public class SellTripleTriadCardAction : BaseAction
{
    protected override string Name => nameof(LearnTripleTriadCardAction);
    protected override string[] AddonsToClose => ["SelectIconString", "SelectString", "TripleTriadCoinExchange", "SelectYesno"];

    private const int LongTaskTimeout = 120_000;

    private static readonly Vector3 VendorPosition = new(-55.58408f, 1.6000001f, 16.524887f);
    private const uint VendorDataID = 1016294u;
    private const uint VendorTerritoryID = 144u;
    private const string AetheriteName = "The Gold Saucer";

    private const string MetricsKey = "TripleTriadCards";

    protected override bool Run(params object[] args)
    {
        if (!QuestManager.IsQuestComplete(65970)) {
            LogInfo("Stopping selling TT cards, reason: player has not completed quest 65970 (It Could Happen to You)");
            return false;
        }

        if (!VNavMeshIPC.IsEnabled || !LifestreamIPC.IsEnabled) {
            return false;
        }

        if (!HasEnoughCardsToSell()) {
            return false;
        }

        LocationManager.Reset();
        using var title = TitleManager.RegisterTitle(BitmapFontIcon.GoldStar, "Selling TT cards");

        Enqueue(
            () =>
            {
                AWC.Runner.State.SetMetric(MetricsKey, (uint)CurrencyHelper.GetMGP());
                return true;
            },
            "prepare items metrics"
        );

        Enqueue(
            () => MovementHelper.TeleportTo(AetheriteName, VendorTerritoryID),
            "start moving to territory"
        );

        Enqueue(
            () => MovementHelper.MoveTo(VendorPosition),
            "start moving to npc location",
            LongTaskTimeout
        );

        Enqueue(() =>
        {
            if (!EzThrottler.Throttle("OpeningVendorWindow", 250)) {
                return false;
            }

            var vendor = ObjectHelper.FindGameObject(VendorDataID, VendorPosition);
            if (vendor == null) {
                return false;
            }

            unsafe {
                if (AddonHelper.TryGetReadyAddon("TripleTriadCoinExchange", out _)) {
                    return true;
                }

                if (AddonHelper.TryGetReadyAddon("SelectIconString", out _)) {
                    AddonHelper.ClickSelectIconString(1);
                } else {
                    ObjectHelper.InteractWithObject(vendor);
                }
            }

            return false;
        }, "open window");

        Enqueue(() =>
        {
            if (!EzThrottler.Throttle("SellingTripleTriadCards", 250)) {
                return false;
            }

            try {
                unsafe {
                    if (!AddonHelper.TryGetReadyAddon("TripleTriadCoinExchange", out var exchangeAddon)) {
                        return true;
                    }

                    var exchange = new ReaderTripleTriadCoinExchange(exchangeAddon);
                    if (exchange.Entries.Count == 0) {
                        return true;
                    }

                    if (AddonHelper.TryGetReadyAddon("ShopCardDialog", out var dialogAddon)) {
                        AddonHelper.FireCallBack(dialogAddon, true, 0, exchange.Entries.First().Count);
                        return false;
                    }

                    AddonHelper.FireCallBack(exchangeAddon, true, 0, 0u);

                    return false;
                }
            } catch (Exception) {
                return true;
            }
        }, "sell triple triad card");

        EnqueueDelay(500);

        Enqueue(() => AddonHelper.CloseAddons(AddonsToClose), "close shop window");

        Enqueue(
            () =>
            {
                if (!AWC.Runner.State.HasMetric(MetricsKey)) {
                    return true;
                }

                uint before = AWC.Runner.State.PullMetric(MetricsKey);

                AWC.Config.GetCurrentCharacterMetrics()
                    ?.IncrementMgpEarnedFromSellingCardsCounter((uint)(CurrencyHelper.GetMGP() - before));

                Configuration.Save();

                return true;
            },
            "prepare items metrics"
        );

        return false;
    }

    private static bool HasEnoughCardsToSell()
    {
        List<InventoryItem> cards = InventoryHelper.GetTripleTriadCardsInInventory().ToList();
        if (cards.Count == 0) {
            return false;
        }

        return cards.Sum(card => card.Quantity) > 5;
    }
}

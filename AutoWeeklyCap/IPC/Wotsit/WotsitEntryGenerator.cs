namespace AutoWeeklyCap.IPC.Wotsit;

public static class WotsitEntryGenerator
{
    private static readonly List<WotsitEntry> RunnerEntries =
    [
        new("Start Runner", "start tome cap runner", 16, () => AWC.Runner.Start()),
        new("Resume Runner", "resume tome cap runner", 16, () => AWC.Runner.Resume()),
        new("Stop Runner", "stop tome cap runner", 26, () => AWC.Runner.Stop()),
        new("Force Stop Runner", "force-stop tome cap runner", 26, () => AWC.Runner.Abort())
    ];

    private static readonly List<WotsitEntry> BasicRunnerActions =
    [
        new("Abort Action", "abort to force-stop runner action", 26, () => AWC.TaskManager.Abort()),
        new("Extract Materia", "extract materia from spriritbonded gear", 109, () => ActionInstance.Extract.Invoke()),
        new("Repair Gear", "repair gear", 106, () => ActionInstance.SelfRepair.Invoke()),
        new("Move items to Saddlebag", "move items to saddlebag", 60153, () => ActionInstance.MoveInventoryItemsToSaddlebag.Invoke()),
        new("Enter Grand Company inn", "enter gc (grand company) inn", 113, () => ActionInstance.EnterGrandCompanyInn.Invoke()),
        new("Leave Grand Company inn", "leave gc (grand company) inn", 114, () => ActionInstance.LeaveGrandCompanyInn.Invoke())
    ];

    public static IEnumerable<WotsitEntry> Generate()
    {
        foreach (var entry in RunnerEntries) {
            yield return entry;
        }

        foreach (var entry in BasicRunnerActions) {
            yield return entry;
        }

        foreach (var entry in ThirdPartyRunnerActions()) {
            yield return entry;
        }

        foreach (var entry in ReloggableCharacters()) {
            yield return entry;
        }

        foreach (var entry in SafezoneTeleporterActions()) {
            yield return entry;
        }
    }

    private static IEnumerable<WotsitEntry> ThirdPartyRunnerActions()
    {
        if (VNavMeshIPC.IsEnabled && LifestreamIPC.IsEnabled) {
            yield return new WotsitEntry(
                "Auto Spend Uncapped Tomestone",
                "auto spend uncapped tomestone to buy preferred item",
                3,
                () => ActionInstance.SpendTomestone.Invoke()
            );
        }

        if (VNavMeshIPC.IsEnabled && LifestreamIPC.IsEnabled && DeliverooIPC.IsEnabled) {
            yield return new WotsitEntry(
                "GC Turnins with Deliveroo",
                "gc turn-ins with deliveroo",
                32,
                () => ActionInstance.Deliveroo.Invoke()
            );
        }
    }

    private static IEnumerable<WotsitEntry> ReloggableCharacters()
    {
        if (!LifestreamIPC.IsEnabled || PlayerHelper.InDuty) {
            yield break;
        }

        var currentCharacter = PlayerHelper.GetFullCharacterName();
        foreach (var characterAndWorld in AWC.Config.Characters.Keys) {
            if (currentCharacter == characterAndWorld) {
                continue;
            }

            yield return new WotsitEntry(
                $"Relog to character: {characterAndWorld}",
                $"relog to character: {characterAndWorld}",
                112,
                () => ChatHelper.RunCommand($"awc relog {characterAndWorld}")
            );
        }
    }

    private static IEnumerable<WotsitEntry> SafezoneTeleporterActions()
    {
        if (!LifestreamIPC.IsEnabled || PlayerHelper.InDuty) {
            yield break;
        }

        yield return new WotsitEntry(
            "Enter preferred safe-zone",
            "enter|goto safezone|safe zone",
            113,
            () =>
            {
                var characterName = PlayerHelper.GetFullCharacterName();

                _ = characterName == null
                    ? ActionInstance.Safezone.Invoke()
                    : ActionInstance.Safezone.Invoke(characterName);
            }
        );

        if (LifestreamIPC.HasPrivateHouse()) {
            yield return new WotsitEntry(
                "Enter personal house",
                "enter|goto private|personal house|estate",
                113,
                () => ActionInstance.EnterPrivateHouse.Invoke()
            );
        }

        if (LifestreamIPC.HasApartment()) {
            yield return new WotsitEntry(
                "Enter personal apartment",
                "enter|goto private|personal apartment|estate",
                113,
                () => ActionInstance.EnterApartment.Invoke()
            );
        }

        if (LifestreamIPC.HasFreeCompanyHouse()) {
            yield return new WotsitEntry(
                "Enter free company house",
                "enter|goto fc|free company house",
                113,
                () => ActionInstance.EnterFcHouse.Invoke()
            );
        }
    }
}

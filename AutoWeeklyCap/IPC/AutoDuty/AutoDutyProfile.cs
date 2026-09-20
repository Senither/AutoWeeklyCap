namespace AutoWeeklyCap.IPC.AutoDuty;

public static class AutoDutyProfile
{
     // @formatter:off
    private static readonly Dictionary<string, object> DefaultProfileSettings = new()
    {
        { "Meta.AutoDutyModeEnum", "Looping" },
        { "Meta.DutyModeEnum", "Support" },
        { "Meta.LoopTimes", 1 },
        { "Meta.ShowMainWindowOnStartup", false },
        { "Meta.LoopActionsOpenByDefault", false },

        { "Overlay.Show", false },

        { "Loop.Pre.Enabled", false },
        { "Loop.Between.Enabled", false },
        { "Loop.Termination.Enabled", false },

        { "DutyConfig.AutoExitDuty", true },
        { "DutyConfig.W2WJobs", "Tanks" },
        { "DutyConfig.DisableRenderWhileActive", false },
        { "DutyConfig.UsingAlternativeRotationPlugin", false },
        { "DutyConfig.UsingAlternativeMovementPlugin", false },
        { "DutyConfig.UsingAlternativeBossPlugin", false },

        { "DutyConfig.LootTreasure", false },
        { "DutyConfig.LootMethodEnum", "AutoDuty" },
        { "DutyConfig.LootBossTreasureOnly", false },
        { "DutyConfig.TreasureCofferScanDistance", 25 },

        { "DutyConfig.Stuck.RebuildNavmeshOnStuck", true },
        { "DutyConfig.Stuck.RebuildNavmeshAfterStuckXTimes", 5 },
        { "DutyConfig.Stuck.MinStuckTime", 500 },
        { "DutyConfig.Stuck.StuckOnStep", true },
        { "DutyConfig.Stuck.StuckReturnX", 10 },
        { "DutyConfig.Stuck.StuckReturn", true },
    };
    // @formatter:on

    public static void Pop()
    {
        if (AutoDutyIPC.IsEnabled) {
            AutoDutyIPC.PopConfigOverrides();
        }
    }

    public static void Apply()
    {
        if (!AutoDutyIPC.IsEnabled) {
            return;
        }

        var overrides = new Dictionary<string, object>(DefaultProfileSettings);

        if (ShouldBeOpeningCoffers()) {
            overrides["DutyConfig.LootTreasure"] = true;
        }

        if (RotationSolverRebornIPC.IsEnabled) {
            overrides["DutyConfig.RotationPlugin"] = "RotationSolverReborn";
        }

        AutoDutyIPC.PushConfigOverrides(overrides);
    }

    private static bool ShouldBeOpeningCoffers()
    {
        if (AWC.Runner.State.LevelingMode) {
            return true;
        }

        return AWC.Config.DeliverooEnabled && DeliverooIPC.IsEnabled;
    }
}

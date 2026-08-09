namespace AutoWeeklyCap.IPC.AutoDuty;

public static class AutoDutyProfile
{
    private static readonly Dictionary<string, string> DefaultProfileSettings = new()
    {
        { "AutoScroll", "True" },
        { "LogEventLevel", "Debug" },
        { "autoDutyModeEnum", "Looping" },
        { "LoopTimes", "1" },
        { "dutyModeEnum", "Support" },
        { "Unsynced", "False" },
        { "HideUnavailableDuties", "False" },
        { "PreferTrustOverSupportLeveling", "False" },
        { "SquadronAssignLowestMembers", "True" },
        { "ShowMainWindowOnStartup", "False" },
        { "showOverlay", "False" },
        { "hideOverlayWhenStopped", "False" },
        { "lockOverlay", "False" },
        { "overlayNoBG", "False" },
        { "OverlayAnchorBottom", "False" },
        { "ShowDutyLoopText", "True" },
        { "ShowActionText", "True" },
        { "UseSliderInputs", "False" },
        { "OverrideOverlayButtons", "True" },
        { "GotoButton", "True" },
        { "TurninButton", "True" },
        { "DesynthButton", "True" },
        { "ExtractButton", "True" },
        { "RepairButton", "True" },
        { "EquipButton", "True" },
        { "CofferButton", "True" },
        { "TTButton", "True" },
        { "AutoExitDuty", "True" },
        { "OnlyExitWhenDutyDone", "False" },
        { "AutoManageRotationPluginState", "True" },
        { "rotationPlugin", "RotationSolverReborn" },
        { "Wrath_AutoSetupJobs", "True" },
        { "Wrath_TargetingTank", "Highest_Max" },
        { "Wrath_TargetingNonTank", "Lowest_Current" },
        { "RSR_TargetHostileType", "AllTargetsCanAttack" },
        { "RSR_TargetingTypeTank", "HighMaxHP" },
        { "RSR_TargetingTypeNonTank", "LowHP" },
        { "autoManageBossModAISettings", "True" },
        { "HideBossModAIConfig", "False" },
        { "BM_UpdatePresetsAutomatically", "True" },
        { "maxDistanceToTargetRoleBased", "True" },
        { "MaxDistanceToTargetFloat", "2" },
        { "MaxDistanceToTargetAoEFloat", "2" },
        { "positionalRoleBased", "True" },
        { "MaxDistanceToTargetRoleMelee", "2" },
        { "MaxDistanceToTargetRoleRanged", "10" },
        { "positionalAvarice", "True" },
        { "PositionalEnum", "Any" },
        { "AutoManageVnavAlignCamera", "True" },
        { "LootTreasure", "False" },
        { "LootMethodEnum", "AutoDuty" },
        { "LootBossTreasureOnly", "False" },
        { "TreasureCofferScanDistance", "25" },
        { "RebuildNavmeshOnStuck", "True" },
        { "RebuildNavmeshAfterStuckXTimes", "5" },
        { "MinStuckTime", "500" },
        { "StuckOnStep", "True" },
        { "stuckReturn", "True" },
        { "StuckReturnX", "10" },
        { "PathDrawEnabled", "False" },
        { "PathDrawStepCount", "5" },
        { "DisableRenderWhileActive", "False" },
        { "OverridePartyValidation", "False" },
        { "UsingAlternativeRotationPlugin", "False" },
        { "UsingAlternativeMovementPlugin", "False" },
        { "UsingAlternativeBossPlugin", "False" },
        { "TreatUnsyncAsW2W", "True" },
        { "W2WJobs", "Tanks" },
        { "LevelingListExperimentalEntries", "False" },
        { "EnablePreLoopActions", "True" },
        { "ExecuteCommandsPreLoop", "False" },
        { "RetireMode", "False" },
        { "AutoEquipRecommendedGear", "False" },
        { "AutoEquipRecommendedGearSource", "Vanilla" },
        { "AutoEquipRecommendedGearGearsetterOldToInventory", "False" },
        { "AutoRepair", "True" },
        { "AutoRepairPct", "40" },
        { "AutoRepairSelf", "False" },
        { "AutoConsume", "False" },
        { "AutoConsumeIgnoreStatus", "False" },
        { "AutoConsumeTime", "29" },
        { "EnableBetweenLoopActions", "True" },
        { "ExecuteBetweenLoopActionLastLoop", "False" },
        { "WaitTimeBeforeAfterLoopActions", "0" },
        { "ExecuteCommandsBetweenLoop", "False" },
        { "AutoExtract", "True" },
        { "AutoOpenCoffers", "False" },
        { "AutoOpenCoffersBlacklistUse", "False" },
        { "autoExtractAll", "False" },
        { "autoDesynth", "False" },
        { "autoDesynthSkillUp", "False" },
        { "AutoDesynthSkillUpLimit", "50" },
        { "AutoDesynthNQOnly", "False" },
        { "AutoDesynthNoGearset", "True" },
        { "AutoDesynthCategories", "1" },
        { "autoGCTurnin", "False" },
        { "AutoGCTurninSlotsLeft", "5" },
        { "AutoGCTurninSlotsLeftBool", "False" },
        { "AutoGCTurninUseTicket", "False" },
        { "ArmoireEntrust", "False" },
        { "GlamourChestEntrust", "False" },
        { "TripleTriadRegister", "False" },
        { "TripleTriadSell", "False" },
        { "TripleTriadSellMinItemCount", "1" },
        { "TripleTriadSellMinSlotCount", "1" },
        { "DiscardItems", "False" },
        { "EnableAutoRetainer", "False" },
        { "PreferredSummoningBellEnum", "Inn" },
        { "AutoRetainer_RemainingTime", "0" },
        { "EnableTerminationActions", "True" },
        { "StopLevel", "False" },
        { "StopLevelInt", "1" },
        { "StopNoRestedXP", "False" },
        { "StopItemQty", "False" },
        { "StopItemAll", "False" },
        { "StopItemQtyInt", "1" },
        { "StopWhenDutyGathered", "False" },
        { "TerminationBLUSpellsEnabled", "False" },
        { "TerminationBLUSpellsAll", "False" },
        { "TerminationInventoryFree", "False" },
        { "TerminationInventoryFreeSlots", "0" },
        { "TerminationiLvl", "False" },
        { "TerminationiLvlInt", "0" },
        { "ExecuteCommandsTermination", "False" },
        { "PlayEndSound", "False" },
        { "CustomSound", "False" },
        { "CustomSoundVolume", "0" },
        { "SoundEnum", "None" },
        { "TerminationMethodEnum", "Do_Nothing" },
        { "TerminationKeepActive", "True" },
    };

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

        var overrides = new Dictionary<string, string>(DefaultProfileSettings);

        if (ShouldBeOpeningCoffers()) {
            overrides["LootTreasure"] = "True";
        }

        if (AWC.Config.Extract) {
            overrides["AutoExtract"] = "False";
        }

        if (AWC.Config.Repair) {
            overrides["AutoRepair"] = "False";
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

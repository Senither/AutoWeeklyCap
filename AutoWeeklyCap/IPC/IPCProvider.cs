using System.Diagnostics.CodeAnalysis;

using AutoWeeklyCap.Config;

using ECommons.EzIpcManager;

// ReSharper disable ArrangeMethodOrOperatorBody
// ReSharper disable UnusedMember.Global

namespace AutoWeeklyCap.IPC;

[SuppressMessage("Performance", "CA1822:Mark members as static")]
public class IPCProvider
{
    public IPCProvider()
    {
        ECommonsMain.ReducedLogging = true;
        EzIPC.Init(this);
        ECommonsMain.ReducedLogging = false;
    }

    /// <summary>
    /// Checks if the runner is actually running, or the task manager
    /// has queued up tasks to determine if AWC is busy or not.
    /// </summary>
    [EzIPC]
    public bool IsBusy() => AWC.TaskManager.IsBusy || AWC.Runner.State.IsRunning();

    /// <summary>
    /// Attempts to start the runner loop.
    /// </summary>
    [EzIPC("Runner.Start")]
    public bool RunnerStart() => AWC.Runner.Start();

    /// <summary>
    /// Attempts to stop the runner loop.
    /// </summary>
    [EzIPC("Runner.Stop")]
    public bool RunnerStop() => AWC.Runner.Stop();

    /// <summary>
    /// Resumes the runner loop if graceful shutdown is enabled, otherwise does nothing.
    /// </summary>
    [EzIPC("Runner.Resume")]
    public bool RunnerResume() => AWC.Runner.Resume();

    /// <summary>
    /// Aborts the runner loop immediately, cancelling any tasks that are currently running,
    /// including any tasks being preformed by Lifestream and AutoDuty.
    /// </summary>
    [EzIPC("Runner.Abort")]
    public void RunnerAbort() => AWC.Runner.Abort();

    /// <summary>
    /// Checks if the runner loop is currently running.
    /// </summary>
    [EzIPC("Runner.IsRunning")]
    public bool RunnerIsRunning() => AWC.Runner.State.IsRunning();

    /// <summary>
    /// Checks if the runner loop is currently in "normal mode", will return
    /// false if the runner is in unlimited or leveling mode.
    /// </summary>
    [EzIPC("Runner.IsInNormalMode")]
    public bool RunnerIsInNormalMode() => AWC.Runner.State.IsInNormalMode();

    /// <summary>
    /// Enables unlimited mode, will keep the runner loop running on the players preferred
    /// character until manually stopped, or Stop or Abort is called.
    /// </summary>
    [EzIPC("Runner.EnableUnlimitedMode")]
    public void RunnerEnableUnlimitedMode() => AWC.Runner.State.EnableUnlimitedMode();

    /// <summary>
    /// Enables leveling mode, will level the players preferred character(s) and jobs until
    /// they all reach max level, after that the runner will automatically stop itself.
    /// </summary>
    [EzIPC("Runner.EnableLevelingMode")]
    public void RunnerEnableLevelingMode() => AWC.Runner.State.EnableLevelingMode();

    // ------------------------- Config IPC Methods -------------------------
    // The config IPC methods can be used to temporarily override the users
    // config values, the overrides will persists until manually cleared,
    // or when the runner is stopped or the game shuts down.

    /// <summary>
    /// Gets all the overridable keys with their current values from the config.
    /// The keys are returned in flattened dot notation and each value is
    /// formatted as<c>value (object type)</c>.
    /// </summary>
    [EzIPC("Config.GetKeyValuePairs")]
    public Dictionary<string, string> ConfigGetKeyValuePairs() => ConfigOverrides.GetKeyValuePairs();

    /// <summary>
    /// Pushes a dictionary of config overrides, where the keys represent the config keys,
    /// and the value is the new value that should be used. The config key can be written
    /// using dot notation to override nested keys, and the value type must match
    /// the original type or structure for it to be accepted. Will return true if
    /// all the keys were overwritten, and false if one or more keys failed.
    /// <example>
    /// PushOverrides(new () {
    ///   { "SpendUncappedTomestoneThreshold", 950 },
    ///   { "LevelJobs.UseCharacterOrder", false },
    ///   { "LevelJobs.PreferredGearingProfile", "Partial" }
    /// });
    /// </example>
    /// </summary>
    [EzIPC("Config.PushOverrides")]
    public bool ConfigPushOverrides(Dictionary<string, object> values) => ConfigOverrides.Set(values);

    /// <summary>
    /// Pushes a single key-value pair to the config overrides, where the keys represent
    /// the config key, and the value is the new value that should be used. The config
    /// key can be written using dot notation to override nested keys, and the value
    /// type must match the original type or structure for it to be accepted. Will
    /// return true if the key were successfully overwritten.
    /// <example>
    /// PushOverride("SpendUncappedTomestoneThreshold", 950);
    /// </example>
    /// </summary>
    [EzIPC("Config.PushOverride")]
    public bool ConfigPushOverride(string key, object value) => ConfigOverrides.Set(key, value);

    /// <summary>
    /// Pops the config overrides, returning the config to the state it were in before any
    /// overrides where applied and unlocks the config for the user again. This is
    /// automatically called when a runner is fully stopped.
    /// </summary>
    [EzIPC("Config.PopOverrides")]
    public void ConfigPopOverrides() => ConfigOverrides.Clear();

    // ------------------------- Action IPC Methods -------------------------
    // All action methods returns a bool, which represents if the action was
    // actually queued, or the runner/game is in a state where the action
    // is either not possible, or not needed.

    /// <summary>
    /// Enqueues materia extraction action, will only run if there is at least
    /// one item within the users preferred filter that is spiritbonded.
    /// </summary>
    [EzIPC("Action.EnqueueExtract")]
    public bool EnqueueExtract() => ActionInstance.Extract.Invoke();

    /// <summary>
    /// Enqueues auto spending uncapped tomestone action to go buy the
    /// players selected tome item(s) with their uncapped tomestones.
    /// <br /><br />
    /// Requires Lifestream and vnavmesh to be enabled.
    /// </summary>
    [EzIPC("Action.EnqueueSpendTomestone")]
    public bool EnqueueSpendTomestone() => ActionInstance.SpendTomestone.Invoke();

    /// <summary>
    /// Enqueues self repair action, if the action fails due to missing dark matter, or lack
    /// of leveled crafters, it will automatically call the NPC repair action instead.
    /// </summary>
    [EzIPC("Action.EnqueueSelfRepair")]
    public bool EnqueueSelfRepair() => ActionInstance.SelfRepair.Invoke();

    /// <summary>
    /// Enqueues npc repair action if there are items can need to be repaired.
    /// <br /><br />
    /// Requires Lifestream and vnavmesh to be enabled.
    /// </summary>
    [EzIPC("Action.EnqueueNpcRepair")]
    public bool EnqueueNpcRepair() => ActionInstance.NpcRepair.Invoke();

    /// <summary>
    /// Enqueues homeworld action if the player is not already on their homeworld.
    /// <br /><br />
    /// Requires Lifestream to be enabled.
    /// </summary>
    [EzIPC("Action.EnqueueHomeworld")]
    public bool EnqueueHomeworld() => ActionInstance.Homeworld.Invoke();

    /// <summary>
    /// Enqueues deliveroo action, will make the player go to their Grand Company,
    /// and then enable Deliveroo to hand in any deliverables for GC seals.
    /// <br /><br />
    /// Requires Lifestream, vnavmesh, and Deliveroo to be enabled.
    /// </summary>
    [EzIPC("Action.EnqueueDeliveroo")]
    public bool EnqueueDeliveroo() => ActionInstance.Deliveroo.Invoke();

    /// <summary>
    /// Enqueues move inventory items to saddlebag action, will move any stackable
    /// items that are in both the players inventory and their saddlebag.
    /// </summary>
    [EzIPC("Action.EnqueueMoveInventoryItemsToSaddlebag")]
    public bool EnqueueMoveInventoryItemsToSaddlebag() => ActionInstance.MoveInventoryItemsToSaddlebag.Invoke();

    /// <summary>
    /// Enqueues equip gear upgrades action, will move item upgrades from the players
    /// inventory into their armoury, and then equip them on the current gearset.
    /// <br /><br />
    /// Requires Stylist to be enabled.
    /// </summary>
    [EzIPC("Action.EnqueueEquipGearUpgrade")]
    public bool EnqueueEquipGearUpgrade() => ActionInstance.EquipGearUpgrade.Invoke();

    /// <summary>
    /// Enqueues buy level upgrades action, will teleport and navigate to the NPC that sells
    /// gear matching the players current level, and then buy gear upgrades with gil until
    /// the players item level matches their preferred gearing profile.
    /// <br />
    /// Will do nothing for player jobs that are already max level, their gil threshold is hit,
    /// or their current item level is already above their preferred gearing profile.
    /// <br /><br />
    /// Requires Lifestream, vnavmesh, and Stylist to be enabled.
    /// </summary>
    [EzIPC("Action.EnqueueBuyLevelingUpgrade")]
    public bool EnqueueBuyLevelingUpgrade() => ActionInstance.BuyLevelingUpgrade.Invoke();

    /// <summary>
    /// Enqueues use leveling food action, will use "Orange Juice" to get the food buff
    /// is the player has less than 20 minutes left of their current food buff.
    /// <br />
    /// If the player has no "Orange Juice" in their inventory it will call the buy
    /// leveling food action first, and then use the food afterward.
    /// </summary>
    [EzIPC("Action.EnqueueUseLevelingFood")]
    public bool EnqueueUseLevelingFood() => ActionInstance.UseFood.Invoke();

    /// <summary>
    /// Enqueues buy leveling food action, will teleport to The Golden Saucer and buy
    /// 99 "Orange Juice" from the vendor.
    /// <br /><br />
    /// Requires Lifestream and vnavmesh to be enabled.
    /// </summary>
    [EzIPC("Action.EnqueueBuyLevelingFood")]
    public bool EnqueueBuyLevelingFood() => ActionInstance.BuyFood.Invoke();

    /// <summary>
    /// Enqueues enter safezone action, will teleport to the
    /// players preferred safezone and enter the residence.
    /// <br /><br />
    /// Requires Lifestream and vnavmesh to be enabled.
    /// </summary>
    [EzIPC("Action.EnqueueEnterSafezone")]
    public bool EnqueueEnterSafezone() => ActionInstance.Safezone.Invoke();

    /// <summary>
    /// Enqueues enter grand company inn action, will teleport to the
    /// players Grand Company inn and enter the inn.
    /// <br /><br />
    /// Requires Lifestream and vnavmesh to be enabled.
    /// </summary>
    [EzIPC("Action.EnqueueEnterGrandCompanyInn")]
    public bool EnqueueEnterGrandCompanyInn() => ActionInstance.EnterGrandCompanyInn.Invoke();

    /// <summary>
    /// Enqueues leave grand company inn action, will leave the Grand Company
    /// inn if the player is currently within the inn territory.
    /// <br /><br />
    /// Requires vnavmesh to be enabled.
    /// </summary>
    [EzIPC("Action.EnqueueLeaveGrandCompanyInn")]
    public bool EnqueueLeaveGrandCompanyInn() => ActionInstance.LeaveGrandCompanyInn.Invoke();

    /// <summary>
    /// Enqueues enter apartment action, will teleport to the
    /// players apartment complex and enter their apartment.
    /// <br /><br />
    /// Requires Lifestream to be enabled.
    /// </summary>
    [EzIPC("Action.EnqueueEnterApartment")]
    public bool EnqueueEnterApartment() => ActionInstance.EnterApartment.Invoke();

    /// <summary>
    /// Enqueues enter private house action, will teleport to the
    /// players privaete house plot and then enter their house.
    /// <br /><br />
    /// Requires Lifestream and vnavmesh to be enabled.
    /// </summary>
    [EzIPC("Action.EnqueueEnterPrivateHouse")]
    public bool EnqueueEnterPrivateHouse() => ActionInstance.EnterPrivateHouse.Invoke();

    /// <summary>
    /// Enqueues enter FC house action, will teleport to the players
    /// Free Company house plot and then enter the house.
    /// <br /><br />
    /// Requires Lifestream and vnavmesh to be enabled.
    /// </summary>
    [EzIPC("Action.EnqueueEnterFcHouse")]
    public bool EnqueueEnterFcHouse() => ActionInstance.EnterFcHouse.Invoke();
}

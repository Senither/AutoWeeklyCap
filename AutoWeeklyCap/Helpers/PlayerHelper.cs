using AutoWeeklyCap.Config;

using Dalamud.Game.ClientState.Conditions;
using Dalamud.Utility;

using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;

namespace AutoWeeklyCap.Helpers;

public static class PlayerHelper
{
    internal static bool IsReady => IsValid && !IsOccupied;
    internal static bool IsLoggedIn => AWC.ClientState.IsLoggedIn;
    internal static bool IsOccupied => GenericHelpers.IsOccupied() || Svc.Condition[ConditionFlag.Jumping61];

    public static unsafe bool IsValid =>
        Control.GetLocalPlayer() != null
        && ThreadSafety.IsMainThread
        && Svc.Condition.Any()
        && !Svc.Condition[ConditionFlag.BetweenAreas]
        && !Svc.Condition[ConditionFlag.BetweenAreas51]
        && Player.Available
        && Player.Interactable;

    public static unsafe bool IsCasting => Player.Character->IsCasting;
    public static unsafe bool IsMoving => AgentMap.Instance()->IsPlayerMoving;
    public static unsafe bool InCombat => Player.Character->InCombat;
    public static bool IsJumping => Svc.Condition.Any() && (Svc.Condition[ConditionFlag.Jumping] || Svc.Condition[ConditionFlag.Jumping61]);
    public static bool InDuty => Svc.Condition[ConditionFlag.BoundByDuty] || Svc.Condition[ConditionFlag.BoundByDuty56] || Svc.Condition[ConditionFlag.BoundByDuty95];
    public static unsafe bool IsAnimationLocked => ActionManager.Instance()->AnimationLock > 0;

    public static bool CanSelfRepairWithCrafters =>
        HasMaxJobLevel(PlayerJob.CPR) &&
        HasMaxJobLevel(PlayerJob.BSM) &&
        HasMaxJobLevel(PlayerJob.ARM) &&
        HasMaxJobLevel(PlayerJob.GSM) &&
        HasMaxJobLevel(PlayerJob.LTW) &&
        HasMaxJobLevel(PlayerJob.WVR);

    public static string? GetFullCharacterName()
    {
        if (!AWC.PlayerState.IsLoaded) {
            return null;
        }

        var world = AWC.PlayerState.HomeWorld.ValueNullable;
        if (world == null) {
            return null;
        }

        return AWC.PlayerState.CharacterName + "@" + world.Value.Name.ToString();
    }

    public static bool HasMaxJobLevel(PlayerJob jobType)
    {
        return GetJobLevel(jobType) == Constants.CurrentMaxLevel;
    }

    public static bool HasStatus(uint statusId, float minTime = 0)
    {
        return Player.Available && Player.Status.Any(x => x.StatusId == statusId && (minTime <= 0 || x.RemainingTime > minTime));
    }

    public static int GetJobLevel(PlayerJob jobType)
    {
        if (!AWC.PlayerState.IsLoaded) {
            return 0;
        }

        try {
            unsafe {
                return PlayerState.Instance()->GetClassJobLevel((int)jobType);
            }
        } catch (Exception) {
            return 0;
        }
    }

    public static PlayerJob GetCurrentJob()
    {
        if (!AWC.PlayerState.IsLoaded) {
            return PlayerJob.None;
        }

        return (PlayerJob)AWC.PlayerState.ClassJob.RowId;
    }

    public static CharacterSwapStatus SwitchJob(PlayerJob targetJobId)
    {
        return SwitchJob((uint)targetJobId);
    }

    public static CharacterSwapStatus SwitchJob(uint targetJobId)
    {
        if (!Player.Available) {
            return CharacterSwapStatus.FailedToSwitchJob;
        }

        var currentJobId = Player.ClassJob.RowId;
        if (currentJobId == targetJobId) {
            return CharacterSwapStatus.AlreadyOnTargetJob;
        }

        unsafe {
            var gearsetModule = RaptureGearsetModule.Instance();
            if (gearsetModule == null) {
                return CharacterSwapStatus.FailedToSwitchJob;
            }

            for (byte i = 0; i < 100; i++) {
                if (!gearsetModule->IsValidGearset(i) || gearsetModule->GetGearset(i)->ClassJob != targetJobId) {
                    continue;
                }

                ChatHelper.RunCommand($"gs change {i + 1}");
                return CharacterSwapStatus.SwitchedJob;
            }
        }

        var targetJob = (PlayerJob)targetJobId;
        if (targetJob != PlayerJob.None && targetJob != targetJob.GetEarlyJob()) {
            return SwitchJob(targetJob.GetEarlyJob());
        }

        return CharacterSwapStatus.FailedToSwitchJob;
    }

    public static void UpdateJobLevelsForCurrentCharacter()
    {
        var character = GetFullCharacterName();
        if (character == null) {
            return;
        }

        var options = AWC.Config.GetOrRegisterCharacterOptions(character);
        if (options == null) {
            return;
        }

        var changed = false;

        foreach (var job in PlayerJobExtensions.GetSelectableCombatJobs()) {
            if (job == PlayerJob.None) {
                continue;
            }

            var level = GetJobLevel(job);
            if (options.JobLevels.TryGetValue(job, out var existing) && existing == level) {
                continue;
            }

            options.JobLevels[job] = level;
            changed = true;
        }

        if (changed) {
            Configuration.Save();
        }
    }
}

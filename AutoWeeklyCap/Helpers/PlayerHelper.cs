using Dalamud.Game.ClientState.Conditions;
using Dalamud.Utility;
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
    public static bool IsJumping => Svc.Condition.Any() && (Svc.Condition[ConditionFlag.Jumping] || Svc.Condition[ConditionFlag.Jumping61]);

    public static bool CanSelfRepairWithCrafters =>
        HasMaxJobLevel(PlayerJob.CPR) &&
        HasMaxJobLevel(PlayerJob.BSM) &&
        HasMaxJobLevel(PlayerJob.ARM) &&
        HasMaxJobLevel(PlayerJob.GSM) &&
        HasMaxJobLevel(PlayerJob.LTW) &&
        HasMaxJobLevel(PlayerJob.WVR);

    public static string? GetFullCharacterName()
    {
        if (!AWC.PlayerState.IsLoaded)
            return null;

        var world = AWC.PlayerState.HomeWorld.ValueNullable;
        if (world == null)
            return null;

        return AWC.PlayerState.CharacterName + "@" + world.Value.Name.ToString();
    }

    public static bool HasMaxJobLevel(PlayerJob jobType)
    {
        return GetJobLevel(jobType) == AWC.CurrentMaxLevel;
    }

    public static int GetJobLevel(PlayerJob jobType)
    {
        if (!AWC.PlayerState.IsLoaded)
            return 0;

        try
        {
            unsafe
            {
                return PlayerState.Instance()->GetClassJobLevel((int)jobType);
            }
        }
        catch (Exception)
        {
            return 0;
        }
    }

    public static CharacterSwapStatus SwitchJob(uint targetJobId)
    {
        if (!AWC.PlayerState.IsLoaded)
            return CharacterSwapStatus.FailedToSwitchJob;

        var currentJobId = AWC.PlayerState.ClassJob.RowId;
        if (currentJobId == targetJobId)
            return CharacterSwapStatus.AlreadyOnTargetJob;

        unsafe
        {
            var gearsetModule = RaptureGearsetModule.Instance();
            if (gearsetModule == null)
                return CharacterSwapStatus.FailedToSwitchJob;

            for (byte i = 0; i < 100; i++)
            {
                if (!gearsetModule->IsValidGearset(i) || gearsetModule->GetGearset(i)->ClassJob != targetJobId)
                    continue;

                ChatHelper.RunCommand($"gs change {i + 1}");
                return CharacterSwapStatus.SwitchedJob;
            }
        }

        return CharacterSwapStatus.FailedToSwitchJob;
    }
}

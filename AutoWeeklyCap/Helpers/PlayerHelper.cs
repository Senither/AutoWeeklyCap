using System;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Utility;
using ECommons;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using GrandCompany = ECommons.ExcelServices.GrandCompany;

namespace AutoWeeklyCap.Helpers;

public enum CharacterSwapStatus
{
    FailedToSwitchJob = 0,
    SwitchedJob = 1,
    AlreadyOnTargetJob = 2
}

public enum PlayerJobType
{
    Adventurer = 0,
    Gladiator = 1,
    Pugilist = 2,
    Marauder = 3,
    Lancer = 4,
    Archer = 5,
    Conjurer = 6,
    Thaumaturge = 7,
    Carpenter = 8,
    Blacksmith = 9,
    Armorer = 10,
    Goldsmith = 11,
    Leatherworker = 12,
    Weaver = 13,
    Alchemist = 14,
    Culinarian = 15,
    Miner = 16,
    Botanist = 17,
    Fisher = 18,
    Paladin = 19,
    Monk = 20,
    Warrior = 21,
    Dragoon = 22,
    Bard = 23,
    WhiteMage = 24,
    BlackMage = 25,
    Arcanist = 26,
    Summoner = 27,
    Scholar = 28,
    Rogue = 29,
    Ninja = 30,
    Machinist = 31,
    DarkKnight = 32,
    Astrologian = 33,
    Samurai = 34,
    RedMage = 35,
    BlueMage = 36,
    Gunbreaker = 37,
    Dancer = 38,
    Reaper = 39,
    Sage = 40,
    Pictomancer = 42
}

public static class PlayerHelper
{
    internal static bool IsReady => IsValid && !IsOccupied;

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

    public static bool IsJumping => Svc.Condition.Any() && (Svc.Condition[ConditionFlag.Jumping]
                                                            || Svc.Condition[ConditionFlag.Jumping61]);

    public static bool CanSelfRepairWithCrafters =>
        HasMaxJobLevel(PlayerJobType.Carpenter) &&
        HasMaxJobLevel(PlayerJobType.Blacksmith) &&
        HasMaxJobLevel(PlayerJobType.Armorer) &&
        HasMaxJobLevel(PlayerJobType.Goldsmith) &&
        HasMaxJobLevel(PlayerJobType.Leatherworker) &&
        HasMaxJobLevel(PlayerJobType.Weaver);

    public static bool HasMaxJobLevel(PlayerJobType jobType)
    {
        return GetJobLevel(jobType) == AutoWeeklyCap.CurrentMaxLevel;
    }

    public static int GetJobLevel(PlayerJobType jobType)
    {
        if (!AutoWeeklyCap.PlayerState.IsLoaded)
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
        if (!AutoWeeklyCap.PlayerState.IsLoaded)
            return CharacterSwapStatus.FailedToSwitchJob;

        var currentJobId = AutoWeeklyCap.PlayerState.ClassJob.RowId;
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

                Chat.RunCommand($"gs change {i + 1}");
                return CharacterSwapStatus.SwitchedJob;
            }
        }

        return CharacterSwapStatus.FailedToSwitchJob;
    }

    internal static unsafe uint GetGrandCompanyTerritoryType(GrandCompany grandCompany) => grandCompany switch
    {
        GrandCompany.Maelstrom => 128u,
        GrandCompany.TwinAdder => 132u,
        _ => 130u
    };

    internal static unsafe GrandCompany GetGrandCompany() => (GrandCompany)PlayerState.Instance()->GrandCompany;
}

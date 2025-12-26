using System;
using System.Collections.Generic;
using AutoWeeklyCap.IPC;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace AutoWeeklyCap.Runner;

public class Runner
{
    private bool stopGracefully = false;

    private State state = State.Waiting;
    private string? currentCharacter = null;
    private DateTime timestamp;

    public bool Start()
    {
        if (state != State.Waiting || stopGracefully)
            return false;

        var character = Utils.GetFullCharacterName();
        if (character == null)
        {
            StartCharacterSwap();
            return true;
        }

        var options = Plugin.Config.GetOrRegisterCharacterOptions(character);
        if (!options.Enabled)
        {
            StartCharacterSwap();
            return true;
        }

        currentCharacter = character;
        state = State.PreparingRunner;
        timestamp = DateTime.UtcNow;

        Plugin.Log.Debug("Starting weekly cap runner");

        return true;
    }

    public void Stop()
    {
        if (Plugin.Config.StopRunnerGracefully)
        {
            if (state is State.RunningAutoDuty or State.SwitchingCharacter || !AutoDutyIPC.IsStopped())
            {
                stopGracefully = true;
                return;
            }
        }

        Abort();
    }

    public void Abort()
    {
        currentCharacter = null;
        state = State.Waiting;
        timestamp = DateTime.UtcNow;
        stopGracefully = false;

        LifestreamIPC.Abort();
        AutoDutyIPC.Stop();

        Plugin.Log.Debug("Stopped weekly cap runner");
    }

    public bool IsRunning()
    {
        return state != State.Waiting;
    }

    public bool IsStopping()
    {
        return stopGracefully;
    }

    public string GetStatus()
    {
        return state switch
        {
            State.Waiting => "idle",
            State.PreparingRunner => "Preparing runner",
            State.CheckingTomestone => "Checking Tomestone",
            State.StartingAutoDuty => "Starting AutoDuty",
            State.RunningAutoDuty => stopGracefully ? "Stopping when duty finishes" : "Running AutoDuty",
            State.StartingCharacterSwap => "Starting Character Swap",
            State.SwitchingCharacter => "Switching Character to " + currentCharacter,
            _ => "unknown"
        };
    }

    public void Tick()
    {
        switch (state)
        {
            case State.Waiting:
                break;

            case State.PreparingRunner:
                CheckPrerequisitesForRunnerPreparations();
                break;

            case State.CheckingTomestone:
                CheckTomestoneStage();
                break;

            case State.StartingAutoDuty:
                StartAutoDuty();
                break;

            case State.RunningAutoDuty:
                RunAutoDuty();
                break;

            case State.StartingCharacterSwap:
                StartCharacterSwap();
                break;

            case State.SwitchingCharacter:
                SwitchCharacter();
                break;
        }
    }

    private void CheckPrerequisitesForRunnerPreparations()
    {
        if (stopGracefully)
        {
            Abort();
            return;
        }

        state = State.CheckingTomestone;
    }

    private void CheckTomestoneStage()
    {
        var character = Utils.GetFullCharacterName();
        if (character == null)
            return;

        var tomes = Utils.GetWeeklyAcquiredTomestoneCount();

        timestamp = DateTime.UtcNow;
        state = InventoryManager.GetLimitedTomestoneWeeklyLimit() == tomes
                    ? State.StartingCharacterSwap
                    : State.StartingAutoDuty;
    }

    private void StartAutoDuty()
    {
        if (Plugin.ClientState.TerritoryType == Plugin.Config.ZoneId)
        {
            state = State.RunningAutoDuty;

            if (AutoDutyIPC.IsStopped())
                AutoDutyIPC.Run(Plugin.Config.ZoneId, 1, false);

            return;
        }

        Plugin.Log.Debug(
            $"Seconds elapsed: {(DateTime.UtcNow - timestamp).Seconds}, AutoDuty started: {!AutoDutyIPC.IsStopped()}");
        if ((DateTime.UtcNow - timestamp).Seconds > 10)
        {
            Plugin.Log.Debug("Timed out while trying to start AutoDuty");

            if (currentCharacter == null)
            {
                Plugin.Log.Debug("Stopping runner due to character being NULL");
                Stop();
                return;
            }

            Plugin.Log.Debug($"Disabling AWC for {currentCharacter} and switching character");

            Plugin.Config.Characters[currentCharacter].Enabled = false;
            Plugin.Config.Save();

            state = State.StartingCharacterSwap;
            return;
        }

        if (Utils.IsPluginEnabled("BossModReborn"))
            Utils.RunShellCommand("bmrai on");

        Plugin.Log.Debug($"Starting auto duty for ${currentCharacter} in zone ${Plugin.Config.ZoneId}");
        AutoDutyIPC.Run(Plugin.Config.ZoneId, 1, false);
    }

    private void RunAutoDuty()
    {
        if (!AutoDutyIPC.IsStopped())
            return;

        if ((DateTime.UtcNow - timestamp).Seconds > 30)
        {
            Plugin.Log.Debug("Tried to run auto duty but timed out, stopping weekly cap runner");

            Stop();
        }

        Plugin.Log.Debug("AutoDuty has complete a run, switching to checking tomestones");
        state = State.PreparingRunner;
    }

    private void StartCharacterSwap()
    {
        var limit = InventoryManager.GetLimitedTomestoneWeeklyLimit();

        foreach (var (character, option) in Plugin.Config.Characters)
        {
            if (!option.Enabled)
                continue;

            var tomes = Plugin.Config.CollectedTomes.GetValueOrDefault(character, 0);
            if (tomes == limit)
                continue;

            var parts = character.Split("@");
            if (parts.Length != 2)
            {
                Plugin.Log.Error($"Character {character} is not a valid character name, stopping runner");
                Stop();
                return;
            }

            Plugin.Log.Debug($"Switching character to {parts[0]} on {parts[1]}");
            currentCharacter = character;
            state = State.SwitchingCharacter;
            timestamp = DateTime.UtcNow;
            LifestreamIPC.ChangeCharacter(parts[0], parts[1]);

            return;
        }

        Plugin.Log.Debug("Found no character with missing weekly capped tomestones, stopping runner");
        Stop();
    }

    private void SwitchCharacter()
    {
        if (LifestreamIPC.IsBusy())
            return;

        var character = Utils.GetFullCharacterName();
        if (character == null || character != currentCharacter)
            return;

        Plugin.Log.Debug("Completed character swap, checking tomestones");
        state = State.PreparingRunner;
    }
}

using System;
using System.Collections.Generic;
using AutoWeeklyCap.IPC;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace AutoWeeklyCap.Runner;

public class Runner
{
    private Configuration configuration;

    private State state = State.Waiting;
    private string? currentCharacter = null;
    private DateTime timestamp;

    public Runner(Configuration configuration)
    {
        this.configuration = configuration;
    }

    public bool Start()
    {
        if (state != State.Waiting)
            return false;

        var character = Utils.GetFullCharacterName();
        if (character == null)
        {
            StartCharacterSwap();
            return true;
        }

        currentCharacter = character;
        state = State.CheckingTomestone;
        timestamp = DateTime.UtcNow;

        Plugin.Log.Debug("Starting weekly cap runner");

        return true;
    }

    public void Stop()
    {
        currentCharacter = null;
        state = State.Waiting;
        timestamp = DateTime.UtcNow;

        LifestreamIPC.Abort();
        AutoDutyIPC.Stop();

        Plugin.Log.Debug("Stopped weekly cap runner");
    }

    public bool IsRunning()
    {
        return state != State.Waiting;
    }

    public string GetStatus()
    {
        switch (state)
        {
            case State.Waiting:
                return "idle";
            case State.CheckingTomestone:
                return "Checking Tomestone";
            case State.StartingAutoDuty:
                return "Starting AutoDuty";
            case State.RunningAutoDuty:
                return "Running AutoDuty";
            case State.StartingCharacterSwap:
                return "Starting Character Swap";
            case State.SwitchingCharacter:
                return "Switching Character to " + currentCharacter;

            default:
                return "unknown";
        }
    }

    public void Tick()
    {
        switch (state)
        {
            case State.Waiting:
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
        if (Plugin.ClientState.TerritoryType == configuration.ZoneId)
        {
            state = State.RunningAutoDuty;
            AutoDutyIPC.Run(configuration.ZoneId, 1, false);
            return;
        }
        
        Plugin.Log.Debug($"Seconds elapsed: {(DateTime.UtcNow - timestamp).Seconds}, AutoDuty started: {!AutoDutyIPC.IsStopped()}");
        if ((DateTime.UtcNow - timestamp).Seconds > 10)
        {
            Plugin.Log.Debug("Attempting to start auto duty for 10 seconds, stopping runner");
            Stop();
            return;
        }

        Plugin.Log.Debug($"Starting auto duty for ${currentCharacter} in zone ${configuration.ZoneId}");
        AutoDutyIPC.Run(configuration.ZoneId, 1, false);
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
        state = State.CheckingTomestone;
    }

    private void StartCharacterSwap()
    {
        var limit = InventoryManager.GetLimitedTomestoneWeeklyLimit();

        foreach (var character in configuration.Characters)
        {
            var tomes = configuration.CollectedTomes.GetValueOrDefault(character, 0);
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
        state = State.CheckingTomestone;
    }
}

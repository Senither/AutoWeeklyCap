using System;
using System.Collections.Generic;
using AutoWeeklyCap.Actions;
using AutoWeeklyCap.Helpers;
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

        var zoneName = Utils.GetZoneNameFromId(AutoWeeklyCap.Config.ZoneId);
        if (zoneName == null)
            return false;

        var character = Utils.GetFullCharacterName();
        if (character == null || !AutoWeeklyCap.Config.GetOrRegisterCharacterOptions(character).IsEnabled())
        {
            StartCharacterSwap();
            return true;
        }

        currentCharacter = character;
        state = State.PreparingRunner;
        timestamp = DateTime.UtcNow;

        AutoWeeklyCap.Log.Debug("Starting weekly cap runner");

        return true;
    }

    public void Stop()
    {
        if (AutoWeeklyCap.Config.StopRunnerGracefully)
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
        AutoWeeklyCap.TaskManager.Abort();

        AutoWeeklyCap.Log.Debug("Stopped weekly cap runner");
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
            State.StoppingRunner => "Stopping Runner",
            _ => "unknown"
        };
    }

    public void Tick()
    {
        if (AutoWeeklyCap.TaskManager.IsBusy)
            return;

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

            case State.StoppingRunner:
                StopRunner();
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }

    private void CheckPrerequisitesForRunnerPreparations()
    {
        if (stopGracefully)
        {
            Abort();
            return;
        }

        if (currentCharacter == null)
        {
            AutoWeeklyCap.Log.Debug("Stopping runner due to character being NULL");
            Stop();
            return;
        }

        AutoWeeklyCap.TaskManager.Enqueue(() => AutoWeeklyCap.Config.GetOrRegisterCharacterOptions(currentCharacter)
                                                             .PreferredJob.SwitchToJob(),
                                          "switch to preferred job"
        );

        AutoWeeklyCap.TaskManager.Enqueue(
            () => state = State.CheckingTomestone,
            "next stage: checking tomestone"
        );
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
        AutoWeeklyCap.Log.Debug("Attempting to start auto duty: {@Stats}", new Dictionary<string, object>
        {
            { "Seconds elapsed", (DateTime.UtcNow - timestamp).Seconds },
            { "AutoDuty started", !AutoDutyIPC.IsStopped() },
            { "Current zone", AutoWeeklyCap.ClientState.TerritoryType },
            { "Duty zone", AutoWeeklyCap.Config.ZoneId },
        });
        
        if (AutoWeeklyCap.ClientState.TerritoryType == AutoWeeklyCap.Config.ZoneId)
        {
            state = State.RunningAutoDuty;

            if (AutoDutyIPC.IsStopped())
                AutoDutyIPC.Run(AutoWeeklyCap.Config.ZoneId, 1, false);

            return;
        }

        if ((DateTime.UtcNow - timestamp).Seconds > 15)
        {
            AutoWeeklyCap.Log.Debug("Timed out while trying to start AutoDuty");

            if (currentCharacter == null)
            {
                AutoWeeklyCap.Log.Debug("Stopping runner due to character being NULL");
                Stop();
                return;
            }

            AutoWeeklyCap.Log.Debug($"Disabling AWC for {currentCharacter} and switching character");

            AutoWeeklyCap.Config.Characters[currentCharacter].Enabled = false;
            AutoWeeklyCap.Config.Save();

            state = State.StartingCharacterSwap;
            return;
        }

        if (AutoWeeklyCap.Config.UseBossModRebornAI && BossModReborn.IsEnabled)
        {
            AutoWeeklyCap.Log.Debug("UseBossModRebornAI is enabled and BossMod Reborn is disabled, enabling AI");
            Chat.RunCommand("bmrai on");
        }

        AutoWeeklyCap.Log.Debug($"Starting auto duty for ${currentCharacter} in zone ${AutoWeeklyCap.Config.ZoneId}");
        AutoDutyIPC.Run(AutoWeeklyCap.Config.ZoneId, 1, false);
    }

    private void RunAutoDuty()
    {
        if (!AutoDutyIPC.IsStopped())
            return;

        if (AutoWeeklyCap.ClientState.TerritoryType == AutoWeeklyCap.Config.ZoneId)
            return;

        AutoWeeklyCap.Log.Debug("AutoDuty has complete a run, switching to preparations stage");
        state = State.PreparingRunner;
    }

    private void StartCharacterSwap()
    {
        var limit = InventoryManager.GetLimitedTomestoneWeeklyLimit();

        foreach (var (character, option) in AutoWeeklyCap.Config.Characters)
        {
            if (!option.IsEnabled())
                continue;

            var tomes = AutoWeeklyCap.Config.CollectedTomes.GetValueOrDefault(character, 0);
            if (tomes == limit)
                continue;

            var parts = character.Split("@");
            if (parts.Length != 2)
            {
                AutoWeeklyCap.Log.Error($"Character {character} is not a valid character name, stopping runner");
                Stop();
                return;
            }

            AutoWeeklyCap.Log.Debug($"Switching character to {parts[0]} on {parts[1]}");
            currentCharacter = character;
            state = State.SwitchingCharacter;
            timestamp = DateTime.UtcNow;
            LifestreamIPC.ChangeCharacter(parts[0], parts[1]);

            return;
        }

        AutoWeeklyCap.Log.Debug("Found no character with missing weekly capped tomestones, stopping runner");
        state = State.StoppingRunner;
    }

    private void SwitchCharacter()
    {
        if (LifestreamIPC.IsBusy())
            return;

        var character = Utils.GetFullCharacterName();
        if (character == null || character != currentCharacter)
            return;

        AutoWeeklyCap.Log.Debug("Completed character swap, checking tomestones");
        state = State.PreparingRunner;
    }

    private void StopRunner()
    {
        Abort();

        AutoWeeklyCap.TaskManager.EnqueueDelay(500);
        AutoWeeklyCap.TaskManager.Enqueue(
            () => AutoWeeklyCap.Config.StopAction.Execute(),
            "executing stop action"
        );
    }
}

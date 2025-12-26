using System;
using System.Reflection;
using AutoWeeklyCap.Config;
using AutoWeeklyCap.UI.Windows;
using AutoWeeklyCap.Windows;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ECommons;
using Newtonsoft.Json;
using Module = ECommons.Module;

namespace AutoWeeklyCap;

public sealed class Plugin : IDalamudPlugin
{
    internal static string Name = "Auto Weekly Cap";
    internal static Plugin Instance;
    internal static Configuration Config => Instance.Configuration;
    public static string Version => Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";
    

    [PluginService]
    internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;

    [PluginService]
    internal static ICommandManager CommandManager { get; private set; } = null!;

    [PluginService]
    internal static IFramework Framework { get; private set; } = null!;

    [PluginService]
    internal static IClientState ClientState { get; private set; } = null!;

    [PluginService]
    internal static IPlayerState PlayerState { get; private set; } = null!;

    [PluginService]
    internal static IDataManager DataManager { get; private set; } = null!;

    [PluginService]
    internal static IPluginLog Log { get; private set; } = null!;

    public static Runner.Runner Runner { get; set; }

    private const string CommandName = "/awc";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("AutoWeeklyCap");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }
    private FrameworkListener FrameworkListener { get; init; } = new();

    public Plugin()
    {
        Instance = this;

        ECommonsMain.Init(PluginInterface, this, Module.DalamudReflector);

        try
        {
            Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        }
        catch (Exception e)
        {
            if (e is JsonSerializationException or AggregateException)
                Configuration = new Configuration();
            else
                throw;
        }

        Runner = new Runner.Runner();

        ConfigWindow = new ConfigWindow();
        MainWindow = new MainWindow(this);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Toggles the Auto Weekly Cap main window"
        });

        Framework.Update += FrameworkListener.OnFrameworkUpdate;

        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUi;

        MainWindow.Toggle();
    }

    public void Dispose()
    {
        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi -= ToggleMainUi;

        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args)
    {
        MainWindow.Toggle();
    }

    public void ToggleConfigUi() => ConfigWindow.Toggle();
    public void ToggleMainUi() => MainWindow.Toggle();
}

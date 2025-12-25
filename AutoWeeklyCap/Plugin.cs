using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using AutoWeeklyCap.UI.Windows;
using AutoWeeklyCap.Windows;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ECommons;

namespace AutoWeeklyCap;

public sealed class Plugin : IDalamudPlugin
{
    internal static Plugin Instance;
    internal static Configuration Config => Instance.Configuration;
    
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
    private FrameworkListener FrameworkListener { get; init; }

    public Plugin()
    {
        Instance = this;
        
        ECommonsMain.Init(PluginInterface, this, Module.DalamudReflector);

        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Runner = new(Configuration);

        FrameworkListener = new(this);

        ConfigWindow = new ConfigWindow(this);
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

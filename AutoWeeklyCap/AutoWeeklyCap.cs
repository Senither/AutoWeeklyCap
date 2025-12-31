using System;
using System.Reflection;
using System.Threading.Tasks;
using AutoWeeklyCap.Commands;
using AutoWeeklyCap.Config;
using AutoWeeklyCap.UI.Windows;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ECommons;
using ECommons.Automation.NeoTaskManager;
using Newtonsoft.Json;
using Module = ECommons.Module;

namespace AutoWeeklyCap;

public sealed class AutoWeeklyCap : IDalamudPlugin
{
    internal const string Name = "Auto Weekly Cap";
    internal static AutoWeeklyCap Instance = null!;
    internal static Configuration Config => Instance.Configuration;
    internal static Runner.Runner Runner { get; set; } = null!;
    internal static TaskManager TaskManager { get; set; } = null!;
    internal static string Version => Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";

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

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("AutoWeeklyCap");
    private MainWindow MainWindow { get; init; }
    private ConfigWindow ConfigWindow { get; init; }
    private CharacterOptionWindow CharacterOptionWindow { get; init; }
    private FrameworkListener FrameworkListener { get; init; } = new();

    private const string CommandNameShort = "/awc";
    private const string CommandNameLong = "/autoweeklycap";

    public AutoWeeklyCap()
    {
        Instance = this;

        ECommonsMain.Init(PluginInterface, this, Module.DalamudReflector);

        TaskManager =
            new TaskManager(new TaskManagerConfiguration(abortOnTimeout: true, timeLimitMS: 20000, showDebug: true));

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
        CharacterOptionWindow = new CharacterOptionWindow();

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);
        WindowSystem.AddWindow(CharacterOptionWindow);

        CommandManager.AddHandler(CommandNameLong, new CommandInfo(OnCommand)
        {
            HelpMessage = "Toggles the Auto Weekly Cap main window",
            ShowInHelp = true,
        });

        CommandManager.AddHandler(CommandNameShort, new CommandInfo(OnCommand)
        {
            ShowInHelp = false,
        });

        Framework.Update += FrameworkListener.OnFrameworkUpdate;

        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUi;

#if DEBUG
        ToggleMainUi();
        ToggleConfigUi();
#endif
    }

    public void Dispose()
    {
        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi -= ToggleMainUi;

        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();

        TaskManager.Dispose();

        ECommonsMain.Dispose();

        CommandManager.RemoveHandler(CommandNameShort);
        CommandManager.RemoveHandler(CommandNameLong);
    }

    private static void OnCommand(string command, string args)
    {
        CommandHandler.HandleCommand(args.Split(" "));
    }

    public void ToggleConfigUi() => ConfigWindow.Toggle();
    public void ToggleMainUi() => MainWindow.Toggle();

    public void OpenCharacterOptionsUi(string character) =>
        CharacterOptionWindow.ToggleForCharacterWithOptions(character);
}

using System.Reflection;

using AutoWeeklyCap.Commands;
using AutoWeeklyCap.Config;
using AutoWeeklyCap.IPC.Wotsit;
using AutoWeeklyCap.Listeners;
using AutoWeeklyCap.UI.Dtr;
using AutoWeeklyCap.UI.Windows;

using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;

using ECommons.Automation.NeoTaskManager;
using ECommons.Configuration;
using ECommons.Schedulers;

using Newtonsoft.Json;

using Module = ECommons.Module;

namespace AutoWeeklyCap;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class AutoWeeklyCap : IDalamudPlugin
{
    internal static AWC Instance = null!;

    internal static Configuration Config => Instance.Configuration;
    internal static Runner.Runner Runner { get; private set; } = null!;
    internal static TaskManager TaskManager { get; private set; } = null!;
    internal static string Version => Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";

    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IFramework Framework { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IPlayerState PlayerState { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IDtrBar DtrBar { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;

    public DtrStatusBar DtrStatusBar { get; } = new();
    public Configuration Configuration { get; set; }

    private MainWindow MainWindow { get; }
    private ControlPanelWindow ControlPanelWindow { get; }
    private CharacterOptionWindow CharacterOptionWindow { get; }
    private StatusOverlayWindow StatusOverlayWindow { get; }
    private FeedbackWindow FeedbackWindow { get; }

    private readonly WindowSystem _windowSystem = new("AutoWeeklyCap");

    public AutoWeeklyCap()
    {
        Instance = this;

        ECommonsMain.Init(PluginInterface, this, Module.DalamudReflector);

        TaskManager = new TaskManager(new TaskManagerConfiguration(abortOnTimeout: true, timeLimitMS: 20000, showDebug: true));
        TomestoneItemHelper.RegisterTomestoneItems();

        EzConfig.Migrate<Configuration>();
        Configuration = EzConfig.Init<Configuration>();
        Configuration.NormalizeCharacterPositions();
        Configuration.NormalizeSafezoneOrder();

        Runner = new Runner.Runner();

        _windowSystem.AddWindow(ControlPanelWindow = new ControlPanelWindow());
        _windowSystem.AddWindow(MainWindow = new MainWindow(this));
        _windowSystem.AddWindow(CharacterOptionWindow = new CharacterOptionWindow());
        _windowSystem.AddWindow(StatusOverlayWindow = new StatusOverlayWindow());
        _windowSystem.AddWindow(FeedbackWindow = new FeedbackWindow());

        CommandManager.AddHandler(Constants.CommandNameLong, new CommandInfo(OnCommand) { HelpMessage = "Toggles the Auto Weekly Cap main window", ShowInHelp = true });
        CommandManager.AddHandler(Constants.CommandNameShort, new CommandInfo(OnCommand) { ShowInHelp = false });

        Framework.Update += FrameworkListener.OnFrameworkUpdate;
        ClientState.Logout += ClientListener.OnLogout;

        PluginInterface.UiBuilder.Draw += _windowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUi;

        Log.Debug($"AWC#Startup - OpenWindowOnStartup: {Config.OpenWindowOnStartup}");
        if (Config.OpenWindowOnStartup) {
            OpenMainUi();
        }

#if DEBUG
        OpenMainUi();
        OpenConfigUi();
#endif

        _ = new TickScheduler(() =>
        {
            DtrStatusBar.Start();

            Log.Debug($"AWC#Startup - StartRunnerOnBoot: {Config.StartRunnerOnBoot}");
            if (Config.StartRunnerOnBoot) {
                Runner.AutoStartOnBoot();
            }

            WotsitIPC.Manager.InitializeWotsit("AWC initialization");
        });
    }

    public void Dispose()
    {
        PluginInterface.UiBuilder.Draw -= _windowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi -= ToggleMainUi;

        Framework.Update -= FrameworkListener.OnFrameworkUpdate;
        ClientState.Logout -= ClientListener.OnLogout;

        _windowSystem.RemoveAllWindows();

        DtrStatusBar.Dispose();
        TaskManager.Dispose();

        ECommonsMain.Dispose();
        IPCSubscriber.Dispose();

        CommandManager.RemoveHandler(Constants.CommandNameShort);
        CommandManager.RemoveHandler(Constants.CommandNameLong);
    }

    private static void OnCommand(string command, string args)
    {
        CommandHandler.HandleCommand(args);
    }

    public void ToggleConfigUi()
    {
        ControlPanelWindow.Toggle();
    }

    public void OpenConfigUi(SettingsWindowOption? option = null)
    {
        ControlPanelWindow.IsOpen = true;

        if (option != null) {
            ControlPanelWindow.SetCurrentTab(option.Value);
        }
    }

    public bool IsConfigUiOpen()
    {
        return ControlPanelWindow.IsOpen;
    }

    public void ToggleMainUi()
    {
        MainWindow.Toggle();
    }

    public void OpenMainUi()
    {
        MainWindow.IsOpen = true;
    }

    public void ToggleFeedbackUi()
    {
        FeedbackWindow.Toggle();
    }

    public void OpenCharacterOptionsUi(string character)
    {
        CharacterOptionWindow.ToggleForCharacterWithOptions(character);
    }

    public static bool IsRequiredPluginsEnabled()
    {
        return LifestreamIPC.IsEnabled && AutoDutyIPC.IsEnabled && VNavMeshIPC.IsEnabled;
    }
}

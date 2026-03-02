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
using ECommons.Schedulers;
using Newtonsoft.Json;
using Module = ECommons.Module;

namespace AutoWeeklyCap;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class AutoWeeklyCap : IDalamudPlugin
{
    internal const string Name = "Auto Weekly Cap";
    internal const string InternalName = "AutoWeeklyCap";

    internal const string CommandNameShort = "/awc";
    internal const string CommandNameLong = "/autoweeklycap";

    internal static AWC Instance = null!;
    internal const int CurrentMaxLevel = 100;

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
    internal static IDtrBar DtrBar { get; private set; } = null!;

    [PluginService]
    internal static IPluginLog Log { get; private set; } = null!;

    public DtrStatusBar DtrStatusBar { get; init; } = new();

    public Configuration Configuration { get; set; }

    public readonly WindowSystem WindowSystem = new("AutoWeeklyCap");

    private MainWindow MainWindow { get; init; }
    private ConfigWindow ConfigWindow { get; init; }
    private CharacterOptionWindow CharacterOptionWindow { get; init; }
    private FeedbackWindow FeedbackWindow { get; init; }
    private FrameworkListener FrameworkListener { get; init; } = new();

    public AutoWeeklyCap()
    {
        Instance = this;

        ECommonsMain.Init(PluginInterface, this, Module.DalamudReflector);

        TaskManager = new TaskManager(new TaskManagerConfiguration(abortOnTimeout: true, timeLimitMS: 20000, showDebug: true));
        TomestoneItemHelper.RegisterTomestoneItems();

        try
        {
            Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Configuration.NormalizeCharacterPositions();
        }
        catch (Exception e)
        {
            if (e is JsonSerializationException or AggregateException)
                Configuration = new Configuration();
            else
                throw;
        }

        DtrStatusBar.Start();

        Runner = new Runner.Runner();

        WindowSystem.AddWindow(ConfigWindow = new ConfigWindow());
        WindowSystem.AddWindow(MainWindow = new MainWindow(this));
        WindowSystem.AddWindow(CharacterOptionWindow = new CharacterOptionWindow());
        WindowSystem.AddWindow(FeedbackWindow = new FeedbackWindow());

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
        ClientState.Logout += ClientListener.OnLogout;

        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUi;

        Log.Debug($"AWC#Startup - OpenWindowOnStartup: {Config.OpenWindowOnStartup}");
        if (Config.OpenWindowOnStartup)
            OpenMainUi();

        Log.Debug($"AWC#Startup - StartRunnerOnBoot: {Config.StartRunnerOnBoot}");
        if (Config.StartRunnerOnBoot)
        {
            _ = new TickScheduler(() => Runner.AutoStartOnBoot());
        }

        WotsitIPC.Manager.InitializeWotsit("AWC initialization");

#if DEBUG
        OpenMainUi();
        OpenConfigUi();
#endif
    }

    public void Dispose()
    {
        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi -= ToggleMainUi;

        Framework.Update -= FrameworkListener.OnFrameworkUpdate;
        ClientState.Logout -= ClientListener.OnLogout;

        WindowSystem.RemoveAllWindows();

        DtrStatusBar.Dispose();
        TaskManager.Dispose();

        ECommonsMain.Dispose();
        IPCSubscriber.Dispose();

        CommandManager.RemoveHandler(CommandNameShort);
        CommandManager.RemoveHandler(CommandNameLong);
    }

    private static void OnCommand(string command, string args)
    {
        CommandHandler.HandleCommand(args);
    }

    public void ToggleConfigUi() => ConfigWindow.Toggle();
    public void OpenConfigUi() => ConfigWindow.IsOpen = true;
    public void ToggleMainUi() => MainWindow.Toggle();
    public void OpenMainUi() => MainWindow.IsOpen = true;
    public void ToggleFeedbackUi() => FeedbackWindow.Toggle();

    public void OpenCharacterOptionsUi(string character) =>
        CharacterOptionWindow.ToggleForCharacterWithOptions(character);

    public static bool IsRequiredPluginsEnabled()
    {
        return LifestreamIPC.IsEnabled && AutoDutyIPC.IsEnabled;
    }
}

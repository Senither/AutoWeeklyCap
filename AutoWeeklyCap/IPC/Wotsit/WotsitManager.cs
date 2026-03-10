using Dalamud.Plugin.Ipc;

using ECommons.Events;

namespace AutoWeeklyCap.IPC.Wotsit;

public class WotsitManager : IDisposable
{
    private readonly Dictionary<string, WotsitEntry> _registered = [];
    private HashSet<WotsitEntry> _lastEntries = [];

    private readonly ICallGateSubscriber<bool>? _faAvailable;
    private readonly ICallGateSubscriber<string, bool>? _faInvoke;

    public WotsitManager()
    {
        _faAvailable = Svc.PluginInterface.GetIpcSubscriber<bool>("FA.Available");
        _faAvailable.Subscribe(OnAvailable);
        _faInvoke = Svc.PluginInterface.GetIpcSubscriber<string, bool>("FA.Invoke");
        _faInvoke.Subscribe(HandleInvoke);

        ProperOnLogin.RegisterAvailable(OnLogin, true);
        Svc.ClientState.TerritoryChanged += OnTerritoryChanged;
        Svc.ClientState.Logout += OnLogout;
    }

    public void Dispose()
    {
        ClearWotsit();

        _faAvailable?.Unsubscribe(OnAvailable);
        _faInvoke?.Unsubscribe(HandleInvoke);

        ProperOnLogin.Unregister(OnLogin);
        Svc.ClientState.TerritoryChanged -= OnTerritoryChanged;
        Svc.ClientState.Logout -= OnLogout;

        GC.SuppressFinalize(this);
    }

    private void OnAvailable()
    {
        InitializeWotsit("FA.Available");
    }

    private void OnLogin()
    {
        InitializeWotsit("ProperOnLogin.Available");
    }

    private void OnTerritoryChanged(ushort territory)
    {
        InitializeWotsit("OnTerritoryChanged");
    }

    private void OnLogout(int type, int code)
    {
        ClearWotsit();
    }

    public void InitializeWotsit(string trigger)
    {
        if (!WotsitIPC.IsEnabled) {
            ClearWotsit();
            return;
        }

        AWC.Log.Debug($"Initializing WotsitManager triggered by: {trigger}, status: {WotsitIPC.IsEnabled}");

        var newEntries = WotsitEntryGenerator.Generate().ToHashSet();
        if (_lastEntries.Count != 0 && newEntries.SetEquals(_lastEntries)) {
            AWC.Log.Debug("WotsitManager: Entries have not changed, skipping re-registration");
            return;
        }

        ClearWotsit();

        var faRegisterWithSearch = Svc.PluginInterface.GetIpcSubscriber<string, string, string, uint, string>("FA.RegisterWithSearch");

        _lastEntries = newEntries;
        foreach (var entry in newEntries) {
            var id = faRegisterWithSearch.InvokeFunc(AWC.Name, entry.DisplayName, $"{AWC.Name} {entry.SearchString}", entry.IconId);
            _registered.Add(id, entry);

            AWC.Log.Debug($"WotsitManager: Invoked FA.RegisterWithSearch(\"{AWC.Name}\", \"{entry.DisplayName}\", \"{entry.SearchString}\", {entry.IconId}) => {id}");
        }
    }

    private void ClearWotsit()
    {
        try {
            if (!WotsitIPC.IsEnabled) {
                return;
            }

            Svc.PluginInterface.GetIpcSubscriber<string, bool>("FA.UnregisterAll")
                .InvokeFunc(AWC.Name);

            AWC.Log.Debug($"WotsitManager: Invoked FA.UnregisterAll(\"{AWC.Name}\")");
        } catch (Exception e) {
            AWC.Log.Warning($"WotsitManager: Failed to clear wotsit: {e}");
        } finally {
            _registered.Clear();
            _lastEntries.Clear();
        }
    }

    private void HandleInvoke(string id)
    {
        if (!_registered.TryGetValue(id, out var entry)) {
            return;
        }

        AWC.Log.Debug($"WotsitManager: Received FA.Invoke(\"{id}\") => {entry.DisplayName}");

        try {
            entry.Callback.DynamicInvoke();
        } catch (Exception e) {
            AWC.Log.Error($"WotsitManager: Could not handle FA.Invoke(\"{id}\") ({entry.DisplayName}): {e}");
        }
    }
}

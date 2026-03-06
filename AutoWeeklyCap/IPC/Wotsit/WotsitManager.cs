using Dalamud.Plugin.Ipc;

using ECommons.Events;

namespace AutoWeeklyCap.IPC.Wotsit;

public class WotsitManager : IDisposable
{
    private readonly Dictionary<string, WotsitEntry> registered = [];
    private HashSet<WotsitEntry> lastEntries = [];

    private readonly ICallGateSubscriber<bool> faAvailable;
    private readonly ICallGateSubscriber<string, bool> faInvoke;

    public WotsitManager()
    {
        faAvailable = Svc.PluginInterface.GetIpcSubscriber<bool>("FA.Available");
        faAvailable.Subscribe(OnAvailable);
        faInvoke = Svc.PluginInterface.GetIpcSubscriber<string, bool>("FA.Invoke");
        faInvoke.Subscribe(HandleInvoke);

        ProperOnLogin.RegisterAvailable(OnLogin, true);
        Svc.ClientState.TerritoryChanged += OnTerritoryChanged;
        Svc.ClientState.Logout += OnLogout;
    }

    public void Dispose()
    {
        ClearWotsit();

        faAvailable?.Unsubscribe(OnAvailable);
        faInvoke?.Unsubscribe(HandleInvoke);

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
        if (lastEntries.Count != 0 && newEntries.SetEquals(lastEntries)) {
            AWC.Log.Debug("WotsitManager: Entries have not changed, skipping re-registration");
            return;
        }

        ClearWotsit();

        var faRegisterWithSearch = Svc.PluginInterface.GetIpcSubscriber<string, string, string, uint, string>("FA.RegisterWithSearch");

        lastEntries = newEntries;
        foreach (var entry in newEntries) {
            var id = faRegisterWithSearch!.InvokeFunc(AWC.Name, entry.DisplayName, $"{AWC.Name} {entry.SearchString}", entry.IconId);
            registered.Add(id, entry);

            AWC.Log.Debug($"WotsitManager: Invoked FA.RegisterWithSearch(\"{AWC.Name}\", \"{entry.DisplayName}\", \"{entry.SearchString}\", {entry.IconId}) => {id}");
        }
    }

    public void ClearWotsit()
    {
        try {
            if (!WotsitIPC.IsEnabled) {
                return;
            }

            var faUnregisterAll = Svc.PluginInterface.GetIpcSubscriber<string, bool>("FA.UnregisterAll");
            faUnregisterAll!.InvokeFunc(AWC.Name);

            AWC.Log.Debug($"WotsitManager: Invoked FA.UnregisterAll(\"{AWC.Name}\")");
        } catch (Exception e) {
            AWC.Log.Warning($"WotsitManager: Failed to clear wotsit: {e}");
        } finally {
            registered.Clear();
            lastEntries.Clear();
        }
    }

    private void HandleInvoke(string id)
    {
        if (!registered.TryGetValue(id, out var entry)) {
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

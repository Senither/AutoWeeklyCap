namespace AutoWeeklyCap.IPC.Wotsit;

public class WotsitEntry(string displayName, string searchString, uint iconId, Delegate callback)
{
    public string DisplayName { get; init; } = displayName;
    public string SearchString { get; init; } = searchString;
    public uint IconId { get; init; } = iconId;
    public Delegate Callback { get; init; } = callback;

    public override int GetHashCode() => HashCode.Combine(DisplayName, SearchString, IconId);
    public override bool Equals(object? obj) => obj is WotsitEntry entry && Equals(entry);
    public bool Equals(WotsitEntry other) => DisplayName == other.DisplayName && SearchString == other.SearchString && IconId == other.IconId;

    public override string ToString() => $"{GetType().Name}(\"{DisplayName}\", \"{SearchString}\", {IconId})";
}

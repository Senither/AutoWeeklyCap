namespace AutoWeeklyCap.IPC.Wotsit;

public class WotsitEntry(string displayName, string searchString, uint iconId, Delegate callback)
{
    public string DisplayName { get; } = displayName;
    public string SearchString { get; } = searchString;
    public uint IconId { get; } = iconId;
    public Delegate Callback { get; } = callback;

    public override int GetHashCode()
    {
        return HashCode.Combine(DisplayName, SearchString, IconId);
    }

    public override bool Equals(object? obj)
    {
        return obj is WotsitEntry entry && Equals(entry);
    }

    public bool Equals(WotsitEntry other)
    {
        return DisplayName == other.DisplayName && SearchString == other.SearchString && IconId == other.IconId;
    }

    public override string ToString()
    {
        return $"{GetType().Name}(\"{DisplayName}\", \"{SearchString}\", {IconId})";
    }
}

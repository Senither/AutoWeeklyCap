namespace AutoWeeklyCap.IPC.Wotsit;

public class WotsitEntry(string displayName, string searchString, uint iconId, Delegate callback)
{
    public string DisplayName { get; init; } = displayName;
    public string SearchString { get; init; } = searchString;
    public uint IconId { get; init; } = iconId;
    public Delegate Callback { get; init; } = callback;

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

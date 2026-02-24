using System.Diagnostics.CodeAnalysis;
using System.Reflection;

// ReSharper disable ConvertToConstant.Global
// ReSharper disable InconsistentNaming

namespace AutoWeeklyCap.IPC.AutoRetainer;

[Serializable]
[Obfuscation(Exclude = true, ApplyToMembers = true)]
public class OfflineCharacterData
{
    public ulong CID = 0;
    public string Name = "Unknown";
    public string World = "";

    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    public bool ShouldSerializeIdentity() => false;

    public override string ToString()
    {
        return $"{Name}@{World}";
    }
}

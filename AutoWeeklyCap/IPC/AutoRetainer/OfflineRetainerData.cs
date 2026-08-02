using System.Reflection;

// ReSharper disable ConvertToConstant.Global
// ReSharper disable InconsistentNaming

namespace AutoWeeklyCap.IPC.AutoRetainer;

[Serializable]
[Obfuscation(Exclude = true, ApplyToMembers = true)]
public class OfflineRetainerData
{
    public string Name = "";
    public long VentureEndsAt = 0;
    public bool HasVenture = false;
}

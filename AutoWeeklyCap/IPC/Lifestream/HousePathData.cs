using System.Reflection;

// ReSharper disable InconsistentNaming
// ReSharper disable UnassignedField.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace AutoWeeklyCap.IPC.Lifestream;

public class HousePathData
{
    [Obfuscation] public int ResidentialDistrict;
    [Obfuscation] public int Ward;
    [Obfuscation] public int Plot;
    [Obfuscation] public bool IsPrivate;
    [Obfuscation] public ulong CID;
}

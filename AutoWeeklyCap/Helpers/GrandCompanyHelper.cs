using ECommons.ExcelServices;
using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace AutoWeeklyCap.Helpers;

public static class GrandCompanyHelper
{
    internal static unsafe GrandCompany GetGrandCompany() => (GrandCompany)PlayerState.Instance()->GrandCompany;

    internal static uint TerritoryId => GetGrandCompany() switch
    {
        GrandCompany.Maelstrom => 128u,
        GrandCompany.TwinAdder => 132u,
        _ => 130u
    };

    internal static string AetheriteName => GetGrandCompany() switch
    {
        GrandCompany.Maelstrom => "The Aftcastle",
        GrandCompany.TwinAdder => "New Gridania",
        _ => "Steps of Nald",
    };

    internal static Vector3 TurnInLocation => GetGrandCompany() switch
    {
        GrandCompany.Maelstrom => new Vector3(94.02527f, 40.275368f, 75.61174f),
        GrandCompany.TwinAdder => new Vector3(-67.994354f, -0.50152725f, -8.873131f),
        _ => new Vector3(-142.4761f, 4.0999994f, -106.80103f),
    };

    internal static Vector3 RepairVendorLocation => GetGrandCompany() switch
    {
        GrandCompany.Maelstrom => new Vector3(17.715698f, 40.200005f, 3.9520264f),
        GrandCompany.TwinAdder => new Vector3(24.826416f, -8f, 93.18677f),
        _ => new Vector3(32.85266f, 6.999999f, -81.31531f),
    };

    internal static uint RepairVendorId => GetGrandCompany() switch
    {
        GrandCompany.Maelstrom => 1003251u,
        GrandCompany.TwinAdder => 1000394u,
        _ => 1004416u,
    };
}

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

    internal static uint InnTerritoryId => GetGrandCompany() switch
    {
        GrandCompany.Maelstrom => 177u,
        GrandCompany.TwinAdder => 179u,
        _ => 178u
    };

    internal static uint InnDoorId => GetGrandCompany() switch
    {
        GrandCompany.Maelstrom => 2001010u,
        GrandCompany.TwinAdder => 2000087u,
        _ => 2001011u
    };

    internal static uint InnVendorId => GetGrandCompany() switch
    {
        GrandCompany.Maelstrom => 1000974u,
        GrandCompany.TwinAdder => 1000102u,
        _ => 1001976u
    };

    internal static uint RepairVendorId => GetGrandCompany() switch
    {
        GrandCompany.Maelstrom => 1003251u,
        GrandCompany.TwinAdder => 1000394u,
        _ => 1004416u,
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

    internal static Vector3 InnVendorLocation => GetGrandCompany() switch
    {
        GrandCompany.Maelstrom => new Vector3(13.198463f, 39.999985f, 12.237406f),
        GrandCompany.TwinAdder => new Vector3(25.738628f, -8f, 100.01823f),
        _ => new Vector3(29.22171f, 6.999999f, -80.168755f),
    };

    internal static Vector3 RepairVendorLocation => GetGrandCompany() switch
    {
        GrandCompany.Maelstrom => new Vector3(17.715698f, 40.200005f, 3.9520264f),
        GrandCompany.TwinAdder => new Vector3(24.826416f, -8f, 93.18677f),
        _ => new Vector3(32.85266f, 6.999999f, -81.31531f),
    };
}

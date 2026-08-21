using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Info;

namespace AutoWeeklyCap.Helpers;

public static unsafe class FreeCompanyHelper
{
    public static bool IsInFreeCompany => PlayerHelper.IsValid && InfoProxyFreeCompany.Instance() != null && InfoProxyFreeCompany.Instance()->Id != 0;

    // The FC's own progression level (1-30, capped), NOT the local player's rank tier; byte.MaxValue = not in an FC
    public static byte GetFreeCompanyLevel()
    {
        if (!IsInFreeCompany) {
            return byte.MaxValue;
        }

        try {
            return InfoProxyFreeCompany.Instance()->Rank;
        } catch (Exception) {
            return byte.MaxValue;
        }
    }

    // 0 = Master (highest tier); byte.MaxValue = not in an FC/member entry not loaded yet
    // RankNumber is Gray-coded into ExtraFlags bits 12-15 (confirmed against a live 4-rank FC roster)
    public static byte GetRank()
    {
        var entry = GetLocalMemberEntry();
        if (entry == null) {
            return byte.MaxValue;
        }

        byte value = (byte)((entry->ExtraFlags >> 12) & 0xF);

        value ^= (byte)(value >> 1);
        value ^= (byte)(value >> 2);

        return value;
    }

    public static bool CanExecuteActions()
    {
        var data = GetCurrentRankData();
        return data is { } rankData &&
               rankData.BasicSettingsData.HasFlag(InfoProxyFreeCompany.RankData.BasicSettings.Invitations);
    }

    public static bool CanDiscardActions()
    {
        return GetCurrentRankData() is { } rankData &&
               rankData.BasicSettingsData.HasFlag(InfoProxyFreeCompany.RankData.BasicSettings.DiscardingActions);
    }

    // Buying/refilling FC actions spends company credits, which is gated by this permission
    public static bool CanBuyActions()
    {
        return GetCurrentRankData() is { } rankData &&
               rankData.BasicSettingsData.HasFlag(InfoProxyFreeCompany.RankData.BasicSettings.CompanyCredists);
    }

    private static InfoProxyFreeCompany.RankData? GetCurrentRankData()
    {
        AWC.Log.Debug("TEST: FreeCompanyHelper.GetCurrentRankData - 1");
        if (!IsInFreeCompany) {
            AWC.Log.Debug("TEST: FreeCompanyHelper.GetCurrentRankData - 1.1");
            return null;
        }

        AWC.Log.Debug("TEST: FreeCompanyHelper.GetCurrentRankData - 2");

        var rank = GetRank();
        if (rank == byte.MaxValue) {
            AWC.Log.Debug("TEST: FreeCompanyHelper.GetCurrentRankData - 2.1");
            return null;
        }

        try {
            AWC.Log.Debug("TEST: FreeCompanyHelper.GetCurrentRankData - 3");
            InfoProxyFreeCompany* proxy = InfoProxyFreeCompany.Instance();
            if (proxy == null) {
                AWC.Log.Debug("TEST: FreeCompanyHelper.GetCurrentRankData - 3.1");
                return null;
            }

            AWC.Log.Debug("TEST: TestValue = {0}", proxy->Ranks[0].BasicSettingsData);

            foreach (var rankData in proxy->Ranks) {
                AWC.Log.Debug($"RANK: {rankData.NameString}");
                if (rankData.RankNumber == rank) {
                    AWC.Log.Debug("TEST: FreeCompanyHelper.GetCurrentRankData - 4");
                    return rankData;
                }
            }

            AWC.Log.Debug("TEST: FreeCompanyHelper.GetCurrentRankData - 5");
            // The rank permission list isn't included in the base FC data request; ask for it keyed to the local player
            proxy->RequestDataForCharacter(PlayerState.Instance()->EntityId);
            return null;
        } catch (Exception) {
            AWC.Log.Debug("TEST: FreeCompanyHelper.GetCurrentRankData - ???");
            return null;
        }
    }

    // Local player's own entry in the FC member list, kicking off the server request if it hasn't loaded yet
    private static InfoProxyCommonList.CharacterData* GetLocalMemberEntry()
    {
        if (!IsInFreeCompany) {
            return null;
        }

        try {
            var memberProxy = InfoProxyFreeCompanyMember.Instance();
            if (memberProxy == null) {
                return null;
            }

            if (memberProxy->EntryCount != 0) {
                return memberProxy->GetEntryByContentId(PlayerState.Instance()->ContentId);
            }

            memberProxy->RequestData();

            return null;
        } catch (Exception) {
            return null;
        }
    }
}

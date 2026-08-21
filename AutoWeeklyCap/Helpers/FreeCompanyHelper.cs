using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Info;

namespace AutoWeeklyCap.Helpers;

public static unsafe class FreeCompanyHelper
{
    public static bool IsInFreeCompany => PlayerHelper.IsValid && InfoProxyFreeCompany.Instance() != null && InfoProxyFreeCompany.Instance()->Id != 0;

    // FFXIVClientStructs declares RankData.RankNumber/Permissions/Name at 0x22/0x00/0x23, but that no longer matches the
    // live client layout. Verified by hex-dumping a populated RankData entry: MemberCount (0x20) and sizeof (0x58) are still
    // correct, but the real RankNumber sits right before the rank's name string, and Permissions sits right after MemberCount.
    private const int RankDataPermissionsOffset = 0x22;
    private const int RankDataRankNumberOffset = 0x42;

    // The BasicSettings enum's bit positions (upstream) don't hold either: comparing permission bytes between a rank with
    // "Execute Company Actions" on vs off showed bytes[0..1] (where BasicSettings supposedly lives) were identical, while
    // bytes[7] flipped 0x08 -> 0x00. Confirmed empirically; treat as the real bit until proven otherwise.
    private const int ExecutingActionsPermissionByte = 7;
    private const byte ExecutingActionsPermissionMask = 0x08;

    private static DateTime _nextRankDataRequest = DateTime.MinValue;

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
        return GetCurrentRankPermissions() is { } permissions &&
               (permissions[ExecutingActionsPermissionByte] & ExecutingActionsPermissionMask) != 0;
    }

    // TODO these still use the disproven upstream BasicSettings bit mapping (see CanExecuteActions comment above) and need
    // the same empirical byte-diff verification before being trusted.
    public static bool CanDiscardActions()
    {
        return GetCurrentRankBasicSettings() is { } settings &&
               settings.HasFlag(InfoProxyFreeCompany.RankData.BasicSettings.DiscardingActions);
    }

    // Buying/refilling FC actions spends company credits, which is gated by this permission
    public static bool CanBuyActions()
    {
        return GetCurrentRankBasicSettings() is { } settings &&
               settings.HasFlag(InfoProxyFreeCompany.RankData.BasicSettings.CompanyCredists);
    }

    public static bool CanInviteActions()
    {
        return GetCurrentRankBasicSettings() is { } settings &&
               settings.HasFlag(InfoProxyFreeCompany.RankData.BasicSettings.Invitations);
    }

    // Returns the 10 raw permission bytes for the local player's current rank
    private static byte[]? GetCurrentRankPermissions()
    {
        if (!IsInFreeCompany) {
            return null;
        }

        var rank = GetRank();
        if (rank == byte.MaxValue) {
            return null;
        }

        try {
            InfoProxyFreeCompany* proxy = InfoProxyFreeCompany.Instance();
            if (proxy == null) {
                return null;
            }

            if (proxy->TotalMembers == 0) {
                proxy->RequestData();
                return null;
            }

            for (var i = 0; i < proxy->Ranks.Length; i++) {
                var rankData = proxy->Ranks[i];
                var raw = (byte*)&rankData;
                if (raw[RankDataRankNumberOffset] != rank) {
                    continue;
                }

                var permissions = new byte[10];
                for (var o = 0; o < permissions.Length; o++) {
                    permissions[o] = raw[RankDataPermissionsOffset + o];
                }

                return permissions;
            }

            // No match means the permission table hasn't come back yet; request it (throttled) and try again on a later call
            if (DateTime.UtcNow >= _nextRankDataRequest) {
                _nextRankDataRequest = DateTime.UtcNow.AddSeconds(2);
                proxy->RequestDataForCharacter(PlayerState.Instance()->EntityId);
            }

            return null;
        } catch (Exception) {
            return null;
        }
    }

    private static InfoProxyFreeCompany.RankData.BasicSettings? GetCurrentRankBasicSettings()
    {
        if (GetCurrentRankPermissions() is not { } permissions) {
            return null;
        }

        return (InfoProxyFreeCompany.RankData.BasicSettings)(ushort)(((permissions[1] & 0x7F) << 8) + permissions[0]);
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

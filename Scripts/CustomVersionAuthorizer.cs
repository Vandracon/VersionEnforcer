using System;
using JetBrains.Annotations;
using VersionEnforcer.Harmony;
namespace VersionEnforcer.Scripts
{
    [UsedImplicitly]
    public class CustomVersionAuthorizer : AuthorizerAbs
    {
        public override int Order => 71;
        public override string AuthorizerName => "CustomVersion";
        public override string StateLocalizationKey => "authstate_customversion";
        
        public override (EAuthorizerSyncResult, GameUtils.KickPlayerData?) Authorize(ClientInfo _clientInfo)
        {
            Harmony_NetPackagePlayerLogin.PlatformUserIdToProvidedCustomVersion.TryGetValue(
                _clientInfo.PlatformId.ReadablePlatformUserIdentifier, out var userProvidedCustomVersion);

            Harmony_NetPackagePlayerLogin.PlatformUserIdToProvidedCustomVersion.Remove(_clientInfo.PlatformId
                .ReadablePlatformUserIdentifier);

            Log.Out($"[VersionEnforcer] - Comparing user login modpack version ({userProvidedCustomVersion}) " +
                    $"against server ({Globals.CustomVersion})");
            
            if (!string.Equals(Globals.CustomVersion, userProvidedCustomVersion))
            {
                EAuthorizerSyncResult item = EAuthorizerSyncResult.SyncDeny;
                int apiResponseEnum = 0;
                return new ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?>(item,
                    new GameUtils.KickPlayerData((GameUtils.EKickReason)500, apiResponseEnum, default, Globals.CustomVersion));
            }
            return new ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?>(EAuthorizerSyncResult.SyncAllow, null);
        }
    }
}
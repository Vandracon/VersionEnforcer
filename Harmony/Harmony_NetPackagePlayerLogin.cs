using System.Collections.Generic;
using HarmonyLib;
using JetBrains.Annotations;
using VersionEnforcer.Scripts;

namespace VersionEnforcer.Harmony
{
    [UsedImplicitly]
    public class Harmony_NetPackagePlayerLogin
    {
        public static readonly Dictionary<string, string> PlatformUserIdToProvidedCustomVersion =
            new Dictionary<string, string>();
        
        [UsedImplicitly]
        public class NetPackagePlayerLogin_ReadPatch
        {
            [HarmonyPatch(typeof(NetPackagePlayerLogin), "read")]
            private static bool Prefix(NetPackagePlayerLogin __instance, PooledBinaryReader _br)
            {
                var instanceTraverse = Traverse.Create(__instance);
                
                Log.Out("NPPL.Read");
                // Vanilla Fields
                instanceTraverse.Field("playerName")?.SetValue(_br.ReadString());
                instanceTraverse.Field("platformUserAndToken")?.SetValue((
                    PlatformUserIdentifierAbs.FromStream(_br, _inclCustomData: true), _br.ReadString()));
                instanceTraverse.Field("crossplatformUserAndToken")?.SetValue((
                    PlatformUserIdentifierAbs.FromStream(_br, _inclCustomData: true), _br.ReadString()));
                instanceTraverse.Field("version")?.SetValue(_br.ReadString());
                instanceTraverse.Field("compVersion")?.SetValue(_br.ReadString());

                // Modded Fields
                var customVersion = _br.ReadString();

                var platformUserAndToken =
                    // ReSharper disable once PossibleNullReferenceException
                    ((PlatformUserIdentifierAbs userId, string token))instanceTraverse.Field("platformUserAndToken")
                        ?.GetValue();

                if (PlatformUserIdToProvidedCustomVersion.ContainsKey(platformUserAndToken.userId
                        .ReadablePlatformUserIdentifier))
                {
                    PlatformUserIdToProvidedCustomVersion[platformUserAndToken.userId.ReadablePlatformUserIdentifier] =
                        customVersion;
                }
                else
                {
                    PlatformUserIdToProvidedCustomVersion.Add(
                        platformUserAndToken.userId.ReadablePlatformUserIdentifier, customVersion);
                }

                return false;
            }
        }

        [UsedImplicitly]
        public class NetPackagePlayerLogin_WritePatch
        {
            [HarmonyPatch(typeof(NetPackagePlayerLogin), "write")]
            private static bool Prefix(NetPackagePlayerLogin __instance, PooledBinaryWriter _bw)
            {
                var instanceTraverse = Traverse.Create(__instance);
                var playerName = instanceTraverse.Field("playerName")?.GetValue() as string;
                // ReSharper disable once PossibleNullReferenceException
                var platformUserAndToken =
                    ((PlatformUserIdentifierAbs userId, string token))instanceTraverse.Field("platformUserAndToken")
                        ?.GetValue();
                // ReSharper disable once PossibleNullReferenceException
                var crossplatformUserAndToken =
                    ((PlatformUserIdentifierAbs userId, string token))instanceTraverse
                        .Field("crossplatformUserAndToken")?.GetValue();
                var version = instanceTraverse.Field("version")?.GetValue() as string;
                var compVersion = instanceTraverse.Field("compVersion")?.GetValue() as string;

                // Vanilla Fields
                Log.Out("NPPL.Write");
                __instance.write(_bw);
                // ReSharper disable once AssignNullToNotNullAttribute
                _bw.Write(playerName);
                platformUserAndToken.userId.ToStream(_bw, true);
                _bw.Write(platformUserAndToken.token ?? "");
                crossplatformUserAndToken.userId.ToStream(_bw, true);
                _bw.Write(crossplatformUserAndToken.token ?? "");
                // ReSharper disable once AssignNullToNotNullAttribute
                _bw.Write(version);
                // ReSharper disable once AssignNullToNotNullAttribute
                _bw.Write(compVersion);
                
                // Modded Fields
                _bw.Write(Globals.CustomVersion);

                return false;
            }
        }
    }
}
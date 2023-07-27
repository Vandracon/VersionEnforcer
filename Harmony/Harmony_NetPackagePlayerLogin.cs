﻿using System;
using System.Collections.Generic;
using HarmonyLib;
using JetBrains.Annotations;
using VersionEnforcer.Scripts;

namespace VersionEnforcer.Harmony
{
    [UsedImplicitly]
    public class Harmony_NetPackagePlayerLogin
    {
        private const string DELIMITER = ":::";

        private static int lastCustomFieldsByteSize;

        [HarmonyPatch(typeof(NetPackagePlayerLogin), "read")]
        public class NetPackagePlayerLogin_ReadPatch
        {
            [UsedImplicitly]
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
                var numMods = _br.ReadInt32();
                
                var mods = new List<CustomVersionAuthorizer.ModVersionInfo>();
                var strLen = 0;
                for (int i = 0; i < numMods; i++)
                {
                    var str = _br.ReadString();
                    strLen += str.Length;
                    string[] modInfo = str.Split(new[] { DELIMITER }, StringSplitOptions.None);
                    if (modInfo.Length < 2) continue;
                    mods.Add(new CustomVersionAuthorizer.ModVersionInfo { ModName = modInfo[0], ModVersion = modInfo[1]});
                }

                lastCustomFieldsByteSize = sizeof(Int32) + strLen * 2;

                var platformUserAndToken =
                    // ReSharper disable once PossibleNullReferenceException
                    ((PlatformUserIdentifierAbs userId, string token))instanceTraverse.Field("platformUserAndToken")
                        ?.GetValue();

                if (CustomVersionAuthorizer.PlatformUserIdToProvidedCustomVersion.ContainsKey(platformUserAndToken.userId
                        .ReadablePlatformUserIdentifier))
                {
                    CustomVersionAuthorizer.PlatformUserIdToProvidedCustomVersion[
                        platformUserAndToken.userId.ReadablePlatformUserIdentifier] = mods;
                }
                else
                {
                    CustomVersionAuthorizer.PlatformUserIdToProvidedCustomVersion.Add(
                        platformUserAndToken.userId.ReadablePlatformUserIdentifier, mods);
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(NetPackagePlayerLogin), "write")]
        public class NetPackagePlayerLogin_WritePatch
        {
            [UsedImplicitly]
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
                #region base.write(_bw) equivalent replacement
                _bw.Write((byte) __instance.PackageId);
                #endregion
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
                
                // Modded
                var loadedMods = ModManager.GetLoadedMods();
                var mods = new List<string>();
                foreach (var mod in loadedMods)
                {
                    mods.Add($"{mod.Name}{DELIMITER}{mod.VersionString}");
                }
                
                _bw.Write(loadedMods.Count);
                foreach (var str in mods)
                {
                    _bw.Write(str);
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(NetPackagePlayerLogin), "GetLength")]
        public class NetPackagePlayerLogin_GetLengthPatch
        {
            [UsedImplicitly]
            // ReSharper disable once RedundantAssignment
            private static bool Prefix(NetPackagePlayerLogin __instance, ref int __result)
            {
                __result = __instance.GetLength() + lastCustomFieldsByteSize;
                return false;
            }
        }
    }
}
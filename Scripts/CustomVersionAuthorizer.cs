using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using JetBrains.Annotations;
namespace VersionEnforcer.Scripts
{
    [UsedImplicitly]
    public class CustomVersionAuthorizer : AuthorizerAbs
    {
        internal static readonly Dictionary<string, List<ModVersionInfo>> PlatformUserIdToProvidedCustomVersion =
            new Dictionary<string, List<ModVersionInfo>>();
        
        public struct ModVersionInfo
        {
            public string ModName;
            public string ModVersion;
        }

        public override int Order => 71;
        public override string AuthorizerName => "CustomVersion";
        public override string StateLocalizationKey => "authstate_customversion";
        
        private enum ModIssue { Missing, InvalidVersion }
        private struct ModIssues
        {
            public string ModName;
            public ModIssue Issue;
            public string ServerVersion;
            public string ClientVersion;
        }

        private readonly List<string> ignoreList = new List<string>();

        public override void Init(IAuthorizationResponses _authResponsesHandler)
        {
            base.Init(_authResponsesHandler);
            LoadIgnoreList();
        }

        public override (EAuthorizerSyncResult, GameUtils.KickPlayerData?) Authorize(ClientInfo _clientInfo)
        {
            PlatformUserIdToProvidedCustomVersion.TryGetValue(
                _clientInfo.PlatformId.ReadablePlatformUserIdentifier, out var userProvidedCustomVersion);

            PlatformUserIdToProvidedCustomVersion.Remove(_clientInfo.PlatformId
                .ReadablePlatformUserIdentifier);
            
            var loadedMods = ModManager.GetLoadedMods();
            var issues = new List<ModIssues>();
            foreach (var mod in loadedMods)
            {
                if (ignoreList.Contains(mod.Name)) continue;
                var clientMatch = userProvidedCustomVersion?.Where(x => x.ModName == mod.Name)
                    .Cast<ModVersionInfo?>().FirstOrDefault();
                if (clientMatch == null)
                {
                    issues.Add(new ModIssues { ModName = mod.Name, Issue = ModIssue.Missing, ServerVersion = mod.VersionString, ClientVersion = ""});
                }
                else if (!clientMatch.Value.ModVersion.Equals(mod.VersionString))
                {
                    issues.Add(new ModIssues
                    {
                        ModName = mod.Name, Issue = ModIssue.InvalidVersion, ServerVersion = mod.VersionString,
                        ClientVersion = clientMatch.Value.ModVersion
                    });
                }
            }
            
            var issuesMessage = "";
            foreach (var issue in issues)
            {
                if (issue.Issue == ModIssue.Missing) issuesMessage += $"{issue.ModName}@{issue.ServerVersion} missing.";
                else issuesMessage += $"{issue.ModName}@{issue.ClientVersion} version should be {issue.ServerVersion}";
                issuesMessage += "\n";
            }

            if (issues.Count > 0)
            {
                EAuthorizerSyncResult item = EAuthorizerSyncResult.SyncDeny;
                int apiResponseEnum = 0;
                return new ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?>(item,
                    new GameUtils.KickPlayerData((GameUtils.EKickReason)500, apiResponseEnum, default, issuesMessage));
            }
            return new ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?>(EAuthorizerSyncResult.SyncAllow, null);
        }

        private void LoadIgnoreList()
        {
            var filename = "IgnoreList.xml";
            var xmlFile = new XmlFile($"{VersionEnforcerUtils.ModPath}/Resources/", filename);
            XElement root = xmlFile.XmlDoc.Root;
            if (root == null)
            {
                Log.Error($"{Globals.LOG_TAG} {filename} not found or no XML root");
                return;
            }

            foreach (XElement elem in root.Elements("mod"))
            {
                string modName = elem.Attribute("name")?.Value ?? "";
                if (modName.Length > 0) ignoreList.Add(modName);
            }
        }
    }
}
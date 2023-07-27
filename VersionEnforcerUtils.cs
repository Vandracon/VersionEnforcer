namespace VersionEnforcer
{
    public static class VersionEnforcerUtils
    {
        public static readonly string ModName = "VersionEnforcer";
        public static readonly Mod Mod = ModManager.GetMod(ModName, true);
        public static readonly string ModPath = Mod.Path;
    }
}
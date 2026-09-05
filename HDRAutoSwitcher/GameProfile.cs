namespace HDRAutoSwitcher
{
    internal class GameProfile
    {
        public string Name { get; set; }

        public string? LaunchPath { get; set; }

        public string? SteamAppId { get; set; }

        public string ProcessName { get; set; }

        public bool RequiresHDR { get; set; }

        public string? IconPath { get; set; }

        public GameProfile(
            string name,
            string? launchPath,
            string? steamAppId,
            string processName,
            bool requiresHDR,
            string? iconPath = null)
        {
            Name = name;
            LaunchPath = launchPath;
            SteamAppId = steamAppId;
            ProcessName = processName;
            RequiresHDR = requiresHDR;
            IconPath = iconPath;
        }
    }
}
namespace HDRAutoSwitcher
{
    internal class SteamGame
    {
        public string AppId { get; set; }

        public string Name { get; set; }

        public string InstallDir { get; set; }

        public SteamGame(
            string appId,
            string name,
            string installDir)
        {
            AppId = appId;
            Name = name;
            InstallDir = installDir;
        }
    }
}
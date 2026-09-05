using System.Collections.Generic;
using System.Windows;

namespace HDRAutoSwitcher
{
    public partial class AddGameWindow : Window
    {
        private readonly SteamService steamService;
        private readonly GameConfig gameConfig;
        private readonly List<SteamGame> steamGames;

        public AddGameWindow()
        {
            InitializeComponent();

            steamService = new SteamService();
            gameConfig = new GameConfig();

            steamGames = steamService.GetInstalledGames();

            SteamGamesList.ItemsSource = steamGames;
            SteamGamesList.DisplayMemberPath = "Name";
        }

        private void AddButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (SteamGamesList.SelectedItem is not SteamGame selectedGame)
            {
                MessageBox.Show("Please select a game.");
                return;
            }

            string? gameExecutable =
                steamService.GetLikelyGameExecutable(selectedGame);

            if (string.IsNullOrWhiteSpace(gameExecutable))
            {
                MessageBox.Show(
                    "Could not find a game executable.");

                return;
            }

            string processName =
                System.IO.Path.GetFileNameWithoutExtension(
                    gameExecutable);

            GameProfile profile = new GameProfile(
                selectedGame.Name,
                null,
                selectedGame.AppId,
                processName,
                RequiresHdrCheckBox.IsChecked == true,
                gameExecutable);

            gameConfig.AddGame(profile);

            DialogResult = true;
            Close();
        }
    }
}
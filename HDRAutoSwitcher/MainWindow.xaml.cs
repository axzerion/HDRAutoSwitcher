using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;

namespace HDRAutoSwitcher
{
    public partial class MainWindow : Window
    {
        private readonly GameConfig gameConfig;
        private readonly HDRController hdrController;
        private readonly GameManager gameManager;
        private readonly DispatcherTimer hdrStatusTimer;
        private ObservableCollection<GameProfile> allGames = new ObservableCollection<GameProfile>();
        private ICollectionView? gamesView;

        public MainWindow()
        {
            InitializeComponent();

            gameConfig = new GameConfig();
            hdrController = new HDRController();
            gameManager = new GameManager();

            hdrStatusTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            hdrStatusTimer.Tick += (_, _) =>
            {
                LoadHdrStatus();
            };

            hdrStatusTimer.Start();

            LoadHdrStatus();
            LoadGames();
        }

        private void SearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            gamesView?.Refresh();
        }

        private void LaunchButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is GameProfile game)
            {
                gameManager.LaunchGame(game);

                LoadHdrStatus();
            }
        }

        private void EditHdr_Click(object sender, RoutedEventArgs e)
        {
            GameProfile? game = null;

            if (sender is MenuItem mi && mi.CommandParameter is GameProfile gp)
                game = gp;
            else if (sender is FrameworkElement fe && fe.DataContext is GameProfile gp2)
                game = gp2;

            if (game == null)
                return;

            game.RequiresHDR = !game.RequiresHDR;

            gameConfig.UpdateGame(game);

            gamesView?.Refresh();
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            GameProfile? game = null;

            if (sender is MenuItem mi && mi.CommandParameter is GameProfile gp)
                game = gp;
            else if (sender is FrameworkElement fe && fe.DataContext is GameProfile gp2)
                game = gp2;

            if (game == null)
                return;

            MessageBoxResult result = MessageBox.Show(
                $"Remove {game.Name}?",
                "Remove Game",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                gameConfig.RemoveGame(game);

                allGames.Remove(game);
            }
        }

        private void AddGameButton_Click(object sender, RoutedEventArgs e)
        {
            AddGameWindow dlg = new AddGameWindow();

            bool? res = dlg.ShowDialog();

            if (res == true)
            {
                // reload collection from config
                LoadGames();
            }
        }

        private void OpenMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.IsOpen = true;
            }
        }

        private void LoadHdrStatus()
        {
            bool hdrEnabled = hdrController.IsHDREnabled();

            HdrStatusText.Text = hdrEnabled ? "HDR: ON" : "HDR: OFF";

            // update status dot color
            if (HdrStatusDot != null)
            {
                HdrStatusDot.Fill = hdrEnabled ?
                    new SolidColorBrush(Color.FromRgb(92, 200, 120)) :
                    new SolidColorBrush(Color.FromRgb(140, 140, 140));
            }

            if (HdrStatusBorder != null)
            {
                HdrStatusBorder.Background = hdrEnabled ?
                    new SolidColorBrush(Color.FromRgb(22, 28, 23)) :
                    new SolidColorBrush(Color.FromRgb(35, 35, 35));
            }
        }

        private void LoadGames()
        {
            allGames = new ObservableCollection<GameProfile>(gameConfig.LoadGames());

            gamesView = CollectionViewSource.GetDefaultView(allGames);
            gamesView.Filter = FilterGames;

            GamesListView.ItemsSource = gamesView;
        }

        private bool FilterGames(object item)
        {
            if (item is not GameProfile game)
                return false;

            if (string.IsNullOrWhiteSpace(SearchBox?.Text))
                return true;

            string q = SearchBox.Text.Trim();

            return game.Name?.IndexOf(q, System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        // The visual card creation has been moved into an ItemTemplate in XAML.
    }
}
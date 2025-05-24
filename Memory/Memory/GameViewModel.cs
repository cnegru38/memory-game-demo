using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows;
using System.Windows.Threading;

namespace Memory
{
    public class GameViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _gameOver = false;

        private bool _isBusy = false;
        public User CurrentUser { get; }
        public string Username => CurrentUser?.Username ?? "";

        public ImageSource ProfileImageSource
        {
            get
            {
                if (CurrentUser?.ProfileImage?.FilePath != null && File.Exists(CurrentUser.ProfileImage.FilePath))
                {
                    return new BitmapImage(new Uri(CurrentUser.ProfileImage.FilePath, UriKind.Absolute));
                }
                return null;
            }
        }
        public ObservableCollection<Card> Cards { get; set; }

        #region Game mode declaration
        public enum GameMode
        {
            Standard, // 4x4
            Custom_3x4,
            Custom_4x5
        }

        private GameMode _currentGameMode = GameMode.Standard;
        public GameMode CurrentGameMode
        {
            get => _currentGameMode;
            set
            {
                _currentGameMode = value;
                OnPropertyChanged(nameof(CurrentGameMode));
            }
        }
        #endregion

        #region Timer declaration

        private DispatcherTimer _gameTimer;
        private TimeSpan _timeRemaining;

        public string TimeRemainingDisplay => _timeRemaining.ToString(@"mm\:ss");

        private void UpdateTimerDisplay()
        {
            OnPropertyChanged(nameof(TimeRemainingDisplay));
        }

        #endregion

        public enum CardCategory
        {
            Category1,
            Category2,
            Category3
        }

        private CardCategory _currentCategory = CardCategory.Category1;

        public CardCategory CurrentCategory
        {
            get => _currentCategory;
            set
            {
                _currentCategory = value;
                OnPropertyChanged(nameof(CurrentCategory));
            }
        }

        #region Commands
        public ICommand NewGameCommand { get; }
        public ICommand ShowStatisticsCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand ShowAboutCommand { get; }
        public ICommand FlipCardCommand { get; }
        public string StatusMessage { get; set; }
        public ICommand SetStandardModeCommand { get; }
        public ICommand SetCustom3x4Command { get; }
        public ICommand SetCustom4x5Command { get; }
        public ICommand SetCategory1Command { get; }
        public ICommand SetCategory2Command { get; }
        public ICommand SetCategory3Command { get; }


        #endregion

        private readonly MainViewModel _mainViewModel;
        public GameViewModel(User selectedUser, MainViewModel mainViewModel)
        {
            CurrentUser = selectedUser;
            _mainViewModel = mainViewModel;

            #region Constructors

            NewGameCommand = new RelayCommand(_ => NewGame());
            ShowStatisticsCommand = new RelayCommand(_ => ShowStatistics());
            ExitCommand = new RelayCommand(_ => ExitToMainMenu());
            ShowAboutCommand = new RelayCommand(_ => ShowAbout());
            FlipCardCommand = new RelayCommand(param => FlipCard(param as Card));
            SetStandardModeCommand = new RelayCommand(_ => { CurrentGameMode = GameMode.Standard; });
            SetCustom3x4Command = new RelayCommand(_ => { CurrentGameMode = GameMode.Custom_3x4; });
            SetCustom4x5Command = new RelayCommand(_ => { CurrentGameMode = GameMode.Custom_4x5; });
            SetCategory1Command = new RelayCommand(_ => { CurrentCategory = CardCategory.Category1; });
            SetCategory2Command = new RelayCommand(_ => { CurrentCategory = CardCategory.Category2; });
            SetCategory3Command = new RelayCommand(_ => { CurrentCategory = CardCategory.Category3; });

            #endregion

            StatusMessage = "Ready to play!";
        }

        private Card firstFlipped = null;

        #region File
        private void NewGame()
        {
            StatusMessage = "Game started!";
            OnPropertyChanged(nameof(StatusMessage));

            _gameOver = false;
            _isBusy = false;
            firstFlipped = null;

            if (CurrentUser != null)
                CurrentUser.GamesPlayed++;
            _mainViewModel.SaveUsersToJson();

            int pairCount = CurrentGameMode switch
            {
                GameMode.Standard => 8,         // 4x4 = 16 cards → 8 pairs
                GameMode.Custom_3x4 => 6,       // 12 cards → 6 pairs
                GameMode.Custom_4x5 => 10,      // 20 cards → 10 pairs
                _ => 8
            };

            string categoryPath = $"Assets/Cards/{CurrentCategory}";
            if (!Directory.Exists(categoryPath))
            {
                MessageBox.Show($"Image folder not found: {categoryPath}");
                return;
            }

            var images = Directory.GetFiles(categoryPath)
                                  .OrderBy(_ => Guid.NewGuid())
                                  .Take(pairCount)
                                  .ToList();

            var cardImages = images.Concat(images)
                                    .OrderBy(_ => Guid.NewGuid())
                                    .ToList();

            Cards = new ObservableCollection<Card>(
                cardImages.Select(img => new Card { FrontImagePath = img }));

            OnPropertyChanged(nameof(Cards));

            _timeRemaining = TimeSpan.FromSeconds(90);
            UpdateTimerDisplay();

            if (_gameTimer == null)
            {
                _gameTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(1)
                };
                _gameTimer.Tick += GameTimer_Tick;
            }

            _gameTimer.Start();
        }
        private void ShowStatistics()
        {
            if (CurrentUser != null)
            {
                string message = $"Games Played: {CurrentUser.GamesPlayed}\nGames Won: {CurrentUser.GamesWon}";
                string title = $"{CurrentUser.Username}'s Statistics";
                System.Windows.MessageBox.Show(message, title, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
        }
        private void ExitToMainMenu()
        {
            // Open a new MainWindow
            var mainWindow = new MainWindow();
            mainWindow.Show();

            // Close the current GameWindow
            foreach (var window in System.Windows.Application.Current.Windows)
            {
                if (window is GameWindow)
                {
                    ((System.Windows.Window)window).Close();
                    break;
                }
            }
        }
        #endregion
        private void ShowAbout()
        {
            System.Windows.MessageBox.Show("Memory Game\nCreated by: Cosmin-Stefan Negru\nhttps://github.com/cnegru38", "About", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }

        #region Game logic commands
        private async void FlipCard(Card card)
        {
            if (_gameOver || _isBusy || card == null || card.IsFlipped || card.IsMatched || firstFlipped == card)
                return;

            card.IsFlipped = true;

            if (firstFlipped == null)
            {
                firstFlipped = card;
            }
            else
            {
                _isBusy = true;

                await Task.Delay(1000); // Wait for user to see both cards

                if (firstFlipped.FrontImagePath == card.FrontImagePath)
                {
                    firstFlipped.IsMatched = true;
                    card.IsMatched = true;
                    if (Cards.All(c => c.IsMatched))
                    {
                        _gameOver = true;
                        _gameTimer?.Stop();
                        StatusMessage = "You won!";
                        OnPropertyChanged(nameof(StatusMessage));

                        if (CurrentUser != null)
                        {
                            CurrentUser.GamesWon++;
                            _mainViewModel.SaveUsersToJson();
                        }
                    }
                }
                else
                {
                    firstFlipped.IsFlipped = false;
                    card.IsFlipped = false;
                }

                firstFlipped = null;
                _isBusy = false;
            }
        }
        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (_timeRemaining > TimeSpan.Zero)
            {
                _timeRemaining = _timeRemaining.Subtract(TimeSpan.FromSeconds(1));
                UpdateTimerDisplay();
            }
            else
            {
                _gameTimer.Stop();
                _gameOver = true;
                StatusMessage = "Time's up!";
                OnPropertyChanged(nameof(StatusMessage));
            }
        }
        #endregion
        private void OnPropertyChanged(string propName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}

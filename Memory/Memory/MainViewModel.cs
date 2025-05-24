using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Memory
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region Logo image handling

        private ImageSource _logoImage;
        public ImageSource LogoImage
        {
            get => _logoImage;
            set
            {
                _logoImage = value;
                OnPropertyChanged(nameof(LogoImage));
            }
        }
        #endregion

        private ObservableCollection<Image> _images;

        private int _currentIndex;

        public string CurrentImagePath
        {
            get
            {
                if (_images.Count == 0) return null;
                return _images.Count > 0 ? _images[_currentIndex].FilePath : null;
            }
        }

        private ObservableCollection<User> _users = new ObservableCollection<User>();
        public ObservableCollection<User> Users
        {
            get => _users;
            set
            {
                _users = value;
                OnPropertyChanged(nameof(Users));
            }
        }

        private User _selectedUser;
        public User SelectedUser
        {
            get => _selectedUser;
            set
            {
                _selectedUser = value;
                OnPropertyChanged(nameof(SelectedUser));
                CommandManager.InvalidateRequerySuggested();

                if (_selectedUser?.ProfileImage != null)
                {
                    // Update current index to match the selected user's image
                    var index = _images.IndexOf(_selectedUser.ProfileImage);
                    if (index >= 0)
                    {
                        _currentIndex = index;
                        OnPropertyChanged(nameof(CurrentImagePath));
                    }
                }
            }
        }

        #region Commands
        public ICommand NextImageCommand { get; }
        public ICommand PreviousImageCommand { get; }
        public ICommand AddUserCommand { get; }
        public ICommand DeleteUserCommand { get; }
        public ICommand PlayCommand { get; }
        public ICommand CancelCommand { get; }

        #endregion

        public MainViewModel()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string imagesDir = Path.Combine(baseDir, "Assets");

            LogoImage = new BitmapImage(new Uri("pack://application:,,,/Assets/logo.png", UriKind.Absolute));

            _images = new ObservableCollection<Image>
            {
                #region Profile photo links
                new Image(1, "pack://application:,,,/Assets/lion.png"),
                new Image(2, "pack://application:,,,/Assets/horse.png"),
                new Image(3, "pack://application:,,,/Assets/flower.png"),
                new Image(4, "pack://application:,,,/Assets/chess.png"),
                new Image(5, "pack://application:,,,/Assets/cat.png"),
                new Image(6, "pack://application:,,,/Assets/dog.png"),
                new Image(7, "pack://application:,,,/Assets/frog.png"),
                new Image(8, "pack://application:,,,/Assets/love.png")
                #endregion
            };


            _currentIndex = 0;

            #region Constructors

            NextImageCommand = new RelayCommand(_ => NextImage());
            PreviousImageCommand = new RelayCommand(_ => PreviousImage());

            AddUserCommand = new RelayCommand(_ => AddUser());
            DeleteUserCommand = new RelayCommand(_ => DeleteUser(), _ => SelectedUser != null);

            PlayCommand = new RelayCommand(_ => PlayGame(), _ => SelectedUser != null);
            CancelCommand = new RelayCommand(_ => CancelApp());

            #endregion

            LoadUsersFromJson();
        }

        private readonly string _usersJsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "users.json");

        #region JSON functionality
        // Call this at startup
        private void LoadUsersFromJson()
        {
            if (File.Exists(_usersJsonPath))
            {
                try
                {
                    string json = File.ReadAllText(_usersJsonPath);
                    var users = JsonSerializer.Deserialize<ObservableCollection<User>>(json);
                    if (users != null)
                    {
                        // Link Image object based on ID
                        foreach (var user in users)
                        {
                            user.ProfileImage = _images.FirstOrDefault(img => img.Id == user.ProfileImageId);
                        }
                        Users = users;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to load users: {ex.Message}");
                }
            }
        }
        public void SaveUsersToJson()
        {
            try
            {
                // Ensure ProfileImageId is set for all users before saving
                foreach (var user in Users)
                {
                    user.ProfileImageId = user.ProfileImage?.Id ?? 0;
                }

                string json = JsonSerializer.Serialize(Users, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(_usersJsonPath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save users: {ex.Message}");
            }
        }
        #endregion

        #region Image commands
        private void NextImage()
        {
            if (_images.Count == 0) return;
            _currentIndex = (_currentIndex + 1) % _images.Count;
            OnPropertyChanged(nameof(CurrentImagePath));
        }

        private void PreviousImage()
        {
            if (_images.Count == 0) return;
            _currentIndex = (_currentIndex - 1 + _images.Count) % _images.Count;
            OnPropertyChanged(nameof(CurrentImagePath));
        }
        #endregion

        #region User commands
        private void AddUser()
        {
            string username = Microsoft.VisualBasic.Interaction.InputBox("Enter username:", "Add User", "User");
            if (string.IsNullOrWhiteSpace(username)) return;

            var currentImage = _images[_currentIndex];
            var newUser = new User(username, currentImage);
            Users.Add(newUser);

            SaveUsersToJson();
        }
        private void DeleteUser()
        {
            if (SelectedUser != null)
            {
                var userToRemove = Users.FirstOrDefault(u => u.Username == SelectedUser.Username);
                if (userToRemove != null)
                {
                    Users.Remove(userToRemove);
                    SaveUsersToJson();
                }
            }
        }

        #endregion

        #region Play and cancel
        private void PlayGame()
        {
            if (SelectedUser == null)
            {
                MessageBox.Show("Please select a user before starting the game.", "No User Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var gameWindow = new GameWindow(SelectedUser, this);
            gameWindow.Show();

            foreach (Window window in App.Current.Windows)
            {
                if (window is MainWindow)
                {
                    window.Close();
                    break;
                }
            }
        }

        private void CancelApp()
        {
            Application.Current.Shutdown();
        }
        #endregion
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

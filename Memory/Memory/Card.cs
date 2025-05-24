using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;

namespace Memory
{
    public class Card : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _isFlipped;
        private bool _isMatched;

        public string FrontImagePath { get; set; }  // Unique image per card pair
        public string BackImagePath { get; set; } = "Assets/card_back.png"; // Default back image

        public bool IsFlipped
        {
            get => _isFlipped;
            set
            {
                _isFlipped = value;
                OnPropertyChanged(nameof(IsFlipped));
                OnPropertyChanged(nameof(CurrentImage));
            }
        }

        public bool IsMatched
        {
            get => _isMatched;
            set
            {
                _isMatched = value;
                OnPropertyChanged(nameof(IsMatched));
                OnPropertyChanged(nameof(Visibility));
            }
        }

        public ImageSource CurrentImage =>
            new BitmapImage(new Uri(IsFlipped || IsMatched ? GetUri(FrontImagePath) : GetUri(BackImagePath), UriKind.Absolute));

        public Visibility Visibility => IsMatched ? Visibility.Collapsed : Visibility.Visible;

        private string GetUri(string path) => Path.GetFullPath(path);

        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

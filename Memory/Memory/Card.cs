using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Memory
{
    public class Card : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _isFlipped;
        private bool _isMatched;

        public string FrontImagePath { get; set; }

        // Default pack URI for card back image
        public string BackImagePath { get; set; } = "pack://application:,,,/Assets/card_back.png";

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
                OnPropertyChanged(nameof(CurrentImage));
            }
        }

        public ImageSource CurrentImage
        {
            get
            {
                string uri = IsFlipped || IsMatched ? FrontImagePath : BackImagePath;
                return new BitmapImage(new Uri(uri, UriKind.Absolute));
            }
        }

        public Visibility Visibility => IsMatched ? Visibility.Collapsed : Visibility.Visible;

        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

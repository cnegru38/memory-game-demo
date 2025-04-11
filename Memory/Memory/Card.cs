using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.IO;

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
            }
        }

        public ImageSource CurrentImage =>
            new BitmapImage(new Uri(IsFlipped || IsMatched ? GetUri(FrontImagePath) : GetUri(BackImagePath), UriKind.Absolute));

        private string GetUri(string path) => Path.GetFullPath(path); // full path needed for BitmapImage

        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

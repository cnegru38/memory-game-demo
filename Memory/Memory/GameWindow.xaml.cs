using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.IO;
using System.Xml.Serialization;

namespace Memory
{
    /// <summary>
    /// Interaction logic for GameWindow.xaml
    /// </summary>
    public partial class GameWindow : Window
    {
        public GameWindow(User selectedUser, MainViewModel mainViewModel)
        {
            InitializeComponent();
            DataContext = new GameViewModel(selectedUser, mainViewModel);
            string iconRelativePath = @"Assets\Icon.png";
            string iconFullPath = Path.GetFullPath(iconRelativePath);
            this.Icon = BitmapFrame.Create(new Uri(iconFullPath, UriKind.Absolute));
        }
    }
}

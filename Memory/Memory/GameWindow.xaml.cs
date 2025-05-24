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

            this.Icon = BitmapFrame.Create(new Uri("pack://application:,,,/Assets/Icon.png", UriKind.Absolute));

        }
    }
}

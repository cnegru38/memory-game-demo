using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.IO;

namespace Memory
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            string iconRelativePath = @"Assets\Icon.png";
            string iconFullPath = Path.GetFullPath(iconRelativePath);
            this.Icon = BitmapFrame.Create(new Uri(iconFullPath, UriKind.Absolute));
        }
    }
}
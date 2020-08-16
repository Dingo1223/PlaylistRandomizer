using System.Windows;

namespace PlaylistRandomizer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new AppVM();
        }
    }
}

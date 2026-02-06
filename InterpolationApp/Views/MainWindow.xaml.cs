using System.Windows;
using InterpolationApp.ViewModels;

namespace InterpolationApp.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            // Set DataContext AFTER InitializeComponent to avoid binding errors
            DataContext = new MainViewModel();
        }
    }
}

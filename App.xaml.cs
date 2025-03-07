using VideoExtractor.Stores;
using VideoExtractor.ViewModels;
using System.Windows;

namespace VideoExtractor
{
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            NavigationStore navigationStore = new();

            navigationStore.CurrentViewModel = new HomeViewModel(navigationStore);

            MainWindow = new MainWindow()
            {
                DataContext = new MainViewModel(navigationStore)
            };
            MainWindow.Show();

            base.OnStartup(e);
        }
    }

}

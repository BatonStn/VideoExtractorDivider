using VideoExtractor.Commands;
using VideoExtractor.Stores;
using System.Windows.Input;
using System.Diagnostics;

namespace VideoExtractor.ViewModels
{
    public class HomeViewModel : ViewModelBase
    {
        public string VideoSourceString { get; set; } = "";
        
        public ICommand NavigateEditCommand { get; }

        public HomeViewModel(NavigationStore navigationStore)
        {
            NavigateEditCommand = new NavigateCommand<EditViewModel>(navigationStore, () =>
            new EditViewModel(navigationStore, new Uri(VideoSourceString)));
        }

        public HomeViewModel(NavigationStore navigationStore, string videoSource)
        {
            NavigateEditCommand = new NavigateCommand<EditViewModel>(navigationStore, () => 
            new EditViewModel(navigationStore, new Uri(VideoSourceString)));

            VideoSourceString = videoSource;
        }
    }
}
using VideoExtractor.Commands;
using VideoExtractor.Stores;
using System.Windows.Input;

namespace VideoExtractor.ViewModels
{
    public class EditViewModel : ViewModelBase
    {
        public Uri VideoSourceUri { get; set; }

        public ICommand NavigateHomeCommand { get; }

        public EditViewModel(NavigationStore navigationStore, Uri videoSource)
        {
            NavigateHomeCommand = new NavigateCommand<HomeViewModel>(navigationStore, () => 
            new HomeViewModel(navigationStore, VideoSourceUri.OriginalString));

            VideoSourceUri = videoSource;
        }
    }
}
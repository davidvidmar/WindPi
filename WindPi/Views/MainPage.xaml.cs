using System.Diagnostics;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using WindPi.Helpers;
using WindPi.ViewModels;

namespace WindPi.Views
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();

            Debug.WriteLine(Debugging.GetDisplayInfo());
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var svm = new SensorsViewModel();
            await svm.InitSensorsAsync();

            DataContext = svm;

            base.OnNavigatedTo(e);
        }
    }
}

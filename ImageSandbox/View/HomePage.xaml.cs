using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using GroupCStegafy.View.Dialogs;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace GroupCStegafy.View
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomePage
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="HomePage" /> class.
        /// </summary>
        public HomePage()
        {
            this.InitializeComponent();
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            titleBar.ButtonForegroundColor = Colors.White;
            this.navigationView.SelectedItem = this.navigationView.MenuItems[0];
        }

        #endregion

        #region Methods

        private void NavigationView_Loaded(object sender, RoutedEventArgs e)
        {
            this.navigationView.IsPaneOpen = false;
        }

        private void Home_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(HomePage));
        }

        private void ImageTriangulationArt_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(TriangulationPage));
        }

        private void Steganography_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(StegafyPage));
        }

        private void AbstractTriangulationArt_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //TODO add abstract triangulation art page
        }

        private void PaintByNumberGenerator_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //TODO add paint by number generator page
        }

        #endregion
    }
}
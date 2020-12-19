using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

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
        }

        #endregion

        #region Methods

        private void steganographyTextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(SteganographyPage));
        }

        private void imageTriangulationTextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(TriangulationPage));
        }

        private void abstractTriangulationTextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(AbstractTriangulationPage));
        }

        private void steganographyTextBlock_OnPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            this.steganographyUnderline.Visibility = Visibility.Visible;
        }

        private void imageTriangulationTextBlock_OnPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            this.imageTriangulationUnderline.Visibility = Visibility.Visible;
        }

        private void abstractTriangulationTextBlock_OnPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            this.abstractTriangulationUnderline.Visibility = Visibility.Visible;
        }

        private void steganographyTextBlock_OnPointerExited(object sender, PointerRoutedEventArgs e)
        {
            this.steganographyUnderline.Visibility = Visibility.Collapsed;
        }

        private void imageTriangulationTextBlock_OnPointerExited(object sender, PointerRoutedEventArgs e)
        {
            this.imageTriangulationUnderline.Visibility = Visibility.Collapsed;
        }

        private void abstractTriangulationTextBlock_OnPointerExited(object sender, PointerRoutedEventArgs e)
        {
            this.abstractTriangulationUnderline.Visibility = Visibility.Collapsed;
        }

        #endregion
    }
}
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Imagely.View.Dialogs;
using Imagely.Viewmodel;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Imagely.View
{
    /// <summary>
    ///     Defines the abstract triangulation page.
    /// </summary>
    /// <seealso cref="Windows.UI.Xaml.Markup.IComponentConnector" />
    /// <seealso cref="Windows.UI.Xaml.Markup.IComponentConnector2" />
    /// <seealso cref="Windows.UI.Xaml.Controls.Page" />
    public sealed partial class AbstractTriangulationPage
    {
        #region Data members

        private readonly AbstractTriangulationPageViewModel viewModel;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="AbstractTriangulationPage" /> class.
        /// </summary>
        public AbstractTriangulationPage()
        {
            this.InitializeComponent();
            this.viewModel = new AbstractTriangulationPageViewModel {
                SourcePicture = {Height = (uint) this.recentImage.Height, Width = (uint) this.recentImage.Width}
            };
            this.navigationView.SelectedItem = this.navigationView.MenuItems[3];
        }

        #endregion

        #region Methods

        private async void drawTriangulationButton_Click(object sender, RoutedEventArgs e)
        {
            this.progressRing.IsActive = true;

            this.setCanvasBoundary();
            this.triangulationCanvas.Children.Clear();
            var pointsAmount = this.pointsTextBox.Text;
            var triangulationSuccessful = false;
            await Task.Run(() =>
            {
                triangulationSuccessful =
                    this.viewModel.DrawAbstractTriangleArt(this.triangulationCanvas, pointsAmount).Result;
            });
            if (triangulationSuccessful)
            {
                await this.convertCanvasToImage();
                this.triangulationCanvas.Children.Clear();
            }

            this.progressRing.IsActive = false;
        }

        private void checkClearButtonStatus()
        {
            if (this.viewModel.NamedColors.Count == 0)
            {
                this.clearColorsButton.IsEnabled = false;
            }
            else
            {
                this.clearColorsButton.IsEnabled = true;
            }
        }

        private void setCanvasBoundary()
        {
            var height = this.recentImage.Width;
            var width = this.recentImage.Height;
            this.canvasBoundaryRect.Rect = new Rect(0, 0, height, width);
        }

        private async Task convertCanvasToImage()
        {
            var renderTargetBitmap = new RenderTargetBitmap();
            await renderTargetBitmap.RenderAsync(this.triangulationCanvas);
            this.recentImage.Source = renderTargetBitmap;
        }

        private async void SaveSymbol_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (this.recentImage.Source != null)
            {
                await this.viewModel.SaveImage(this.recentImage);
            }
        }

        private void ClearSymbol_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.triangulationCanvas.Children.Clear();
            this.recentImage.Source = null;
            this.colorsListView.ItemsSource = null;
            this.viewModel.ClearNamedColors();
            this.viewModel.ResetColorPropertyInfos();
            this.checkClearButtonStatus();
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
            Frame.Navigate(typeof(SteganographyPage));
        }

        private void AbstractTriangulationArt_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(AbstractTriangulationPage));
        }

        private void SaveSymbol_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            this.saveSymbol.Foreground = (Brush) Application.Current.Resources["SymbolHoverColor"];
        }

        private void SaveSymbol_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            this.resetSymbolColors();
        }

        private void resetSymbolColors()
        {
            this.clearSymbol.Foreground = (Brush) Application.Current.Resources["SymbolColor"];
            this.saveSymbol.Foreground = (Brush) Application.Current.Resources["SymbolColor"];
        }

        private void ClearSymbol_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            this.clearSymbol.Foreground = (Brush) Application.Current.Resources["SymbolHoverColor"];
        }

        private void ClearSymbol_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            this.resetSymbolColors();
        }

        private void NavigationView_Loaded(object sender, RoutedEventArgs e)
        {
            this.navigationView.IsPaneOpen = false;
        }

        private void navigationView_OnBackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private void colorsListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.colorsListView.SelectedIndex != -1)
            {
                this.removeColorButton.IsEnabled = true;
            }
            else
            {
                this.removeColorButton.IsEnabled = false;
            }
        }

        private void removeColorButton_Click(object sender, RoutedEventArgs e)
        {
            this.viewModel.RemoveColorAt(this.colorsListView.SelectedIndex);
            this.colorsListView.ItemsSource = null;
            this.colorsListView.ItemsSource = this.viewModel.NamedColors;
            this.viewModel.ResetColorPropertyInfos();
            this.viewModel.RemoveRemainingSelectedColorsFromPropertyInfosList();
            this.checkClearButtonStatus();
        }

        private async void addColorButton_Click(object sender, RoutedEventArgs e)
        {
            if (await this.showNewColorDialog())
            {
                this.colorsListView.ItemsSource = null;
                this.colorsListView.ItemsSource = this.viewModel.NamedColors;
            }

            this.checkClearButtonStatus();
        }

        private async Task<bool> showNewColorDialog()
        {
            var dialog = new AddNewColorDialog(this.viewModel.NamedColors, this.viewModel.ColorPropertyInfos);
            await dialog.ShowAsync();
            this.viewModel.UpdateColorPropertyInfoSource(dialog.RemainingColorPropertyInfos);
            return dialog.ChangeOccured;
        }

        private void addRandomColorButton_Click(object sender, RoutedEventArgs e)
        {
            this.viewModel.AddRandomColor();
            this.colorsListView.ItemsSource = null;
            this.colorsListView.ItemsSource = this.viewModel.NamedColors;
            this.checkClearButtonStatus();
        }

        private void clearColorsButton_Click(object sender, RoutedEventArgs e)
        {
            this.viewModel.ResetColorPropertyInfos();
            this.viewModel.ClearNamedColors();
            this.colorsListView.ItemsSource = null;
            this.checkClearButtonStatus();
        }

        private void pointsTextBox_OnBeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            args.Cancel = args.NewText.Any(c => !char.IsDigit(c));
        }

        private void AbstractCircle_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(AbstractCirclePackingPage));
        }

        private void CirclePacking_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(CirclePackingPage));
        }

        #endregion
    }
}
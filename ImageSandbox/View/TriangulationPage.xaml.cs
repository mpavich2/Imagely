using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using GroupCStegafy.Enums;
using GroupCStegafy.Model;
using GroupCStegafy.Viewmodel;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace GroupCStegafy.View
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TriangulationPage
    {
        #region Data members

        private const string DropText = "Drop File Here";

        private readonly TriangulationPageViewModel viewModel;
        private readonly TextBlock[] imageTextBlocks;
        private ImageType recentImageType;

        #endregion

        #region Properties

        private Picture SourcePicture => this.viewModel.SourcePicture;

        private Picture ModifiedPicture => this.viewModel.CopiedPicture;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="TriangulationPage" /> class.
        /// </summary>
        public TriangulationPage()
        {
            this.InitializeComponent();
            this.viewModel = new TriangulationPageViewModel();
            this.imageTextBlocks = new[] {
                this.sourceImageTextBlock, this.sobelImageTextBlock, this.triangulationImageTextBlock
            };
            this.navigationView.SelectedItem = this.navigationView.MenuItems[1];
        }

        #endregion

        #region Methods

        private async Task<bool> openSourcePicture()
        {
            try
            {
                await this.viewModel.OpenSourceImage();

                using (var writeStream = this.SourcePicture.ModifiedImage.PixelBuffer.AsStream())
                {
                    await writeStream.WriteAsync(this.SourcePicture.Pixels, 0, this.SourcePicture.Pixels.Length);
                    this.sourceImage.Source = this.SourcePicture.ModifiedImage;
                }

                this.recentImage.Source = this.SourcePicture.ModifiedImage;
                this.updateRecentImageTextColor(this.sourceImageTextBlock);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async void DrawTriangulationButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.sobelImage.Source == null)
            {
                await this.applySobelAndGrayScale();
            }

            this.triangulationCanvas.Children.Clear();
            this.viewModel.DrawTriangulation(this.triangulationCanvas, this.pointsTextBox.Text);
            await this.convertCanvasToImage();
            this.triangulationCanvas.Children.Clear();
            this.showSymbols();
        }

        private async Task applySobelAndGrayScale()
        {
            this.viewModel.applyGrayscaleFilter();
            this.viewModel.applySobelFilter();
            using (var writeStream = this.ModifiedPicture.ModifiedImage.PixelBuffer.AsStream())
            {
                await writeStream.WriteAsync(this.ModifiedPicture.Pixels, 0, this.ModifiedPicture.Pixels.Length);
                this.sobelImage.Source = this.ModifiedPicture.ModifiedImage;
            }
        }

        private async Task convertCanvasToImage()
        {
            var renderTargetBitmap = new RenderTargetBitmap();
            await renderTargetBitmap.RenderAsync(this.triangulationCanvas);
            this.triangulationImage.Source = renderTargetBitmap;
            this.recentImage.Source = renderTargetBitmap;
            this.updateRecentImageTextColor(this.triangulationImageTextBlock);
        }

        private void drawAbstractTriangleArt()
        {
            //eventually move into own class to create random abstract triangle art
            this.viewModel.DrawAbstractTriangleArt(this.triangulationCanvas, this.pointsTextBox.Text);
        }

        private async void FileDropArea_Drop(object sender, DragEventArgs dragEvent)
        {
            await this.openImage(dragEvent);
            this.showDrawTriangulationButton();
            this.showReadyTextAndPointsBox();
            this.updateRecentImageTextColor(this.sourceImageTextBlock);
            this.dropAreaRectangle.Visibility = Visibility.Collapsed;
        }

        private async Task openImage(DragEventArgs dragEvent)
        {
            await this.viewModel.OpenDraggedImage(dragEvent);

            using (var writeStream = this.SourcePicture.ModifiedImage.PixelBuffer.AsStream())
            {
                await writeStream.WriteAsync(this.SourcePicture.Pixels, 0, this.SourcePicture.Pixels.Length);
                this.sourceImage.Source = this.SourcePicture.ModifiedImage;
            }

            this.recentImage.Source = this.SourcePicture.ModifiedImage;
        }

        private void FileDropArea_DragLeave(object sender, DragEventArgs e)
        {
            if (this.dropAreaRectangle.Visibility == Visibility.Visible)
            {
                this.showDropArea();
            }
        }

        private void showDropArea()
        {
            this.dragAndDropTextBlock.Visibility = Visibility.Visible;
            this.orTextBlock.Visibility = Visibility.Visible;
            this.browseFilesButton.Visibility = Visibility.Visible;
            this.browseFilesButton.IsEnabled = true;
        }

        private void FileDropArea_DragOver(object sender, DragEventArgs dragEvent)
        {
            if (this.dropAreaRectangle.Visibility == Visibility.Visible)
            {
                this.hideDropArea();
                dragEvent.AcceptedOperation = DataPackageOperation.Copy;
                if (dragEvent.DragUIOverride != null)
                {
                    dragEvent.DragUIOverride.Caption = DropText;
                    dragEvent.DragUIOverride.IsCaptionVisible = true;
                    dragEvent.DragUIOverride.IsContentVisible = true;
                }
            }
        }

        private void hideDropArea()
        {
            this.dragAndDropTextBlock.Visibility = Visibility.Collapsed;
            this.orTextBlock.Visibility = Visibility.Collapsed;
            this.browseFilesButton.Visibility = Visibility.Collapsed;
            this.browseFilesButton.IsEnabled = false;
        }

        private void updateRecentImageTextColor(TextBlock textBlock)
        {
            this.resetTextBlocksColor();
            if (textBlock != null)
            {
                if (textBlock == this.sourceImageTextBlock)
                {
                    this.sourceImageTextBlock.Foreground =
                        (Brush) Application.Current.Resources["HighlightedTextColor"];
                    this.recentImageType = ImageType.SourceImage;
                }
                else if (textBlock == this.sobelImageTextBlock)
                {
                    this.sobelImageTextBlock.Foreground = (Brush) Application.Current.Resources["HighlightedTextColor"];
                    this.recentImageType = ImageType.SobelImage;
                }
                else
                {
                    this.triangulationImageTextBlock.Foreground =
                        (Brush) Application.Current.Resources["HighlightedTextColor"];
                    this.recentImageType = ImageType.TriangulationImage;
                }
            }
        }

        private async void BrowseFilesButton_Click(object sender, RoutedEventArgs e)
        {
            if (!await this.openSourcePicture() && this.sourceImage.Source == null)
            {
                //TODO insert flyout for incorrect source image
                this.showDropArea();
            }
            else
            {
                this.hideDropArea();
                this.showDrawTriangulationButton();
                this.showReadyTextAndPointsBox();
                this.dropAreaRectangle.Visibility = Visibility.Collapsed;
            }
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

        private void showReadyTextAndPointsBox()
        {
            this.readyTextBlock.Visibility = Visibility.Visible;
            this.pointsTextBox.Visibility = Visibility.Visible;
        }

        private void showDrawTriangulationButton()
        {
            this.drawTriangulationButton.Visibility = Visibility.Visible;
            this.drawTriangulationButton.IsEnabled = true;
        }

        private async void SaveSymbol_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await this.viewModel.SaveImage(this.recentImage);
        }

        private void ClearSymbol_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.viewModel.clear();
            this.triangulationCanvas.Children.Clear();
            this.recentImage.Source = null;
            this.sourceImage.Source = null;
            this.sobelImage.Source = null;
            this.triangulationImage.Source = null;
            this.resetView();
        }

        private void resetView()
        {
            this.showDropArea();
            this.hideDrawTriangulationButton();
            this.hideReadyTextAndPointsBox();
            this.hideSymbols();
            this.dropAreaRectangle.Visibility = Visibility.Visible;
            this.resetTextBlocksColor();
        }

        private void resetTextBlocksColor()
        {
            foreach (var textBlock in this.imageTextBlocks)
            {
                textBlock.Foreground = (Brush) Application.Current.Resources["DefaultTextColor"];
            }
        }

        private void showSymbols()
        {
            this.clearSymbol.Visibility = Visibility.Visible;
            this.saveSymbol.Visibility = Visibility.Visible;
        }

        private void hideSymbols()
        {
            this.clearSymbol.Visibility = Visibility.Collapsed;
            this.saveSymbol.Visibility = Visibility.Collapsed;
        }

        private void hideReadyTextAndPointsBox()
        {
            this.readyTextBlock.Visibility = Visibility.Collapsed;
            this.pointsTextBox.Visibility = Visibility.Collapsed;
        }

        private void hideDrawTriangulationButton()
        {
            this.drawTriangulationButton.Visibility = Visibility.Collapsed;
            this.drawTriangulationButton.IsEnabled = false;
        }

        private void TriangulationImage_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.setRecentImageAsSelectedImage(sender);
            this.updateRecentImageTextColor(this.triangulationImageTextBlock);
        }

        private void SobelImage_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.setRecentImageAsSelectedImage(sender);
            this.updateRecentImageTextColor(this.sobelImageTextBlock);
        }

        private void SourceImage_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.setRecentImageAsSelectedImage(sender);
            this.updateRecentImageTextColor(this.sourceImageTextBlock);
        }

        private void setRecentImageAsSelectedImage(object sender)
        {
            var image = sender as Image;
            this.recentImage.Source = image?.Source;
        }

        private void SourceImage_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            this.setTextHoverColor(this.sourceImageTextBlock);

        }

        private void SourceImage_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            this.resetTextHoverColor();
        }

        private void SobelImage_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            this.setTextHoverColor(this.sobelImageTextBlock);
        }

        private void SobelImage_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            this.resetTextHoverColor();
        }

        private void TriangulationImage_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            this.setTextHoverColor(this.triangulationImageTextBlock);
        }

        private void TriangulationImage_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            this.resetTextHoverColor();
        }

        private void setTextHoverColor(TextBlock textBlock)
        {
            textBlock.Foreground = (Brush) Application.Current.Resources["HoverTextColor"];
        }

        private void resetTextHoverColor()
        {
            this.resetTextBlocksColor();
            switch (this.recentImageType)
            {
                case ImageType.SourceImage:
                    this.updateRecentImageTextColor(this.sourceImageTextBlock);
                    break;
                case ImageType.SobelImage:
                    this.updateRecentImageTextColor(this.sobelImageTextBlock);
                    break;
                case ImageType.TriangulationImage:
                    this.updateRecentImageTextColor(this.triangulationImageTextBlock);
                    break;
                default:
                    this.resetTextBlocksColor();
                    break;
            }
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

        #endregion
    }
}
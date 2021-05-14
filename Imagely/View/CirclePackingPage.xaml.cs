using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Imagely.Enums;
using Imagely.Model;
using Imagely.Model.CirclePacking;
using Imagely.Viewmodel;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Imagely.View
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CirclePackingPage : Page
    {
        #region Data members

        private const string DropText = "Drop File Here";

        private readonly CirclePackingViewModel viewModel;
        private readonly TextBlock[] imageTextBlocks;
        private ImageType recentImageType;

        #endregion

        #region Properties

        private Picture SourcePicture => this.viewModel.SourcePicture;

        private Picture ModifiedPicture => this.viewModel.CopiedPicture;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="CirclePackingPage" /> class.
        /// </summary>
        public CirclePackingPage()
        {
            this.InitializeComponent();
            this.viewModel = new CirclePackingViewModel();
            this.imageTextBlocks = new[] {
                this.sourceImageTextBlock, this.segmentedImageTextBlock, this.circlePackingImageTextBlock
            };
            this.navigationView.SelectedItem = this.navigationView.MenuItems[5];
        }

        #endregion

        #region Methods

        private async Task<bool> openSourcePicture()
        {
            try
            {
                var openSuccessful = await this.viewModel.OpenSourceImage();
                if (!openSuccessful)
                {
                    return false;
                }

                await this.writePictureToImage(this.SourcePicture, this.sourceImage);
                this.recentImage.Source = this.SourcePicture.ModifiedImage;
                this.updateRecentImageTextColor(this.sourceImageTextBlock);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task writePictureToImage(Picture picture, Image image)
        {
            if (picture == null)
            {
                return;
            }

            using (var writeStream = picture.ModifiedImage.PixelBuffer.AsStream())
            {
                await writeStream.WriteAsync(picture.Pixels, 0,
                    picture.Pixels.Length);
                image.Source = picture.ModifiedImage;
            }
        }

        private async void DrawCirclePackingButton_Click(object sender, RoutedEventArgs e)
        {
            this.progressRing.IsActive = true;

            if (this.segmentedImage.Source == null)
            {
                await this.applyImageSegmentation();
            }

            this.setCanvasBoundary();
            this.circlePackingCanvas.Children.Clear();
            await Task.Run(() => { this.viewModel.DrawCirclePacking(this.circlePackingCanvas); });
            await this.convertCanvasToImage();
            this.circlePackingCanvas.Children.Clear();

            this.progressRing.IsActive = false;
        }

        private async Task applyImageSegmentation()
        {
            ImageSegmentation imageSegmentation = new ImageSegmentation(this.ModifiedPicture);
            await imageSegmentation.ApplyImageSegmentation(6, 10);
        }

        private void setCanvasBoundary()
        {
            var height = this.ModifiedPicture.Width;
            var width = this.ModifiedPicture.Height;
            this.canvasBoundaryRect.Rect = new Rect(0, 0, height, width);
        }

        private async Task convertCanvasToImage()
        {
            var renderTargetBitmap = new RenderTargetBitmap();
            await renderTargetBitmap.RenderAsync(this.circlePackingCanvas);
            this.circlePackingImage.Source = renderTargetBitmap;
            this.recentImage.Source = renderTargetBitmap;
            this.updateRecentImageTextColor(this.circlePackingImageTextBlock);
        }

        private async void FileDropArea_Drop(object sender, DragEventArgs dragEvent)
        {
            if (!await this.openImage(dragEvent) && this.sourceImage.Source == null)
            {
                this.showDropArea();
            }
            else
            {
                this.showTriangulationUi();
            }
        }

        private void showTriangulationUi()
        {
            this.hideDropArea();
            this.showDrawCirclePackingButton();
            this.showReadyText();
            this.dropAreaRectangle.Visibility = Visibility.Collapsed;
        }

        private async Task<bool> openImage(DragEventArgs dragEvent)
        {
            if (!await this.viewModel.OpenDraggedImage(dragEvent))
            {
                return false;
            }

            await this.writePictureToImage(this.SourcePicture, this.sourceImage);
            this.recentImage.Source = this.SourcePicture.ModifiedImage;
            return true;
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
                else if (textBlock == this.segmentedImageTextBlock)
                {
                    this.segmentedImageTextBlock.Foreground =
                        (Brush) Application.Current.Resources["HighlightedTextColor"];
                    this.recentImageType = ImageType.SobelImage;
                }
                else
                {
                    this.circlePackingImageTextBlock.Foreground =
                        (Brush) Application.Current.Resources["HighlightedTextColor"];
                    this.recentImageType = ImageType.TriangulationImage;
                }
            }
        }

        private async void BrowseFilesButton_Click(object sender, RoutedEventArgs e)
        {
            if (!await this.openSourcePicture() && this.sourceImage.Source == null)
            {
                this.showDropArea();
            }
            else
            {
                this.showTriangulationUi();
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
            Frame.Navigate(typeof(SteganographyPage));
        }

        private void AbstractTriangulationArt_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(AbstractTriangulationPage));
        }

        private void CirclePacking_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(CirclePackingPage));
        }

        private void showReadyText()
        {
            this.readyTextBlock.Visibility = Visibility.Visible;
        }

        private void showDrawCirclePackingButton()
        {
            this.drawCirclePackingButton.Visibility = Visibility.Visible;
            this.drawCirclePackingButton.IsEnabled = true;
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
            this.viewModel.Clear();
            this.circlePackingCanvas.Children.Clear();
            this.recentImage.Source = null;
            this.sourceImage.Source = null;
            this.segmentedImage.Source = null;
            this.circlePackingImage.Source = null;
            this.resetView();
        }

        private void resetView()
        {
            this.showDropArea();
            this.hideDrawCirclePackingButton();
            this.hideReadyText();
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

        private void hideReadyText()
        {
            this.readyTextBlock.Visibility = Visibility.Collapsed;
        }

        private void hideDrawCirclePackingButton()
        {
            this.drawCirclePackingButton.Visibility = Visibility.Collapsed;
            this.drawCirclePackingButton.IsEnabled = false;
        }

        private void CirclePackingImage_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.setRecentImageAsSelectedImage(sender);
            this.updateRecentImageTextColor(this.circlePackingImageTextBlock);
        }

        private void SegmentedImage_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.setRecentImageAsSelectedImage(sender);
            this.updateRecentImageTextColor(this.segmentedImageTextBlock);
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

        private void SegmentedImage_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            this.setTextHoverColor(this.segmentedImageTextBlock);
        }

        private void SegmentedImage_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            this.resetTextHoverColor();
        }

        private void CirclePackingImage_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            this.setTextHoverColor(this.circlePackingImageTextBlock);
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
                    this.updateRecentImageTextColor(this.segmentedImageTextBlock);
                    break;
                case ImageType.TriangulationImage:
                    this.updateRecentImageTextColor(this.circlePackingImageTextBlock);
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

        private void navigationView_OnBackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private void pointsTextBox_OnBeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            args.Cancel = args.NewText.Any(c => !char.IsDigit(c));
        }

        private void AbstractCircle_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(AbstractCirclePackingPage));
        }

        #endregion
    }
}
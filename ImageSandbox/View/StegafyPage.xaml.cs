using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using GroupCStegafy.Enums;
using GroupCStegafy.Model;
using GroupCStegafy.Viewmodel;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace GroupCStegafy.View
{
    /// <summary>
    ///     Defines the StegafyPage class.
    /// </summary>
    public sealed partial class StegafyPage
    {
        #region Data members

        private const string DropSourceFileTextDisplay = "Drag and Drop Image Here";
        private const string DropHiddenFileTextDisplay = "Drag and Drop Image or Text Here";
        private const string HiddenTextDisplay = "Hidden Text";
        private const string HiddenImageDisplay = "Hidden Image";
        private const string ExtractedTextDisplay = "Extracted Text";
        private const string ExtractedImageDisplay = "Extracted Image";
        private const string EncryptedImageTextDisplay = "Encrypted Image";
        private const string DecryptedImageTextDisplay = "Decrypted Image";
        private const string EncryptedTextDisplay = "Encrypted Text";
        private const string DecryptedTextDisplay = "Decrypted Text";
        private const string DropTextDisplay = "Drop File Here";

        private readonly StegafyPageViewModel viewModel;
        private readonly TextBlock[] textBlocks;
        private ImageType recentImageType;

        #endregion

        #region Properties

        private Picture SourcePicture => this.viewModel.SourcePicture;
        private Picture HiddenPicture => this.viewModel.HiddenPicture;
        private Picture EmbeddedPicture => this.viewModel.EmbeddedPicture;
        private Picture ExtractedPicture => this.viewModel.ExtractedPicture;
        private Picture DecryptedPicture => this.viewModel.DecryptedPicture;
        private string HiddenText => this.viewModel.HiddenText;
        private FileType HiddenFileType => this.viewModel.HiddenFileType;
        private EncryptionType EncryptionType => this.viewModel.EncryptionType;
        private string ExtractedText => this.viewModel.ExtractedText;
        private string DecryptedText => this.viewModel.DecryptedText;
        private bool IsHiddenTextTooBig => this.viewModel.IsHiddenTextTooBig;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="StegafyPage" /> class.
        /// </summary>
        public StegafyPage()
        {
            this.InitializeComponent();
            this.viewModel = new StegafyPageViewModel();
            this.textBlocks = new[] {
                this.sourceTextBlock, this.hiddenTextBlock, this.embeddedTextBlock, this.extractedTextBlock
            };
            this.navigationView.SelectedItem = this.navigationView.MenuItems[2];
        }

        #endregion

        #region Methods

        private async void BrowseFilesButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.sourceImage.Source == null)
            {
                if (!await this.openSourcePicture() && this.sourceImage.Source == null)
                {
                    this.showDropArea();
                }
                else
                {
                    this.updateRecentImageTextColor(this.sourceTextBlock);
                    this.changeDragAndDropTextBlock(DropHiddenFileTextDisplay);
                }
            }
            else
            {
                if (!await this.openHiddenPicture() && this.hiddenImage.Source == null)
                {
                    this.showDropArea();
                }
                else
                {
                    this.hideDropArea();
                    this.determineSettingsToDisplay();
                    this.updateRecentImageTextColor(this.hiddenTextBlock);
                    this.hideDropAreaRectangle();
                }
            }
        }

        private async void FileDropArea_Drop(object sender, DragEventArgs dragEvent)
        {
            //TODO add error handling for folders, etc.
            var fileType = await this.viewModel.CheckFileType(dragEvent);
            if (fileType == FileType.Picture && this.sourceImage.Source == null)
            {
                await this.openDraggedSourceImage(dragEvent);
                this.updateRecentImageTextColor(this.sourceTextBlock);
                this.showDropArea();
                this.changeDragAndDropTextBlock(DropHiddenFileTextDisplay);
            }
            else if (fileType == FileType.Picture && this.sourceImage.Source != null && await this.openDraggedHiddenImage(dragEvent))
            {
                this.updateRecentImageTextColor(this.hiddenTextBlock);
                this.hideDropAreaRectangle();
                this.showEncryptionTypeSettings();
                this.showEmbedButton();
            }
            else if (fileType == FileType.Text && this.sourceImage.Source != null)
            {
                await this.openDraggedHiddenText(dragEvent);
                this.updateRecentImageTextColor(this.hiddenTextBlock);
                this.hideDropAreaRectangle();
                this.determineSettingsToDisplay();
                this.showEmbedButton();
            }
            else
            {
                this.showDropArea();
            }
        }

        private void FileDropArea_DragLeave(object sender, DragEventArgs e)
        {
            if (this.dropAreaRectangle.Visibility == Visibility.Visible)
            {
                this.showDropArea();
            }
        }

        private void FileDropArea_DragOver(object sender, DragEventArgs dragEvent)
        {
            if (this.dropAreaRectangle.Visibility == Visibility.Visible)
            {
                this.hideDropArea();
                dragEvent.AcceptedOperation = DataPackageOperation.Copy;
                if (dragEvent.DragUIOverride != null)
                {
                    dragEvent.DragUIOverride.Caption = DropTextDisplay;
                    dragEvent.DragUIOverride.IsCaptionVisible = true;
                    dragEvent.DragUIOverride.IsContentVisible = true;
                }
            }
        }

        private void BpccSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            this.viewModel?.SetBitsPerColorChannelAmount((int) this.bpccSlider.Value);
        }

        private void UnencryptedRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (this.viewModel != null)
            {
                this.viewModel.SetEncryptionType(EncryptionType.Unencrypted);
                this.determineSettingsToDisplay();
            }
            //TODO hook this up to do it when encryption type changed
        }
        
        private void EncryptedRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (this.viewModel != null)
            {
                this.viewModel.SetEncryptionType(EncryptionType.Encrypted);
                this.determineSettingsToDisplay();
            }
            //TODO hook this up to do it when encryption type changed
        }

        private async void EncryptedTextRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (this.viewModel != null)
            {
                if (this.HiddenFileType == FileType.Text)
                {
                    this.recentTextBox.Text = this.ExtractedText;
                    this.extractedTextBox.Text = this.ExtractedText;
                }
                else
                {
                    using (var writeStream = this.ExtractedPicture.ModifiedImage.PixelBuffer.AsStream())
                    {
                        await writeStream.WriteAsync(this.ExtractedPicture.Pixels, 0, this.ExtractedPicture.Pixels.Length);
                        this.extractedImage.Source = this.ExtractedPicture.ModifiedImage;
                    }
                    this.recentImage.Source = this.ExtractedPicture.ModifiedImage;
                }
            }
        }

        private async void DecryptedTextRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (this.viewModel != null)
            {
                if (this.HiddenFileType == FileType.Text)
                {
                    this.viewModel.DecryptText();
                    this.recentTextBox.Text = this.DecryptedText;
                    this.extractedTextBox.Text = this.DecryptedText;
                }
                else
                {
                    this.viewModel.DecryptImage();
                    using (var writeStream = this.DecryptedPicture.ModifiedImage.PixelBuffer.AsStream())
                    {
                        await writeStream.WriteAsync(this.DecryptedPicture.Pixels, 0, this.DecryptedPicture.Pixels.Length);
                        this.extractedImage.Source = this.DecryptedPicture.ModifiedImage;
                    }
                    this.recentImage.Source = this.DecryptedPicture.ModifiedImage;
                }
            }
        }

        private async void EmbedButton_Click(object sender, RoutedEventArgs e)
        {
            this.viewModel.SetEncryptionKeyword(this.keywordTextBox.Text);
            if (await this.viewModel.EmbedFile())
            {
                using (var writeStream = this.EmbeddedPicture.ModifiedImage.PixelBuffer.AsStream())
                {
                    await writeStream.WriteAsync(this.EmbeddedPicture.Pixels, 0, this.EmbeddedPicture.Pixels.Length);
                    this.embeddedImage.Source = this.EmbeddedPicture.ModifiedImage;
                }

                this.updateRecentImageTextColor(this.embeddedTextBlock);
                this.showExtractButton();
                this.hideAllSettings();
                this.switchRecentDisplayToImage();
                this.recentImage.Source = this.EmbeddedPicture.ModifiedImage;
            }

            this.checkHiddenTextSize();
        }

        private void checkHiddenTextSize()
        {
            if (this.IsHiddenTextTooBig)
            {
                this.hideAllSettings();
                this.showDropArea();
                this.showDropAreaRectangle();
                this.recentTextBox.Text = string.Empty;
                this.hiddenTextBox.Text = string.Empty;
            }
        }

        private async void ExtractButton_Click(object sender, RoutedEventArgs e)
        {
            this.viewModel.ExtractFile();
            if (this.HiddenFileType == FileType.Picture)
            {
                using (var writeStream = this.ExtractedPicture.ModifiedImage.PixelBuffer.AsStream())
                {
                    await writeStream.WriteAsync(this.ExtractedPicture.Pixels, 0, this.ExtractedPicture.Pixels.Length);
                    this.extractedImage.Source = this.ExtractedPicture.ModifiedImage;
                }

                this.recentImage.Source = this.ExtractedPicture.ModifiedImage;
            }
            else
            {
                this.extractedTextBox.Text = this.viewModel.ExtractedText;
                this.recentTextBox.Text = this.viewModel.ExtractedText;
                this.switchRecentDisplayToText();
            }

            this.updateRecentImageTextColor(this.extractedTextBlock);
            this.hideExtractButton();
            this.showDecryptionSettings();
        }

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
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> openHiddenPicture()
        {
            try
            {
                if (await this.viewModel.OpenHiddenFile())
                {
                    if (this.HiddenFileType == FileType.Text)
                    {
                        this.switchToTextMode();
                        this.hiddenTextBox.Text = this.HiddenText;
                        this.recentTextBox.Text = this.HiddenText;
                        return true;
                    }

                    using (var writeStream = this.HiddenPicture.ModifiedImage.PixelBuffer.AsStream())
                    {
                        await writeStream.WriteAsync(this.HiddenPicture.Pixels, 0, this.HiddenPicture.Pixels.Length);
                        this.hiddenImage.Source = this.HiddenPicture.ModifiedImage;
                    }

                    this.recentImage.Source = this.HiddenPicture.ModifiedImage;
                    this.switchToImageMode();

                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private async Task openDraggedSourceImage(DragEventArgs dragEvent)
        {
            await this.viewModel.openDraggedFile(dragEvent, this.SourcePicture);

            using (var writeStream = this.SourcePicture.ModifiedImage.PixelBuffer.AsStream())
            {
                await writeStream.WriteAsync(this.SourcePicture.Pixels, 0, this.SourcePicture.Pixels.Length);
                this.sourceImage.Source = this.SourcePicture.ModifiedImage;
            }

            this.recentImage.Source = this.SourcePicture.ModifiedImage;
        }

        private async Task<bool> openDraggedHiddenImage(DragEventArgs dragEvent)
        {
            if (await this.viewModel.openDraggedFile(dragEvent, this.HiddenPicture))
            {
                using (var writeStream = this.HiddenPicture.ModifiedImage.PixelBuffer.AsStream())
                {
                    await writeStream.WriteAsync(this.HiddenPicture.Pixels, 0, this.HiddenPicture.Pixels.Length);
                    this.hiddenImage.Source = this.HiddenPicture.ModifiedImage;
                }

                this.recentImage.Source = this.HiddenPicture.ModifiedImage;
                this.switchToImageMode();
                return true;
            }

            return false;
        }

        private async Task openDraggedHiddenText(DragEventArgs dragEvent)
        {
            await this.viewModel.OpenDraggedText(dragEvent);
            this.hiddenTextBox.Text = this.HiddenText;
            this.recentTextBox.Text = this.HiddenText;
            this.switchToTextMode();
        }

        private void updateRecentImageTextColor(TextBlock textBlock)
        {
            this.resetTextBlocksColor();
            if (textBlock != null)
            {
                if (textBlock == this.sourceTextBlock)
                {
                    this.sourceTextBlock.Foreground =
                        (Brush) Application.Current.Resources["HighlightedTextColor"];
                    this.recentImageType = ImageType.SourceImage;
                }
                else if (textBlock == this.hiddenTextBlock)
                {
                    this.hiddenTextBlock.Foreground = (Brush) Application.Current.Resources["HighlightedTextColor"];
                    this.recentImageType = ImageType.HiddenImage;
                }
                else if (textBlock == this.embeddedTextBlock)
                {
                    this.embeddedTextBlock.Foreground =
                        (Brush) Application.Current.Resources["HighlightedTextColor"];
                    this.recentImageType = ImageType.EmbeddedImage;
                }
                else
                {
                    this.extractedTextBlock.Foreground =
                        (Brush) Application.Current.Resources["HighlightedTextColor"];
                    this.recentImageType = ImageType.ExtractedImage;
                }
            }
        }

        private void resetTextBlocksColor()
        {
            foreach (var textBlock in this.textBlocks)
            {
                textBlock.Foreground = (Brush) Application.Current.Resources["DefaultTextColor"];
            }
        }

        private void switchToTextMode()
        {
            this.hiddenImage.Visibility = Visibility.Collapsed;
            this.hiddenTextBox.Visibility = Visibility.Visible;
            this.extractedImage.Visibility = Visibility.Collapsed;
            this.extractedTextBox.Visibility = Visibility.Visible;
            this.recentImage.Visibility = Visibility.Collapsed;
            this.recentTextBox.Visibility = Visibility.Visible;
            this.encryptedTextRadioButton.Content = EncryptedTextDisplay;
            this.decryptedTextRadioButton.Content = DecryptedTextDisplay;
            this.changeHiddenTextBlock(HiddenTextDisplay);
            this.changeExtractedTextBlock(ExtractedTextDisplay);
        }

        private void switchToImageMode()
        {
            this.hiddenImage.Visibility = Visibility.Visible;
            this.hiddenTextBox.Visibility = Visibility.Collapsed;
            this.extractedImage.Visibility = Visibility.Visible;
            this.extractedTextBox.Visibility = Visibility.Collapsed;
            this.recentImage.Visibility = Visibility.Visible;
            this.recentTextBox.Visibility = Visibility.Collapsed;
            this.encryptedTextRadioButton.Content = EncryptedImageTextDisplay;
            this.decryptedTextRadioButton.Content = DecryptedImageTextDisplay;
            this.changeHiddenTextBlock(HiddenImageDisplay);
            this.changeExtractedTextBlock(ExtractedImageDisplay);
        }

        private void switchRecentDisplayToImage()
        {
            this.recentImage.Visibility = Visibility.Visible;
            this.recentTextBox.Visibility = Visibility.Collapsed;
        }

        private void switchRecentDisplayToText()
        {
            this.recentImage.Visibility = Visibility.Collapsed;
            this.recentTextBox.Visibility = Visibility.Visible;
        }

        private void changeHiddenTextBlock(string text)
        {
            this.hiddenTextBlock.Text = text;
        }

        private void changeExtractedTextBlock(string text)
        {
            this.extractedTextBlock.Text = text;
        }

        private void showDropArea()
        {
            this.dragAndDropTextBlock.Visibility = Visibility.Visible;
            this.orTextBlock.Visibility = Visibility.Visible;
            this.browseFilesButton.Visibility = Visibility.Visible;
            this.browseFilesButton.IsEnabled = true;
        }

        private void hideDropArea()
        {
            this.dragAndDropTextBlock.Visibility = Visibility.Collapsed;
            this.orTextBlock.Visibility = Visibility.Collapsed;
            this.browseFilesButton.Visibility = Visibility.Collapsed;
            this.browseFilesButton.IsEnabled = false;
        }

        private void showDropAreaRectangle()
        {
            this.dropAreaRectangle.Visibility = Visibility.Visible;
        }

        private void hideDropAreaRectangle()
        {
            this.dropAreaRectangle.Visibility = Visibility.Collapsed;
        }

        private void changeDragAndDropTextBlock(string text)
        {
            this.dragAndDropTextBlock.Text = text;
        }

        private void NavigationView_Loaded(object sender, RoutedEventArgs e)
        {
            this.navigationView.IsPaneOpen = false;
        }

        private void determineSettingsToDisplay()
        {
            this.showEncryptionTypeSettings();
            this.showEmbedButton();
            if (this.HiddenFileType == FileType.Picture)
            {
                this.hideBitsPerColorChannelSettings();
                this.hideBitsPerColorChannelSettings();
            }
            else if (this.HiddenFileType == FileType.Text)
            {
                this.showBitsPerColorChannelSettings();
                if (this.EncryptionType == EncryptionType.Encrypted)
                {
                    this.showEncryptionKeywordSettings();
                }
                else
                {
                    this.hideEncryptionKeywordSettings();
                }
            }
        }

        private void hideAllSettings()
        {
            this.hideEncryptionSettings();
            this.hideBitsPerColorChannelSettings();
            this.hideEncryptionKeywordSettings();
            this.hideEmbedButton();
        }

        private void showEmbedButton()
        {
            this.embedButton.Visibility = Visibility.Visible;
        }

        private void hideEmbedButton()
        {
            this.embedButton.Visibility = Visibility.Collapsed;
        }

        private void showExtractButton()
        {
            this.extractButton.Visibility = Visibility.Visible;
        }

        private void hideExtractButton()
        {
            this.extractButton.Visibility = Visibility.Collapsed;
        }

        private void showEncryptionTypeSettings()
        {
            this.encryptedRadioButton.Visibility = Visibility.Visible;
            this.unencryptedRadioButton.Visibility = Visibility.Visible;
            this.chooseEncryptionTextBlock.Visibility = Visibility.Visible;
        }

        private void hideEncryptionSettings()
        {
            this.encryptedRadioButton.Visibility = Visibility.Collapsed;
            this.unencryptedRadioButton.Visibility = Visibility.Collapsed;
            this.chooseEncryptionTextBlock.Visibility = Visibility.Collapsed;
        }

        private void showEncryptionKeywordSettings()
        {
            this.chooseKeywordTextBlock.Visibility = Visibility.Visible;
            this.keywordTextBox.Visibility = Visibility.Visible;
        }

        private void hideEncryptionKeywordSettings()
        {
            this.chooseKeywordTextBlock.Visibility = Visibility.Collapsed;
            this.keywordTextBox.Visibility = Visibility.Collapsed;
        }

        private void showBitsPerColorChannelSettings()
        {
            if (this.HiddenFileType == FileType.Text)
            {
                this.bpccSlider.Visibility = Visibility.Visible;
                this.bpccValueTextBlock.Visibility = Visibility.Visible;
                this.chooseBpccAmountTextBlock.Visibility = Visibility.Visible;
            }
        }

        private void hideBitsPerColorChannelSettings()
        {
            this.bpccSlider.Visibility = Visibility.Collapsed;
            this.bpccValueTextBlock.Visibility = Visibility.Collapsed;
            this.chooseBpccAmountTextBlock.Visibility = Visibility.Collapsed;
        }

        private void showDecryptionSettings()
        {
            if (this.EncryptionType == EncryptionType.Encrypted)
            {
                this.encryptedTextRadioButton.Visibility = Visibility.Visible;
                this.decryptedTextRadioButton.Visibility = Visibility.Visible;
            }
        }

        private void hideDecryptionSettings()
        {
            this.encryptedTextRadioButton.Visibility = Visibility.Collapsed;
            this.decryptedTextRadioButton.Visibility = Visibility.Collapsed;
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
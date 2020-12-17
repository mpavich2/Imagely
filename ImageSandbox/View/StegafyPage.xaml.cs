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
            this.hiddenTextBox.AddHandler(TappedEvent, new TappedEventHandler(this.HiddenTextBox_Tapped), true);
            this.extractedTextBox.AddHandler(TappedEvent, new TappedEventHandler(this.ExtractedTextBox_Tapped), true);
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
            var fileType = await this.viewModel.CheckFileType(dragEvent);
            if (fileType == FileType.Picture && this.sourceImage.Source == null)
            {
                await this.openDraggedSourceImage(dragEvent);
                this.updateRecentImageTextColor(this.sourceTextBlock);
                this.showDropArea();
                this.changeDragAndDropTextBlock(DropHiddenFileTextDisplay);
            }
            else if (fileType == FileType.Picture && this.sourceImage.Source != null &&
                     await this.openDraggedHiddenImage(dragEvent))
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
        }

        private void EncryptedRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (this.viewModel != null)
            {
                this.viewModel.SetEncryptionType(EncryptionType.Encrypted);
                this.determineSettingsToDisplay();
            }
        }

        private void EncryptedTextRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (this.viewModel != null)
            {
                this.updateRecentImageTextColor(this.extractedTextBlock);
                if (this.HiddenFileType == FileType.Text)
                {
                    this.switchRecentDisplayToText();
                    this.recentTextBox.Text = this.ExtractedText;
                    this.extractedTextBox.Text = this.ExtractedText;
                }
                else
                {
                    this.writePictureToImage(this.ExtractedPicture, this.extractedImage);
                    this.recentImage.Source = this.ExtractedPicture.ModifiedImage;
                    this.switchRecentDisplayToImage();
                }
            }
        }

        private async void writePictureToImage(Picture picture, Image image)
        {
            using (var writeStream = picture.ModifiedImage.PixelBuffer.AsStream())
            {
                await writeStream.WriteAsync(picture.Pixels, 0,
                    picture.Pixels.Length);
                image.Source = picture.ModifiedImage;
            }
        }

        private void DecryptedTextRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (this.viewModel != null)
            {
                this.updateRecentImageTextColor(this.extractedTextBlock);
                if (this.HiddenFileType == FileType.Text)
                {
                    this.viewModel.DecryptText();
                    this.switchRecentDisplayToText();
                    this.recentTextBox.Text = this.DecryptedText;
                    this.extractedTextBox.Text = this.DecryptedText;
                }
                else
                {
                    this.viewModel.DecryptImage();
                    this.writePictureToImage(this.DecryptedPicture, this.extractedImage);

                    this.recentImage.Source = this.DecryptedPicture.ModifiedImage;
                    this.switchRecentDisplayToImage();
                }
            }
        }

        private async void EmbedButton_Click(object sender, RoutedEventArgs e)
        {
            this.viewModel.SetEncryptionKeyword(this.keywordTextBox.Text);
            if (await this.viewModel.EmbedFile())
            {
                this.writePictureToImage(this.EmbeddedPicture, this.embeddedImage);
                this.updateRecentImageTextColor(this.embeddedTextBlock);
                this.showExtractUi();
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

        private void ExtractButton_Click(object sender, RoutedEventArgs e)
        {
            this.viewModel.ExtractFile();
            if (this.HiddenFileType == FileType.Picture)
            {
                this.writePictureToImage(this.ExtractedPicture, this.extractedImage);
                this.recentImage.Source = this.ExtractedPicture.ModifiedImage;
            }
            else
            {
                this.extractedTextBox.Text = this.viewModel.ExtractedText;
                this.recentTextBox.Text = this.viewModel.ExtractedText;
                this.switchRecentDisplayToText();
            }

            this.updateRecentImageTextColor(this.extractedTextBlock);
            this.hideExtractUi();
            this.showDecryptionSettings();
        }

        private async Task<bool> openSourcePicture()
        {
            if (await this.viewModel.OpenSourceImage())
            {
                this.writePictureToImage(this.SourcePicture, this.sourceImage);
                this.recentImage.Source = this.SourcePicture.ModifiedImage;
                return true;
            }

            return false;
        }

        private async Task<bool> openHiddenPicture()
        {
            if (!await this.viewModel.OpenHiddenFile())
            {
                return false;
            }
            if (this.HiddenFileType == FileType.Text)
            {
                this.hiddenTextBox.Text = this.HiddenText;
                this.recentTextBox.Text = this.HiddenText;
                this.switchToTextMode();
                this.showTextOverlays();
                return true;
            }

            this.writePictureToImage(this.HiddenPicture, this.hiddenImage);
            this.recentImage.Source = this.HiddenPicture.ModifiedImage;
            this.switchToImageMode();
            this.hideTextOverlays();

            return true;
        }

        private async Task openDraggedSourceImage(DragEventArgs dragEvent)
        {
            await this.viewModel.OpenDraggedFile(dragEvent, this.SourcePicture);
            this.writePictureToImage(this.SourcePicture, this.sourceImage);
            this.recentImage.Source = this.SourcePicture.ModifiedImage;
        }

        private async Task<bool> openDraggedHiddenImage(DragEventArgs dragEvent)
        {
            if (await this.viewModel.OpenDraggedFile(dragEvent, this.HiddenPicture))
            {
                this.writePictureToImage(this.HiddenPicture, this.hiddenImage);
                this.recentImage.Source = this.HiddenPicture.ModifiedImage;
                this.switchToImageMode();
                this.hideTextOverlays();
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
            this.showTextOverlays();
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
            this.encryptedTextRadioButton.Content = EncryptedTextDisplay;
            this.decryptedTextRadioButton.Content = DecryptedTextDisplay;
            this.switchRecentDisplayToText();
            this.changeHiddenTextBlock(HiddenTextDisplay);
            this.changeExtractedTextBlock(ExtractedTextDisplay);
        }

        private void switchToImageMode()
        {
            this.hiddenImage.Visibility = Visibility.Visible;
            this.hiddenTextBox.Visibility = Visibility.Collapsed;
            this.extractedImage.Visibility = Visibility.Visible;
            this.extractedTextBox.Visibility = Visibility.Collapsed;
            this.encryptedTextRadioButton.Content = EncryptedImageTextDisplay;
            this.decryptedTextRadioButton.Content = DecryptedImageTextDisplay;
            this.switchRecentDisplayToImage();
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

        private void showTextOverlays()
        {
            this.hiddenTextOverlay.Visibility = Visibility.Visible;
            this.extractedTextOverlay.Visibility = Visibility.Visible;
        }

        private void hideTextOverlays()
        {
            this.hiddenTextOverlay.Visibility = Visibility.Collapsed;
            this.extractedTextOverlay.Visibility = Visibility.Collapsed;
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
                this.switchButtonsToImageMode();
            }
            else if (this.HiddenFileType == FileType.Text)
            {
                this.showBitsPerColorChannelSettings();
                this.switchButtonsToTextMode();
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

        private void switchButtonsToTextMode()
        {
            this.embedButton.Content = "Embed Text";
            this.extractButton.Content = "Extract Text";
        }

        private void switchButtonsToImageMode()
        {
            this.embedButton.Content = "Embed Image";
            this.extractButton.Content = "Extract Image";
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

        private void showExtractUi()
        {
            this.extractButton.Visibility = Visibility.Visible;
            this.extractTextBlock.Visibility = Visibility.Visible;
        }

        private void hideExtractUi()
        {
            this.extractButton.Visibility = Visibility.Collapsed;
            this.extractTextBlock.Visibility = Visibility.Collapsed;
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
                this.extractedSuccessfullyEncryptedTextBlock.Visibility = Visibility.Visible;
                this.encryptedTextRadioButton.Visibility = Visibility.Visible;
                this.decryptedTextRadioButton.Visibility = Visibility.Visible;
            }
            else
            {
                this.extractedSuccessfullyUnencryptedTextBlock.Visibility = Visibility.Visible;
            }
        }

        private void hideDecryptionSettings()
        {
            this.encryptedTextRadioButton.Visibility = Visibility.Collapsed;
            this.decryptedTextRadioButton.Visibility = Visibility.Collapsed;
            this.extractedSuccessfullyUnencryptedTextBlock.Visibility = Visibility.Collapsed;
            this.extractedSuccessfullyEncryptedTextBlock.Visibility = Visibility.Collapsed;
        }

        private void resetTextHoverColor()
        {
            this.resetTextBlocksColor();
            switch (this.recentImageType)
            {
                case ImageType.SourceImage:
                    this.updateRecentImageTextColor(this.sourceTextBlock);
                    break;
                case ImageType.HiddenImage:
                    this.updateRecentImageTextColor(this.hiddenTextBlock);
                    break;
                case ImageType.EmbeddedImage:
                    this.updateRecentImageTextColor(this.embeddedTextBlock);
                    break;
                case ImageType.ExtractedImage:
                    this.updateRecentImageTextColor(this.extractedTextBlock);
                    break;
                default:
                    this.resetTextBlocksColor();
                    break;
            }
        }

        private void setTextHoverColor(TextBlock textBlock)
        {
            textBlock.Foreground = (Brush) Application.Current.Resources["HoverTextColor"];
        }

        private void setRecentImageAsSelectedImage(object sender)
        {
            var image = sender as Image;
            this.recentImage.Source = image?.Source;
        }

        private void setRecentTextAsSelectedText(TextBox textBox)
        {
            this.recentTextBox.Text = textBox.Text;
        }

        private bool isExtractedTextBoxVisible()
        {
            return this.extractedTextBox.Visibility == Visibility.Visible && this.extractedTextBox.Text != string.Empty;
        }

        private bool isHiddenTextBoxVisible()
        {
            return this.hiddenTextBox.Visibility == Visibility.Visible && this.hiddenTextBox.Text != string.Empty;
        }

        private void SourceImage_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            this.setTextHoverColor(this.sourceTextBlock);
        }

        private void SourceImage_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            this.resetTextHoverColor();
        }

        private void SourceImage_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.switchRecentDisplayToImage();
            this.setRecentImageAsSelectedImage(sender);
            this.updateRecentImageTextColor(this.sourceTextBlock);
        }

        private void HiddenImage_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            this.setTextHoverColor(this.hiddenTextBlock);
        }

        private void HiddenImage_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            this.resetTextHoverColor();
        }

        private void HiddenImage_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.switchRecentDisplayToImage();
            this.setRecentImageAsSelectedImage(sender);
            this.updateRecentImageTextColor(this.hiddenTextBlock);
        }

        private void EmbeddedImage_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            this.setTextHoverColor(this.embeddedTextBlock);
        }

        private void EmbeddedImage_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            this.resetTextHoverColor();
        }

        private void EmbeddedImage_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.switchRecentDisplayToImage();
            this.setRecentImageAsSelectedImage(sender);
            this.updateRecentImageTextColor(this.embeddedTextBlock);
        }

        private void ExtractedImage_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            this.setTextHoverColor(this.extractedTextBlock);
        }

        private void ExtractedImage_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            this.resetTextHoverColor();
        }

        private void ExtractedImage_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.switchRecentDisplayToImage();
            this.setRecentImageAsSelectedImage(sender);
            this.updateRecentImageTextColor(this.extractedTextBlock);
        }

        private void HiddenTextBox_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (this.isHiddenTextBoxVisible())
            {
                this.switchRecentDisplayToText();
                this.setRecentTextAsSelectedText(this.hiddenTextBox);
                this.updateRecentImageTextColor(this.hiddenTextBlock);
            }
        }

        private void HiddenTextBox_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (this.isHiddenTextBoxVisible())
            {
                this.setTextHoverColor(this.hiddenTextBlock);
            }
        }

        private void HiddenTextBox_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (this.isHiddenTextBoxVisible())
            {
                this.resetTextHoverColor();
            }
        }

        private void ExtractedTextBox_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (this.isExtractedTextBoxVisible())
            {
                this.switchRecentDisplayToText();
                this.setRecentTextAsSelectedText(this.extractedTextBox);
                this.updateRecentImageTextColor(this.extractedTextBlock);
            }
        }

        private void ExtractedTextBox_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (this.isExtractedTextBoxVisible())
            {
                this.setTextHoverColor(this.extractedTextBlock);
            }
        }

        private void ExtractedTextBox_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (this.isExtractedTextBoxVisible())
            {
                this.resetTextHoverColor();
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

        private async void SaveSymbol_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (this.recentImage.Source != null)
            {
                await this.viewModel.SaveImage(this.recentImage);
            }
        }

        private void ClearSymbol_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.resetImages();
            this.resetTextBoxes();
            this.resetView();
        }

        private void resetTextBoxes()
        {
            this.extractedTextBox.Text = string.Empty;
            this.hiddenTextBox.Text = string.Empty;
            this.recentTextBox.Text = string.Empty;
        }

        private void resetView()
        {
            this.resetRadioButtonsToDefault();
            this.resetEmbeddingSettingsToDefault();
            this.showDropArea();
            this.hideAllSettings();
            this.hideExtractUi();
            this.hideDecryptionSettings();
            this.dropAreaRectangle.Visibility = Visibility.Visible;
            this.resetImages();
            this.resetTextBlocksColor();
            this.hideBitsPerColorChannelSettings();
            this.hideEncryptionKeywordSettings();
            this.switchToImageMode();
            this.switchRecentDisplayToImage();
        }

        private void resetEmbeddingSettingsToDefault()
        {
            this.bpccSlider.Value = 1;
            this.keywordTextBox.Text = string.Empty;
        }

        private void resetRadioButtonsToDefault()
        {
            this.encryptedRadioButton.IsChecked = true;
            this.encryptedTextRadioButton.IsChecked = true;
            this.decryptedTextRadioButton.IsChecked = false;
            this.unencryptedRadioButton.IsChecked = false;
        }

        private void resetImages()
        {
            this.hiddenImage.Source = null;
            this.sourceImage.Source = null;
            this.embeddedImage.Source = null;
            this.extractedImage.Source = null;
            this.recentImage.Source = null;
        }

        private void SaveSymbol_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            this.saveSymbol.Foreground = (Brush)Application.Current.Resources["SymbolHoverColor"];
        }

        private void SaveSymbol_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            this.resetSymbolColors();
        }

        private void resetSymbolColors()
        {
            this.clearSymbol.Foreground = (Brush)Application.Current.Resources["SymbolColor"];
            this.saveSymbol.Foreground = (Brush)Application.Current.Resources["SymbolColor"];
        }

        private void ClearSymbol_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            this.clearSymbol.Foreground = (Brush)Application.Current.Resources["SymbolHoverColor"];
        }

        private void ClearSymbol_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            this.resetSymbolColors();
        }

        private void navigationView_OnBackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }

        #endregion
    }
}
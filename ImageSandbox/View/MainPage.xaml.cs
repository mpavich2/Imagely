using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using GroupCStegafy.Enums;
using GroupCStegafy.Model;
using GroupCStegafy.View.Dialogs;
using GroupCStegafy.Viewmodel;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace GroupCStegafy.View
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage
    {
        #region Data members

        private readonly MainPageViewModel viewModel;
        private bool isEncrypted;
        private bool isText;
        private int bpcc;

        #endregion

        #region Properties

        private Picture SourcePicture => this.viewModel.SourcePicture;

        private Picture HiddenPicture => this.viewModel.HiddenPicture;

        

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MainPage" /> class.
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();
            this.viewModel = new MainPageViewModel();
            this.setupFileTypeComboBox();
            this.setupBpccComboBox();
            this.setupEncryptionComboBox();
            this.setupDecryptionComboBox();
            this.isEncrypted = true;
            this.isText = false;
            this.bpcc = 1;
        }

        #endregion

        #region Methods

        private void setupEncryptionComboBox()
        {
            var itemCollection = this.encryptionComboBox.Items;
            if (itemCollection != null)
            {
                itemCollection.Add(EncryptionType.Encrypted);
                itemCollection.Add(EncryptionType.Unencrypted);
            }
        }

        private void setupDecryptionComboBox()
        {
            var itemCollection = this.viewEncryptedOrUnencryptedComboBox.Items;
            if (itemCollection != null)
            {
                itemCollection.Add(DecryptionType.Encrypted);
                itemCollection.Add(DecryptionType.Decrypted);
            }
        }

        private void setupBpccComboBox()
        {
            var items = this.bpccComboBox.Items;
            if (items != null)
            {
                items.Add(BitsPerColorChannels.One);
                items.Add(BitsPerColorChannels.Two);
                items.Add(BitsPerColorChannels.Three);
                items.Add(BitsPerColorChannels.Four);
                items.Add(BitsPerColorChannels.Five);
                items.Add(BitsPerColorChannels.Six);
                items.Add(BitsPerColorChannels.Seven);
                items.Add(BitsPerColorChannels.Eight);
            }
        }

        private void setupFileTypeComboBox()
        {
            var comboBoxItems = this.fileTypeComboBox.Items;
            if (comboBoxItems != null)
            {
                comboBoxItems.Add(FileType.Picture);
                comboBoxItems.Add(FileType.Text);
            }
        }

        private async void openSourceButton_Click(object sender, RoutedEventArgs e)
        {
            if (!await this.openSourcePicture() && this.sourceImage.Source == null)
            {
                this.sourceImageFlyout.ShowAt(this);
            }
        }

        private async void OpenHiddenButton_Click(object sender, RoutedEventArgs e)
        {
            if (!await this.openHiddenPicture() && this.hiddenImage.Source == null)
            {
                this.hiddenImageFlyout.ShowAt(this);
            }
        }

        private async void OpenEmbeddedButton_Click(object sender, RoutedEventArgs e)
        {
            if (!await this.openEmbeddedPicture() && this.embeddedImage.Source == null)
            {
                this.embeddedImageFlyout.ShowAt(this);
            }
        }

        private void saveEncryptedButton_Click(object sender, RoutedEventArgs e)
        {
            this.viewModel.SaveImage();
        }

        private void saveDecryptedButton_Click(object sender, RoutedEventArgs e)
        {
            var comboBoxSelectedItem = this.fileTypeComboBox.SelectedItem;
            if (comboBoxSelectedItem != null && comboBoxSelectedItem.Equals(FileType.Picture))
            {
                this.viewModel.SaveImage();
            }
            else
            {
                this.viewModel.SaveText(this.decryptedText.Text);
            }
        }

        private async void EmbedButton_Click(object sender, RoutedEventArgs e)
        {
            if (await this.checkSelectedSettings())
            {
                await this.embed();
            }
        }

        private async void ExtractButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!this.viewModel.ContainsHiddenMessage())
            {
                await this.showNoHiddenMessageDialog();
            }
            else
            {
                await this.extract();

                this.checkForEncryption();
                this.disableDecryptButton();
                this.enableSaveDecryptedButton();
            }
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

                this.enableOpenHiddenButton();
                this.clearEmbeddedAndDecoded();
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
                var comboBoxSelectedItem = this.fileTypeComboBox.SelectedItem;
                if (comboBoxSelectedItem != null && comboBoxSelectedItem.Equals(FileType.Picture))
                {
                    await this.viewModel.OpenHiddenFile();
                    using (var writeStream = this.HiddenPicture.ModifiedImage.PixelBuffer.AsStream())
                    {
                        await writeStream.WriteAsync(this.HiddenPicture.Pixels, 0, this.HiddenPicture.Pixels.Length);
                        this.hiddenImage.Source = this.HiddenPicture.ModifiedImage;
                    }
                }
                else
                {
                    this.hiddenText.Text = await this.viewModel.OpenHiddenText();
                }

                this.enableEncryptButton();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> openEmbeddedPicture()
        {
            try
            {
                await this.viewModel.OpenSourceImage();
                using (var writeStream = this.SourcePicture.ModifiedImage.PixelBuffer.AsStream())
                {
                    await writeStream.WriteAsync(this.SourcePicture.Pixels, 0, this.SourcePicture.Pixels.Length);
                    this.embeddedImage.Source = this.SourcePicture.ModifiedImage;
                }

                if (this.viewModel.IsEncrypted())
                {
                    this.enableDecryptionComboBox();
                }

                this.enableDecryptButton();
                this.enableSaveEncryptedButton();
                this.clearSourceAndHidden();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task embed()
        {
            var comboBoxSelectedItem = this.fileTypeComboBox.SelectedItem;
            if (comboBoxSelectedItem != null && comboBoxSelectedItem.Equals(FileType.Picture))
            {
                await this.embedImage();

                this.disableEncryptButton();
                this.disableOpenHiddenButton();
                this.enableDecryptButton();
                this.enableSaveEncryptedButton();
            }
            else
            {
                await this.embedText();
                if (!this.keywordTextBox.Text.Equals(string.Empty) || !this.isEncrypted)
                {
                    this.disableEncryptButton();
                    this.disableOpenHiddenButton();
                    this.enableDecryptButton();
                    this.enableSaveEncryptedButton();
                }
            }
        }

        private async Task extract()
        {
            if (!this.viewModel.IsText())
            {
                await this.extractImage();
            }
            else
            {
                this.extractText();
            }
        }

        private async Task embedText()
        {
            if (this.keywordTextBox.Text.Equals(string.Empty) && this.isEncrypted)
            {
                this.keywordFlyout.ShowAt(this);
            }
            else
            {
                this.viewModel.EmbedText(this.keywordTextBox.Text);
                if (!this.viewModel.IsMessageTooLarge())
                {
                    using (var writeStream = this.SourcePicture.ModifiedImage.PixelBuffer.AsStream())
                    {
                        await writeStream.WriteAsync(this.SourcePicture.Pixels, 0, this.SourcePicture.Pixels.Length);
                        this.embeddedImage.Source = this.SourcePicture.ModifiedImage;
                    }
                }
            }
        }

        private async Task embedImage()
        {
            this.viewModel.EmbedImage();
            using (var writeStream = this.SourcePicture.ModifiedImage.PixelBuffer.AsStream())
            {
                await writeStream.WriteAsync(this.SourcePicture.Pixels, 0, this.SourcePicture.Pixels.Length);
                this.embeddedImage.Source = this.SourcePicture.ModifiedImage;
            }
        }

        private void extractText()
        {
            this.hideImageShowText();
            var text = this.viewModel.ExtractText();
            var selectedItem = this.viewEncryptedOrUnencryptedComboBox.SelectedItem;
            if (selectedItem != null && selectedItem.Equals(DecryptionType.Decrypted) && this.viewModel.IsEncrypted())
            {
                text = this.viewModel.DecryptText();
                this.decryptedText.Text = text;
            }
            else
            {
                this.decryptedText.Text = text;
            }
        }

        private async Task extractImage()
        {
            this.hideTextShowImage();
            this.viewModel.ExtractImage();
            using (var writeStream = this.SourcePicture.ModifiedImage.PixelBuffer.AsStream())
            {
                await writeStream.WriteAsync(this.SourcePicture.Pixels, 0, this.SourcePicture.Pixels.Length);
                this.encryptedImage.Source = this.SourcePicture.ModifiedImage;
            }

            await this.decryptImage();

            this.showEncryptedImage();
        }

        private async Task decryptImage()
        {
            if (this.viewModel.IsEncrypted())
            {
                this.viewModel.DecryptImage();

                using (var writeStream = this.SourcePicture.ModifiedImage.PixelBuffer.AsStream())
                {
                    await writeStream.WriteAsync(this.SourcePicture.Pixels, 0, this.SourcePicture.Pixels.Length);
                    this.decryptedImage.Source = this.SourcePicture.ModifiedImage;
                }
            }
        }

        private async Task<bool> checkSelectedSettings()
        {
            if (this.bpccComboBox.SelectedItem == null)
            {
                await this.showInvalidBpccDialog();
                return false;
            }

            if (this.encryptionComboBox.SelectedItem == null)
            {
                await this.showInvalidEncryptionTypeDialog();
                return false;
            }

            if (!this.viewModel.CheckKeyword(this.keywordTextBox.Text) && this.isEncrypted && this.isText)
            {
                await this.showInvalidKeywordDialog();
                return false;
            }

            this.enableEncryptButton();
            return true;
        }

        private void checkForEncryption()
        {
            if (this.viewModel.IsEncrypted())
            {
                this.enableDecryptionComboBox();
            }
        }

        private void hideImageShowText()
        {
            this.decryptedImage.Source = null;
            this.decryptedImage.Visibility = Visibility.Collapsed;
            this.decryptedText.Visibility = Visibility.Visible;
        }

        private void hideTextShowImage()
        {
            this.decryptedText.Text = string.Empty;
            this.decryptedText.Visibility = Visibility.Collapsed;
            this.decryptedImage.Visibility = Visibility.Visible;
        }

        private void switchToTextBox()
        {
            this.hiddenText.Visibility = Visibility.Visible;
            this.decryptedText.Visibility = Visibility.Visible;
            this.hiddenImage.Visibility = Visibility.Collapsed;
            this.decryptedImage.Visibility = Visibility.Collapsed;
            this.encryptedImage.Visibility = Visibility.Collapsed;
            this.enableOpenHiddenButton();
            this.hiddenHeaderText.Text = "Hidden Text";
            this.isText = true;
        }

        private void switchToImageView()
        {
            this.hiddenText.Visibility = Visibility.Collapsed;
            this.decryptedText.Visibility = Visibility.Collapsed;
            this.hiddenImage.Visibility = Visibility.Visible;
            this.decryptedImage.Visibility = Visibility.Visible;
            this.encryptedImage.Visibility = Visibility.Visible;
            this.enableOpenHiddenButton();
            this.hiddenHeaderText.Text = "Hidden Picture";
            this.isText = false;
        }

        private void FileTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBoxSelectedItem = this.fileTypeComboBox.SelectedItem;
            if (comboBoxSelectedItem != null && comboBoxSelectedItem.Equals(FileType.Picture))
            {
                this.switchToImageView();
                this.disableKeywordTextBox();
                this.disableBpccComboBox();
            }
            else if (comboBoxSelectedItem != null && comboBoxSelectedItem.Equals(FileType.Text))
            {
                this.switchToTextBox();
                this.enableKeywordTextBox();
                this.enableBpccComboBox();
            }
            else
            {
                this.disableOpenHiddenButton();
            }

            this.viewModel.UpdateHeader(this.isEncrypted, this.isText, this.bpcc);
        }

        private void BpccComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = this.bpccComboBox.SelectedItem;
            if (item != null)
            {
                this.bpcc = (int) item;
            }

            if (this.embeddedImage.Source == null && this.decryptedImage.Source == null &&
                this.sourceImage.Source != null && this.hiddenText.Text != string.Empty)
            {
                this.enableEncryptButton();
            }

            this.viewModel.UpdateHeader(this.isEncrypted, this.isText, this.bpcc);
        }

        private void EncryptionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = this.encryptionComboBox.SelectedItem;
            if (selectedItem != null && selectedItem.Equals(EncryptionType.Encrypted))
            {
                this.isEncrypted = true;
                if (this.isText)
                {
                    this.enableKeywordTextBox();
                }
            }
            else if (selectedItem != null && selectedItem.Equals(EncryptionType.Unencrypted))
            {
                this.isEncrypted = false;
                this.disableKeywordTextBox();
            }

            this.viewModel.UpdateHeader(this.isEncrypted, this.isText, this.bpcc);
        }

        private void ViewEncryptedOrUnencryptedComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = this.viewEncryptedOrUnencryptedComboBox.SelectedItem;
            if (selectedItem != null && selectedItem.Equals(DecryptionType.Encrypted) &&
                this.embeddedImage.Source != null)
            {
                if (this.isText)
                {
                    this.decryptedText.Text = this.viewModel.EncryptedText;
                }
                else
                {
                    if (this.embeddedImage.Source != null)
                    {
                        this.showEncryptedImage();
                    }
                }
            }
            else
            {
                if (this.isText)
                {
                    this.decryptedText.Text = this.viewModel.DecryptText();
                }
                else
                {
                    if (this.embeddedImage.Source != null)
                    {
                        this.showDecryptedImage();
                    }
                }
            }
        }

        private void clearEmbeddedAndDecoded()
        {
            this.embeddedImage.Source = null;
            this.decryptedImage.Source = null;
            this.decryptedText.Text = string.Empty;
            this.disableSaveDecryptedButton();
            this.disableSaveEncryptedButton();
            this.disableDecryptionComboBox();
        }

        private void clearSourceAndHidden()
        {
            this.sourceImage.Source = null;
            this.hiddenImage.Source = null;
            this.hiddenText.Text = string.Empty;
            this.disableEncryptButton();
        }

        private void enableOpenHiddenButton()
        {
            if (this.sourceImage.Source != null && this.fileTypeComboBox.SelectedItem != null)
            {
                this.openHiddenButton.Visibility = Visibility.Visible;
                this.openHiddenButton.IsEnabled = true;
            }
        }

        private void disableBpccComboBox()
        {
            this.bpccComboBox.Visibility = Visibility.Collapsed;
            this.bpccComboBox.IsEnabled = false;
        }

        private void enableBpccComboBox()
        {
            this.bpccComboBox.Visibility = Visibility.Visible;
            this.bpccComboBox.IsEnabled = true;
        }

        private void disableOpenHiddenButton()
        {
            this.openHiddenButton.Visibility = Visibility.Collapsed;
            this.openHiddenButton.IsEnabled = false;
        }

        private void enableDecryptButton()
        {
            this.extractButton.Visibility = Visibility.Visible;
            this.extractButton.IsEnabled = true;
        }

        private void disableDecryptButton()
        {
            this.extractButton.Visibility = Visibility.Collapsed;
            this.extractButton.IsEnabled = false;
        }

        private void enableSaveEncryptedButton()
        {
            this.saveEncryptedButton.Visibility = Visibility.Visible;
            this.saveEncryptedButton.IsEnabled = true;
        }

        private void disableSaveEncryptedButton()
        {
            this.saveEncryptedButton.Visibility = Visibility.Collapsed;
            this.saveEncryptedButton.IsEnabled = false;
        }

        private void enableSaveDecryptedButton()
        {
            this.saveDecryptedButton.Visibility = Visibility.Visible;
            this.saveDecryptedButton.IsEnabled = true;
        }

        private void disableSaveDecryptedButton()
        {
            this.saveDecryptedButton.Visibility = Visibility.Collapsed;
            this.saveDecryptedButton.IsEnabled = false;
        }

        private void enableEncryptButton()
        {
            this.embedButton.Visibility = Visibility.Visible;
            this.embedButton.IsEnabled = true;
        }

        private void disableEncryptButton()
        {
            this.embedButton.Visibility = Visibility.Collapsed;
            this.embedButton.IsEnabled = false;
        }

        private void enableDecryptionComboBox()
        {
            this.viewEncryptedOrUnencryptedComboBox.Visibility = Visibility.Visible;
            this.viewEncryptedOrUnencryptedComboBox.IsEnabled = true;
        }

        private void disableDecryptionComboBox()
        {
            this.viewEncryptedOrUnencryptedComboBox.Visibility = Visibility.Collapsed;
            this.viewEncryptedOrUnencryptedComboBox.IsEnabled = false;
        }

        private void disableKeywordTextBox()
        {
            this.keywordTextBox.Visibility = Visibility.Collapsed;
            this.keywordTextBox.IsEnabled = false;
        }

        private void enableKeywordTextBox()
        {
            if (this.isEncrypted)
            {
                this.keywordTextBox.Visibility = Visibility.Visible;
                this.keywordTextBox.IsEnabled = true;
            }
        }

        private void showEncryptedImage()
        {
            this.encryptedImage.Visibility = Visibility.Visible;
            this.decryptedImage.Visibility = Visibility.Collapsed;
        }

        private void showDecryptedImage()
        {
            this.decryptedImage.Visibility = Visibility.Visible;
            this.encryptedImage.Visibility = Visibility.Collapsed;
        }

        private async Task showInvalidBpccDialog()
        {
            var dialog = new InvalidBpccDialog();
            await dialog.ShowAsync();
        }

        private async Task showInvalidKeywordDialog()
        {
            var dialog = new InvalidKeywordDialog();
            await dialog.ShowAsync();
        }

        private async Task showNoHiddenMessageDialog()
        {
            var dialog = new NoHiddenMessageDialog();
            await dialog.ShowAsync();
        }

        private async Task showInvalidEncryptionTypeDialog()
        {
            var dialog = new InvalidEncryptionTypeDialog();
            await dialog.ShowAsync();
        }

        #endregion
    }
}
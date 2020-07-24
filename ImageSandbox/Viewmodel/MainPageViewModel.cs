using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using GroupCStegafy.Enums;
using GroupCStegafy.Model;
using GroupCStegafy.Utils;
using GroupCStegafy.View.Dialogs;

namespace GroupCStegafy.Viewmodel
{
    /// <summary>
    ///     Defines the MainPageViewModel class.
    /// </summary>
    public class MainPageViewModel
    {
        #region Data members

        private string hiddenText;
        private string keyword;

        private readonly EmbeddingManager embeddingManager;
        private readonly HeaderManager headerManager;
        private readonly ExtractionManager extractionManager;

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the source picture.
        /// </summary>
        /// <value>
        ///     The source picture.
        /// </value>
        public Picture SourcePicture { get; }

        /// <summary>
        ///     Gets or sets the hidden picture.
        /// </summary>
        /// <value>
        ///     The hidden picture.
        /// </value>
        public Picture HiddenPicture { get; }

        /// <summary>
        ///     Gets the encrypted key.
        /// </summary>
        /// <value>
        ///     The encrypted key.
        /// </value>
        public string EncryptedText { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="MainPageViewModel" /> class.
        /// </summary>
        public MainPageViewModel()
        {
            this.SourcePicture = new Picture();
            this.HiddenPicture = new Picture();
            this.embeddingManager = new EmbeddingManager(this.HiddenPicture, this.SourcePicture);
            this.extractionManager = new ExtractionManager(this.SourcePicture);
            this.headerManager = new HeaderManager(this.SourcePicture);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Opens the source image.
        /// </summary>
        public async Task OpenSourceImage()
        {
            var sourceImageFile = await FileUtilities.SelectFile();
            var copyBitmapImage = await FileUtilities.MakeCopyOfTheImage(sourceImageFile);

            using (var fileStream = await sourceImageFile.OpenAsync(FileAccessMode.Read))
            {
                var decoder = await BitmapDecoder.CreateAsync(fileStream);
                var transform = new BitmapTransform {
                    ScaledWidth = Convert.ToUInt32(copyBitmapImage.PixelWidth),
                    ScaledHeight = Convert.ToUInt32(copyBitmapImage.PixelHeight)
                };
                this.SourcePicture.ModifiedImage =
                    new WriteableBitmap((int) decoder.PixelWidth, (int) decoder.PixelHeight);
                this.SourcePicture.DpiX = decoder.DpiX;
                this.SourcePicture.DpiY = decoder.DpiY;

                var pixelData = await decoder.GetPixelDataAsync(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Straight,
                    transform,
                    ExifOrientationMode.IgnoreExifOrientation,
                    ColorManagementMode.DoNotColorManage
                );

                var sourcePixels = pixelData.DetachPixelData();
                this.SourcePicture.Pixels = sourcePixels;
                this.SourcePicture.Width = decoder.PixelWidth;
                this.SourcePicture.Height = decoder.PixelHeight;
            }
        }

        /// <summary>
        ///     Opens the hidden image.
        /// </summary>
        public async Task OpenHiddenFile()
        {
            var hiddenImageFile = await FileUtilities.SelectFile();
            var copyBitmapImage = await FileUtilities.MakeCopyOfTheImage(hiddenImageFile);

            using (var fileStream = await hiddenImageFile.OpenAsync(FileAccessMode.Read))
            {
                var decoder = await BitmapDecoder.CreateAsync(fileStream);
                var transform = new BitmapTransform {
                    ScaledWidth = Convert.ToUInt32(copyBitmapImage.PixelWidth),
                    ScaledHeight = Convert.ToUInt32(copyBitmapImage.PixelHeight)
                };
                if (decoder.PixelWidth > this.SourcePicture.Width ||
                    decoder.PixelHeight > this.SourcePicture.Height)
                {
                    if (!await this.showIncompatibleImageDialog())
                    {
                        return;
                    }
                }

                this.HiddenPicture.ModifiedImage =
                    new WriteableBitmap((int) decoder.PixelWidth, (int) decoder.PixelHeight);

                var pixelData = await decoder.GetPixelDataAsync(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Straight,
                    transform,
                    ExifOrientationMode.IgnoreExifOrientation,
                    ColorManagementMode.DoNotColorManage
                );

                var hiddenPixels = pixelData.DetachPixelData();
                this.HiddenPicture.Pixels = hiddenPixels;
                this.HiddenPicture.Height = decoder.PixelHeight;
                this.HiddenPicture.Width = decoder.PixelWidth;
            }
        }

        /// <summary>
        ///     Opens the hidden key.
        /// </summary>
        /// <returns></returns>
        public async Task<string> OpenHiddenText()
        {
            var hiddenTextFile = await FileUtilities.SelectTextFile();
            var text = await FileIO.ReadTextAsync(hiddenTextFile);
            this.hiddenText = text;
            return text;
        }

        /// <summary>
        ///     Embeds the key.
        /// </summary>
        /// <param name="key">The key.</param>
        public void EmbedText(string key)
        {
            this.SourcePicture.Pixels = this.headerManager.SetHeaderForHiddenMessage();
            this.embeddingManager.EmbedText(this.hiddenText, key);
            this.SourcePicture.ModifiedImage =
                new WriteableBitmap((int) this.SourcePicture.Width, (int) this.SourcePicture.Height);
        }

        /// <summary>
        ///     Encrypts the image.
        /// </summary>
        public void EmbedImage()
        {
            this.SourcePicture.Pixels = this.headerManager.SetHeaderForHiddenMessage();
            this.embeddingManager.EmbedImage(this.SourcePicture.Pixels);
            this.SourcePicture.ModifiedImage =
                new WriteableBitmap((int) this.SourcePicture.Width, (int) this.SourcePicture.Height);
        }

        /// <summary>
        ///     Extracts this instance.
        /// </summary>
        /// <returns>
        ///     bool
        /// </returns>
        public bool IsText()
        {
            return this.extractionManager.IsText();
        }

        /// <summary>
        ///     Determines whether [contains hidden message].
        /// </summary>
        /// <returns>
        ///     <c>true</c> if [contains hidden message]; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsHiddenMessage()
        {
            return this.extractionManager.ContainsHiddenMessage();
        }

        /// <summary>
        ///     Determines whether [is message too large].
        /// </summary>
        /// <returns>
        ///     <c>true</c> if [is message too large]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsMessageTooLarge()
        {
            return this.embeddingManager.MessageTooLarge;
        }

        /// <summary>
        ///     Determines whether this instance is encrypted.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if this instance is encrypted; otherwise, <c>false</c>.
        /// </returns>
        public bool IsEncrypted()
        {
            return this.extractionManager.IsEncrypted() == EncryptionType.Encrypted;
        }

        /// <summary>
        ///     Decrypts the image.
        /// </summary>
        public void ExtractImage()
        {
            this.extractionManager.ExtractImage(this.SourcePicture.Pixels);
            this.SourcePicture.ModifiedImage =
                new WriteableBitmap((int) this.SourcePicture.Width, (int) this.SourcePicture.Height);
        }

        /// <summary>
        ///     Decrypts the image.
        /// </summary>
        public void DecryptImage()
        {
            DecryptionManager.DecryptPicture(this.SourcePicture);
            this.SourcePicture.ModifiedImage =
                new WriteableBitmap((int) this.SourcePicture.Width, (int) this.SourcePicture.Height);
        }

        /// <summary>
        ///     Decrypts the key.
        /// </summary>
        /// <returns>
        ///     Decrypted text
        /// </returns>
        public string DecryptText()
        {
            return DecryptionManager.DecryptText(this.EncryptedText, this.keyword);
        }

        /// <summary>
        ///     Decrypts the key.
        /// </summary>
        /// <returns>
        ///     Extracted text
        /// </returns>
        public string ExtractText()
        {
            var extractedText = this.extractionManager.ExtractText();
            this.keyword = this.extractionManager.KeyWord;
            this.EncryptedText = extractedText;
            return extractedText;
        }

        /// <summary>
        ///     Saves the Image
        /// </summary>
        public void SaveImage()
        {
            FileUtilities.SaveImage(this.SourcePicture);
        }

        /// <summary>
        ///     Saves the key.
        /// </summary>
        /// <param name="text">The key.</param>
        public void SaveText(string text)
        {
            FileUtilities.SaveText(text);
        }

        /// <summary>
        ///     Checks the keyword.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        ///     True if keyword contains only letters, False otherwise.
        /// </returns>
        public bool CheckKeyword(string key)
        {
            return Regex.IsMatch(key, @"^[a-zA-Z]+$");
        }

        /// <summary>
        ///     Updates the header.
        /// </summary>
        /// <param name="isEncrypted">if set to <c>true</c> [is encrypted].</param>
        /// <param name="isText">if set to <c>true</c> [is key].</param>
        /// <param name="bpcc">The BPCC.</param>
        public void UpdateHeader(bool isEncrypted, bool isText, int bpcc)
        {
            //this.headerManager.UpdateHeaderInfo(isEncrypted, isText, bpcc);
        }

        private async Task<bool> showIncompatibleImageDialog()
        {
            var dialog = new InvalidKeywordDialog();
            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                await this.OpenHiddenFile();
                return true;
            }

            return false;
        }

        #endregion
    }
}
﻿using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using GroupCStegafy.Constants;
using GroupCStegafy.Enums;
using GroupCStegafy.Model;
using GroupCStegafy.Utils;
using GroupCStegafy.View.Dialogs;

namespace GroupCStegafy.Viewmodel
{
    /// <summary>
    ///     Defines the StegafyPageViewModel class.
    /// </summary>
    public class StegafyPageViewModel
    {
        #region Data members

        private readonly EmbeddingManager embeddingManager;
        private readonly HeaderManager headerManager;
        private readonly ExtractionManager extractionManager;
        private string keyword;
        private const string MessageTooLongText = "Hidden image will not fit with any bits per color channel amount selected.";

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
        ///     Gets the hidden picture.
        /// </summary>
        /// <value>
        ///     The hidden picture.
        /// </value>
        public Picture HiddenPicture { get; }

        /// <summary>
        ///     Gets the embedded picture.
        /// </summary>
        /// <value>
        ///     The embedded picture.
        /// </value>
        public Picture EmbeddedPicture { get; }

        /// <summary>
        ///     Gets the extracted picture.
        /// </summary>
        /// <value>
        ///     The extracted picture.
        /// </value>
        public Picture ExtractedPicture { get; }

        /// <summary>
        /// Gets the decrypted picture.
        /// </summary>
        /// <value>
        /// The decrypted picture.
        /// </value>
        public Picture DecryptedPicture { get; private set; }

        /// <summary>
        ///     Gets the hidden text.
        /// </summary>
        /// <value>
        ///     The hidden text.
        /// </value>
        public string HiddenText { get; private set; }

        /// <summary>
        ///     Gets the type of the hidden file.
        /// </summary>
        /// <value>
        ///     The type of the hidden file.
        /// </value>
        public FileType HiddenFileType { get; private set; }

        /// <summary>
        /// Gets the type of the encryption.
        /// </summary>
        /// <value>
        /// The type of the encryption.
        /// </value>
        public EncryptionType EncryptionType { get; private set; }

        /// <summary>
        /// Gets the extracted text.
        /// </summary>
        /// <value>
        /// The extracted text.
        /// </value>
        public string ExtractedText { get; private set; }

        /// <summary>
        /// Gets the bits per color channel.
        /// </summary>
        /// <value>
        /// The bits per color channel.
        /// </value>
        public int BitsPerColorChannel { get; private set; }

        /// <summary>
        /// Gets the decrypted text.
        /// </summary>
        /// <value>
        /// The decrypted text.
        /// </value>
        public string DecryptedText { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is hidden text too big.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is hidden text too big; otherwise, <c>false</c>.
        /// </value>
        public bool IsHiddenTextTooBig { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="StegafyPageViewModel" /> class.
        /// </summary>
        public StegafyPageViewModel()
        {
            this.SourcePicture = new Picture();
            this.HiddenPicture = new Picture();
            this.EmbeddedPicture = new Picture();
            this.ExtractedPicture = new Picture();
            this.embeddingManager = new EmbeddingManager(this.HiddenPicture, this.EmbeddedPicture);
            this.extractionManager = new ExtractionManager(this.ExtractedPicture);
            this.headerManager = new HeaderManager(this.SourcePicture);
            this.BitsPerColorChannel = 1;
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
        ///     Opens the hidden file.
        /// </summary>
        public async Task<bool> OpenHiddenFile()
        {
            var value = true;
            var hiddenFile = await FileUtilities.SelectFile();
            if (this.checkFileType(hiddenFile) == FileType.Picture)
            {
                value = await this.openHiddenImage(hiddenFile);
                this.HiddenFileType = FileType.Picture;
            }
            else
            {
                this.HiddenText = await FileIO.ReadTextAsync(hiddenFile);
                this.HiddenFileType = FileType.Text;
            }

            return value;
        }

        /// <summary>
        ///     Opens the dragged file.
        /// </summary>
        /// <param name="dragEvent">The <see cref="DragEventArgs" /> instance containing the event data.</param>
        /// <param name="picture">The picture.</param>
        public async Task<bool> openDraggedFile(DragEventArgs dragEvent, Picture picture)
        {
            return await this.openDraggedImage(dragEvent, picture);
        }

        /// <summary>
        ///     Checks the type of the file.
        /// </summary>
        /// <param name="dragEvent">The <see cref="DragEventArgs" /> instance containing the event data.</param>
        /// <returns></returns>
        public async Task<FileType> CheckFileType(DragEventArgs dragEvent)
        {
            var storageFile = await this.getDroppedStorageFile(dragEvent);
            return this.checkFileType(storageFile);
        }

        /// <summary>
        ///     Opens the hidden key.
        /// </summary>
        /// <returns></returns>
        public async Task OpenDraggedText(DragEventArgs dragEvent)
        {
            var items = await dragEvent.DataView.GetStorageItemsAsync();
            var storageFile = items[0] as StorageFile;
            var text = await FileIO.ReadTextAsync(storageFile);
            this.HiddenText = text;
            this.HiddenFileType = FileType.Text;
        }

        /// <summary>
        /// Embeds the file.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> EmbedFile()
        {
            if (await this.keywordSettingsInvalid() || this.HiddenFileType == FileType.Text && await this.isTextFileTooBig())
            {
                return false;
            }
            this.headerManager.UpdateHeaderInfo(this.EncryptionType, this.HiddenFileType, this.BitsPerColorChannel);
            this.SourcePicture.Pixels = this.headerManager.SetHeaderForHiddenMessage();
            this.copySourcePicture(this.SourcePicture, this.EmbeddedPicture);
            if (this.HiddenFileType == FileType.Picture)
            {
                this.embeddingManager.EmbedImage(this.EmbeddedPicture.Pixels);
                this.EmbeddedPicture.ModifiedImage =
                    new WriteableBitmap((int) this.EmbeddedPicture.Width, (int) this.EmbeddedPicture.Height);
            }
            else
            {
                this.embeddingManager.EmbedText(this.HiddenText, this.keyword);
                this.EmbeddedPicture.ModifiedImage =
                    new WriteableBitmap((int) this.EmbeddedPicture.Width, (int) this.EmbeddedPicture.Height);
            }

            return true;
        }

        /// <summary>
        ///     Extracts the file from the image.
        /// </summary>
        public void ExtractFile()
        {
            this.copySourcePicture(this.EmbeddedPicture, this.ExtractedPicture);
            if (!this.extractionManager.IsText())
            {
                this.extractionManager.ExtractImage(this.ExtractedPicture.Pixels);
                this.ExtractedPicture.ModifiedImage =
                    new WriteableBitmap((int) this.ExtractedPicture.Width, (int) this.ExtractedPicture.Height);
            }
            else
            {
                var extractedText = this.extractionManager.ExtractText();
                this.keyword = this.extractionManager.KeyWord;
                this.ExtractedText = extractedText;
            }

            this.EncryptionType = this.extractionManager.IsEncrypted();
        }


        /// <summary>
        /// Decrypts the text.
        /// </summary>
        public void DecryptText()
        {
            if (this.DecryptedText == null)
            {
                this.DecryptedText = DecryptionManager.DecryptText(this.ExtractedText, this.keyword);
            }
        }

        /// <summary>
        /// Decrypts the image.
        /// </summary>
        public void DecryptImage()
        {
            if (this.DecryptedPicture == null)
            {
                this.DecryptedPicture = new Picture();
                this.copySourcePicture(this.ExtractedPicture, this.DecryptedPicture);
                DecryptionManager.DecryptPicture(this.DecryptedPicture);
                this.DecryptedPicture.ModifiedImage =
                    new WriteableBitmap((int)this.DecryptedPicture.Width, (int)this.DecryptedPicture.Height);
            }
        }

        /// <summary>
        ///     Sets the encryption text.
        /// </summary>
        /// <param name="text">The text.</param>
        public void SetEncryptionKeyword(string text)
        {
            this.keyword = text;
        }

        /// <summary>
        ///     Sets the type of the encryption.
        /// </summary>
        /// <param name="encryptionType">Type of the encryption.</param>
        public void SetEncryptionType(EncryptionType encryptionType)
        {
            this.EncryptionType = encryptionType;
        }

        /// <summary>
        ///     Sets the bits per color channel amount.
        /// </summary>
        /// <param name="amount">The amount.</param>
        public void SetBitsPerColorChannelAmount(int amount)
        {
            this.BitsPerColorChannel = amount;
        }

        private async Task<bool> openDraggedImage(DragEventArgs dragEvent, Picture picture)
        {
            if (dragEvent.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await dragEvent.DataView.GetStorageItemsAsync();
                var storageFile = items[0] as StorageFile;
                var copyBitmapImage = await FileUtilities.MakeCopyOfTheImage(storageFile);

                if (storageFile != null)
                {
                    using (var fileStream = await storageFile.OpenAsync(FileAccessMode.Read))
                    {
                        var decoder = await BitmapDecoder.CreateAsync(fileStream);
                        var transform = new BitmapTransform {
                            ScaledWidth = Convert.ToUInt32(copyBitmapImage.PixelWidth),
                            ScaledHeight = Convert.ToUInt32(copyBitmapImage.PixelHeight)
                        };
                        if (this.HiddenPicture.ModifiedImage == null && this.SourcePicture.ModifiedImage != null && await this.isHiddenImageTooLarge(decoder))
                        {
                            return false;
                        }
                        picture.ModifiedImage =
                            new WriteableBitmap((int) decoder.PixelWidth, (int) decoder.PixelHeight);
                        picture.DpiX = decoder.DpiX;
                        picture.DpiY = decoder.DpiY;

                        var pixelData = await decoder.GetPixelDataAsync(
                            BitmapPixelFormat.Bgra8,
                            BitmapAlphaMode.Straight,
                            transform,
                            ExifOrientationMode.IgnoreExifOrientation,
                            ColorManagementMode.DoNotColorManage
                        );

                        var sourcePixels = pixelData.DetachPixelData();
                        picture.Pixels = sourcePixels;
                        picture.Width = decoder.PixelWidth;
                        picture.Height = decoder.PixelHeight;
                    }
                }
            }

            return true;
        }

        private async Task<bool> openHiddenImage(StorageFile hiddenFile)
        {
            var copyBitmapImage = await FileUtilities.MakeCopyOfTheImage(hiddenFile);

            using (var fileStream = await hiddenFile.OpenAsync(FileAccessMode.Read))
            {
                var decoder = await BitmapDecoder.CreateAsync(fileStream);
                var transform = new BitmapTransform {
                    ScaledWidth = Convert.ToUInt32(copyBitmapImage.PixelWidth),
                    ScaledHeight = Convert.ToUInt32(copyBitmapImage.PixelHeight)
                };
                if (await this.isHiddenImageTooLarge(decoder))
                {
                    return false;
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

            return true;
        }

        private async Task<bool> isTextFileTooBig()
        {
            var textArray = this.HiddenText.StringToBitArray();
            var pixels = PictureConverter.ConvertBytesIntoBitArray(this.SourcePicture);
            if (textArray.Length > pixels.Length / ImageConstants.ByteLength * this.BitsPerColorChannel)
            {
                await this.showMessageCantFitDialog(textArray, pixels);
                return true;
            }

            return false;
        }

        private async Task<bool> isHiddenImageTooLarge(BitmapDecoder decoder)
        {
            if (decoder.PixelWidth > this.SourcePicture.Width ||
                decoder.PixelHeight > this.SourcePicture.Height)
            {
                await this.showInvalidHiddenImageDialog();
                return true;
            }

            return false;
        }

        private async Task<bool> keywordSettingsInvalid()
        {
            if (this.HiddenFileType == FileType.Text && this.EncryptionType == EncryptionType.Encrypted &&
                this.keyword == string.Empty)
            {
                await this.showInvalidKeywordDialog();
                return true;
            }

            return false;
        }

        private FileType checkFileType(StorageFile storageFile)
        {
            if (storageFile.FileType.Equals(".png") || storageFile.FileType.Equals(".bmp"))
            {
                return FileType.Picture;
            }

            return FileType.Text;
        }

        private async Task<StorageFile> getDroppedStorageFile(DragEventArgs dragEvent)
        {
            if (!dragEvent.DataView.Contains(StandardDataFormats.StorageItems))
            {
                //TODO add error handling
            }

            var items = await dragEvent.DataView.GetStorageItemsAsync();
            return items.First() as StorageFile;
        }

        private void copySourcePicture(Picture picture, Picture otherPicture)
        {
            otherPicture.Pixels = (byte[]) picture.Pixels.Clone();
            otherPicture.ModifiedImage = picture.ModifiedImage;
            otherPicture.Height = picture.Height;
            otherPicture.Width = picture.Width;
        }

        private int numNecessaryBits(char[] text, bool[] pixels)
        {
            var currentBpcc = this.BitsPerColorChannel;

            for (var i = currentBpcc; i <= ImageConstants.ByteLength; i++)
            {
                if (text.Length < pixels.Length / ImageConstants.ByteLength * i)
                {
                    return i;
                }
            }

            return 9;
        }

        private string adjustMessageCantFitText(char[] text, bool[] pixels)
        {
            return "Hidden image was too large for selected bits per color channel amount. Please select the " +
                   this.numNecessaryBits(text, pixels) + " bits per color channel option.";
        }

        private async Task showInvalidKeywordDialog()
        {
            var dialog = new InvalidKeywordDialog();
            await dialog.ShowAsync();
        }

        private async Task showInvalidHiddenImageDialog()
        {
            var dialog = new InvalidHiddenImageDialog();
            await dialog.ShowAsync();
        }

        private async Task showMessageCantFitDialog(char[] text, bool[] pixels)
        {
            var dialog = new InvalidMessageLengthDialog();
            if (this.numNecessaryBits(text, pixels) == ImageConstants.ByteLength + 1)
            {
                dialog.setErrorMessageText(MessageTooLongText);
                this.IsHiddenTextTooBig = true;
            }
            else
            {
                dialog.setErrorMessageText(this.adjustMessageCantFitText(text, pixels));
            }

            await dialog.ShowAsync();
        }

        #endregion
    }
}
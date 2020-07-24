using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using GroupCStegafy.Model;

namespace GroupCStegafy.Utils
{
    /// <summary>Defines the FileUtilities class.</summary>
    public static class FileUtilities
    {
        #region Methods


        /// <summary>
        /// Selects the file.
        /// </summary>
        /// <returns></returns>
        public static async Task<StorageFile> SelectFile()
        {
            var openPicker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };
            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".bmp");
            openPicker.FileTypeFilter.Add(".txt");
            var file = await openPicker.PickSingleFileAsync();

            return file;
        }

        /// <summary>
        ///     Selects the text file.
        /// </summary>
        /// <returns>
        ///     Selected file
        /// </returns>
        public static async Task<StorageFile> SelectTextFile()
        {
            var openPicker = new FileOpenPicker {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };
            openPicker.FileTypeFilter.Add(".txt");
            var file = await openPicker.PickSingleFileAsync();
            return file;
        }

        /// <summary>
        ///     Makes the copy of the image.
        /// </summary>
        /// <param name="imageFile">The image file.</param>
        /// <returns>
        ///     Copy of bitmap
        /// </returns>
        public static async Task<BitmapImage> MakeCopyOfTheImage(StorageFile imageFile)
        {
            IRandomAccessStream inputStream = await imageFile.OpenReadAsync();
            var newImage = new BitmapImage();
            newImage.SetSource(inputStream);
            return newImage;
        }

        /// <summary>
        ///     Saves the text.
        /// </summary>
        /// <param name="text">The text.</param>
        public static async void SaveText(string text)
        {
            var fileSavePicker = new FileSavePicker {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                SuggestedFileName = "New Document"
            };
            fileSavePicker.FileTypeChoices.Add("Plain Text", new List<string> {".txt"});
            var saveFile = await fileSavePicker.PickSaveFileAsync();

            if (saveFile != null)
            {
                CachedFileManager.DeferUpdates(saveFile);
                await FileIO.WriteTextAsync(saveFile, text);
                await CachedFileManager.CompleteUpdatesAsync(saveFile);
            }
        }

        /// <summary>
        ///     Saves the Image
        /// </summary>
        /// <param name="picture">The picture.</param>
        public static async void SaveImage(Picture picture)
        {
            var fileSavePicker = new FileSavePicker {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                SuggestedFileName = "New Image"
            };
            fileSavePicker.FileTypeChoices.Add("PNG files", new List<string> {".png"});
            var saveFile = await fileSavePicker.PickSaveFileAsync();

            if (saveFile != null)
            {
                var stream = await saveFile.OpenAsync(FileAccessMode.ReadWrite);
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);

                var pixelStream = picture.ModifiedImage.PixelBuffer.AsStream();
                var pixels = new byte[pixelStream.Length];
                await pixelStream.ReadAsync(pixels, 0, pixels.Length);

                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore,
                    (uint) picture.ModifiedImage.PixelWidth,
                    (uint) picture.ModifiedImage.PixelHeight, picture.DpiX, picture.DpiY, pixels);
                await encoder.FlushAsync();

                stream.Dispose();
            }
        }

        #endregion
    }
}
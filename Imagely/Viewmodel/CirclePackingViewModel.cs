using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Imagely.Model;
using Imagely.Utils;
using Imagely.View.Dialogs;

namespace Imagely.Viewmodel
{
    /// <summary>
    ///     Defines the CirclePackingViewModel class.
    /// </summary>
    public class CirclePackingViewModel
    {
        #region Properties

        /// <summary>
        ///     Gets or sets the source picture.
        /// </summary>
        /// <value>
        ///     The source picture.
        /// </value>
        public Picture SourcePicture { get; private set; }

        /// <summary>
        ///     Gets the copied picture.
        /// </summary>
        /// <value>
        ///     The copied picture.
        /// </value>
        public Picture CopiedPicture { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="CirclePackingViewModel" /> class.
        /// </summary>
        public CirclePackingViewModel()
        {
            this.SourcePicture = new Picture();
            this.CopiedPicture = new Picture();
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Draws the circle packing.
        /// </summary>
        /// <param name="circlePackingCanvas">The circle packing canvas.</param>
        public async void DrawCirclePacking(Canvas circlePackingCanvas)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                () =>
                {
                    //TODO: implement circle packing
                    //idea: for each segmented color
                    //get all pixels for that color
                    //while current radius > min radius
                    //place as many of max circle size that is inside the boundaries of the segmented color
                    //decrement max circle size
                }
            );
        }

        /// <summary>
        ///     Opens the source image.
        /// </summary>
        public async Task<bool> OpenSourceImage()
        {
            var sourceImageFile = await FileUtilities.SelectPngFile();
            if (!await this.isFileValid(sourceImageFile))
            {
                return false;
            }

            var copyBitmapImage = await FileUtilities.MakeCopyOfTheImage(sourceImageFile);
            await PictureUtilities.LoadImageData(this.SourcePicture, sourceImageFile, copyBitmapImage);
            this.SourcePicture.File = sourceImageFile;
            this.CopiedPicture.File = sourceImageFile;
            return true;
        }

        /// <summary>
        ///     Opens the dragged image.
        /// </summary>
        /// <param name="dragEvent">The <see cref="DragEventArgs" /> instance containing the event data.</param>
        /// <returns>True if file is valid, false otherwise</returns>
        public async Task<bool> OpenDraggedImage(DragEventArgs dragEvent)
        {
            try
            {
                await this.openDraggedFile(dragEvent);
                return true;
            }
            catch
            {
                await this.showInvalidFileDialog();
                return false;
            }
        }

        /// <summary>
        ///     Converts image to Picture then saves it.
        /// </summary>
        /// <param name="image">The image.</param>
        public async Task SaveImage(Image image)
        {
            var picture = await PictureUtilities.ConvertImageToPicture(image);
            FileUtilities.SaveImage(picture);
        }

        /// <summary>
        ///     Clears the source and copied pictures.
        /// </summary>
        public void Clear()
        {
            this.SourcePicture = new Picture();
            this.CopiedPicture = new Picture();
        }

        private async Task openDraggedFile(DragEventArgs dragEvent)
        {
            if (dragEvent.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await dragEvent.DataView.GetStorageItemsAsync();
                var sourceImageFile = items[0] as StorageFile;
                var copyBitmapImage = await FileUtilities.MakeCopyOfTheImage(sourceImageFile);

                if (sourceImageFile != null)
                {
                    await PictureUtilities.LoadImageData(this.SourcePicture, sourceImageFile, copyBitmapImage);
                }
            }
        }

        private async Task<bool> isFileValid(StorageFile file)
        {
            if (file == null)
            {
                await this.showInvalidFileDialog();
                return false;
            }

            return true;
        }

        private async Task showInvalidFileDialog()
        {
            var dialog = new InvalidFileDialog();
            await dialog.ShowAsync();
        }

        #endregion
    }
}
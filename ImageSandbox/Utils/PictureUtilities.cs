using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using GroupCStegafy.Model;

namespace GroupCStegafy.Utils
{
    /// <summary>
    ///     Defines the PictureConverter class.
    /// </summary>
    public static class PictureUtilities
    {
        #region Methods

        /// <summary>
        ///     Converts the image to picture.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <returns></returns>
        public static async Task<Picture> ConvertImageToPicture(Image image)
        {
            var renderTargetBitmap = new RenderTargetBitmap();
            await renderTargetBitmap.RenderAsync(image);
            var pixelBuffer = await renderTargetBitmap.GetPixelsAsync();
            var pixels = pixelBuffer.ToArray();
            var writableBitmap = new WriteableBitmap(renderTargetBitmap.PixelWidth, renderTargetBitmap.PixelHeight);
            using (var stream = writableBitmap.PixelBuffer.AsStream())
            {
                await stream.WriteAsync(pixels, 0, pixels.Length);
            }

            var picture = new Picture {
                ModifiedImage = writableBitmap,
                Width = (uint) renderTargetBitmap.PixelWidth,
                Height = (uint) renderTargetBitmap.PixelHeight
            };
            return picture;
        }

        /// <summary>
        ///     Converts the bytes into bit array.
        /// </summary>
        /// <param name="picture">The picture.</param>
        /// <returns>BitArray</returns>
        public static bool[] ConvertBytesIntoBitArray(Picture picture)
        {
            var byteArray = PictureToByteArray(picture);
            var bits = byteArray.SelectMany(getBits).ToArray();
            return bits;
        }

        /// <summary>
        ///     Convert picture to byte array of pixels.
        /// </summary>
        /// <param name="picture">The picture.</param>
        /// <returns>
        ///     Byte array of pixels.
        /// </returns>
        public static byte[] PictureToByteArray(Picture picture)
        {
            var amountOfPixels = picture.Width * picture.Height * 3;
            var pixelArray = new byte[amountOfPixels];
            var arrayIndex = 0;
            for (var i = 0; i < picture.Height; i++)
            {
                for (var j = 0; j < picture.Width; j++)
                {
                    j = HeaderManager.SkipHeaderLocation(i, j);

                    var sourcePixelColor = PixelUtilities.GetPixelBgra8(picture.Pixels, i, j,
                        picture.Width, picture.Height);

                    var colorChannels = new List<byte> {
                        sourcePixelColor.R,
                        sourcePixelColor.G,
                        sourcePixelColor.B
                    };

                    foreach (var currentByte in colorChannels)
                    {
                        pixelArray[arrayIndex] = currentByte;
                        arrayIndex++;
                    }
                }
            }

            return pixelArray;
        }

        /// <summary>
        ///     Loads the image data.
        /// </summary>
        /// <param name="picture">The source picture.</param>
        /// <param name="sourceImageFile">The source image file.</param>
        /// <param name="copyBitmapImage">The copy bitmap image.</param>
        public static async Task LoadImageData(Picture picture, StorageFile sourceImageFile,
            BitmapImage copyBitmapImage)
        {
            using (var fileStream = await sourceImageFile.OpenAsync(FileAccessMode.Read))
            {
                var decoder = await BitmapDecoder.CreateAsync(fileStream);
                var transform = new BitmapTransform {
                    ScaledWidth = Convert.ToUInt32(copyBitmapImage.PixelWidth),
                    ScaledHeight = Convert.ToUInt32(copyBitmapImage.PixelHeight)
                };
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

        private static IEnumerable<bool> getBits(byte b)
        {
            for (var i = 0; i < 8; i++)
            {
                yield return (b & 0x80) != 0;
                b *= 2;
            }
        }

        #endregion
    }
}
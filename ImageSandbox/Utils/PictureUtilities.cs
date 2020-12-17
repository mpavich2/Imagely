using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
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
        ///     Converts the bytes into bit array.
        /// </summary>
        /// <param name="picture">The picture.</param>
        /// <returns>BitArray</returns>
        public static bool[] ConvertBytesIntoBitArray(Picture picture)
        {
            var byteArray = PictureToByteArray(picture);
            bool[] bits = byteArray.SelectMany(getBits).ToArray();
            //var bitArray = new BitArray(byteArray);
            return bits;
        }

        /// <summary>
        ///     Picture to byte array.
        /// </summary>
        /// <param name="picture">The picture.</param>
        /// <returns>
        ///     Byte array
        /// </returns>
        public static byte[] PictureToByteArray(Picture picture)
        {
            var amountOfPixels = picture.Width * picture.Height * 3;
            var array = new byte[amountOfPixels];
            var arrayIndex = 0;
            for (var i = 0; i < picture.Height; i++)
            {
                for (var j = 0; j < picture.Width; j++)
                {
                    j = HeaderManager.SkipHeaderLocation(i, j);

                    var sourcePixelColor = PixelUtilities.GetPixelBgra8(picture.Pixels, i, j,
                        picture.Width, picture.Height);

                    var bytes = new List<byte> {
                        sourcePixelColor.R,
                        sourcePixelColor.G,
                        sourcePixelColor.B
                    };

                    foreach (var b in bytes)
                    {
                        array[arrayIndex] = b;
                        arrayIndex++;
                    }
                }
            }

            return array;
        }

        /// <summary>
        /// Loads the image data.
        /// </summary>
        /// <param name="picture">The source picture.</param>
        /// <param name="sourceImageFile">The source image file.</param>
        /// <param name="copyBitmapImage">The copy bitmap image.</param>
        public static async Task LoadImageData(Picture picture, StorageFile sourceImageFile, BitmapImage copyBitmapImage)
        {
            using (var fileStream = await sourceImageFile.OpenAsync(FileAccessMode.Read))
            {
                var decoder = await BitmapDecoder.CreateAsync(fileStream);
                var transform = new BitmapTransform
                {
                    ScaledWidth = Convert.ToUInt32(copyBitmapImage.PixelWidth),
                    ScaledHeight = Convert.ToUInt32(copyBitmapImage.PixelHeight)
                };
                picture.ModifiedImage =
                    new WriteableBitmap((int)decoder.PixelWidth, (int)decoder.PixelHeight);

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
            for (int i = 0; i < 8; i++)
            {
                yield return (b & 0x80) != 0;
                b *= 2;
            }
        }

        #endregion
    }
}
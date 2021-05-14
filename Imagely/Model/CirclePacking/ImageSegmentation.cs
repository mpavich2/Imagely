using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using ColorThiefDotNet;
using Imagely.Utils;
using Color = Windows.UI.Color;

namespace Imagely.Model.CirclePacking
{
    /// <summary>
    ///     Defines the ImageSegmentation class.
    /// </summary>
    public class ImageSegmentation
    {
        #region Data members

        private const double MaxDistance = double.MaxValue;
        private readonly Picture sourcePicture;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ImageSegmentation" /> class.
        /// </summary>
        /// <param name="picture">The picture.</param>
        public ImageSegmentation(Picture picture)
        {
            this.sourcePicture = picture;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Applies image segmentation on an image to get the amount of specified colors from the image.
        /// </summary>
        /// <param name="colorCount">The color count.</param>
        /// <param name="quality">The quality.</param>
        public async Task ApplyImageSegmentation(int colorCount, int quality)
        {
            var colorThief = new ColorThief();
            var fileStream = await this.sourcePicture.File.OpenAsync(FileAccessMode.Read);
            var decoder = await BitmapDecoder.CreateAsync(fileStream);
            var colors = await colorThief.GetPalette(decoder, colorCount, quality, false);
            for (var i = 0; i < this.sourcePicture.Width - 1; i++)
            {
                for (var j = 0; j < this.sourcePicture.Height - 1; j++)
                {
                    var sourcePixelColor = PixelUtilities.GetPixelBgra8(this.sourcePicture.Pixels, j, i,
                        this.sourcePicture.Width,
                        this.sourcePicture.Height);
                    this.setPixelToClosestQuantizedColor(colors, sourcePixelColor, MaxDistance, j, i);
                }
            }

            this.sourcePicture.ModifiedImage =
                new WriteableBitmap((int) this.sourcePicture.Width, (int) this.sourcePicture.Height);
        }

        private void setPixelToClosestQuantizedColor(List<QuantizedColor> colors, Color sourcePixelColor,
            double distance, int j, int i)
        {
            foreach (var quantizedColor in colors)
            {
                var color = quantizedColor.Color;
                var curDist = Math.Pow(color.R - sourcePixelColor.R, 2) +
                              Math.Pow(color.G - sourcePixelColor.G, 2) +
                              Math.Pow(color.B - sourcePixelColor.B, 2);
                if (curDist < distance)
                {
                    distance = curDist;
                    var newPixelColor = Color.FromArgb(255, color.R, color.G, color.B);
                    PixelUtilities.SetPixelBgra8(this.sourcePicture.Pixels, j, i, newPixelColor,
                        this.sourcePicture.Width,
                        this.sourcePicture.Height);
                }
            }
        }

        #endregion
    }
}
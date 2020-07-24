using System;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
using GroupCStegafy.Utils;

namespace GroupCStegafy.Model.Filters
{
    /// <summary>
    ///     Defines the Grayscale Filter Class.
    /// </summary>
    public class GrayscaleFilter
    {
        #region Data members

        private readonly Picture sourcePicture;
        private readonly byte maxRgbValue = (byte) Constants.ImageConstants.MaxRgbValue;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="GrayscaleFilter" /> class.
        /// </summary>
        /// <param name="picture">The picture.</param>
        public GrayscaleFilter(Picture picture)
        {
            this.sourcePicture = picture;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Applies the grayscale filter.
        /// </summary>
        public void applyGrayscaleFilter()
        {
            for (var i = 0; i < this.sourcePicture.Width - 1; i++)
            {
                for (var j = 0; j < this.sourcePicture.Height - 1; j++)
                {
                    var sourcePixelColor = PixelManager.GetPixelBgra8(this.sourcePicture.Pixels, j, i,
                        this.sourcePicture.Width,
                        this.sourcePicture.Height);
                    var averageColor = (sourcePixelColor.R + sourcePixelColor.G + sourcePixelColor.B) / 3;
                    var averageColorByte = Convert.ToByte(averageColor);
                    var newPixelColor = Color.FromArgb(this.maxRgbValue, averageColorByte, averageColorByte,
                        averageColorByte);
                    PixelManager.SetPixelBgra8(this.sourcePicture.Pixels, j, i, newPixelColor, this.sourcePicture.Width,
                        this.sourcePicture.Height);
                }
            }

            this.sourcePicture.ModifiedImage =
                new WriteableBitmap((int) this.sourcePicture.Width, (int) this.sourcePicture.Height);
        }

        #endregion
    }
}
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
using GroupCStegafy.Utils;

namespace GroupCStegafy.Model.Filters
{
    /// <summary>
    /// Defines the Sobel Filter Class.
    /// </summary>
    public class SobelFilter
    {
        #region Data members

        private const int MaxLimit = 16384;

        private readonly byte maxRgbValue = (byte) Constants.ImageConstants.MaxRgbValue;
        private readonly byte minRgbValue = (byte) Constants.ImageConstants.MinRgbValue;
        private readonly Picture sourcePicture;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SobelFilter"/> class.
        /// </summary>
        /// <param name="picture">The picture.</param>
        public SobelFilter(Picture picture)
        {
            this.sourcePicture = picture;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Applies the sobel filter.
        /// </summary>
        public void applySobelFilter()
        {
            var gx = new[,] {{-1, 0, 1}, 
                             {-2, 0, 2}, 
                             {-1, 0, 1}};
            var gy = new[,] {{1, 2, 1}, 
                             {0, 0, 0}, 
                             {-1, -2, -1}};
            var allPixR = new int[this.sourcePicture.Width, this.sourcePicture.Height];
            var allPixG = new int[this.sourcePicture.Width, this.sourcePicture.Height];
            var allPixB = new int[this.sourcePicture.Width, this.sourcePicture.Height];

            for (var i = 0; i < this.sourcePicture.Width - 1; i++)
            {
                for (var j = 0; j < this.sourcePicture.Height - 1; j++)
                {
                    var sourcePixelColor = PixelManager.GetPixelBgra8(this.sourcePicture.Pixels, j, i,
                        this.sourcePicture.Width,
                        this.sourcePicture.Height);
                    allPixR[i, j] = sourcePixelColor.R;
                    allPixG[i, j] = sourcePixelColor.G;
                    allPixB[i, j] = sourcePixelColor.B;
                }
            }

            var width = (int) this.sourcePicture.Width - 1;
            var height = (int) this.sourcePicture.Height - 1;
            for (var i = 1; i < width; i++)
            {
                for (var j = 1; j < height; j++)
                {
                    var newRx = 0;
                    var newRy = 0;
                    var newGx = 0;
                    var newGy = 0;
                    var newBx = 0;
                    var newBy = 0;

                    for (var wi = -1; wi < 2; wi++)
                    {
                        for (var hw = -1; hw < 2; hw++)
                        {
                            var rc = allPixR[i + hw, j + wi];
                            newRx += gx[wi + 1, hw + 1] * rc;
                            newRy += gy[wi + 1, hw + 1] * rc;

                            var gc = allPixG[i + hw, j + wi];
                            newGx += gx[wi + 1, hw + 1] * gc;
                            newGy += gy[wi + 1, hw + 1] * gc;

                            var bc = allPixB[i + hw, j + wi];
                            newBx += gx[wi + 1, hw + 1] * bc;
                            newBy += gy[wi + 1, hw + 1] * bc;
                        }
                    }

                    if (newRx * newRx + newRy * newRy > MaxLimit || newGx * newGx + newGy * newGy > MaxLimit ||
                        newBx * newBx + newBy * newBy > MaxLimit)
                    {
                        var sourcePixelColor =
                            Color.FromArgb(this.maxRgbValue, this.maxRgbValue, this.maxRgbValue, this.maxRgbValue);
                        PixelManager.SetPixelBgra8(this.sourcePicture.Pixels, j, i, sourcePixelColor,
                            this.sourcePicture.Width, this.sourcePicture.Height);
                    }
                    else
                    {
                        var sourcePixelColor =
                            Color.FromArgb(this.maxRgbValue, this.minRgbValue, this.minRgbValue, this.minRgbValue);
                        PixelManager.SetPixelBgra8(this.sourcePicture.Pixels, j, i, sourcePixelColor,
                            this.sourcePicture.Width, this.sourcePicture.Height);
                    }
                }

                this.sourcePicture.ModifiedImage =
                    new WriteableBitmap((int) this.sourcePicture.Width, (int) this.sourcePicture.Height);
            }
        }

        #endregion
    }
}
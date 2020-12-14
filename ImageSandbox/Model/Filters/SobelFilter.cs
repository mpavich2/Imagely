using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
using GroupCStegafy.Constants;
using GroupCStegafy.Utils;

namespace GroupCStegafy.Model.Filters
{
    /// <summary>
    ///     Defines the Sobel Filter Class.
    /// </summary>
    public class SobelFilter
    {
        #region Data members

        private const int MaxLimit = 16384;
        private readonly int[,] gx = {{-1, 0, 1}, {-2, 0, 2}, {-1, 0, 1}};
        private readonly int[,] gy = {{1, 2, 1}, {0, 0, 0}, {-1, -2, -1}};

        private readonly byte maxRgbValue = (byte) ImageConstants.MaxRgbValue;
        private readonly byte minRgbValue = (byte) ImageConstants.MinRgbValue;
        private readonly Picture sourcePicture;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="SobelFilter" /> class.
        /// </summary>
        /// <param name="picture">The picture.</param>
        public SobelFilter(Picture picture)
        {
            this.sourcePicture = picture;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Applies the sobel filter.
        /// </summary>
        public void applySobelFilter()
        {
            var allPixR = new int[this.sourcePicture.Width, this.sourcePicture.Height];
            var allPixG = new int[this.sourcePicture.Width, this.sourcePicture.Height];
            var allPixB = new int[this.sourcePicture.Width, this.sourcePicture.Height];

            this.loadAllPixelColorData(allPixR, allPixG, allPixB);

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

                    newRx = this.applyConvolutionToPixels(allPixR, i, j, newRx, allPixG, allPixB, ref newRy, ref newGx,
                        ref newGy, ref newBx, ref newBy);

                    if (this.isEdge(newRx, newRy, newGx, newGy, newBx, newBy))
                    {
                        this.setPixelWhite(j, i);
                    }
                    else
                    {
                        this.setPixelBlack(j, i);
                    }
                }

                this.sourcePicture.ModifiedImage =
                    new WriteableBitmap((int) this.sourcePicture.Width, (int) this.sourcePicture.Height);
            }
        }

        private void setPixelBlack(int j, int i)
        {
            var sourcePixelColor =
                Color.FromArgb(this.maxRgbValue, this.minRgbValue, this.minRgbValue, this.minRgbValue);
            PixelManager.SetPixelBgra8(this.sourcePicture.Pixels, j, i, sourcePixelColor,
                this.sourcePicture.Width, this.sourcePicture.Height);
        }

        private void setPixelWhite(int j, int i)
        {
            var sourcePixelColor =
                Color.FromArgb(this.maxRgbValue, this.maxRgbValue, this.maxRgbValue, this.maxRgbValue);
            PixelManager.SetPixelBgra8(this.sourcePicture.Pixels, j, i, sourcePixelColor,
                this.sourcePicture.Width, this.sourcePicture.Height);
        }

        private bool isEdge(int newRx, int newRy, int newGx, int newGy, int newBx, int newBy)
        {
            return newRx * newRx + newRy * newRy > MaxLimit || newGx * newGx + newGy * newGy > MaxLimit ||
                   newBx * newBx + newBy * newBy > MaxLimit;
        }

        private int applyConvolutionToPixels(int[,] allPixR, int i, int j, int newRx, int[,] allPixG, int[,] allPixB,
            ref int newRy, ref int newGx, ref int newGy, ref int newBx, ref int newBy)
        {
            for (var x = -1; x < 2; x++)
            {
                for (var y = -1; y < 2; y++)
                {
                    var redConvolution = allPixR[i + y, j + x];
                    newRx += this.gx[x + 1, y + 1] * redConvolution;
                    newRy += this.gy[x + 1, y + 1] * redConvolution;

                    var greenConvolution = allPixG[i + y, j + x];
                    newGx += this.gx[x + 1, y + 1] * greenConvolution;
                    newGy += this.gy[x + 1, y + 1] * greenConvolution;

                    var blueConvolution = allPixB[i + y, j + x];
                    newBx += this.gx[x + 1, y + 1] * blueConvolution;
                    newBy += this.gy[x + 1, y + 1] * blueConvolution;
                }
            }

            return newRx;
        }

        private void loadAllPixelColorData(int[,] allPixR, int[,] allPixG, int[,] allPixB)
        {
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
        }

        #endregion
    }
}
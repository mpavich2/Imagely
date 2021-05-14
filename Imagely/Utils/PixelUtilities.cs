using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI;
using Imagely.Model;
using Imagely.Model.DelaunayTriangulation;

namespace Imagely.Utils
{
    /// <summary>
    ///     Defines the PixelManager class.
    /// </summary>
    public static class PixelUtilities
    {
        #region Methods

        /// <summary>
        ///     Gets the pixel bgra8.
        /// </summary>
        /// <param name="pixels">The pixels.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns>
        ///     Color
        /// </returns>
        public static Color GetPixelBgra8(byte[] pixels, int x, int y, uint width, uint height)
        {
            var offset = (x * (int) width + y) * 4;
            var r = pixels[offset + 2];
            var g = pixels[offset + 1];
            var b = pixels[offset + 0];
            return Color.FromArgb(0, r, g, b);
        }

        /// <summary>
        ///     Sets the pixel bgra8.
        /// </summary>
        /// <param name="pixels">The pixels.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="color">The color.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public static void SetPixelBgra8(byte[] pixels, int x, int y, Color color, uint width, uint height)
        {
            var offset = (x * (int) width + y) * 4;
            pixels[offset + 2] = color.R;
            pixels[offset + 1] = color.G;
            pixels[offset + 0] = color.B;
        }

        /// <summary>
        ///     Gets the LSB.
        /// </summary>
        /// <param name="byteValue">The int value.</param>
        /// <returns>
        ///     Least significant bit
        /// </returns>
        public static int GetLeastSignificantBit(int byteValue)
        {
            return byteValue % 2 == 0 ? 0 : 1;
        }

        /// <summary>
        ///     Determines whether [is color black] [the specified hidden pixel color].
        /// </summary>
        /// <param name="pixelColor">Color of the hidden pixel.</param>
        /// <returns>
        ///     <c>true</c> if [is color black] [the specified hidden pixel color]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsColorBlack(Color pixelColor)
        {
            return pixelColor.R == byte.MinValue && pixelColor.G == byte.MinValue &&
                   pixelColor.B == byte.MinValue;
        }

        /// <summary>
        ///     Determines whether [is color white] [the specified pixel color].
        /// </summary>
        /// <param name="pixelColor">Color of the pixel.</param>
        /// <returns>
        ///     <c>true</c> if [is color white] [the specified pixel color]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsColorWhite(Color pixelColor)
        {
            return pixelColor.R == byte.MaxValue && pixelColor.G == byte.MaxValue && pixelColor.B == byte.MaxValue;
        }

        /// <summary>
        ///     Gets the average color.
        /// </summary>
        /// <param name="picture">The picture.</param>
        /// <param name="firstPoint">The first point.</param>
        /// <param name="secondPoint">The second point.</param>
        /// <returns>Average color in area of picture</returns>
        public static Color GetAverageColor(Picture picture, Point firstPoint, Point secondPoint)
        {
            var rValues = new List<int>();
            var gValues = new List<int>();
            var bValues = new List<int>();
            for (var i = firstPoint.X; i < secondPoint.X; i++)
            {
                for (var j = firstPoint.Y; j < secondPoint.Y; j++)
                {
                    var pixelColor = GetPixelBgra8(picture.Pixels, Convert.ToInt32(j), Convert.ToInt32(i),
                        picture.Width,
                        picture.Height);
                    rValues.Add(pixelColor.R);
                    gValues.Add(pixelColor.G);
                    bValues.Add(pixelColor.B);
                }
            }

            var averageRedValue = Convert.ToByte(rValues.Sum() / rValues.Count);
            var averageGreenValue = Convert.ToByte(gValues.Sum() / gValues.Count);
            var averageBlueValue = Convert.ToByte(bValues.Sum() / bValues.Count);
            return Color.FromArgb(byte.MaxValue, averageRedValue, averageGreenValue, averageBlueValue);
        }

        /// <summary>
        ///     Calculates the distance between pixels.
        /// </summary>
        /// <param name="x1">The x1.</param>
        /// <param name="y1">The y1.</param>
        /// <param name="x2">The x2.</param>
        /// <param name="y2">The y2.</param>
        /// <returns>Distance between pixels</returns>
        public static double CalculateDistanceBetweenPixels(double x1, double y1, double x2, double y2)
        {
            var distance = Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
            return distance;
        }

        #endregion
    }
}
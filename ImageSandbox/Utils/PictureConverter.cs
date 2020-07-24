using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GroupCStegafy.Model;

namespace GroupCStegafy.Utils
{
    /// <summary>
    ///     Defines the PictureConverter class.
    /// </summary>
    public static class PictureConverter
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

                    var sourcePixelColor = PixelManager.GetPixelBgra8(picture.Pixels, i, j,
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
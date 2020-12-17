using System.Text;
using Windows.UI.Xaml;
using GroupCStegafy.Utils;

namespace GroupCStegafy.Model
{
    /// <summary>
    ///     Defines the DecryptionManager class.
    /// </summary>
    public static class DecryptionManager
    {
        #region Methods

        /// <summary>
        ///     Decrypts the picture.
        /// </summary>
        /// <param name="extractedImage">The extracted image.</param>
        public static void DecryptPicture(Picture extractedImage)
        {
            for (var i = 0; i < extractedImage.Height / 2; i++)
            {
                for (var j = 0; j < extractedImage.Width; j++)
                {
                    var currentIndex = i;
                    var oppositeYValue = (int) (currentIndex + extractedImage.VerticalCenter);
                    var oppositePixelColor = PixelUtilities.GetPixelBgra8(extractedImage.Pixels, oppositeYValue, j,
                        extractedImage.Width, extractedImage.Height);
                    var pixelColor = PixelUtilities.GetPixelBgra8(extractedImage.Pixels, i, j, extractedImage.Width,
                        extractedImage.Height);
                    PixelUtilities.SetPixelBgra8(extractedImage.Pixels, i, j, oppositePixelColor, extractedImage.Width,
                        extractedImage.Height);
                    PixelUtilities.SetPixelBgra8(extractedImage.Pixels, oppositeYValue, j, pixelColor,
                        extractedImage.Width, extractedImage.Height);
                }
            }
        }

        /// <summary>
        ///     Decrypts the text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="keyword">The keyword.</param>
        /// <returns>
        ///     Decrypted text
        /// </returns>
        public static string DecryptText(string text, string keyword)
        {
            var asciiValueOfA = (int) Application.Current.Resources["AsciiValueOfA"];
            var alphabetLength = (int) Application.Current.Resources["AlphabetLength"];
            var decryptedBytes = new byte[text.Length];

            for (var i = 0; i < text.Length; i++)
            {
                var textBytes = Encoding.ASCII.GetBytes(text);
                var keyBytes = Encoding.ASCII.GetBytes(keyword);

                var textLetter = (byte) (textBytes[i] - asciiValueOfA);
                var keyLetter = (byte) (keyBytes[i % keyBytes.Length] - asciiValueOfA);

                var decryptedLetter =
                    (byte) ((textLetter - keyLetter + alphabetLength) % alphabetLength + asciiValueOfA);
                decryptedBytes[i] = decryptedLetter;
            }

            var decryptedText = Encoding.ASCII.GetString(decryptedBytes);
            return decryptedText;
        }

        #endregion
    }
}
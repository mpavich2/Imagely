using System.Collections.Generic;
using System.Text;
using Windows.UI;
using GroupCStegafy.Constants;
using GroupCStegafy.Utils;

namespace GroupCStegafy.Model
{
    /// <summary>
    ///     Defines the EncryptionManager class.
    /// </summary>
    public class EncryptionManager
    {
        #region Data members

        private const string KEY_END = "#KEY#";

        #endregion

        #region Methods

        /// <summary>
        ///     Encrypts the text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="keyword">The keyword.</param>
        /// <returns>
        ///     Encrypted text
        /// </returns>
        public static string EncryptText(string text, string keyword)
        {
            text = text.RemoveNonAlphabeticCharacters();
            text = text.ToUpper();
            keyword = keyword.ToUpper();
            var keywordText = new StringBuilder(text.Length);

            addToKeyword(keyword, keywordText);

            var keywordTextBytes = Encoding.ASCII.GetBytes(keywordText.ToString());
            var messageTextBytes = Encoding.ASCII.GetBytes(text);
            var encryptedText = new byte[keywordTextBytes.Length];

            encryptLetter(text, messageTextBytes, keywordTextBytes, encryptedText);

            var finalMessage = Encoding.ASCII.GetString(encryptedText);
            return keyword + KEY_END + finalMessage;
        }

        /// <summary>
        ///     Encrypts the image embedding.
        /// </summary>
        /// <param name="pixels">The pixels.</param>
        /// <param name="sourcePicture">The source picture.</param>
        /// <param name="hiddenPicture">The hidden picture.</param>
        public static void EncryptImage(byte[] pixels, Picture sourcePicture, Picture hiddenPicture)
        {
            for (var i = 0; i < sourcePicture.Height; i++)
            {
                for (var j = 0; j < sourcePicture.Width; j++)
                {
                    j = HeaderManager.SkipHeaderLocation(i, j);
                    var sourcePixelColor =
                        PixelUtilities.GetPixelBgra8(pixels, i, j, sourcePicture.Width, sourcePicture.Height);
                    var hiddenPixelColor = Colors.White;

                    hiddenPixelColor =
                        getOppositeVerticalHalfPixelColor(i, j, hiddenPixelColor, sourcePicture, hiddenPicture);

                    if (PixelUtilities.IsColorBlack(hiddenPixelColor))
                    {
                        embedBlackPixel(pixels, sourcePixelColor, i, j, sourcePicture);
                    }

                    else
                    {
                        embedWhitePixel(pixels, sourcePixelColor, i, j, sourcePicture);
                    }
                }
            }
        }

        private static void encryptLetter(string text, byte[] messageTextBytes, IReadOnlyList<byte> keywordTextBytes,
            IList<byte> encryptedText)
        {
            for (var i = 0; i < text.Length; i++)
            {
                var messageTextChar = (byte) (messageTextBytes[i] - ImageConstants.AsciiValueOfA);
                var keywordTextChar = (byte) (keywordTextBytes[i] - ImageConstants.AsciiValueOfA);

                encryptedText[i] = (byte) ((messageTextChar + keywordTextChar) % ImageConstants.AlphabetLength +
                                           ImageConstants.AsciiValueOfA);
            }
        }

        private static void addToKeyword(string keyword, StringBuilder keywordText)
        {
            for (var i = 0; i < keywordText.Capacity; i++)
            {
                keywordText.Append(keyword[i % keyword.Length]);
            }
        }

        private static void embedWhitePixel(byte[] pixels, Color sourcePixelColor, int i, int j, Picture sourcePicture)
        {
            sourcePixelColor.B |= 1;
            PixelUtilities.SetPixelBgra8(pixels, i, j, sourcePixelColor, sourcePicture.Width, sourcePicture.Height);
        }

        private static void embedBlackPixel(byte[] pixels, Color sourcePixelColor, int i, int j, Picture sourcePicture)
        {
            sourcePixelColor.B = (byte) (sourcePixelColor.B & ~1);
            PixelUtilities.SetPixelBgra8(pixels, i, j, sourcePixelColor,
                sourcePicture.Width, sourcePicture.Height);
        }

        private static Color getOppositeVerticalHalfPixelColor(int i, int j, Color hiddenPixelColor,
            Picture sourcePicture, Picture hiddenPicture)
        {
            if (i <= sourcePicture.VerticalCenter)
            {
                var swappedYValue = (int) (i + sourcePicture.VerticalCenter);
                if (swappedYValue < hiddenPicture.Height && j < hiddenPicture.Width)
                {
                    hiddenPixelColor = PixelUtilities.GetPixelBgra8(hiddenPicture.Pixels, swappedYValue, j,
                        hiddenPicture.Width, hiddenPicture.Height);
                }
            }

            else
            {
                var swappedYValue = (int) (i - sourcePicture.VerticalCenter);
                if (swappedYValue < hiddenPicture.Height && j < hiddenPicture.Width)
                {
                    hiddenPixelColor = PixelUtilities.GetPixelBgra8(hiddenPicture.Pixels, swappedYValue, j,
                        hiddenPicture.Width, hiddenPicture.Height);
                }
            }

            return hiddenPixelColor;
        }

        #endregion
    }
}
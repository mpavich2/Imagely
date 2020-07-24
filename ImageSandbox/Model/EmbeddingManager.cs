using System.Collections.Generic;
using Windows.UI;
using GroupCStegafy.Constants;
using GroupCStegafy.Enums;
using GroupCStegafy.Utils;

namespace GroupCStegafy.Model
{
    /// <summary>
    ///     Defines the EncryptionManager model class.
    /// </summary>
    public class EmbeddingManager
    {
        #region Data members

        private readonly Picture hiddenPicture;
        private readonly Picture sourcePicture;

        private bool isEncrypted;
        private int bpcc;
        private readonly int byteLength = ImageConstants.ByteLength;
        private readonly string endOfText = ImageConstants.EndOfText;

        #endregion

        #region Properties

        /// <summary>
        ///     Gets a value indicating whether [message too large].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [message too large]; otherwise, <c>false</c>.
        /// </value>
        public bool MessageTooLarge { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="EmbeddingManager" /> class.
        /// </summary>
        /// <param name="hiddenPicture">The hidden picture.</param>
        /// <param name="sourcePicture">The source picture.</param>
        public EmbeddingManager(Picture hiddenPicture, Picture sourcePicture)
        {
            this.sourcePicture = sourcePicture;
            this.hiddenPicture = hiddenPicture;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Embeds the image in source image.
        /// </summary>
        /// <param name="pixels">The pixels.</param>
        public void EmbedImage(byte[] pixels)
        {
            this.checkForEncryption();
            if (this.isEncrypted)
            {
                this.encryptedImageEmbedding(pixels);
            }
            else
            {
                this.imageEmbedding(pixels);
            }
        }

        /// <summary>
        ///     Embeds the text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="keyword">The keyword.</param>
        public void EmbedText(string text, string keyword)
        {
            this.checkForEncryption();
            this.checkBpccValue();
            if (this.isEncrypted)
            {
                this.textEmbedding(EncryptionManager.EncryptText(text, keyword));
            }
            else
            {
                this.textEmbedding(text);
            }
        }

        private void checkForEncryption()
        {
            var sourcePixelColor = PixelManager.GetPixelBgra8(this.sourcePicture.Pixels, 0, 1, this.sourcePicture.Width,
                this.sourcePicture.Height);
            if (HeaderManager.CheckForEncryption(sourcePixelColor) == EncryptionType.Encrypted)
            {
                this.isEncrypted = true;
            }
            else
            {
                this.isEncrypted = false;
            }
        }

        private void checkBpccValue()
        {
            var sourcePixelColor = PixelManager.GetPixelBgra8(this.sourcePicture.Pixels, 0, 1, this.sourcePicture.Width,
                this.sourcePicture.Height);
            this.bpcc = HeaderManager.CheckBpccValue(sourcePixelColor);
        }

        private void imageEmbedding(byte[] pixels)
        {
            for (var i = 0; i < this.hiddenPicture.Height; i++)
            {
                for (var j = 0; j < this.hiddenPicture.Width; j++)
                {
                    j = HeaderManager.SkipHeaderLocation(i, j);
                    var sourcePixelColor = PixelManager.GetPixelBgra8(pixels, i, j, this.sourcePicture.Width,
                        this.sourcePicture.Height);
                    var hiddenPixelColor = PixelManager.GetPixelBgra8(this.hiddenPicture.Pixels, i, j,
                        this.hiddenPicture.Width, this.hiddenPicture.Height);

                    this.embedPixel(pixels, hiddenPixelColor, sourcePixelColor, i, j);
                }
            }
        }

        private void embedPixel(byte[] pixels, Color hiddenPixelColor, Color sourcePixelColor, int i, int j)
        {
            if (PixelManager.IsColorBlack(hiddenPixelColor))
            {
                this.embedBlackPixel(pixels, sourcePixelColor, i, j);
            }
            else
            {
                this.embedWhitePixel(pixels, sourcePixelColor, i, j);
            }
        }

        private void encryptedImageEmbedding(byte[] pixels)
        {
            EncryptionManager.EncryptImage(pixels, this.sourcePicture, this.hiddenPicture);
        }

        private void updateSourceImage(IReadOnlyList<byte> bytes)
        {
            var index = 0;
            for (var i = 0; i < this.sourcePicture.Height; i++)
            {
                for (var j = 0; j < this.sourcePicture.Width; j++)
                {
                    j = HeaderManager.SkipHeaderLocation(i, j);
                    var sourcePixelColor = PixelManager.GetPixelBgra8(this.sourcePicture.Pixels, i, j,
                        this.sourcePicture.Width, this.sourcePicture.Height);

                    sourcePixelColor.R = bytes[index];
                    index++;

                    sourcePixelColor.G = bytes[index];
                    index++;

                    sourcePixelColor.B = bytes[index];
                    index++;

                    PixelManager.SetPixelBgra8(this.sourcePicture.Pixels, i, j, sourcePixelColor,
                        this.sourcePicture.Width, this.sourcePicture.Height);
                }
            }
        }

        private void embedWhitePixel(byte[] pixels, Color sourcePixelColor, int i, int j)
        {
            sourcePixelColor.B |= 1;
            PixelManager.SetPixelBgra8(pixels, i, j, sourcePixelColor, this.sourcePicture.Width,
                this.sourcePicture.Height);
        }

        private void embedBlackPixel(byte[] pixels, Color sourcePixelColor, int i, int j)
        {
            sourcePixelColor.B = (byte) (sourcePixelColor.B & ~1);
            PixelManager.SetPixelBgra8(pixels, i, j, sourcePixelColor,
                this.sourcePicture.Width, this.sourcePicture.Height);
        }

        private void textEmbedding(string text)
        {
            text = this.removeNonLettersIfNotEncrypted(text);
            text += this.endOfText;
            var textArray = text.StringToBitArray();
            var textArrayIndex = 0;
            var pixels = PictureConverter.ConvertBytesIntoBitArray(this.sourcePicture);

            this.MessageTooLarge = false;
            for (var i = 0; i < pixels.Length; i++)
            {
                if (this.changeBitsPerByte(i, pixels, textArray, ref textArrayIndex))
                {
                    return;
                }
            }
        }

        private string removeNonLettersIfNotEncrypted(string text)
        {
            if (!this.isEncrypted)
            {
                text = text.RemoveNonAlphabeticCharacters();
            }

            return text;
        }

        private bool changeBitsPerByte(int i, bool[] pixels, IReadOnlyList<char> textArray, ref int index)
        {
            if (i % this.byteLength == 0 && i > 0)
            {
                var currentIndex = i;
                currentIndex -= this.bpcc;
                if (this.bpcc == 8)
                {
                    if (this.changeAllBitsPerColorChannel(pixels, textArray, ref index, currentIndex))
                    {
                        return true;
                    }
                }
                else
                {
                    if (this.changeBitsPerColorChannel(pixels, textArray, ref index, currentIndex))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool changeAllBitsPerColorChannel(bool[] pixels, IReadOnlyList<char> textArray, ref int index,
            int current)
        {
            while (current % this.byteLength < this.bpcc)
            {
                pixels[current] = textArray[index] == '1';
                current++;
                index++;
                if (this.checkForEndOfText(pixels, textArray, index))
                {
                    return true;
                }
            }

            return false;
        }

        private bool checkForEndOfText(bool[] pixels, IReadOnlyList<char> textArray, int index)
        {
            if (index >= textArray.Count)
            {
                var ret = new byte[pixels.Length / 8];
                for (var i = 0; i < pixels.Length; i += 8)
                {
                    var value = 0;
                    for (var j = 0; j < 8; j++)
                    {
                        if (pixels[i + j])
                        {
                            value += 1 << (7 - j);
                        }
                    }
                    ret[i / 8] = (byte)value;
                }
                this.updateSourceImage(ret);
                return true;
            }

            return false;
        }

        private bool changeBitsPerColorChannel(bool[] pixels, IReadOnlyList<char> textArray, ref int index,
            int current)
        {
            while (current % this.byteLength != 0)
            {
                pixels[current] = textArray[index] == '1';
                current++;
                index++;
                if (this.checkForEndOfText(pixels, textArray, index))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
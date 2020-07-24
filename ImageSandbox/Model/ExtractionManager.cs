using System.Collections;
using System.Text;
using Windows.UI;
using GroupCStegafy.Constants;
using GroupCStegafy.Enums;
using GroupCStegafy.Utils;

namespace GroupCStegafy.Model
{
    /// <summary>
    ///     Defines the DecryptionManager model class.
    /// </summary>
    public class ExtractionManager
    {
        #region Data members

        private const int EndOfText = 3;
        private const int BeginningOfText = 2;
        private const int EndOfKey = 1;

        private readonly Picture sourcePicture;
        private int bpcc;
        private bool hasPassedKey;
        private int endOfTextCharactersPassed;

        private readonly int byteLength = ImageConstants.ByteLength;
        private readonly byte maxRgbValue = (byte) ImageConstants.MaxRgbValue;
        private readonly byte minRgbValue = (byte) ImageConstants.MinRgbValue;
        private readonly string endOfText = ImageConstants.Ending;
        private readonly int firstX = ImageConstants.FirstX;
        private readonly int secondX = ImageConstants.SecondX;

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the key word.
        /// </summary>
        /// <value>
        ///     The key word.
        /// </value>
        public string KeyWord { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ExtractionManager" /> class.
        /// </summary>
        /// <param name="sourcePicture">The source picture.</param>
        public ExtractionManager(Picture sourcePicture)
        {
            this.sourcePicture = sourcePicture;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Extracts the image.
        /// </summary>
        /// <param name="pixels">The pixels.</param>
        public void ExtractImage(byte[] pixels)
        {
            for (var i = 0; i < this.sourcePicture.Height; i++)
            {
                for (var j = 0; j < this.sourcePicture.Width; j++)
                {
                    j = HeaderManager.SkipHeaderLocation(i, j);
                    var hiddenPixelColor = PixelManager.GetPixelBgra8(pixels, i, j,
                        this.sourcePicture.Width,
                        this.sourcePicture.Height);
                    this.setPixelToMonochromeColor(pixels, hiddenPixelColor, i, j);
                }
            }
        }

        /// <summary>
        ///     Extracts the text.
        /// </summary>
        /// <returns>
        ///     Extracted text
        /// </returns>
        public string ExtractText()
        {
            this.checkBpccValue();
            this.IsEncrypted();
            var text = this.extractTextFromBytes();

            this.resetHasPassedKey();
            return text;
        }

        /// <summary>
        ///     Determines whether [contains hidden message].
        /// </summary>
        /// <returns>
        ///     <c>true</c> if [contains hidden message]; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsHiddenMessage()
        {
            var sourcePixelColor = PixelManager.GetPixelBgra8(this.sourcePicture.Pixels, this.firstX, this.firstX,
                this.sourcePicture.Width,
                this.sourcePicture.Height);
            return HeaderManager.ContainsHiddenMessage(sourcePixelColor);
        }

        /// <summary>
        ///     Determines whether this instance is encrypted.
        /// </summary>
        /// <returns>
        ///     <c>Encrypted</c> if this instance is encrypted; otherwise, <c>Unencrypted</c>.
        /// </returns>
        public EncryptionType IsEncrypted()
        {
            var sourcePixelColor = PixelManager.GetPixelBgra8(this.sourcePicture.Pixels, this.firstX, this.secondX,
                this.sourcePicture.Width,
                this.sourcePicture.Height);
            return HeaderManager.CheckForEncryption(sourcePixelColor);
        }

        /// <summary>
        ///     Determines whether this instance is text.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if this instance is text; otherwise, <c>false</c>.
        /// </returns>
        public bool IsText()
        {
            var sourcePixelColor = PixelManager.GetPixelBgra8(this.sourcePicture.Pixels, this.firstX, this.secondX,
                this.sourcePicture.Width,
                this.sourcePicture.Height);
            return HeaderManager.CheckFileType(sourcePixelColor);
        }

        private void setPixelToMonochromeColor(byte[] pixels, Color hiddenPixelColor, int i, int j)
        {
            if (PixelManager.GetLeastSignificantBit(hiddenPixelColor.B) == 0)
            {
                this.setPixelBlack(pixels, i, j);
            }
            else
            {
                this.setPixelWhite(pixels, i, j);
            }
        }

        private void setPixelWhite(byte[] pixels, int i, int j)
        {
            var sourcePixelColor =
                Color.FromArgb(this.maxRgbValue, this.maxRgbValue, this.maxRgbValue, this.maxRgbValue);
            PixelManager.SetPixelBgra8(pixels, i, j, sourcePixelColor,
                this.sourcePicture.Width,
                this.sourcePicture.Height);
        }

        private void setPixelBlack(byte[] pixels, int i, int j)
        {
            var sourcePixelColor =
                Color.FromArgb(this.maxRgbValue, this.minRgbValue, this.minRgbValue, this.minRgbValue);
            PixelManager.SetPixelBgra8(pixels, i, j, sourcePixelColor,
                this.sourcePicture.Width,
                this.sourcePicture.Height);
        }

        private string extractTextFromBytes()
        {
            var text = new StringBuilder();
            var binaryText = new StringBuilder();
            var pixels = PictureConverter.ConvertBytesIntoBitArray(this.sourcePicture);

            for (var i = 0; i < pixels.Length; i++)
            {
                if (this.getTextFromBits(i, pixels, binaryText, text, out var extractTextFromBytes1))
                {
                    return extractTextFromBytes1;
                }
            }

            return text.ToString();
        }

        private bool getTextFromBits(int i, bool[] pixels, StringBuilder binaryText, StringBuilder text,
            out string extractTextFromBytes1)
        {
            extractTextFromBytes1 = string.Empty;
            if (i % this.byteLength != 0 || i == 0)
            {
                return false;
            }

            var current = i;
            current -= this.bpcc;
            return this.bpcc == this.byteLength
                ? this.getTextIfBpccIs8(pixels, binaryText, text, ref extractTextFromBytes1, current)
                : this.getTextIfBpccLessThan8(pixels, binaryText, text, ref extractTextFromBytes1, current);
        }

        private bool getTextIfBpccIs8(bool[] pixels, StringBuilder binaryText, StringBuilder text,
            ref string extractedText, int current)
        {
            while (current % this.byteLength < this.bpcc)
            {
                this.appendBit(pixels, current, binaryText);
                current++;
                this.checkIfPassedKey(text);
                if (this.checkIfTextOver(binaryText, text))
                {
                    return this.convertTextToString(text, out extractedText);
                }
            }

            return false;
        }

        private bool getTextIfBpccLessThan8(bool[] pixels, StringBuilder binaryText, StringBuilder text,
            ref string extractedText, int current)
        {
            while (current % this.byteLength != this.firstX)
            {
                this.appendBit(pixels, current, binaryText);
                current++;
                this.checkIfPassedKey(text);
                if (this.checkIfTextOver(binaryText, text))
                {
                    return this.convertTextToString(text, out extractedText);
                }
            }

            return false;
        }

        private bool convertTextToString(StringBuilder text, out string extractTextFromBytes1)
        {
            this.resetHasPassedKey();
            {
                extractTextFromBytes1 = text.ToString();
                return true;
            }
        }

        private void appendBit(bool[] pixels, int current, StringBuilder binaryText)
        {
            if (pixels[current])
            {
                binaryText.Append('1');
            }
            else
            {
                binaryText.Append('0');
            }
        }

        private void checkIfPassedKey(StringBuilder text)
        {
            if (this.hasPassedKey && this.IsEncrypted() == EncryptionType.Encrypted)
            {
                text.Clear();
                this.hasPassedKey = false;
            }
        }

        private void resetHasPassedKey()
        {
            this.endOfTextCharactersPassed = 0;
            this.hasPassedKey = false;
        }

        private bool checkIfTextOver(StringBuilder binaryText, StringBuilder text)
        {
            if (this.isBinaryStringLongEnough(binaryText))
            {
                var letter = this.convertBinaryToLetter(binaryText);
                if (this.matchesEnding(text, letter))
                {
                    return true;
                }

                text.Append(letter);
                binaryText.Clear();
            }

            return false;
        }

        private bool matchesEnding(StringBuilder text, string letter)
        {
            if (letter.Equals(this.endOfText))
            {
                this.endOfTextCharactersPassed++;
                this.setKeyword(text);

                if (this.isTextOver())
                {
                    return true;
                }

                this.checkIfPassedKey();
            }

            return false;
        }

        private void checkIfPassedKey()
        {
            if (this.endOfTextCharactersPassed >= BeginningOfText && this.IsEncrypted() == EncryptionType.Encrypted)
            {
                this.hasPassedKey = true;
            }
        }

        private bool isTextOver()
        {
            return this.endOfTextCharactersPassed == EndOfText && this.IsEncrypted() == EncryptionType.Encrypted || this.IsEncrypted() == EncryptionType.Unencrypted;
        }

        private void setKeyword(StringBuilder text)
        {
            if (this.endOfTextCharactersPassed == EndOfKey)
            {
                this.KeyWord = text.ToString();
            }
        }

        private void checkBpccValue()
        {
            var sourcePixelColor = PixelManager.GetPixelBgra8(this.sourcePicture.Pixels, this.firstX, this.secondX,
                this.sourcePicture.Width,
                this.sourcePicture.Height);
            this.bpcc = sourcePixelColor.G;
        }

        private bool isBinaryStringLongEnough(StringBuilder binaryText)
        {
            return binaryText.Length == this.byteLength;
        }

        private string convertBinaryToLetter(StringBuilder binaryText)
        {
            var data = binaryText.ToString().GetBytesFromBinaryString();
            var letter = Encoding.ASCII.GetString(data);
            return letter;
        }

        #endregion
    }
}
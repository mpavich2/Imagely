using Windows.UI;
using GroupCStegafy.Constants;
using GroupCStegafy.Enums;
using GroupCStegafy.Utils;

namespace GroupCStegafy.Model
{
    /// <summary>
    ///     Defines the HeaderManager model class.
    /// </summary>
    public class HeaderManager
    {
        #region Data members

        private readonly Picture sourcePicture;

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the type of the encryption.
        /// </summary>
        /// <value>
        ///     The type of the encryption.
        /// </value>
        public EncryptionType EncryptionType { get; private set; }

        /// <summary>
        ///     Gets the BPCC.
        /// </summary>
        /// <value>
        ///     The BPCC.
        /// </value>
        public int Bpcc { get; private set; }

        /// <summary>
        ///     Gets the type of the file.
        /// </summary>
        /// <value>
        ///     The type of the file.
        /// </value>
        public FileType FileType { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="HeaderManager" /> class.
        /// </summary>
        /// <param name="sourcePicture">The source picture.</param>
        public HeaderManager(Picture sourcePicture)
        {
            this.sourcePicture = sourcePicture;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Updates the header information.
        /// </summary>
        /// <param name="encryptionType">Type of the encryption.</param>
        /// <param name="fileType">Type of the file.</param>
        /// <param name="bpcc">The BPCC.</param>
        public void UpdateHeaderInfo(EncryptionType encryptionType, FileType fileType, int bpcc)
        {
            this.EncryptionType = encryptionType;
            this.FileType = fileType;
            this.Bpcc = bpcc;
        }

        /// <summary>
        ///     Sets the header for hidden message.
        /// </summary>
        /// <returns>
        ///     source pixels
        /// </returns>
        public byte[] SetHeaderForHiddenMessage()
        {
            var imageWithHeader = this.sourcePicture.Pixels;
            for (var i = 0; i <= ImageConstants.SecondX; i++)
            {
                var pixelColor = PixelUtilities.GetPixelBgra8(imageWithHeader, ImageConstants.FirstX, i,
                    this.sourcePicture.Width,
                    this.sourcePicture.Height);
                if (i == ImageConstants.FirstX)
                {
                    this.setFirstPixel(pixelColor, imageWithHeader);
                }
                else
                {
                    this.setSecondPixel(pixelColor, imageWithHeader);
                }
            }

            return imageWithHeader;
        }

        /// <summary>
        ///     Skips the header location.
        /// </summary>
        /// <param name="height">The height.</param>
        /// <param name="width">The width.</param>
        /// <returns>
        ///     Width
        /// </returns>
        public static int SkipHeaderLocation(int height, int width)
        {
            switch (height)
            {
                case 0 when width == ImageConstants.FirstX:
                    width += 2;
                    break;
                case 0 when width == ImageConstants.SecondX:
                    width++;
                    break;
            }

            return width;
        }

        /// <summary>
        ///     Checks for encryption.
        /// </summary>
        /// <param name="pixelColor">Color of the pixel.</param>
        /// <returns>
        ///     Encrypted if LSB == 0, Unencrypted otherwise
        /// </returns>
        public static EncryptionType CheckForEncryption(Color pixelColor)
        {
            if (PixelUtilities.GetLeastSignificantBit(pixelColor.R) != ImageConstants.MinRgbValue)
            {
                return EncryptionType.Encrypted;
            }

            return EncryptionType.Unencrypted;
        }

        /// <summary>
        ///     Checks the BPCC value.
        /// </summary>
        /// <param name="pixelColor">Color of the pixel.</param>
        /// <returns>
        ///     Bpcc value
        /// </returns>
        public static int CheckBpccValue(Color pixelColor)
        {
            return pixelColor.G;
        }

        /// <summary>
        ///     Determines whether [contains hidden message].
        /// </summary>
        /// <param name="pixelColor">Color of the pixel.</param>
        /// <returns>
        ///     <c>true</c> if [contains hidden message]; otherwise, <c>false</c>.
        /// </returns>
        public static bool ContainsHiddenMessage(Color pixelColor)
        {
            return pixelColor.R == ImageConstants.HiddenMessageValue &&
                   pixelColor.G == ImageConstants.HiddenMessageValue &&
                   pixelColor.B == ImageConstants.HiddenMessageValue;
        }

        /// <summary>
        ///     Determines whether this instance is text.
        /// </summary>
        /// <param name="pixelColor">Color of the pixel.</param>
        /// <returns>
        ///     <c>true</c> if this instance is text; otherwise, <c>false</c>.
        /// </returns>
        public static bool CheckFileType(Color pixelColor)
        {
            return PixelUtilities.GetLeastSignificantBit(pixelColor.B) != ImageConstants.MinRgbValue;
        }

        private void setFirstPixel(Color pixelColor, byte[] imageWithHeader)
        {
            //TODO change magic numbers
            pixelColor.R = (byte) ImageConstants.HiddenMessageValue;
            pixelColor.G = (byte) ImageConstants.HiddenMessageValue;
            pixelColor.B = (byte) ImageConstants.HiddenMessageValue;
            PixelUtilities.SetPixelBgra8(imageWithHeader, ImageConstants.FirstX, ImageConstants.FirstX,
                pixelColor, this.sourcePicture.Width,
                this.sourcePicture.Height);
        }

        private void setSecondPixel(Color pixelColor, byte[] imageWithHeader)
        {
            if (this.EncryptionType == EncryptionType.Encrypted)
            {
                pixelColor.R |= 1;
            }
            else
            {
                pixelColor.R &= 0xfe;
            }

            if (this.FileType == FileType.Text)
            {
                pixelColor.B |= 1;
            }
            else
            {
                pixelColor.B &= 0xfe;
            }

            pixelColor.G = (byte) this.Bpcc;
            PixelUtilities.SetPixelBgra8(imageWithHeader, ImageConstants.FirstX, ImageConstants.SecondX,
                pixelColor, this.sourcePicture.Width,
                this.sourcePicture.Height);
        }

        #endregion
    }
}
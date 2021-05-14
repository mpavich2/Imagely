using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace Imagely.Model
{
    /// <summary>
    ///     Defines the Picture model class
    /// </summary>
    public class Picture
    {
        #region Properties

        /// <summary>
        ///     Gets the modified image.
        /// </summary>
        /// <value>
        ///     The modified image.
        /// </value>
        public WriteableBitmap ModifiedImage { get; set; }

        /// <summary>
        ///     Gets the pixels.
        /// </summary>
        /// <value>
        ///     The pixels.
        /// </value>
        public byte[] Pixels { get; set; }

        /// <summary>
        ///     Gets the height.
        /// </summary>
        /// <value>
        ///     The height.
        /// </value>
        public uint Height { get; set; }

        /// <summary>
        ///     Gets the width.
        /// </summary>
        /// <value>
        ///     The width.
        /// </value>
        public uint Width { get; set; }

        /// <summary>
        ///     Gets the dpi x.
        /// </summary>
        /// <value>
        ///     The dpi x.
        /// </value>
        public double DpiX { get; set; }

        /// <summary>
        ///     Gets the dpi y.
        /// </summary>
        /// <value>
        ///     The dpi y.
        /// </value>
        public double DpiY { get; set; }

        /// <summary>
        ///     Gets the vertical center.
        /// </summary>
        /// <value>
        ///     The vertical center.
        /// </value>
        public uint VerticalCenter => this.Height / 2;

        /// <summary>
        ///     Gets or sets the file.
        /// </summary>
        /// <value>
        ///     The file.
        /// </value>
        public StorageFile File { get; set; }

        #endregion
    }
}
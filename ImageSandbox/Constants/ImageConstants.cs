using Windows.UI.Xaml;

namespace GroupCStegafy.Constants
{
    /// <summary>
    ///     Defines the Constants class.
    /// </summary>
    internal class ImageConstants
    {
        #region Properties

        /// <summary>
        ///     Gets the length of the byte.
        /// </summary>
        /// <value>
        ///     The length of the byte.
        /// </value>
        public static int ByteLength => (int) Application.Current.Resources["ByteLength"];

        /// <summary>
        ///     Gets the maximum RGB value.
        /// </summary>
        /// <value>
        ///     The maximum RGB value.
        /// </value>
        public static int MaxRgbValue => (int) Application.Current.Resources["MaxRgbValue"];

        /// <summary>
        ///     Gets the minimum RGB value.
        /// </summary>
        /// <value>
        ///     The minimum RGB value.
        /// </value>
        public static int MinRgbValue => (int) Application.Current.Resources["MinRgbValue"];

        /// <summary>
        ///     Gets the end of text.
        /// </summary>
        /// <value>
        ///     The end of text.
        /// </value>
        public static string Ending => Application.Current.Resources["Ending"].ToString();

        /// <summary>
        ///     Gets the end of text.
        /// </summary>
        /// <value>
        ///     The end of text.
        /// </value>
        public static string EndOfText => Application.Current.Resources["EndOfText"].ToString();

        /// <summary>
        ///     Gets the first x.
        /// </summary>
        /// <value>
        ///     The first x.
        /// </value>
        public static int FirstX => (int) Application.Current.Resources["FirstX"];

        /// <summary>
        ///     Gets the second x.
        /// </summary>
        /// <value>
        ///     The second x.
        /// </value>
        public static int SecondX => (int) Application.Current.Resources["SecondX"];

        /// <summary>
        ///     Gets the hidden message value.
        /// </summary>
        /// <value>
        ///     The hidden message value.
        /// </value>
        public static int HiddenMessageValue => (int) Application.Current.Resources["HiddenMessageValue"];

        /// <summary>
        ///     Gets the ASCII value of a.
        /// </summary>
        /// <value>
        ///     The ASCII value of a.
        /// </value>
        public static int AsciiValueOfA => (int) Application.Current.Resources["AsciiValueOfA"];

        /// <summary>
        ///     Gets the length of the alphabet.
        /// </summary>
        /// <value>
        ///     The length of the alphabet.
        /// </value>
        public static int AlphabetLength => (int) Application.Current.Resources["AlphabetLength"];

        /// <summary>
        /// Gets the color channel count.
        /// </summary>
        /// <value>
        /// The color channel count.
        /// </value>
        public static int ColorChannelCount => (int) Application.Current.Resources["ColorChannelCount"];

        #endregion
    }
}
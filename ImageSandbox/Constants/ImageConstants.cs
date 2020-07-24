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

        #endregion
    }
}
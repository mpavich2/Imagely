using Windows.UI.Xaml;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Imagely.View.Dialogs
{
    /// <summary>
    ///     Defines the InvalidHiddenImageDialog Content Dialog class.
    /// </summary>
    /// <seealso cref="Windows.UI.Xaml.Controls.ContentDialog" />
    public sealed partial class InvalidHiddenImageDialog
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="InvalidHiddenImageDialog" /> class.
        /// </summary>
        public InvalidHiddenImageDialog()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Methods

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        #endregion
    }
}
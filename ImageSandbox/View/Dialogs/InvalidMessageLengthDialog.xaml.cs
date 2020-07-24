using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace GroupCStegafy.View.Dialogs
{
    /// <summary>
    ///     Defines the InvalidMessageLengthDialog Content Dialog class.
    /// </summary>
    /// <seealso cref="Windows.UI.Xaml.Controls.ContentDialog" />
    public sealed partial class InvalidMessageLengthDialog : ContentDialog
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="InvalidMessageLengthDialog" /> class.
        /// </summary>
        public InvalidMessageLengthDialog()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sets the error message text.
        /// </summary>
        /// <param name="text">The text.</param>
        public void setErrorMessageText(string text)
        {
            this.errorMessageTextBlock.Text = text;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        #endregion
    }
}
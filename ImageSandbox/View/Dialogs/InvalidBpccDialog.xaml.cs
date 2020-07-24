using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace GroupCStegafy.View.Dialogs
{
    /// <summary></summary>
    /// <seealso cref="Windows.UI.Xaml.Controls.ContentDialog" />
    public sealed partial class InvalidBpccDialog : ContentDialog
    {
        #region Constructors

        /// <summary>Initializes a new instance of the <see cref="InvalidBpccDialog" /> class.</summary>
        public InvalidBpccDialog()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Methods

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        #endregion
    }
}
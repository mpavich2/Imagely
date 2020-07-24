using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace GroupCStegafy.View.Dialogs
{
    /// <summary>
    /// </summary>
    /// <seealso cref="Windows.UI.Xaml.Controls.ContentDialog" />
    /// <seealso cref="Windows.UI.Xaml.Markup.IComponentConnector" />
    /// <seealso cref="Windows.UI.Xaml.Markup.IComponentConnector2" />
    public sealed partial class NoHiddenMessageDialog : ContentDialog
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="NoHiddenMessageDialog" /> class.
        /// </summary>
        public NoHiddenMessageDialog()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Methods

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        #endregion
    }
}
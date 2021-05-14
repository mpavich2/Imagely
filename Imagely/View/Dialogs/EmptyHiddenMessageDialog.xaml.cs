using Windows.UI.Xaml;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Imagely.View.Dialogs
{
    /// <summary>
    ///     The empty hidden message content dialog to display an error message.
    /// </summary>
    /// <seealso cref="Windows.UI.Xaml.Controls.ContentDialog" />
    public sealed partial class EmptyHiddenMessageDialog
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="EmptyHiddenMessageDialog" /> class.
        /// </summary>
        public EmptyHiddenMessageDialog()
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
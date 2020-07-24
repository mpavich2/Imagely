using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace GroupCStegafy.View.Dialogs
{
    /// <summary>
    ///     Defines the InvalidKeywordDialog Content Dialog class.
    /// </summary>
    /// <seealso cref="Windows.UI.Xaml.Controls.ContentDialog" />
    /// <seealso cref="Windows.UI.Xaml.Markup.IComponentConnector" />
    /// <seealso cref="Windows.UI.Xaml.Markup.IComponentConnector2" />
    public sealed partial class InvalidKeywordDialog : ContentDialog
    {
        #region Constructors

        /// <summary>Initializes a new instance of the <see cref="InvalidKeywordDialog" /> class.</summary>
        public InvalidKeywordDialog()
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
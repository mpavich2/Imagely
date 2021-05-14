using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Imagely.View.Dialogs
{
    /// <summary>
    ///     Defines the invalid file dialog content dialog.
    /// </summary>
    /// <seealso cref="Windows.UI.Xaml.Controls.ContentDialog" />
    public sealed partial class InvalidFileDialog
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="InvalidFileDialog" /> class.
        /// </summary>
        public InvalidFileDialog()
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

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Imagely.Model;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Imagely.View.Dialogs
{
    /// <summary>
    ///     Defines the add new color content dialog.
    /// </summary>
    /// <seealso cref="Windows.UI.Xaml.Controls.ContentDialog" />
    /// <seealso cref="Windows.UI.Xaml.Markup.IComponentConnector" />
    /// <seealso cref="Windows.UI.Xaml.Markup.IComponentConnector2" />
    public sealed partial class AddNewColorDialog
    {
        #region Data members

        /// <summary>
        ///     The remaining color property infos
        /// </summary>
        public readonly List<PropertyInfo> RemainingColorPropertyInfos;

        /// <summary>
        ///     The named colors list has changed.
        /// </summary>
        public bool ChangeOccured;

        private readonly List<NamedColor> namedColors;
        private readonly List<NamedColor> selectedColors;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="AddNewColorDialog" /> class.
        /// </summary>
        public AddNewColorDialog(List<NamedColor> selectedColors, List<PropertyInfo> remainingColorPropertyInfos)
        {
            this.InitializeComponent();
            this.namedColors = new List<NamedColor>();
            this.selectedColors = selectedColors;
            this.RemainingColorPropertyInfos = remainingColorPropertyInfos;
            this.loadColorsIntoListView();
        }

        #endregion

        #region Methods

        private void loadColorsIntoListView()
        {
            for (var i = 0; i < this.RemainingColorPropertyInfos.Count(); i++)
            {
                this.namedColors.Add(new NamedColor(this.RemainingColorPropertyInfos.ElementAt(i).Name,
                    (Color) this.RemainingColorPropertyInfos.ElementAt(i).GetValue(null)));
            }

            this.colorsListView.ItemsSource = this.namedColors;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.ChangeOccured = false;
            Hide();
        }

        private void colorsListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.colorsListView.SelectedItems.Count != 0)
            {
                this.addButton.IsEnabled = true;
            }
            else
            {
                this.addButton.IsEnabled = false;
            }
        }

        private void addSelectedColors()
        {
            foreach (NamedColor selectedItem in this.colorsListView.SelectedItems)
            {
                this.selectedColors.Add(selectedItem);
            }

            var colorsToRemove = new List<PropertyInfo>();
            for (var i = 0; i < this.RemainingColorPropertyInfos.Count; i++)
            {
                foreach (var color in this.selectedColors)
                {
                    if (color.Name.Equals(this.RemainingColorPropertyInfos.ElementAt(i).Name))
                    {
                        colorsToRemove.Add(this.RemainingColorPropertyInfos.ElementAt(i));
                    }
                }
            }

            foreach (var propertyInfo in colorsToRemove)
            {
                this.RemainingColorPropertyInfos.Remove(propertyInfo);
            }
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            this.ChangeOccured = true;
            this.addSelectedColors();
            Hide();
        }

        #endregion
    }
}
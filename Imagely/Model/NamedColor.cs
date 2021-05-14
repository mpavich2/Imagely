using Windows.UI;
using Windows.UI.Xaml.Media;

namespace Imagely.Model
{
    /// <summary>
    ///     Defines the named color class. Contains the name of the color and the value of the color.
    /// </summary>
    public class NamedColor
    {
        #region Properties

        /// <summary>
        ///     Gets or sets the name.
        /// </summary>
        /// <value>
        ///     The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the color.
        /// </summary>
        /// <value>
        ///     The color.
        /// </value>
        public Color Color { get; set; }

        /// <summary>
        ///     Gets the brush.
        /// </summary>
        /// <value>
        ///     The brush.
        /// </value>
        public SolidColorBrush Brush => new SolidColorBrush(this.Color);

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="NamedColor" /> class.
        /// </summary>
        /// <param name="colorName">Name of the color.</param>
        /// <param name="colorValue">The color value.</param>
        public NamedColor(string colorName, Color colorValue)
        {
            this.Name = colorName;
            this.Color = colorValue;
        }

        #endregion
    }
}
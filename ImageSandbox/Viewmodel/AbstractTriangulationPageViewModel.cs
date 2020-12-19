using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using GroupCStegafy.Model;
using GroupCStegafy.Model.DelaunayTriangulation;
using GroupCStegafy.Utils;
using Point = Windows.Foundation.Point;

namespace GroupCStegafy.Viewmodel
{
    /// <summary>
    ///     Defines the abstract triangulation page view model.
    /// </summary>
    public class AbstractTriangulationPageViewModel
    {
        #region Data members

        private readonly Delaunay delaunay;

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the source picture.
        /// </summary>
        /// <value>
        ///     The source picture.
        /// </value>
        public Picture SourcePicture { get; }

        /// <summary>
        ///     Gets the named colors.
        /// </summary>
        /// <value>
        ///     The named colors.
        /// </value>
        public List<NamedColor> NamedColors { get; }

        /// <summary>
        ///     Gets the color property infos.
        /// </summary>
        /// <value>
        ///     The color property infos.
        /// </value>
        public List<PropertyInfo> ColorPropertyInfos { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="AbstractTriangulationPageViewModel" /> class.
        /// </summary>
        public AbstractTriangulationPageViewModel()
        {
            this.SourcePicture = new Picture();
            this.delaunay = new Delaunay();
            this.NamedColors = new List<NamedColor>();
            this.ResetColorPropertyInfos();
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Draws the abstract triangle art.
        /// </summary>
        /// <param name="canvas">The canvas.</param>
        /// <param name="pointsText">The points text.</param>
        /// <param name="colors">The colors.</param>
        public void DrawAbstractTriangleArt(Canvas canvas, string pointsText, List<NamedColor> colors)
        {
            var pointCount = int.Parse(pointsText);
            var points = this.delaunay.GenerateRandomPoints(this.SourcePicture, pointCount, this.SourcePicture.Width,
                this.SourcePicture.Height);

            var triangulation = this.delaunay.BowyerWatson(points);
            foreach (var triangle in triangulation)
            {
                var polygon = new Polygon();
                var point1 = new Point(triangle.Vertices[0].X, triangle.Vertices[0].Y);
                var point2 = new Point(triangle.Vertices[1].X, triangle.Vertices[1].Y);
                var point3 = new Point(triangle.Vertices[2].X, triangle.Vertices[2].Y);
                if (polygon.Points != null)
                {
                    polygon.Points.Add(point1);
                    polygon.Points.Add(point2);
                    polygon.Points.Add(point3);
                }

                var rnd = new Random();
                var b = new byte[3];
                rnd.NextBytes(b);
                var color = Color.FromArgb(255, b[0], b[1], b[2]);
                if (colors.Count != 0)
                {
                    var randomIndex = rnd.Next(0, colors.Count);
                    color = colors[randomIndex].Color;
                }

                Brush brush = new SolidColorBrush(color);

                polygon.Fill = brush;
                polygon.Stroke = brush;
                polygon.StrokeThickness = 1.0;
                canvas.Children.Add(polygon);
            }
        }

        /// <summary>
        ///     Converts image to Picture then saves it.
        /// </summary>
        /// <param name="image">The image.</param>
        public async Task SaveImage(Image image)
        {
            var picture = await PictureUtilities.ConvertImageToPicture(image);
            FileUtilities.SaveImage(picture);
        }

        /// <summary>
        ///     Resets the color property infos.
        /// </summary>
        public void ResetColorPropertyInfos()
        {
            this.ColorPropertyInfos = typeof(Colors).GetRuntimeProperties().ToList();
        }

        /// <summary>
        ///     Clears the named colors.
        /// </summary>
        public void ClearNamedColors()
        {
            this.NamedColors.Clear();
        }

        /// <summary>
        ///     Removes the color at.
        /// </summary>
        /// <param name="index">The index.</param>
        public void RemoveColorAt(int index)
        {
            this.NamedColors.RemoveAt(index);
        }

        /// <summary>
        ///     Adds the random color.
        /// </summary>
        public void AddRandomColor()
        {
            var random = new Random();
            var randomIndex = random.Next(0, this.ColorPropertyInfos.Count());
            this.NamedColors.Add(new NamedColor(this.ColorPropertyInfos.ElementAt(randomIndex).Name,
                (Color) this.ColorPropertyInfos.ElementAt(randomIndex).GetValue(null)));
            this.ColorPropertyInfos.RemoveAt(randomIndex);
        }

        /// <summary>
        ///     Updates the color property information source.
        /// </summary>
        /// <param name="newColorPropertyInfos">The new color property infos.</param>
        public void UpdateColorPropertyInfoSource(List<PropertyInfo> newColorPropertyInfos)
        {
            this.ColorPropertyInfos = newColorPropertyInfos;
        }

        /// <summary>
        ///     Removes the remaining selected colors from property infos list.
        /// </summary>
        public void RemoveRemainingSelectedColorsFromPropertyInfosList()
        {
            var colorsToRemove = new List<PropertyInfo>();
            for (var i = 0; i < this.ColorPropertyInfos.Count; i++)
            {
                this.checkForMatchingColorName(i, colorsToRemove);
            }

            this.removeColorsFromPropertyInfos(colorsToRemove);
        }

        private void removeColorsFromPropertyInfos(List<PropertyInfo> colorsToRemove)
        {
            foreach (var propertyInfo in colorsToRemove)
            {
                this.ColorPropertyInfos.Remove(propertyInfo);
            }
        }

        private void checkForMatchingColorName(int i, List<PropertyInfo> colorsToRemove)
        {
            foreach (var color in this.NamedColors)
            {
                if (color.Name.Equals(this.ColorPropertyInfos.ElementAt(i).Name))
                {
                    colorsToRemove.Add(this.ColorPropertyInfos.ElementAt(i));
                }
            }
        }

        #endregion
    }
}
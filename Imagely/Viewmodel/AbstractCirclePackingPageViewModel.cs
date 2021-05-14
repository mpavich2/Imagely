using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Imagely.Model;
using Imagely.Model.CirclePacking;
using Imagely.Utils;

namespace Imagely.Viewmodel
{
    /// <summary>
    ///     Defines the abstract circle packing page view model.
    /// </summary>
    public class AbstractCirclePackingPageViewModel
    {
        #region Data members

        private int currentNewCircleRadius = 15;

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

        /// <summary>
        ///     Gets the circles.
        /// </summary>
        /// <value>
        ///     The circles.
        /// </value>
        public List<Circle> Circles { get; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="AbstractCirclePackingPageViewModel" /> class.
        /// </summary>
        /// <param name="height">The height.</param>
        /// <param name="width">The width.</param>
        public AbstractCirclePackingPageViewModel(uint height, uint width)
        {
            this.SourcePicture = new Picture { Height = height, Width = width };
            this.NamedColors = new List<NamedColor>();
            this.Circles = new List<Circle>();
            this.ResetColorPropertyInfos();
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Draws the abstract circle packing.
        /// </summary>
        /// <param name="canvas">The canvas.</param>
        /// <param name="circlesText">The points text.</param>
        /// <param name="randomizeColors">if set to <c>true</c> [randomize colors].</param>
        public void DrawAbstractCirclePacking(Canvas canvas, string circlesText, bool randomizeColors)
        {
            var circleCount = double.MaxValue;
            if (circlesText != string.Empty)
            {
                circleCount = int.Parse(circlesText);
            }

            var total = 10;
            var count = 0;
            var attempts = 0;
            while (this.currentNewCircleRadius > 0 && this.Circles.Count < circleCount)
            {
                attempts = this.addCircles(count, total, circleCount, attempts);
                if (attempts > 1000)
                {
                    break;
                }

                this.growCircles();
                count = 0;
                attempts = 0;
            }

            this.finishCircleGrowing();
            this.drawCircles(canvas, randomizeColors);
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
        ///     Clears the circles.
        /// </summary>
        public void ClearCircles()
        {
            this.Circles.Clear();
            this.currentNewCircleRadius = 15;
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
                (Color)this.ColorPropertyInfos.ElementAt(randomIndex).GetValue(null)));
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

        private Circle createCircle()
        {
            var random = new Random();
            var x = random.Next(17, (int)this.SourcePicture.Width - 17);
            var y = random.Next(17, (int)this.SourcePicture.Height - 17);
            var valid = true;
            foreach (var dummy in this.Circles.Where(circle => this.checkNewCircleCollision(x, y, circle, ref valid)))
            {
                break;
            }

            return valid ? new Circle(x, y, this.currentNewCircleRadius) : null;
        }

        private bool checkNewCircleCollision(int x, int y, Circle circle, ref bool valid)
        {
            var d = PixelUtilities.CalculateDistanceBetweenPixels(x, y, circle.X, circle.Y);
            if (d < circle.Radius + this.currentNewCircleRadius + 2)
            {
                valid = false;
                return true;
            }

            return false;
        }

        private static bool checkCircleGrowthCollision(Circle circle, Circle other, List<bool> growingBools, int i)
        {
            var d = PixelUtilities.CalculateDistanceBetweenPixels(circle.X, circle.Y, other.X,
                other.Y);
            var distance = circle.Radius + other.Radius;
            if (d - 2 < distance)
            {
                circle.Growing = false;
                growingBools[i] = false;
                return true;
            }

            return false;
        }

        private static bool checkCircleGrowthCollision(Circle circle, Circle other)
        {
            var d = PixelUtilities.CalculateDistanceBetweenPixels(circle.X, circle.Y, other.X,
                other.Y);
            var distance = circle.Radius + other.Radius;
            if (d - 2 < distance)
            {
                circle.Growing = false;
                return true;
            }

            return false;
        }

        private void finishCircleGrowing()
        {
            var growingBools = new List<bool>();
            foreach (var circle in this.Circles)
            {
                growingBools.Add(circle.Growing);
            }

            while (growingBools.Contains(true))
            {
                for (var i = 0; i < this.Circles.Count; i++)
                {
                    var circle = this.Circles[i];
                    if (circle.Growing)
                    {
                        if (circle.ExceedsBoundary(0, 0, (int)this.SourcePicture.Width,
                            (int)this.SourcePicture.Height))
                        {
                            circle.Growing = false;
                            growingBools[i] = false;
                        }
                        else
                        {
                            foreach (var dummy in this.Circles.Where(other => !circle.Equals(other)).Where(other =>
                                checkCircleGrowthCollision(circle, other, growingBools, i)))
                            {
                                break;
                            }
                        }
                    }

                    circle.Grow();
                }
            }
        }

        private void growCircles()
        {
            foreach (var circle in this.Circles)
            {
                if (circle.Growing)
                {
                    if (circle.ExceedsBoundary(0, 0, (int)this.SourcePicture.Width, (int)this.SourcePicture.Height))
                    {
                        circle.Growing = false;
                    }
                    else
                    {
                        foreach (var dummy in this.Circles.Where(other => !circle.Equals(other))
                                                  .Where(other => checkCircleGrowthCollision(circle, other)))
                        {
                            break;
                        }
                    }
                }

                circle.Grow();
            }
        }

        private int addCircles(int count, int total, double circleCount, int attempts)
        {
            while (count < total && this.Circles.Count < circleCount)
            {
                var newC = this.createCircle();
                if (newC != null)
                {
                    this.Circles.Add(newC);
                    count++;
                }

                attempts++;
                if (attempts > 1000 && this.currentNewCircleRadius > 0)
                {
                    this.currentNewCircleRadius--;
                    attempts = 0;
                }
            }

            return attempts;
        }

        private async void drawCircles(Canvas canvas, bool randomizeColors)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    foreach (var circle in this.Circles)
                    {

                        var path = new Path();
                        var ellipse = circle.GetEllipseFromCircle();
                        var brush = this.getCircleColorBrush(randomizeColors);

                        path.Fill = brush;
                        path.Stroke = brush;
                        path.StrokeThickness = 1.0;
                        path.Data = ellipse;

                        canvas.Children.Add(path);
                    }
                }
            );
        }

        private Brush getCircleColorBrush(bool randomizeColors)
        {
            var rnd = new Random();
            var b = new byte[3];
            rnd.NextBytes(b);
            var color = Color.FromArgb(255, b[0], b[1], b[2]);
            if (!randomizeColors && this.NamedColors.Count > 0)
            {
                var randomIndex = rnd.Next(0, this.NamedColors.Count);
                color = this.NamedColors[randomIndex].Color;
            }

            Brush brush = new SolidColorBrush(color);
            return brush;
        }

        #endregion
    }
}

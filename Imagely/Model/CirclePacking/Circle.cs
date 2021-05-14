using Windows.Foundation;
using Windows.UI.Xaml.Media;

namespace Imagely.Model.CirclePacking
{
    /// <summary>
    ///     Defines the circle class.
    /// </summary>
    public class Circle
    {
        #region Properties

        /// <summary>
        ///     Gets the x.
        /// </summary>
        /// <value>
        ///     The x.
        /// </value>
        public double X { get; }

        /// <summary>
        ///     Gets the y.
        /// </summary>
        /// <value>
        ///     The y.
        /// </value>
        public double Y { get; }

        /// <summary>
        ///     Gets the radius.
        /// </summary>
        /// <value>
        ///     The radius.
        /// </value>
        public double Radius { get; private set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this <see cref="Circle" /> is growing.
        /// </summary>
        /// <value>
        ///     <c>true</c> if growing; otherwise, <c>false</c>.
        /// </value>
        public bool Growing { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="Circle" /> class.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        public Circle(double x, double y)
        {
            this.X = x;
            this.Y = y;
            this.Radius = 3;
            this.Growing = true;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Circle" /> class.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="radius">The radius.</param>
        public Circle(double x, double y, double radius)
        {
            this.X = x;
            this.Y = y;
            this.Radius = radius;
            this.Growing = true;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Grows the circles radius.
        /// </summary>
        public void Grow()
        {
            if (this.Growing)
            {
                this.Radius += 0.5;
            }
        }

        /// <summary>
        ///     Checks if circle exceeds the boundary
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns>
        ///     True if exceeds boundary; false otherwise
        /// </returns>
        public bool ExceedsBoundary(int x, int y, int width, int height)
        {
            return this.X + this.Radius >= width ||
                   this.X - this.Radius <= x ||
                   this.Y + this.Radius >= height ||
                   this.Y - this.Radius <= y;
        }

        /// <summary>
        ///     Gets the ellipse from circle object.
        /// </summary>
        /// <returns>Ellipse Geometry</returns>
        public EllipseGeometry GetEllipseFromCircle()
        {
            return new EllipseGeometry
                {RadiusX = this.Radius, RadiusY = this.Radius, Center = new Point(this.X, this.Y)};
        }

        /// <summary>
        ///     Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///     <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return this.equals(obj as Circle);
        }

        /// <summary>
        ///     Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        ///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = 17;
                hashCode = (hashCode * 23) ^ this.X.GetHashCode();
                hashCode = (hashCode * 23) ^ this.Y.GetHashCode();
                return hashCode;
            }
        }

        private bool equals(Circle other)
        {
            return this.X.Equals(other.X) && this.Y.Equals(other.Y) && this.Radius.Equals(other.Radius) &&
                   this.Growing == other.Growing;
        }

        #endregion
    }
}
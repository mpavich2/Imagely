using System.Collections.Generic;

namespace Imagely.Model.DelaunayTriangulation
{
    /// <summary>
    ///     Defines the Point class.
    /// </summary>
    public class Point
    {
        #region Data members

        /// <summary>
        ///     Used only for generating a unique ID for each instance of this class that gets generated
        /// </summary>
        private static int counter;

        /// <summary>
        ///     Used for identifying an instance of a class; can be useful in troubleshooting when geometry goes weird
        ///     (e.g. when trying to identify when Triangle objects are being created with the same Point object twice)
        /// </summary>
        private readonly int instanceId = counter++;

        #endregion

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
        ///     Gets the adjacent triangles.
        /// </summary>
        /// <value>
        ///     The adjacent triangles.
        /// </value>
        public HashSet<Triangle> AdjacentTriangles { get; } = new HashSet<Triangle>();

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="Point" /> class.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        public Point(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Converts to string.
        /// </summary>
        /// <returns>
        ///     A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            // Simple way of seeing what's going on in the debugger when investigating weirdness
            return $"{nameof(Point)} {this.instanceId} {this.X:0.##}@{this.Y:0.##}";
        }

        #endregion
    }
}
namespace Imagely.Model.DelaunayTriangulation
{
    /// <summary>
    ///     Defines the Edge class.
    /// </summary>
    public class Edge
    {
        #region Properties

        /// <summary>
        ///     Gets the point1.
        /// </summary>
        /// <value>
        ///     The point1.
        /// </value>
        public Point Point1 { get; }

        /// <summary>
        ///     Gets the point2.
        /// </summary>
        /// <value>
        ///     The point2.
        /// </value>
        public Point Point2 { get; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="Edge" /> class.
        /// </summary>
        /// <param name="point1">The point1.</param>
        /// <param name="point2">The point2.</param>
        public Edge(Point point1, Point point2)
        {
            this.Point1 = point1;
            this.Point2 = point2;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///     <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            var edge = obj as Edge;

            var samePoints = edge != null && this.Point1 == edge.Point1 && this.Point2 == edge.Point2;
            var samePointsReversed = edge != null && this.Point1 == edge.Point2 && this.Point2 == edge.Point1;
            return samePoints || samePointsReversed;
        }

        /// <summary>
        ///     Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        ///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            var hCode = (int) this.Point1.X ^ (int) this.Point1.Y ^ (int) this.Point2.X ^ (int) this.Point2.Y;
            return hCode.GetHashCode();
        }

        #endregion
    }
}
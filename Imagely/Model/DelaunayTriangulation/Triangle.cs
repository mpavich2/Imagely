using System;
using System.Collections.Generic;
using System.Linq;

namespace Imagely.Model.DelaunayTriangulation
{
    /// <summary>
    ///     Defines the Triangle class.
    /// </summary>
    public class Triangle
    {
        #region Data members

        /// <summary>
        ///     The radius squared
        /// </summary>
        public double RadiusSquared;

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the vertices.
        /// </summary>
        /// <value>
        ///     The vertices.
        /// </value>
        public Point[] Vertices { get; } = new Point[3];

        /// <summary>
        ///     Gets the circumcenter.
        /// </summary>
        /// <value>
        ///     The circumcenter.
        /// </value>
        public Point Circumcenter { get; private set; }

        /// <summary>
        ///     Gets the triangles with shared edge.
        /// </summary>
        /// <value>
        ///     The triangles with shared edge.
        /// </value>
        public IEnumerable<Triangle> TrianglesWithSharedEdge
        {
            get
            {
                var neighbors = new HashSet<Triangle>();
                foreach (var vertex in this.Vertices)
                {
                    var trianglesWithSharedEdge =
                        vertex.AdjacentTriangles.Where(o => o != this && this.SharesEdgeWith(o));
                    neighbors.UnionWith(trianglesWithSharedEdge);
                }

                return neighbors;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="Triangle" /> class.
        /// </summary>
        /// <param name="point1">The point1.</param>
        /// <param name="point2">The point2.</param>
        /// <param name="point3">The point3.</param>
        /// <exception cref="ArgumentException">Must be 3 distinct points</exception>
        public Triangle(Point point1, Point point2, Point point3)
        {
            if (point1 == point2 || point1 == point3 || point2 == point3)
            {
                throw new ArgumentException("Must be 3 distinct points");
            }

            if (!this.isCounterClockwise(point1, point2, point3))
            {
                this.Vertices[0] = point1;
                this.Vertices[1] = point3;
                this.Vertices[2] = point2;
            }
            else
            {
                this.Vertices[0] = point1;
                this.Vertices[1] = point2;
                this.Vertices[2] = point3;
            }

            this.Vertices[0].AdjacentTriangles.Add(this);
            this.Vertices[1].AdjacentTriangles.Add(this);
            this.Vertices[2].AdjacentTriangles.Add(this);
            this.updateCircumcircle();
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Shareses the edge with.
        /// </summary>
        /// <param name="triangle">The triangle.</param>
        /// <returns></returns>
        public bool SharesEdgeWith(Triangle triangle)
        {
            var sharedVertices = this.Vertices.Count(o => triangle.Vertices.Contains(o));
            return sharedVertices == 2;
        }

        /// <summary>
        ///     Determines whether [is point inside circumcircle] [the specified point].
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns>
        ///     <c>true</c> if [is point inside circumcircle] [the specified point]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsPointInsideCircumcircle(Point point)
        {
            var dSquared = (point.X - this.Circumcenter.X) * (point.X - this.Circumcenter.X) +
                           (point.Y - this.Circumcenter.Y) * (point.Y - this.Circumcenter.Y);
            return dSquared < this.RadiusSquared;
        }

        private void updateCircumcircle()
        {
            var p0 = this.Vertices[0];
            var p1 = this.Vertices[1];
            var p2 = this.Vertices[2];
            var dA = p0.X * p0.X + p0.Y * p0.Y;
            var dB = p1.X * p1.X + p1.Y * p1.Y;
            var dC = p2.X * p2.X + p2.Y * p2.Y;

            var aux1 = dA * (p2.Y - p1.Y) + dB * (p0.Y - p2.Y) + dC * (p1.Y - p0.Y);
            var aux2 = -(dA * (p2.X - p1.X) + dB * (p0.X - p2.X) + dC * (p1.X - p0.X));
            var div = 2 * (p0.X * (p2.Y - p1.Y) + p1.X * (p0.Y - p2.Y) + p2.X * (p1.Y - p0.Y));

            if (div == 0)
            {
                throw new DivideByZeroException();
            }

            var center = new Point(aux1 / div, aux2 / div);
            this.Circumcenter = center;
            this.RadiusSquared = (center.X - p0.X) * (center.X - p0.X) + (center.Y - p0.Y) * (center.Y - p0.Y);
        }

        private bool isCounterClockwise(Point point1, Point point2, Point point3)
        {
            var result = (point2.X - point1.X) * (point3.Y - point1.Y) -
                         (point3.X - point1.X) * (point2.Y - point1.Y);
            return result > 0;
        }

        #endregion
    }
}
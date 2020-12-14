using System;
using System.Collections.Generic;
using System.Linq;
using GroupCStegafy.Utils;

namespace GroupCStegafy.Model.DelaunayTriangulation
{
    /// <summary>
    ///     Defines the Delaunay class.
    /// </summary>
    public class Delaunay
    {
        #region Data members

        private IEnumerable<Triangle> border;

        #endregion

        #region Properties

        private double MaxX { get; set; }
        private double MaxY { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Generates the points.
        /// </summary>
        /// <param name="sourcePicture">The source picture.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="maxX">The maximum x.</param>
        /// <param name="maxY">The maximum y.</param>
        /// <returns></returns>
        public IEnumerable<Point> GeneratePoints(Picture sourcePicture, int amount, double maxX, double maxY)
        {
            this.MaxX = maxX;
            this.MaxY = maxY;

            var point0 = new Point(0, 0);
            var point1 = new Point(0, this.MaxY);
            var point2 = new Point(this.MaxX, this.MaxY);
            var point3 = new Point(this.MaxX, 0);
            var points = new List<Point> { point0, point1, point2, point3 };
            var tri1 = new Triangle(point0, point1, point2);
            var tri2 = new Triangle(point0, point2, point3);
            this.border = new List<Triangle> { tri1, tri2 };

            var random = new Random();
            //for (var i = 0; i < amount - 4; i++)
            //{
            //    var pointX = random.NextDouble() * this.MaxX;
            //    var pointY = random.NextDouble() * this.MaxY;
            //    points.Add(new Point(pointX, pointY));
            //}
            //TODO add error handling for when not enough white points
            var whitePoints = this.findAllWhitePoints(sourcePicture).ToList();
            if (amount > whitePoints.Count)
            {
                throw new Exception("Amount of points cannot exceed the number of available points for triangles.");
            }
            for (var i = 0; i < amount - 4; i++)
            {
                var randomIndex = random.Next(whitePoints.Count());
                points.Add(whitePoints.ElementAt(randomIndex));
                whitePoints.RemoveAt(randomIndex);
            }

            return points;
        }

        /// <summary>
        ///     Applies the Bowyer Watson algorithm.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <returns>Enumerable collection of Triangles.</returns>
        public IEnumerable<Triangle> BowyerWatson(IEnumerable<Point> points)
        {
            var triangulation = new HashSet<Triangle>(this.border);

            foreach (var point in points)
            {
                var badTriangles = this.findBadTriangles(point, triangulation);
                var polygon = this.findHoleBoundaries(badTriangles);

                foreach (var triangle in badTriangles)
                {
                    foreach (var vertex in triangle.Vertices)
                    {
                        vertex.AdjacentTriangles.Remove(triangle);
                    }
                }

                triangulation.RemoveWhere(o => badTriangles.Contains(o));

                foreach (var edge in polygon.Where(possibleEdge =>
                    possibleEdge.Point1 != point && possibleEdge.Point2 != point))
                {
                    var triangle = new Triangle(point, edge.Point1, edge.Point2);
                    triangulation.Add(triangle);
                }
            }

            return triangulation;
        }

        /// <summary>
        /// Finds all white points.
        /// </summary>
        /// <param name="sourcePicture">The source picture.</param>
        /// <returns>Enumerable collection of all white points</returns>
        private IEnumerable<Point> findAllWhitePoints(Picture sourcePicture)
        {
            var points = new List<Point>();
            for (var i = 0; i < sourcePicture.Height; i++)
            {
                for (var j = 0; j < sourcePicture.Width; j++)
                {
                    var pixelColor = PixelManager.GetPixelBgra8(sourcePicture.Pixels, i, j, sourcePicture.Width,
                        sourcePicture.Height);
                    if (PixelManager.IsColorWhite(pixelColor))
                    {
                        points.Add(new Point(j, i));
                    }
                }
            }

            return points;
        }

        private IEnumerable<Edge> findHoleBoundaries(ISet<Triangle> badTriangles)
        {
            var edges = new List<Edge>();
            foreach (var triangle in badTriangles)
            {
                edges.Add(new Edge(triangle.Vertices[0], triangle.Vertices[1]));
                edges.Add(new Edge(triangle.Vertices[1], triangle.Vertices[2]));
                edges.Add(new Edge(triangle.Vertices[2], triangle.Vertices[0]));
            }

            var boundaryEdges = edges.GroupBy(o => o).Where(o => o.Count() == 1).Select(o => o.First());
            return boundaryEdges.ToList();
        }

        private ISet<Triangle> findBadTriangles(Point point, HashSet<Triangle> triangles)
        {
            var badTriangles = triangles.Where(o => o.IsPointInsideCircumcircle(point));
            return new HashSet<Triangle>(badTriangles);
        }

        #endregion
    }
}
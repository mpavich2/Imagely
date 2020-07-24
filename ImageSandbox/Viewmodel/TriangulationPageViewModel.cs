using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using GroupCStegafy.Model;
using GroupCStegafy.Model.DelaunayTriangulation;
using GroupCStegafy.Model.Filters;
using GroupCStegafy.Utils;
using Point = Windows.Foundation.Point;

namespace GroupCStegafy.Viewmodel
{
    /// <summary>
    ///     Defines the triangulation page view model.
    /// </summary>
    public class TriangulationPageViewModel
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
        public Picture SourcePicture { get; private set; }

        /// <summary>
        ///     Gets the copied picture.
        /// </summary>
        /// <value>
        ///     The copied picture.
        /// </value>
        public Picture CopiedPicture { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="TriangulationPageViewModel" /> class.
        /// </summary>
        public TriangulationPageViewModel()
        {
            this.SourcePicture = new Picture();
            this.CopiedPicture = new Picture();
            this.delaunay = new Delaunay();
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Opens the source image.
        /// </summary>
        public async Task OpenSourceImage()
        {
            var sourceImageFile = await FileUtilities.SelectFile();
            var copyBitmapImage = await FileUtilities.MakeCopyOfTheImage(sourceImageFile);

            using (var fileStream = await sourceImageFile.OpenAsync(FileAccessMode.Read))
            {
                var decoder = await BitmapDecoder.CreateAsync(fileStream);
                var transform = new BitmapTransform {
                    ScaledWidth = Convert.ToUInt32(copyBitmapImage.PixelWidth),
                    ScaledHeight = Convert.ToUInt32(copyBitmapImage.PixelHeight)
                };
                this.SourcePicture.ModifiedImage =
                    new WriteableBitmap((int) decoder.PixelWidth, (int) decoder.PixelHeight);

                this.SourcePicture.DpiX = decoder.DpiX;
                this.SourcePicture.DpiY = decoder.DpiY;

                var pixelData = await decoder.GetPixelDataAsync(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Straight,
                    transform,
                    ExifOrientationMode.IgnoreExifOrientation,
                    ColorManagementMode.DoNotColorManage
                );

                var sourcePixels = pixelData.DetachPixelData();
                this.SourcePicture.Pixels = sourcePixels;
                this.SourcePicture.Width = decoder.PixelWidth;
                this.SourcePicture.Height = decoder.PixelHeight;
            }
        }

        /// <summary>
        ///     Opens the dragged image.
        /// </summary>
        /// <param name="dragEvent">The <see cref="DragEventArgs" /> instance containing the event data.</param>
        public async Task OpenDraggedImage(DragEventArgs dragEvent)
        {
            try
            {
                await this.openDraggedFile(dragEvent);
            }
            catch (Exception exception)
            {
                //TODO make flyout that says please select a png or bmp image.
                Debug.WriteLine(exception.Message);
            }
        }

        /// <summary>
        ///     Applies the sobel filter.
        /// </summary>
        public void applySobelFilter()
        {
            var sobelFilter = new SobelFilter(this.CopiedPicture);
            sobelFilter.applySobelFilter();
        }

        /// <summary>
        ///     Applies the grayscale filter.
        /// </summary>
        public void applyGrayscaleFilter()
        {
            this.copySourcePicture();
            var grayscaleFilter = new GrayscaleFilter(this.CopiedPicture);
            grayscaleFilter.applyGrayscaleFilter();
        }

        /// <summary>
        ///     Converts image to Picture then saves it.
        /// </summary>
        /// <param name="image">The image.</param>
        public async Task SaveImage(Image image)
        {
            var renderTargetBitmap = new RenderTargetBitmap();
            await renderTargetBitmap.RenderAsync(image);
            var pixelBuffer = await renderTargetBitmap.GetPixelsAsync();
            var pixels = pixelBuffer.ToArray();
            var writableBitmap = new WriteableBitmap(renderTargetBitmap.PixelWidth, renderTargetBitmap.PixelHeight);
            using (var stream = writableBitmap.PixelBuffer.AsStream())
            {
                await stream.WriteAsync(pixels, 0, pixels.Length);
            }

            var picture = new Picture {
                ModifiedImage = writableBitmap,
                Width = (uint) renderTargetBitmap.PixelWidth,
                Height = (uint) renderTargetBitmap.PixelHeight
            };
            FileUtilities.SaveImage(picture);
        }

        /// <summary>
        ///     Clears the source and copied pictures.
        /// </summary>
        public void clear()
        {
            this.SourcePicture = new Picture();
            this.CopiedPicture = new Picture();
        }

        /// <summary>
        ///     Draws the triangulation.
        /// </summary>
        /// <param name="canvas">The canvas.</param>
        /// <param name="pointsText">The points text.</param>
        public void DrawTriangulation(Canvas canvas, string pointsText)
        {
            var pointCount = int.Parse(pointsText);
            var points = this.delaunay.GeneratePoints(this.CopiedPicture, pointCount, this.CopiedPicture.Width,
                this.CopiedPicture.Height);

            var triangulation = this.delaunay.BowyerWatson(points);
            foreach (var triangle in triangulation)
            {
                this.drawTriangle(canvas, triangle);
            }
        }

        /// <summary>
        ///     Draws the abstract triangle art.
        /// </summary>
        /// <param name="canvas">The canvas.</param>
        /// <param name="pointsText">The points text.</param>
        public void DrawAbstractTriangleArt(Canvas canvas, string pointsText)
        {
            //TODO refactor the hell out of this if keeping
            var pointCount = int.Parse(pointsText);
            var points = this.delaunay.GeneratePoints(this.CopiedPicture, pointCount, this.CopiedPicture.Width,
                this.CopiedPicture.Height);

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

                Brush brush = new SolidColorBrush(color);

                polygon.Fill = brush;
                polygon.Stroke = brush;
                polygon.StrokeThickness = 1.0;
                canvas.Children.Add(polygon);
            }
        }

        private void drawTriangle(Canvas canvas, Triangle triangle)
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

            if (polygon.Points != null)
            {
                var maxX = polygon.Points.Max(point => point.X);
                var maxY = polygon.Points.Max(point => point.Y);
                var minX = polygon.Points.Min(point => point.X);
                var minY = polygon.Points.Min(point => point.Y);
                var firstPoint = new Model.DelaunayTriangulation.Point(minX, minY);
                var secondPoint = new Model.DelaunayTriangulation.Point(maxX, maxY);
                var averageColor = PixelManager.GetAverageColor(this.SourcePicture, firstPoint, secondPoint);

                Brush brush = new SolidColorBrush(averageColor);
                polygon.Fill = brush;
                polygon.Stroke = brush;
                polygon.StrokeThickness = 1.0;
                canvas.Children.Add(polygon);
            }
        }

        private void copySourcePicture()
        {
            this.CopiedPicture.Pixels = (byte[]) this.SourcePicture.Pixels.Clone();
            this.CopiedPicture.ModifiedImage = this.SourcePicture.ModifiedImage;
            this.CopiedPicture.Height = this.SourcePicture.Height;
            this.CopiedPicture.Width = this.SourcePicture.Width;
        }

        private async Task openDraggedFile(DragEventArgs dragEvent)
        {
            if (dragEvent.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await dragEvent.DataView.GetStorageItemsAsync();
                var storageFile = items[0] as StorageFile;
                var copyBitmapImage = await FileUtilities.MakeCopyOfTheImage(storageFile);

                if (storageFile != null)
                {
                    using (var fileStream = await storageFile.OpenAsync(FileAccessMode.Read))
                    {
                        var decoder = await BitmapDecoder.CreateAsync(fileStream);
                        var transform = new BitmapTransform {
                            ScaledWidth = Convert.ToUInt32(copyBitmapImage.PixelWidth),
                            ScaledHeight = Convert.ToUInt32(copyBitmapImage.PixelHeight)
                        };
                        this.SourcePicture.ModifiedImage =
                            new WriteableBitmap((int) decoder.PixelWidth, (int) decoder.PixelHeight);
                        this.SourcePicture.DpiX = decoder.DpiX;
                        this.SourcePicture.DpiY = decoder.DpiY;

                        var pixelData = await decoder.GetPixelDataAsync(
                            BitmapPixelFormat.Bgra8,
                            BitmapAlphaMode.Straight,
                            transform,
                            ExifOrientationMode.IgnoreExifOrientation,
                            ColorManagementMode.DoNotColorManage
                        );

                        var sourcePixels = pixelData.DetachPixelData();
                        this.SourcePicture.Pixels = sourcePixels;
                        this.SourcePicture.Width = decoder.PixelWidth;
                        this.SourcePicture.Height = decoder.PixelHeight;
                    }
                }
            }
        }

        #endregion
    }
}
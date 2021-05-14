using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Imagely.Model;
using Imagely.Model.DelaunayTriangulation;
using Imagely.Model.Filters;
using Imagely.Utils;
using Imagely.View.Dialogs;

namespace Imagely.Viewmodel
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
        public async Task<bool> OpenSourceImage()
        {
            var sourceImageFile = await FileUtilities.SelectPngFile();
            if (!await this.isFileValid(sourceImageFile))
            {
                return false;
            }

            var copyBitmapImage = await FileUtilities.MakeCopyOfTheImage(sourceImageFile);
            await PictureUtilities.LoadImageData(this.SourcePicture, sourceImageFile, copyBitmapImage);
            this.SourcePicture.File = sourceImageFile;
            this.CopiedPicture.File = sourceImageFile;
            return true;
        }

        /// <summary>
        ///     Opens the dragged image.
        /// </summary>
        /// <param name="dragEvent">The <see cref="DragEventArgs" /> instance containing the event data.</param>
        /// <returns>True if file is valid, false otherwise</returns>
        public async Task<bool> OpenDraggedImage(DragEventArgs dragEvent)
        {
            try
            {
                await this.openDraggedFile(dragEvent);
                return true;
            }
            catch
            {
                await this.showInvalidFileDialog();
                return false;
            }
        }

        /// <summary>
        ///     Applies the sobel filter.
        /// </summary>
        public async void ApplySobelFilter()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    var sobelFilter = new SobelFilter(this.CopiedPicture);
                    sobelFilter.ApplySobelFilter();
                });
        }

        /// <summary>
        ///     Applies the grayscale filter.
        /// </summary>
        public async Task ApplyGrayscaleFilter()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    this.copySourcePicture();
                    var grayscaleFilter = new GrayscaleFilter(this.CopiedPicture);
                    grayscaleFilter.applyGrayscaleFilter();
                }
            );
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
        ///     Clears the source and copied pictures.
        /// </summary>
        public void Clear()
        {
            this.SourcePicture = new Picture();
            this.CopiedPicture = new Picture();
        }

        /// <summary>
        ///     Draws the triangulation.
        /// </summary>
        /// <param name="canvas">The canvas.</param>
        /// <param name="pointsText">The points text.</param>
        public async void DrawTriangulation(Canvas canvas, string pointsText)
        {
            try
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                    CoreDispatcherPriority.Normal,
                    () =>
                    {
                        var pointCount = int.Parse(pointsText);
                        var points = this.delaunay.GeneratePoints(this.CopiedPicture, pointCount,
                            this.CopiedPicture.Width,
                            this.CopiedPicture.Height);
                        var triangulation = this.delaunay.BowyerWatson(points);

                        foreach (var triangle in triangulation)
                        {
                            this.drawTriangle(canvas, triangle);
                        }
                    }
                );
            }
            catch
            {
                await this.showDivideByZeroDialog();
            }
        }

        private async Task<bool> isFileValid(StorageFile file)
        {
            if (file == null)
            {
                await this.showInvalidFileDialog();
                return false;
            }

            return true;
        }

        private async Task showInvalidFileDialog()
        {
            var dialog = new InvalidFileDialog();
            await dialog.ShowAsync();
        }

        private void drawTriangle(Canvas canvas, Triangle triangle)
        {
            var polygon = this.createPolygonFromTriangle(triangle);
            this.colorPolygon(polygon);
            canvas.Children.Add(polygon);
        }

        private void colorPolygon(Polygon polygon)
        {
            if (polygon.Points != null)
            {
                var maxX = polygon.Points.Max(point => point.X);
                var maxY = polygon.Points.Max(point => point.Y);
                var minX = polygon.Points.Min(point => point.X);
                var minY = polygon.Points.Min(point => point.Y);
                var firstPoint = new Point(minX, minY);
                var secondPoint = new Point(maxX, maxY);
                var averageColor = PixelUtilities.GetAverageColor(this.SourcePicture, firstPoint, secondPoint);

                Brush brush = new SolidColorBrush(averageColor);
                polygon.Fill = brush;
                polygon.Stroke = brush;
                polygon.StrokeThickness = 1.0;
            }
        }

        private Polygon createPolygonFromTriangle(Triangle triangle)
        {
            var polygon = new Polygon();
            var point1 = new Windows.Foundation.Point(triangle.Vertices[0].X, triangle.Vertices[0].Y);
            var point2 = new Windows.Foundation.Point(triangle.Vertices[1].X, triangle.Vertices[1].Y);
            var point3 = new Windows.Foundation.Point(triangle.Vertices[2].X, triangle.Vertices[2].Y);
            if (polygon.Points != null)
            {
                polygon.Points.Add(point1);
                polygon.Points.Add(point2);
                polygon.Points.Add(point3);
            }

            return polygon;
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
                var sourceImageFile = items[0] as StorageFile;
                var copyBitmapImage = await FileUtilities.MakeCopyOfTheImage(sourceImageFile);

                if (sourceImageFile != null)
                {
                    await PictureUtilities.LoadImageData(this.SourcePicture, sourceImageFile, copyBitmapImage);
                }
            }
        }

        private async Task showDivideByZeroDialog()
        {
            var dialog = new DivideByZeroDialog();
            await dialog.ShowAsync();
        }

        #endregion
    }
}
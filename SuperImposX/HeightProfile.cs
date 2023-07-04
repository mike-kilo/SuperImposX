using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using static SuperImposX.GPXProcessing;

namespace SuperImposX
{
    internal static class HeightProfileHelpers
    {
        private static double ToRadians(this double val)
        {
            return (Math.PI / 180) * val;
        }

        public static double GetDistance(this TrackPoint thisPoint, TrackPoint nextPoint) // Haversine formula
        {
            double R = 6371;
            var lat = (nextPoint.Latitude - thisPoint.Latitude).ToRadians();
            var lng = (nextPoint.Longitude - thisPoint.Longitude).ToRadians();
            var h1 = Math.Sin(lat / 2) * Math.Sin(lat / 2) +
                     Math.Cos(thisPoint.Latitude.ToRadians()) * Math.Cos(nextPoint.Latitude.ToRadians()) *
                     Math.Sin(lng / 2) * Math.Sin(lng / 2);
            var h2 = 2 * Math.Asin(Math.Min(1, Math.Sqrt(h1)));
            return R * h2;
        }
    }

    internal class HeightProfile
    {
        private struct ElevationPoint
        {
            public double Elevation { get; set; }

            public double Distance { get; set; }
        }

        public Canvas HeightProfileCanvas { get; private set; }

        private List<ElevationPoint> _points { get; set; }

        private Polygon _heightProfileTotal { get; set; }

        private Polygon _heightProfileElapsed { get; set; }

        private int _elapsedPointsCount;

        public int ElapsedPointsCount 
        {
            get
            {
                return this._elapsedPointsCount;
            }

            set
            {
                this._elapsedPointsCount = value;
                this.Redraw();
            }
        }

        private static List<ElevationPoint> ConvertToElevationPoints(IEnumerable<TrackPoint> points)
        {
            return points
                .Zip(points.Skip(1), (p, n) => new ElevationPoint { Elevation = n.Elevation ?? 0.0, Distance = n.GetDistance(p) })
                .Prepend(new ElevationPoint { Elevation = points.First().Elevation ?? 0.0, Distance = 0.0 })
                .ToList();
        }

        public HeightProfile(IEnumerable<TrackPoint> points)
        {
            this.HeightProfileCanvas = new Canvas();
            this.HeightProfileCanvas.ClipToBounds = true;
            this.HeightProfileCanvas.Background = new SolidColorBrush(Colors.LightGray) { Opacity = 0.309 };
            this.ElapsedPointsCount = 0;
            this.HeightProfileCanvas.SizeChanged += HeightProfileCanvas_SizeChanged;
            this._points = ConvertToElevationPoints(points);
        }

        ~HeightProfile()
        {
            this.HeightProfileCanvas.SizeChanged -= HeightProfileCanvas_SizeChanged;
        }

        private void HeightProfileCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_points is null) return;
            this.HeightProfileCanvas.Children.RemoveRange(0, this.HeightProfileCanvas.Children.Count);

            this._heightProfileTotal = this.GenerateHeightPolygon(new SolidColorBrush(Colors.LightGray) { Opacity = 0.309 });
            this.HeightProfileCanvas.Children.Add(this._heightProfileTotal);
            this._heightProfileElapsed = this.GenerateHeightPolygon(new SolidColorBrush(Colors.White) { Opacity = 0.618 });
            this.HeightProfileCanvas.Children.Add(this._heightProfileElapsed);

            this.Redraw();
        }

        private Polygon GenerateHeightPolygon(Brush brush)
        {
            var elevationBase = this._points.MinBy(p => p.Elevation).Elevation - 10.0;
            var elevationTop = this._points.MaxBy(p => p.Elevation).Elevation + 10.0;

            var cumulativeDistance = 0.0;
            var distances = this._points.Select(p => { cumulativeDistance += p.Distance; return cumulativeDistance; }).ToList();

            var scale = new Size() { Width = this.HeightProfileCanvas.ActualWidth / this._points.Select(P => P.Distance).Sum(), Height = this.HeightProfileCanvas.ActualHeight / (elevationTop - elevationBase) };
            var points = distances.Zip(this._points, (d, p) => new Point()
            {
                X = d * scale.Width,
                Y = (elevationTop - p.Elevation) * scale.Height,
            })
                .Prepend(new Point() { X = 0, Y = (elevationTop - elevationBase) * scale.Height })
                .Append(new Point() { X = this.HeightProfileCanvas.ActualWidth, Y = (elevationTop - elevationBase) * scale.Height });

            return new Polygon
            {
                Points = new PointCollection(points),
                StrokeStartLineCap = PenLineCap.Round,
                StrokeLineJoin = PenLineJoin.Round,
                StrokeEndLineCap = PenLineCap.Round,
                StrokeDashCap = PenLineCap.Round,
                Fill = brush,
            };
        }

        private void Redraw()
        {
            if (this._points is null) return;

            var elapsedDistance = this._points.Take(this.ElapsedPointsCount).Sum(p => p.Distance);
            var totalDistance = this._points.Sum(p => p.Distance);
            this._heightProfileTotal.Clip = new RectangleGeometry()
            {
                Rect = new Rect(
                    new Point() { X = this.HeightProfileCanvas.ActualWidth * elapsedDistance / totalDistance, Y = 0 },
                    new Point() { X = this.HeightProfileCanvas.ActualWidth, Y = this.HeightProfileCanvas.ActualHeight }),
            };

            this._heightProfileElapsed.Clip = new RectangleGeometry()
            {
                Rect = new Rect(
                    new Point() { X = 0, Y = 0 },
                    new Point() { X = this.HeightProfileCanvas.ActualWidth * elapsedDistance / totalDistance, Y = this.HeightProfileCanvas.ActualHeight })
            };
        }
    }
}

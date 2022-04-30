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
        public struct ElevationPoint
        {
            public double Elevation { get; set; }

            public double Distance { get; set; }
        }

        private static HeightProfile _instance;

        public static HeightProfile Instance
        {
            get
            {
                if (_instance == null) _instance = new HeightProfile();
                return _instance;
            }

            private set
            {
                _instance = value;
            }
        }

        public Canvas HeightProfileCanvas { get; private set; }

        public List<ElevationPoint> Points { get; set; }

        private HeightProfile()
        {
            this.HeightProfileCanvas = new Canvas();
            this.HeightProfileCanvas.ClipToBounds = true;
            this.Points = new List<ElevationPoint>();
        }

        public void Redraw()
        {
            this.HeightProfileCanvas.Children.Clear();
            var elevationBase = this.Points.MinBy(p => p.Elevation).Elevation - 10.0;
            var elevationTop = this.Points.MaxBy(p => p.Elevation).Elevation + 10.0;

            var cumulativeDistance = 0.0;
            var distances = this.Points.Select(p => { cumulativeDistance += p.Distance; return cumulativeDistance; }).ToList();

            var scale = new Size() { Width = this.HeightProfileCanvas.ActualWidth / this.Points.Select(P => P.Distance).Sum(), Height = this.HeightProfileCanvas.ActualHeight / ( elevationTop - elevationBase) };
            var points = distances.Zip(this.Points, (d, p) => new Point()
                {
                    X = d * scale.Width,
                    Y = (elevationTop - p.Elevation) * scale.Height,
                })
                .Prepend(new Point() { X = 0, Y = this.HeightProfileCanvas.ActualHeight })
                .Append(new Point() { X = this.HeightProfileCanvas.ActualWidth, Y = this.HeightProfileCanvas.ActualHeight });

            var height = new Polygon
            {
                Points = new PointCollection(points),
                StrokeStartLineCap = PenLineCap.Round,
                StrokeLineJoin = PenLineJoin.Round,
                StrokeEndLineCap = PenLineCap.Round,
                StrokeDashCap = PenLineCap.Round,
            };

            height.Fill = Brushes.Black;
            height.Opacity = 0.309;

            this.HeightProfileCanvas.Children.Add(height);
        }
    }
}

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
    internal static class TrackDrawing
    {
        public static Polyline CreatePolyline(this IEnumerable<TrackPoint> points, Size canvasSize, Helpers.Bounds<TrackPoint> trackBounds, int margin = 0)
        {
            var scale = new Size()
            {
                Width = (canvasSize.Width - 2 * margin) / (trackBounds.Max.Longitude - trackBounds.Min.Longitude),
                Height = (canvasSize.Height - 2 * margin) / (trackBounds.Max.Latitude - trackBounds.Min.Latitude),
            };

            var line = new Polyline
            {
                Points = new PointCollection(points.Select(p => new Point()
                {
                    X = margin + (p.Longitude - trackBounds.Min.Longitude) * scale.Width,
                    Y = margin + (trackBounds.Max.Latitude - p.Latitude) * scale.Height,
                })),
                StrokeStartLineCap = PenLineCap.Round,
                StrokeLineJoin = PenLineJoin.Round,
                StrokeEndLineCap = PenLineCap.Round,
                StrokeDashCap = PenLineCap.Round,
            };

            return line;
        }

        public static Ellipse CreateEllipse(this TrackPoint point, Size canvasSize, Helpers.Bounds<TrackPoint> trackBounds, int margin = 0)
        {
            var scale = new Size()
            {
                Width = (canvasSize.Width - 2 * margin) / (trackBounds.Max.Longitude - trackBounds.Min.Longitude),
                Height = (canvasSize.Height - 2 * margin) / (trackBounds.Max.Latitude - trackBounds.Min.Latitude),
            };

            var ellipse = new Ellipse()
            {
                Width = 10,
                Height = 10,
                Fill = Brushes.White,
                Stroke = Brushes.Black,
                StrokeThickness = 1,
            };

            Canvas.SetTop(ellipse, (trackBounds.Max.Latitude - point.Latitude) * scale.Height + 5);
            Canvas.SetLeft(ellipse, (point.Longitude - trackBounds.Min.Longitude) * scale.Width + 5);

            return ellipse;
        }
    }
}

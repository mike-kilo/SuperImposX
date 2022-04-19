using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using static SuperImposX.GPXProcessing;

namespace SuperImposX
{
    internal static class TrackDrawing
    {
        public static Polyline CreatePolyline(this IEnumerable<TrackPoint> points, Size canvasSize)
        {
            var trackBounds = points.GetBounds();
            var scale = new Size()
            {
                Width = canvasSize.Width / (trackBounds.Item2.Longitude - trackBounds.Item1.Longitude),
                Height = canvasSize.Height / (trackBounds.Item2.Latitude - trackBounds.Item1.Latitude),
            };

            var line = new Polyline
            {
                Points = new PointCollection(points.Select(p => new Point()
                {
                    X = (p.Longitude - trackBounds.Item1.Longitude) * scale.Width,
                    Y = (trackBounds.Item2.Latitude - p.Latitude) * scale.Height,
                })),
                StrokeStartLineCap = PenLineCap.Round,
                StrokeLineJoin = PenLineJoin.Round,
                StrokeEndLineCap = PenLineCap.Round,
                StrokeDashCap = PenLineCap.Round,
            };

            return line;
        }
    }
}

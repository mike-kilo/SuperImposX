using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static SuperImposX.Helpers;

namespace SuperImposX
{
    internal static class GPXProcessing
    {
        public struct TrackPoint
        {
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public double? Elevation { get; set; }
            public DateTime Timestamp { get; set; }
        }

        public static IEnumerable<TrackPoint> ReadGPX(string file)
        {
            XDocument gpxDoc = XDocument.Load(file);
            var n = gpxDoc.Nodes().ToList();
            return gpxDoc
                .Descendants()
                .Where(e => e.Name.LocalName == "trkpt")
                .Select(p =>
                {
                    return new TrackPoint
                    {
                        Latitude = double.Parse(p.Attribute("lat")?.Value ?? "0", System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture),
                        Longitude = double.Parse(p.Attribute("lon")?.Value ?? "0", System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture),
                        Elevation = double.Parse(p.Elements().Where(e => e.Name.LocalName == "ele").First()?.Value ?? "0", System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture),
                        Timestamp = DateTime.Parse(p.Elements().Where(e => e.Name.LocalName == "time").First()?.Value ?? DateTime.Now.ToString(), System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal),
                    };
                });
        }

        public static Bounds<TrackPoint> GetBounds(this IEnumerable<TrackPoint> points)
        {
            return new Bounds<TrackPoint>
            {
                Min = new TrackPoint
                {
                    Latitude = points.MinBy(p => p.Latitude).Latitude,
                    Longitude = points.MinBy(p => p.Longitude).Longitude,
                    Elevation = points.MinBy(p => p.Elevation).Elevation,
                    Timestamp = points.MinBy(p => p.Timestamp).Timestamp,
                },

                Max = new TrackPoint
                {
                    Latitude = points.MaxBy(p => p.Latitude).Latitude,
                    Longitude = points.MaxBy(p => p.Longitude).Longitude,
                    Elevation = points.MaxBy(p => p.Elevation).Elevation,
                    Timestamp = points.MaxBy(p => p.Timestamp).Timestamp,
                },
            };
        }
    }
}

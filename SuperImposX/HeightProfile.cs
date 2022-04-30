using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
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
            this.Points = new List<ElevationPoint>();
        }
    }
}

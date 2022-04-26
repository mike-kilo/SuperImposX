using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperImposX
{
    public static class Helpers
    {
        public struct Bounds<T>
        {
            public T Min { get; set; }

            public T Max { get; set; }
        }

        public static void Sort<T>(this ObservableCollection<T> collection)
        {
            var temp = collection.OrderBy(i => i).ToList();
            collection.Clear();
            temp.ForEach(i => collection.Add(i));
        }

        public struct ElapsedPoint : IComparable<ElapsedPoint>
        {
            public TimeSpan ElapsedTime { get; set; }

            public double Distance { get; set; }

            public string Filename { get; set; }

            public DateTime FileTime { get; set; }

            public int CompareTo(ElapsedPoint that)
            {
                return this.ElapsedTime.CompareTo(that.ElapsedTime);
            }
        }
    }
}

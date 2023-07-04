using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

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

        public static void SetWidthFromItems(this ComboBox comboBox)
        {
            if (comboBox.Template.FindName("PART_Popup", comboBox) is Popup popup && popup.Child is FrameworkElement popupContent)
            {
                popupContent.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                // suggested in comments, original answer has a static value 19.0
                var emptySize = SystemParameters.VerticalScrollBarWidth + comboBox.Padding.Left + comboBox.Padding.Right;
                comboBox.Width = emptySize + popupContent.DesiredSize.Width;
            }
        }
    }
}

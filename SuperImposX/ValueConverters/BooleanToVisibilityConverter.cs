using System;
using System.Windows;
using System.Windows.Data;

namespace SuperImposX.ValueConverters
{
    internal class BooleanToVisibilityConverter : IValueConverter
    {
        #region Constants
        
        private const string Message = "The target must be a visibility";

        #endregion

        #region IValueConverter Properties

        public Visibility TrueValue { get; set; }
        public Visibility FalseValue { get; set; }

        #endregion

        #region Constructors

        public BooleanToVisibilityConverter()
        {
            // set defaults
            FalseValue = Visibility.Hidden;
            TrueValue = Visibility.Visible;
        }

        #endregion

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(Visibility))
                throw new InvalidOperationException(Message);

            return (bool)value ? this.TrueValue : this.FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}

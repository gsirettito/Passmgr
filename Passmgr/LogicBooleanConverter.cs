namespace SiretT.Converters {
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    public enum LogicOperator {
        AND, OR, NOT, XOR
    }

    /// <summary>
    /// Checks equality of value and the converter parameter.
    /// Returns <see cref="true"/> if they are equal.
    /// Returns <see cref="false"/> if they are NOT equal.
    /// </summary>
    public class LogicBooleanConverter : IMultiValueConverter {
        public LogicOperator LogicOperator { get; set; }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            switch (LogicOperator) {
                case LogicOperator.AND:
                    for (int i = 0; i < values.Length; i++)
                        if (!(bool)values[i]) return false;
                    return true;
                case LogicOperator.OR:
                    for (int i = 0; i < values.Length; i++)
                        if ((bool)values[i]) return true;
                    return false;
                case LogicOperator.NOT:
                    for (int i = 0; i < values.Length; i++)
                        if ((bool)values[i]) return false;
                    return true;
            }
            return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
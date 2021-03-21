using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SiretT.Converters {
    public class BooleanNippleConverter : INippleConverter {

        public INippleConverter NippleConverter { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (NippleConverter != null) {
                value = NippleConverter.Convert(value, targetType, parameter, culture);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Binding.DoNothing;
        }
    }
}

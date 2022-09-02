using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Unsplasharp.Models;

namespace Stream.Wall.Converters {
    public class ExifIcon : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            if (value == null) return Visibility.Collapsed;
            var exif = (Exif)value;

            if (string.IsNullOrEmpty(exif.Model) && string.IsNullOrEmpty(exif.Make)) {
                return Visibility.Collapsed;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotImplementedException();
        }
    }
}

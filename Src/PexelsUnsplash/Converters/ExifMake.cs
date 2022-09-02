using System;
using Windows.UI.Xaml.Data;
using Unsplasharp.Models;

namespace Stream.Wall.Converters {
    public class ExifMake : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            if (value == null) return "";
            var exif = (Exif)value;

            return exif.Make ?? "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotImplementedException();
        }
    }
}

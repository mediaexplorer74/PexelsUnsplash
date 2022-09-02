using System;
using Windows.UI.Xaml.Data;
using Unsplasharp.Models;

namespace Stream.Wall.Converters {
    public class Username : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var user = (User)value;
            
            if (!string.IsNullOrEmpty(user.Username))
                return user.Username;

            var username = string.Format("{0} {1}", user.FirstName, user.LastName);

            if (!(string.IsNullOrWhiteSpace(username))) {
                return username;
            }

            if (!(string.IsNullOrWhiteSpace(user.Name))) {
                return user.Name;
            }

            return user.Username;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotImplementedException();
        }
    }
}

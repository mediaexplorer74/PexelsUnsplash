using System;
using Windows.UI.Xaml.Data;
using Unsplasharp.Models;

namespace Stream.Wall.Converters
{
    public class UserProfileImage : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return new Uri("ms-appx:///Assets/Icons/user.png");
            
            var profileImage = (ProfileImage)value;
            var path = profileImage.Medium ??
                        profileImage.Large ??
                        profileImage.Small ??
                        "ms-appx:///Assets/Icons/user.png";

            return new Uri(path);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

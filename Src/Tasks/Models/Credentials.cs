using System.IO;
using Windows.Storage;
using Newtonsoft.Json;

namespace Tasks.Models
{
    public static class Credentials
    {
        private static string _unsplashApplicationId = "";
        private static string _pexelsApplicationId = "";

        public static string UnsplashApplicationId
        {
            get
            {
                var settingsValues = ApplicationData.Current.LocalSettings.Values;
                settingsValues.TryGetValue("access_key", out var customAccessKey);

                if (customAccessKey != null)
                    return customAccessKey.ToString();

                if (string.IsNullOrEmpty(_unsplashApplicationId))
                {
                    _unsplashApplicationId = "xxx"; // paste unsplash api key here                       
                    
                    return _unsplashApplicationId;
                }

                return _unsplashApplicationId;
            }
        }
        
        public static string PexelsApplicationId
        {
            get
            {
                if (string.IsNullOrEmpty(_pexelsApplicationId))
                {
                    _pexelsApplicationId = "xxx"; // paste pexels api key here  
                                                   
                    return _pexelsApplicationId;
                }

                return _pexelsApplicationId;
            }
        }
    }
}
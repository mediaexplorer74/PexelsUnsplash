using System;
using System.Collections.Generic;
using System.Linq;
using PexelsDotNetSDK.Models;
using Unsplasharp.Models;
using Photo = Unsplasharp.Models.Photo;
using User = Unsplasharp.Models.User;

namespace Stream.Wall.Models
{
    public static class Mappings
    {
        public static List<Unsplasharp.Models.Photo> ToUnsplashModel(this PhotoPage pexelsPhotos)
        {
            try
            {
                return pexelsPhotos.photos.Select(x => new Unsplasharp.Models.Photo()
                {
                    Urls = new Urls
                    {
                        Full = x.height > x.width ? x.source.portrait : x.source.landscape,
                        Raw = x.source.original,
                        Small = x.source.small,
                        Custom = x.source.large2x,
                        Regular = x.height > x.width ? x.source.portrait : x.source.landscape,
                        Thumbnail = x.height > x.width ? x.source.portrait : x.source.landscape
                    },
                    Id = x.id.ToString(),
                    Height = x.height,
                    Width = x.width,
                    Color = x.avgColor,
                    User = new User {Username = x.photographer}
                })?.ToList();
            }
            catch (Exception e)
            {
                return new List<Photo>();
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Data;
using PexelsDotNetSDK.Api;
using PexelsDotNetSDK.Models;
using Tasks.Models;
using Unsplasharp;
using Unsplasharp.Models;

namespace Stream.Wall.Models
{
    public class PhotosList : ObservableCollection<Unsplasharp.Models.Photo>, ISupportIncrementalLoading
    {
        public string Url { get; set; }

        public int Page { get; set; }

        public bool HasMoreItems { get; set; }

        public UnsplashType Type { get; set; }

        public string Orientation { get; set; }

        public string OrderBy { get; set; }

        public string Query { get; set; }

        public int TotalPhotoCount { get; set; }

        private UnsplasharpClient _Client { get; set; }
        
        private PexelsClient _PexelsClient { get; set; }
        

        public IList<Unsplasharp.Models.Photo> Photos
        {
            get => Items;
        }

        public async Task<int> Fetch()
        {
            HasMoreItems = true;

            Page++;
            int added = 0;
            string fetchUrl = string.Format("{0}?page={1}", Url, Page);

            _Client = _Client ?? new UnsplasharpClient(Credentials.UnsplashApplicationId);
            _PexelsClient = _PexelsClient ?? new PexelsClient(Credentials.PexelsApplicationId);


            try
            {
                List<Unsplasharp.Models.Photo> photos = null;

                switch (Type)
                {
                    case UnsplashType.Default:
                        photos = await _Client.FetchPhotosList(fetchUrl);
                        break;
                    case UnsplashType.Search:
                    {
                        photos = await _Client.SearchPhotos($"{Query}&orientation={Orientation}", Page);
                        if (!photos.Any())
                        {
                            photos = (await _PexelsClient.SearchPhotosAsync(query: Query, orientation: Orientation, pageSize: 10, page: Page))
                                .ToUnsplashModel();
                        }
                        break;
                    }
                    case UnsplashType.Pexels:
                    {
                        photos = (await _PexelsClient.CuratedPhotosAsync(Page, 10))
                            .ToUnsplashModel();
                        break;
                    }
                    case UnsplashType.Random:
                        photos = await _Client.GetRandomPhoto(UnsplasharpClient.Orientation.Landscape, count: 10, query: Query);
                        break;
                }

                added = photos.Count;

                foreach (var photo in photos)
                {
                    Add(photo);
                }

                if (added == 0) HasMoreItems = false;
                return added;
            }
            catch /*(HttpRequestException hre)*/
            {
                HasMoreItems = false;
                return 0;
            }
        }

        IAsyncOperation<LoadMoreItemsResult> ISupportIncrementalLoading.LoadMoreItemsAsync(uint count)
        {
            return LoadMore(count).AsAsyncOperation();
        }

        public virtual async Task<LoadMoreItemsResult> LoadMore(uint count)
        {
            var added = await Fetch();
            return new LoadMoreItemsResult() {Count = (uint) added};
        }
    }
}
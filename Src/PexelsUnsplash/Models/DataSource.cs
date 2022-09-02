using System.Collections.Generic;
using System.Threading.Tasks;
using Stream.Wall.Services;
using Tasks.Models;
using Unsplasharp;
using Unsplasharp.Models;

namespace Stream.Wall.Models
{
    public class DataSource
    {
        #region variables

        private UnsplasharpClient _Client { get; set; }

        public PhotosList RecentPhotos { get; set; }

        public PhotosList CuratedPhotos { get; set; }

        public PhotosList PeoplePhotos { get; set; }

        public PhotosList GamePhotos { get; set; }


        public PhotosList SportPhotos { get; set; }


        public PhotosList NaturalPhotos { get; set; }


        public PhotosList MoviePhotos { get; set; }

        public PhotosList PexelsPhotos { get; set; }
        
        public PhotosList RandomPhotos { get; set; }

        public CollectionsList HomeCollections { get; set; }

        public PhotosList PhotosSearchResults { get; set; }

        public PhotosList UserPhotos { get; set; }

        public CollectionsList UserCollections { get; set; }

        public PhotosList CollectionPhotos { get; set; }

        public PhotosKeyedCollection LocalFavorites { get; set; }

        private static string BaseURI => "https://api.unsplash.com/";

        private static IDictionary<string, string> Endpoints = new Dictionary<string, string>() {
            {"photos", "photos" },
            {"curated_photos", "photos/curated" },
            {"search", "search" },
            {"search_photos", "search/photos" },
            {"users", "users" },
            {"collections", "collections" }
        };


        #endregion variables

        #region methods

        public DataSource()
        {
            _Client = new UnsplasharpClient(Credentials.UnsplashApplicationId);
        }


        public async Task<int> FetchPeoplePhotos()
        {
            if (PeoplePhotos == null) PeoplePhotos = new PhotosList();
            
            PeoplePhotos.Clear();
            PeoplePhotos.Page = 0;
            PeoplePhotos.Orientation = "landscape";
            PeoplePhotos.OrderBy = "latest";
            PeoplePhotos.Query = "people";      
            PeoplePhotos.Type = UnsplashType.People;
            return await PeoplePhotos.Fetch();
        }


        public async Task<int> FetchGamePhotos()
        {
            if (GamePhotos == null) GamePhotos = new PhotosList();
            
            GamePhotos.Clear();
            GamePhotos.Page = 0;
            GamePhotos.Orientation = "landscape";
            GamePhotos.OrderBy = "latest";
            GamePhotos.Query = "game";            
            GamePhotos.Type = UnsplashType.Game;
            return await GamePhotos.Fetch();
        }


        public async Task<int> FetchMoviePhotos()
        {
            if (MoviePhotos == null) MoviePhotos = new PhotosList();
            
            MoviePhotos.Clear();
            MoviePhotos.Page = 0;
            MoviePhotos.Orientation = "landscape";
            MoviePhotos.OrderBy = "latest";
            MoviePhotos.Query = "movie";  
            MoviePhotos.Type = UnsplashType.Movie;
            return await MoviePhotos.Fetch();
        }


        public async Task<int> FetchNaturalPhotos()
        {
            if (NaturalPhotos == null) NaturalPhotos = new PhotosList();
            
            NaturalPhotos.Clear();
            NaturalPhotos.Page = 0;
            NaturalPhotos.Orientation = "landscape";
            NaturalPhotos.OrderBy = "latest";
            NaturalPhotos.Query = "natural";
            NaturalPhotos.Type = UnsplashType.Natural;
            return await NaturalPhotos.Fetch();
        }


        public async Task<int> FetchSportPhotos()
        {
            if (SportPhotos == null) SportPhotos = new PhotosList();
            
            SportPhotos.Clear();
            SportPhotos.Page = 0;
            SportPhotos.Orientation = "landscape";
            SportPhotos.OrderBy = "latest";
            SportPhotos.Query = "sport";
            SportPhotos.Type = UnsplashType.Sport;
            return await SportPhotos.Fetch();
        }

        public async Task<int> FetchPexelsPhotos()
        {
            if (PexelsPhotos == null) PexelsPhotos = new PhotosList();

            PexelsPhotos.Clear();
            PexelsPhotos.Page = 0;
            PexelsPhotos.Orientation = "landscape";
            PexelsPhotos.Type = UnsplashType.Pexels;
            return await PexelsPhotos.Fetch();
        }
        
        public async Task<int> FetchRandomPhotos()
        {
            if (RandomPhotos == null) RandomPhotos = new PhotosList();

            RandomPhotos.Clear();
            RandomPhotos.Page = 0;
            RandomPhotos.Orientation = "landscape";
            RandomPhotos.Type = UnsplashType.Random;
            return await RandomPhotos.Fetch();
        }
        
        public async Task<int> FetchRecentPhotos()
        {
            if (RecentPhotos == null) RecentPhotos = new PhotosList();

            var url = GetUrl("photos");
            if (RecentPhotos.Url == url) return 0;

            RecentPhotos.Clear();
            RecentPhotos.Page = 0;
            RecentPhotos.Url = url;
            
            return await RecentPhotos.Fetch();
        }
        
        public async Task<int> FetchRecentCollections() {
            if (HomeCollections == null) HomeCollections = new CollectionsList();

            var url = GetUrl("collections");
            if (HomeCollections.Url == url) return 0;

            HomeCollections.Clear();
            HomeCollections.Page = 0;
            HomeCollections.Url = url;
            return await HomeCollections.Fetch();
        }
        
        
        public async Task<int> SearchPhotos(string query)
        {
            if (PhotosSearchResults == null) PhotosSearchResults = new PhotosList();

            PhotosSearchResults.Clear();
            PhotosSearchResults.Page = 0;
            PhotosSearchResults.Orientation = "landscape";
            PhotosSearchResults.OrderBy = "latest";
            PhotosSearchResults.Query = query;
            PhotosSearchResults.Type = UnsplashType.Search;
            return await PhotosSearchResults.Fetch();
        }

        public async Task<int> FetchCuratedPhotos()
        {
            if (CuratedPhotos == null) CuratedPhotos = new PhotosList();

            var url = string.Format(GetUrl("curated_photos"));
            if (CuratedPhotos.Url == url) return 0;

            CuratedPhotos.Clear();
            CuratedPhotos.Page = 0;
            CuratedPhotos.Url = url;
            return await CuratedPhotos.Fetch();
        }

        public async Task<int> ReloadPexelsPhotos()
        {
            if (PexelsPhotos == null) return 0;

            PexelsPhotos.Clear();
            PexelsPhotos.Page = 0;
            return await PexelsPhotos.Fetch();
        }
        
        public async Task<int> ReloadRandomPhotos()
        {
            if (RandomPhotos == null) return 0;

            RandomPhotos.Clear();
            RandomPhotos.Page = 0;
            return await RandomPhotos.Fetch();
        }
        
        public async Task<int> ReloadSearchPhotos(string query = "")
        {
            if (PhotosSearchResults == null) return 0;

            PhotosSearchResults.Clear();
            PhotosSearchResults.Page = 0;
            PhotosSearchResults.Query = query;
            return await PhotosSearchResults.Fetch();
        }


        public async Task<int> ReloadPeoplePhotos()
        {
            if (PeoplePhotos == null) return 0;

            PeoplePhotos.Clear();
            PeoplePhotos.Page = 0;
            return await PeoplePhotos.Fetch();
        }

        public async Task<int> ReloadGamePhotos()
        {
            if (GamePhotos == null) return 0;

            GamePhotos.Clear();
            GamePhotos.Page = 0;
            return await GamePhotos.Fetch();
        }

        public async Task<int> ReloadRecentPhotos()
        {
            if (RecentPhotos == null) return 0;

            RecentPhotos.Clear();
            RecentPhotos.Page = 0;
            return await RecentPhotos.Fetch();
        }

        public async Task<int> ReloadMoviePhotos()
        {
            if (MoviePhotos == null) return 0;

            MoviePhotos.Clear();
            MoviePhotos.Page = 0;
            return await MoviePhotos.Fetch();
        }


        public async Task<int> ReloadSportPhotos()
        {
            if (SportPhotos == null) return 0;

            SportPhotos.Clear();
            SportPhotos.Page = 0;
            return await SportPhotos.Fetch();
        }
        
        public async Task<int> ReloadNaturalPhotos()
        {
            if (NaturalPhotos == null) return 0;

            NaturalPhotos.Clear();
            NaturalPhotos.Page = 0;
            return await NaturalPhotos.Fetch();
        }

        public async Task<int> ReloadCuratedPhotos()
        {
            if (CuratedPhotos == null) return 0;

            CuratedPhotos.Clear();
            CuratedPhotos.Page = 0;
            return await CuratedPhotos.Fetch();
        }

        public async Task<int> ReloadRecentCollections()
        {
            if (HomeCollections == null) return 0;

            HomeCollections.Clear();
            HomeCollections.Page = 0;
            return await HomeCollections.Fetch();
        }

        public async Task<Unsplasharp.Models.Photo> GetPhoto(string id)
        {
            return await _Client.GetPhoto(id);
        }

        public async Task<User> GetUser(string username)
        {
            return await _Client.GetUser(username);
        }

        public async Task<int> GetUserPhotos(string username)
        {
            if (UserPhotos == null) UserPhotos = new PhotosList();

            var url = string.Format("{0}/{1}/photos", GetUrl("users"), username);
            if (UserPhotos.Url == url) return 0;

            UserPhotos.Clear();
            UserPhotos.Page = 0;
            UserPhotos.Url = url;
            return await UserPhotos.Fetch();
        }

        public async Task<int> GetUserCollections(string username)
        {
            if (UserCollections == null) UserCollections = new CollectionsList();

            var url = string.Format("{0}/{1}/collections", GetUrl("users"), username);
            if (UserCollections.Url == url) return 0;

            UserCollections.Clear();
            UserCollections.Page = 0;
            UserCollections.Url = url;
            return await UserCollections.Fetch();
        }

        public async Task<Collection> GetCollection(string id)
        {
            return await _Client.GetCollection(id);
        }

        public async Task<int> GetCollectionPhotos(string collectionId)
        {
            if (CollectionPhotos == null) CollectionPhotos = new PhotosList();

            var url = string.Format("{0}/{1}/photos", GetUrl("collections"), collectionId);
            if (CollectionPhotos.Url == url) return 0;

            CollectionPhotos.Clear();
            CollectionPhotos.Page = 0;
            CollectionPhotos.Url = url;
            return await CollectionPhotos.Fetch();
        }

        public async Task<List<Unsplasharp.Models.Photo>> GetRandomPhotos()
        {
            return await _Client.GetRandomPhoto(count: 30);
        }


        public string GetProfileImageLink(User user)
        {
            if (user == null || user.ProfileImage == null) return "";

            return user.ProfileImage.Medium ??
                   user.ProfileImage.Large ??
                   user.ProfileImage.Small;
        }

        public string GetUsernameFormated(User user)
        {
            if (user == null)
                return "";
            return user?.Username ??
                   user?.Name ??
                   string.Format("{0} {1}", user.FirstName, user.LastName);
        }

        public string GetUsername(User user)
        {
            return user.Username ??
                   string.Format("{0}{1}", user.FirstName, user.LastName).ToLower();
        }

        public async Task LoadLocalFavorites()
        {
            if (LocalFavorites == null)
            {
                var savedFavorites = await Settings.GetFavorites();

                if (savedFavorites == null)
                {
                    LocalFavorites = new PhotosKeyedCollection();
                    return;
                }

                LocalFavorites = savedFavorites;
            }
        }

        public async Task AddToFavorites(Unsplasharp.Models.Photo photo)
        {
            if (LocalFavorites.Contains(photo.Id))
            {
                return;
            }

            LocalFavorites.Add(photo);
            await Settings.SaveFavorites(LocalFavorites);
        }

        public async Task RemoveFromFavorites(Unsplasharp.Models.Photo photo)
        {
            if (!LocalFavorites.Contains(photo.Id))
            {
                return;
            }

            LocalFavorites.Remove(photo.Id);
            await Settings.SaveFavorites(LocalFavorites);
        }
        
        public static string GetUrl(string type) {
            switch (type) {
                case "photos":
                    return BaseURI + Endpoints["photos"];
                case "curated_photos":
                    return BaseURI + Endpoints["curated_photos"];
                case "users":
                    return BaseURI + Endpoints["users"];
                case "search":
                    return BaseURI + Endpoints["search"];
                case "search_photos":
                    return BaseURI + Endpoints["search_photos"];
                case "collections":
                    return BaseURI + Endpoints["collections"];
                default:
                    return null;
            }
        }

        #endregion methods
    }
}
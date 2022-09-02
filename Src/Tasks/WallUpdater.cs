using System;
using System.IO;
using System.Linq;
using System.Net.Http; //using System.Net.Http.Json;

using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.System.UserProfile;
using Tasks.Models;
using Unsplasharp;
using Unsplasharp.Models;
using Newtonsoft.Json;

namespace Tasks
{
    public sealed class WallUpdater : IBackgroundTask
    {
        /// <summary>
        ///     Task's Entry Point
        /// </summary>
        /// <param name="taskInstance">Task starting the method</param>
        async void IBackgroundTask.Run(IBackgroundTaskInstance taskInstance)
        {
            taskInstance.Canceled += OnCanceled;
            _Deferral = taskInstance.GetDeferral();

            StorageFile file = null;

            SaveTime(taskInstance);

            try
            {
                var result = await GetDailyBingPhoto();
                
                file = await DownloadAndSaveImagefromServer($"https://www.bing.com/{result.Images?.First()?.Url}", result.Images?.First()?.Title);
            }
            catch (Exception)
            {
                var photo = await GetRandom();

                var urlFormat = ChooseBestPhotoFormat(photo);

                file = await DownloadAndSaveImagefromServer(urlFormat, photo.Id);
            }

            if (taskInstance.Task.Name == WallTaskName)
                await SetWallpaperAsync(file);
            else
                await SetLockscreenAsync(file);

            _Deferral.Complete();
        }

        private string GetActivityKey(IBackgroundTaskInstance instance)
        {
            var key = instance.Task.Name == WallTaskName ? WallTaskName : LockscreenTaskName;
            key += "Activity";
            return key;
        }

        private void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            // Indicate that the background task is canceled.
            var key = GetActivityKey(sender);

            var localSettings = ApplicationData.Current.LocalSettings;
            var activityError = new ApplicationDataCompositeValue
            {
                ["DateTime"] = DateTime.Now.ToLocalTime(),
                ["Exception"] = reason.ToString()
            };

            localSettings.Values[key] = activityError;
        }

        private void SaveTime(IBackgroundTaskInstance instance)
        {
            var key = GetActivityKey(instance);

            var localSettings = ApplicationData.Current.LocalSettings;
            var stats = new ApplicationDataCompositeValue
            {
                ["DateTime"] = DateTime.Now.ToString(),
                ["Exception"] = null
            };

            localSettings.Values[key] = stats;
        }

        private async Task<Photo> GetRandom()
        {
            var client = new UnsplasharpClient(Credentials.UnsplashApplicationId);
            return (await client.GetRandomPhoto(UnsplasharpClient.Orientation.Landscape)).First();
        }

        private async Task<Bing> GetDailyBingPhoto()
        {
            var client = new HttpClient();

            string json = await client.GetStringAsync("https://www.bing.com/HPImageArchive.aspx?format=js&mkt=en-US&n=1");
            Bing b = JsonConvert.DeserializeObject<Bing>(json);
            
            return b; //return await client.GetFromJsonAsync<Bing>("https://www.bing.com/HPImageArchive.aspx?format=js&mkt=en-US&n=1");
        }

        private async Task<StorageFile> DownloadAndSaveImagefromServer(string uri, string filename)
        {
            filename += ".jpg";

            var targetFolder = await KnownFolders.PicturesLibrary.CreateFolderAsync("WallStream",
                CreationCollisionOption.OpenIfExists);

            var file = await targetFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);

            try
            {
                var client = new HttpClient();

                var buffer = await DownloadImageAsync(uri, client);
                await SaveImageAsync(file, buffer);

                return file;
            }
            catch
            {
                return null;
            }
        }

        private async Task SaveImageAsync(StorageFile file, byte[] buffer)
        {
            using (var stream = await file.OpenStreamForWriteAsync())
            {
                stream.Write(buffer, 0, buffer.Length); // Save
            }
        }

        private async Task<byte[]> DownloadImageAsync(string uri, HttpClient client)
        {
            var buffer = await client.GetByteArrayAsync(uri); // Download file
            return buffer;
        }

        // Pass in a relative path to a file inside the local appdata folder 
        private async Task<bool> SetLockscreenAsync(StorageFile file)
        {
            var success = false;

            if (UserProfilePersonalizationSettings.IsSupported())
            {
                var profileSettings = UserProfilePersonalizationSettings.Current;
                success = await profileSettings.TrySetLockScreenImageAsync(file);
            }

            return success;
        }

        private async Task<bool> SetWallpaperAsync(StorageFile file)
        {
            var success = false;

            if (UserProfilePersonalizationSettings.IsSupported())
            {
                var profileSettings = UserProfilePersonalizationSettings.Current;
                success = await profileSettings.TrySetWallpaperImageAsync(file);
            }

            return success;
        }

        private string ChooseBestPhotoFormat(Photo photo)
        {
            var url = photo.Urls.Raw;

            var localSettings = ApplicationData.Current.LocalSettings;

            if (!localSettings.Values.ContainsKey(BestPhotoResolutionKey)) return url;

            localSettings.Values.TryGetValue(BestPhotoResolutionKey, out var resolution);

            var format = (string) resolution;

            switch (format)
            {
                case "small":
                    url = photo.Urls.Small;
                    break;
                case "regular":
                    url = photo.Urls.Regular;
                    break;
                case "full":
                    url = photo.Urls.Full;
                    break;
            }

            return url;
        }

        #region variables

        private BackgroundTaskDeferral _Deferral;

        private static string WallTaskName => "WallUpdaterTask";

        private static string LockscreenTaskName => "LockscreenUpdaterTask";

        private static string BestPhotoResolutionKey => "BestPhotoResolution";

        #endregion variables
    }
}
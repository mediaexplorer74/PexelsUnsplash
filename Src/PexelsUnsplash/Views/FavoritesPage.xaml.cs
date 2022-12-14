using System;
using System.Threading;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Microsoft.Toolkit.Uwp.UI.Animations;
using Stream.Wall.Services;
using Tasks.Models;

namespace Stream.Wall.Views {
    public sealed partial class FavoritesPage : Page {
        #region variables

        double _AnimationDelay { get; set; }

        public static Unsplasharp.Models.Photo _LastSelectedPhoto { get; set; }

        private CoreDispatcher _UIDispatcher { get; set; }

        #endregion variables

        public FavoritesPage() {
            InitializeComponent();
            InitializeVariables();
            InitializeData();
            InitializePageAnimation();
            InitializeTitleBar();
        }

        #region navigation

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e) {
            CoreWindow.GetForCurrentThread().KeyDown -= Page_KeyDown;
            base.OnNavigatingFrom(e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            CoreWindow.GetForCurrentThread().KeyDown += Page_KeyDown;
            base.OnNavigatedTo(e);

            NavigateBackToGridItem();
        }

        private void Page_KeyDown(CoreWindow sender, KeyEventArgs args) {
            if (Events.IsBackOrEscapeKey(args.VirtualKey) && Frame.CanGoBack) {
                Frame.GoBack();
            }
        }

        private void NavigateBackToGridItem() {
            var animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("PhotoImageBack");

            if (animation == null || _LastSelectedPhoto == null) {
                return;
            }

            LocalFavoritesView.Loaded += (s, e) => {
                UI.AnimateBackItemToList(LocalFavoritesView, _LastSelectedPhoto, animation);
            };
        }

        private void CmdGoHome_Tapped(object sender, RoutedEventArgs e) {
            Frame.Navigate(typeof(HomePage));
        }

        #endregion navigation

        #region titlebar

        private void InitializeTitleBar() {
            App.DeviceType = Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily;

            if (App.DeviceType == "Windows.Mobile") {
                TitleBar.Visibility = Visibility.Collapsed;
                var statusBar = StatusBar.GetForCurrentView();
                statusBar.HideAsync();
                return;
            }

            Window.Current.Activated += Current_Activated;
            CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;

            TitleBar.Height = coreTitleBar.Height;
            Window.Current.SetTitleBar(TitleBarMainContent);

            coreTitleBar.IsVisibleChanged += CoreTitleBar_IsVisibleChanged;
            coreTitleBar.LayoutMetricsChanged += CoreTitleBar_LayoutMetricsChanged;
        }

        void CoreTitleBar_IsVisibleChanged(CoreApplicationViewTitleBar titleBar, object args) {
            TitleBar.Visibility = titleBar.IsVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        private void CoreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args) {
            TitleBar.Height = sender.Height;
            RightMask.Width = sender.SystemOverlayRightInset;
        }

        private void Current_Activated(object sender, WindowActivatedEventArgs e) {
            if (e.WindowActivationState != CoreWindowActivationState.Deactivated) {
                TitleBarMainContent.Opacity = 1;
                return;
            }

            TitleBarMainContent.Opacity = 0.5;
        }

        #endregion titlebar

        #region animations

        private void InitializePageAnimation() {
            TransitionCollection collection = new TransitionCollection();
            NavigationThemeTransition theme = new NavigationThemeTransition();

            var info = new ContinuumNavigationTransitionInfo();

            theme.DefaultNavigationTransitionInfo = info;
            collection.Add(theme);
            Transitions = collection;
        }

        #endregion animation

        #region data

        private void InitializeVariables() {
            _UIDispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
        }

        private async void InitializeData() {
            await App.DataSource.LoadLocalFavorites();
            var localFavorites = App.DataSource.LocalFavorites;

            HideLocalFavoritesLoadingView();

            if (localFavorites != null && localFavorites.Count > 0) {
                LocalFavoritesView.ItemsSource = localFavorites;
                return;
            }

            ShowLocalFavoritesEmptyView();
        }

        private void HideLocalFavoritesLoadingView() {
            LocalFavoritesLoadingView.Visibility = Visibility.Collapsed;
        }

        private void ShowLocalFavoritesEmptyView() {
            LocalFavoritesEmptyView.Visibility = Visibility.Visible;
        }

        #endregion data

        #region rightTapped flyout

        void ShowProgress(string message = "") {
            ProgressDeterminate.Value = 0;
            FlyoutNotification.Visibility = Visibility.Visible;

            if (string.IsNullOrEmpty(message)) return;
            FlyoutText.Text = message;
        }

        void HideProgress() {
            FlyoutNotification.Visibility = Visibility.Collapsed;
        }

        private void HttpProgressCallback(Windows.Web.Http.HttpProgress progress) {
            if (progress.TotalBytesToReceive == null) return;

            ProgressDeterminate.Minimum = 0;
            ProgressDeterminate.Maximum = (double)progress.TotalBytesToReceive;
            ProgressDeterminate.Value = progress.BytesReceived;
        }

        private void PhotoItem_RightTapped(object sender, RoutedEventArgs e) {
            var panel = (StackPanel)sender;
            var photo = (Unsplasharp.Models.Photo)panel.DataContext;

            _LastSelectedPhoto = photo;

            CheckIfPhotoInFavorites(photo);

            PhotoRightTappedFlyout.ShowAt(panel);
        }

        private void CheckIfPhotoInFavorites(Unsplasharp.Models.Photo photo) {
            if (App.DataSource.LocalFavorites == null) {
                RightCmdRemoveFavorites.Visibility = Visibility.Collapsed;
                return;
            }

            if (App.DataSource.LocalFavorites.Contains(photo.Id)) {
                RightCmdRemoveFavorites.Visibility = Visibility.Visible;
                return;
            }

            RightCmdRemoveFavorites.Visibility = Visibility.Collapsed;
        }

        private void CmdCopyLink_Tapped(object sender, RoutedEventArgs e) {
            var successMessage = App.ResourceLoader.GetString("CopyLinkSuccess");

            DataTransfer.Copy(_LastSelectedPhoto.Links.Html);
            Notify(successMessage);
        }

        private async void CmdSetAsWallpaper_Tapped(object sender, RoutedEventArgs e) {
            var progressMessage = App.ResourceLoader.GetString("SettingWallpaper");
            var successMessage = App.ResourceLoader.GetString("WallpaperSetSuccess");
            var failedMessage = App.ResourceLoader.GetString("WallpaperSetFailed");

            ShowProgress(progressMessage);
            var success = await Wallpaper.SetAsWallpaper(_LastSelectedPhoto, HttpProgressCallback);
            HideProgress();

            if (success) Notify(successMessage);
            else Notify(failedMessage);
        }

        private async void CmdSetAsLockscreen_Tapped(object sender, RoutedEventArgs e) {
            var progressMessage = App.ResourceLoader.GetString("SettingLockscreen");
            var successMessage = App.ResourceLoader.GetString("LockscreenSetSuccess");
            var failedMessage = App.ResourceLoader.GetString("LockscreenSetFailed");

            ShowProgress(progressMessage);
            var success = await Wallpaper.SetAsLockscreen(_LastSelectedPhoto, HttpProgressCallback);
            HideProgress();

            if (success) Notify(successMessage);
            else Notify(failedMessage);
        }

        private async void CmdOpenInBrowser_Tapped(object sender, RoutedEventArgs e) {
            if (_LastSelectedPhoto == null || _LastSelectedPhoto.Links == null) return;

            var tracking = "?utm_source=Hangon&utm_medium=referral&utm_campaign=" + Credentials.UnsplashApplicationId;
            var userUri = new Uri(string.Format("{0}{1}", _LastSelectedPhoto.Links.Html, tracking));
            var success = await Windows.System.Launcher.LaunchUriAsync(userUri);
        }

        private void CmdDownloadResolution_Tapped(object sender, RoutedEventArgs e) {
            var cmd = (MenuFlyoutItem)sender;
            var resolution = (string)cmd.Tag;
            Download(resolution);
        }

        private async void Download(string size = "") {
            ShowProgress();
            var result = false;

            if (string.IsNullOrEmpty(size)) {
                result = await Wallpaper.SaveToPicturesLibrary(_LastSelectedPhoto, HttpProgressCallback);

            } else {
                string url = getURL();
                result = await Wallpaper.SaveToPicturesLibrary(_LastSelectedPhoto, HttpProgressCallback, url);
            }

            HideProgress();

            var successMessage = App.ResourceLoader.GetString("SavePhotoSuccess");
            var failedMessage = App.ResourceLoader.GetString("SavePhotoFailed");

            if (result) Notify(successMessage);
            else Notify(failedMessage);

            string getURL() {
                switch (size) {
                    case "raw":
                        return _LastSelectedPhoto.Urls.Raw;
                    case "full":
                        return _LastSelectedPhoto.Urls.Full;
                    case "regular":
                        return _LastSelectedPhoto.Urls.Regular;
                    case "small":
                        return _LastSelectedPhoto.Urls.Small;
                    default:
                        return _LastSelectedPhoto.Urls.Regular;
                }
            }
        }

        private void RightCmdRemoveFavorites_Tapped(object sender, RoutedEventArgs e) {
            var localFavorites = App.DataSource.LocalFavorites;
            if (localFavorites == null) return;

            var cmd = (MenuFlyoutItem)sender;
            var photo = (Unsplasharp.Models.Photo)cmd.DataContext;

            if (photo == null || string.IsNullOrEmpty(photo.Id)) {
                DataTransfer.ShowLocalToast(App.ResourceLoader.GetString("PhotoNotFound"));
                return;
            }

            var task = App.DataSource.RemoveFromFavorites(photo);

            // TODO: Notify removed
            var message = App.ResourceLoader.GetString("PhotoSuccessfulRemovedFromFavorites");
            Notify(message);
        }

        #endregion rightTapped flyout

        #region notifications

        private void FlyoutNotification_Dismiss(object sender, RoutedEventArgs e) {
            HideNotification();
        }

        void Notify(string message) {
            FlyoutText.Text = message;

            FlyoutNotification.Opacity = 0;
            FlyoutNotification.Visibility = Visibility.Visible;

            FlyoutNotification
                .Offset(0, -30, 0)
                .Then()
                .Fade(1)
                .Offset(0)
                .Start();

            var autoEvent = new AutoResetEvent(false);
            var timer = new Timer(async (object state) => {
                await _UIDispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                    HideNotification();
                });
            }, autoEvent, TimeSpan.FromSeconds(5), new TimeSpan());
        }

        async void HideNotification() {
            await FlyoutNotification
                .Fade(0)
                .Offset(0, -30)
                .StartAsync();

            FlyoutNotification.Visibility = Visibility.Collapsed;
        }

        #endregion notifications

        #region events

        private void PhotoItem_Loaded(object sender, RoutedEventArgs e) {
            var photoItem = (StackPanel)sender;

            var data = (Unsplasharp.Models.Photo)photoItem.DataContext;

            if (data == _LastSelectedPhoto) {
                photoItem.Fade(1).Start();
                return;
            }

            photoItem.Offset(0, 100, 0)
                    .Then()
                    .Fade(1, 500, _AnimationDelay)
                    .Offset(0, 0, 500, _AnimationDelay)
                    .Start();

            _AnimationDelay += 100;
        }

        private void PhotoItem_Tapped(object sender, RoutedEventArgs e) {
            var item = (StackPanel)sender;
            var photo = (Unsplasharp.Models.Photo)item.DataContext;

            _LastSelectedPhoto = photo;

            var image = (Image)item.FindName("PhotoImage");

            if (image != null) {
                ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("PhotoImage", image);
            }

            var photosListParameter = App.DataSource.LocalFavorites.Photos;
            Frame.Navigate(typeof(PhotoPage), new object[] { photo, photosListParameter, this.GetType() });
        }

        private void Image_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e) {
            var image = (Image)sender;
            image.Scale(1.1f, 1.1f).Start();
        }

        private void Image_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e) {
            var image = (Image)sender;
            image.Scale(1f, 1f).Start();
        }

        #endregion events
    }
}

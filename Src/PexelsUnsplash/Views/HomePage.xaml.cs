using System;
using System.Linq;
using System.Threading;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Toolkit.Uwp.UI.Animations;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Stream.Wall.Models;
using Stream.Wall.Services;
using Tasks.Models;
using Unsplasharp.Models;
using TileDesigner = Stream.Wall.Services.TileDesigner;

namespace Stream.Wall.Views
{
    public sealed partial class HomePage : Page
    {
        #region variables

        private DataSource _PageDataSource { get; set; }

        private double _RecentAnimationDelay { get; set; }

        private double _PexelsAnimationDelay { get; set; }

        private double _SearchAnimationDelay { get; set; }
        
        private double _RandomAnimationDelay { get; set; }

        private int _CollectionAnimationDelay { get; set; }

        public static Unsplasharp.Models.Photo _LastPhotoSelected { get; set; }

        private static int _LastSelectedPivotIndex { get; set; }

        private static Collection _LastCollectionSelected { get; set; }

        private CoreDispatcher _UIDispatcher { get; set; }

        private float _CmdBarOpenedOffset { get; set; }

        private static bool _AreSearchResultsActivated { get; set; }

        private bool _IsPivotHeaderHidden { get; set; }
        
        private static bool _collectionDataIsSelected { get; set; }
        
        private ScrollViewer _scrollViewer;
        public event Action<ScrollViewer> OnScrollViewerViewChanged;
        
        private Visual _listVisual;
        
        private Compositor _compositor;

        private static string temporaryQuery;
        
        #endregion variables

        public HomePage()
        {
            InitializeComponent();
            InitializeVariables();
            InitializeTitleBar();

            BindAppDataSource();
            RestorePivotPosition();
            RecentGridData();
            HideUpdateChangelog();
        }
        
        #region navigation

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            CoreWindow.GetForCurrentThread().KeyDown -= Page_KeyDown;

            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            CoreWindow.GetForCurrentThread().KeyDown += Page_KeyDown;

            base.OnNavigatedTo(e);
        }

        private void Page_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            if (Events.IsBackOrEscapeKey(args.VirtualKey) && Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private void GoToFavorites_Tapped(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(FavoritesPage));
        }

        private void NavView_ItemInvoked(NavigationView sender,
            NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked == true)
            {
                NavView_Navigate("settings", args.RecommendedNavigationTransitionInfo);
            }
            else if (args.InvokedItemContainer != null)
            {
                var navItemTag = args.InvokedItemContainer.Tag.ToString();
                NavView_Navigate(navItemTag, args.RecommendedNavigationTransitionInfo);
            }
        }

        private int RestorePivotPosition()
        {
            PagePivot.SelectedIndex = _LastSelectedPivotIndex;
            return _LastSelectedPivotIndex;
        }

        private void RestoreLastSectionPosition()
        {
            _LastSelectedPivotIndex = PagePivot.SelectedIndex;
        }

        private void NavView_Navigate(
            string navItemTag, NavigationTransitionInfo transitionInfo)
        {
            Type _page = null;
            if (navItemTag == "settings")
            {
                Frame.Navigate(typeof(SettingsPage));
                _collectionDataIsSelected = false;
            }

            if (navItemTag == "favorite")
            {
                Frame.Navigate(typeof(FavoritesPage));
                _collectionDataIsSelected = false;
            }

            if (navItemTag == "pexels")
            {
                PexelsGridData();
                _collectionDataIsSelected = false;
            }
            
            if (navItemTag == "shuffle")
            {
                RandomGridData();
                _collectionDataIsSelected = false;
            }

            if (navItemTag == "collections")
            {
                CollectionsGridData();
            }

            if (navItemTag == "new")
            {
                _LastSelectedPivotIndex = 0;
                _collectionDataIsSelected = false;
                RecentGridData();
            }
        }


        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (!string.IsNullOrEmpty(args.QueryText))
            {
                temporaryQuery = args.QueryText;
                Search_Clicked();
                Search(args.QueryText);
                NavigationViewControl.IsPaneOpen = false;
            }
        }

        private void PexelsGridData()
        {
            PagePivot.SelectedIndex = 2;
            FindName("PexelsPhotosPivotItemContent");
            LoadPexelsData();
            NavigateBackToGridItem();
        }
        
        private void RandomGridData()
        {
            PagePivot.SelectedIndex = 4;
            FindName("RandomPhotosPivotItemContent");
            LoadRandomData();
            NavigateBackToGridItem();
        }

        private void CollectionsGridData()
        {
            PagePivot.SelectedIndex = 1;
            _collectionDataIsSelected = true;
            FindName("CollectionsPivotItemContent");
            LoadCollections();
            NavigateBackToGridItem();
        }

        private void RecentGridData()
        {
            var currentPivot = RestorePivotPosition();
            if (currentPivot == 2)
            {
                PexelsGridData();
            }
            else if (_collectionDataIsSelected)
            {
                CollectionsGridData();
            }
            else if (currentPivot == 3)
            {
                Search_Clicked();
            }
            else if (currentPivot == 4)
            {
                RandomGridData();
            }
            else
            {
                PagePivot.SelectedIndex = 0;
                FindName("RecentPhotosPivotItemContent");
                LoadRecentData();
                NavigateBackToGridItem();
            }
        }

        private void NavigateBackToGridItem()
        {
            if (PagePivot.SelectedIndex == 0)
            {
                var animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("PhotoImageBack");

                if (animation == null || _LastPhotoSelected == null)
                {
                    return;
                }

                animateRecent(animation);
            }
            else if (PagePivot.SelectedIndex == 1)
            {
                var animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("PhotoImageBack");

                if (animation == null || _LastCollectionSelected == null)
                {
                    return;
                }

                animateCollection(animation);
            }

            if (PagePivot.SelectedIndex == 2)
            {
                var animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("PhotoImageBack");

                if (animation == null || _LastPhotoSelected == null)
                {
                    return;
                }

                animatePexels(animation);
            }

            if (PagePivot.SelectedIndex == 3)
            {
                var animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("PhotoImageBack");

                if (animation == null || _LastPhotoSelected == null)
                {
                    return;
                }

                animateSearchResults(animation);
            }
            
            if (PagePivot.SelectedIndex == 4)
            {
                var animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("PhotoImageBack");

                if (animation == null || _LastPhotoSelected == null)
                {
                    return;
                }

                animateRandom(animation);
            }

            void animateRecent(ConnectedAnimation animation)
            {
                RecentView.Loaded += async (s, e) =>
                {
                    //UI.AnimateBackItemToList(RecentView, _LastSelectedPhoto, animation);
                    RecentView.ScrollIntoView(_LastPhotoSelected);
                    await RecentView.TryStartConnectedAnimationAsync(
                        animation, _LastPhotoSelected, "PhotoImage");
                };
            }

            void animatePexels(ConnectedAnimation animation)
            {
                PexelsView.Loaded += async (s, e) =>
                {
                    //UI.AnimateBackItemToList(RecentView, _LastSelectedPhoto, animation);
                    PexelsView.ScrollIntoView(_LastPhotoSelected);
                    await PexelsView.TryStartConnectedAnimationAsync(
                        animation, _LastPhotoSelected, "PhotoImage");
                };
            }
            
            
            void animateRandom(ConnectedAnimation animation)
            {
                RandomView.Loaded += async (s, e) =>
                {
                    //UI.AnimateBackItemToList(RecentView, _LastSelectedPhoto, animation);
                    RandomView.ScrollIntoView(_LastPhotoSelected);
                    await RandomView.TryStartConnectedAnimationAsync(
                        animation, _LastPhotoSelected, "PhotoImage");
                };
            }

            void animateCollection(ConnectedAnimation animation)
            {
                CollectionsView.Loaded += async (s, e) =>
                {
                    //UI.AnimateBackItemToList(CuratedView, _LastSelectedPhoto, animation);
                    CollectionsView.ScrollIntoView(_LastCollectionSelected);
                    await CollectionsView.TryStartConnectedAnimationAsync(
                        animation, _LastCollectionSelected, "PhotoImage");
                };
            }

            void animateSearchResults(ConnectedAnimation animation)
            {
                SearchPhotosView.Loaded += async (s, e) =>
                {
                    //UI.AnimateBackItemToList(SearchPhotosView, _LastSelectedPhoto, animation);
                    SearchPhotosView.ScrollIntoView(_LastPhotoSelected);
                    await SearchPhotosView.TryStartConnectedAnimationAsync(
                        animation, _LastPhotoSelected, "PhotoImage");
                };
            }
        }

        private void PhotoItem_Tapped(object sender, RoutedEventArgs e)
        {
            RestoreLastSectionPosition();
            var item = (StackPanel) sender;
            var photo = (Unsplasharp.Models.Photo) item.DataContext;

            _LastPhotoSelected = photo;
            _LastCollectionSelected = null;

            var image = (Image) item.FindName("PhotoImage");

            if (image != null)
            {
                ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("PhotoImage", image);
            }

            var photosListParameter = GetCurrentPhotosListSelected();
            Frame.Navigate(typeof(PhotoPage), new object[] {photo, photosListParameter, this.GetType()});

            PhotosList GetCurrentPhotosListSelected()
            {
                switch (_LastSelectedPivotIndex)
                {
                    case 0:
                        return _PageDataSource.RecentPhotos;
                    case 1:
                        return _PageDataSource.CollectionPhotos;
                    case 2:
                        return _PageDataSource.PexelsPhotos;
                    case 3:
                        return _PageDataSource.PhotosSearchResults;
                    case 4:
                        return _PageDataSource.RandomPhotos;
                    default:
                        return _PageDataSource.RecentPhotos;
                }
            }
        }

        private void CollectionItem_Tapped(object sender, RoutedEventArgs e)
        {
            var item = (Grid) sender;
            var collection = (Collection) item.DataContext;

            _LastCollectionSelected = collection;
            _LastPhotoSelected = null;

            var image = (Image) item.FindName("PhotoImage");

            if (image != null)
            {
                ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("CollectionCoverImage", image);
            }

            Frame.Navigate(typeof(CollectionPage), collection);
        }

        #endregion navigation

        #region titlebar

        private void InitializeTitleBar()
        {
            App.DeviceType = Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily;

            if (App.DeviceType == "Windows.Mobile")
            {
                var statusBar = StatusBar.GetForCurrentView();
                statusBar.HideAsync();
                return;
            }

            Window.Current.Activated += Current_Activated;
            CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;

            coreTitleBar.IsVisibleChanged += CoreTitleBar_IsVisibleChanged;
            coreTitleBar.LayoutMetricsChanged += CoreTitleBar_LayoutMetricsChanged;
        }

        void CoreTitleBar_IsVisibleChanged(CoreApplicationViewTitleBar titleBar, object args)
        {
        }

        private void CoreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
        }

        private void Current_Activated(object sender, WindowActivatedEventArgs e)
        {
            if (e.WindowActivationState != CoreWindowActivationState.Deactivated)
            {
                return;
            }
        }

        #endregion titlebar


        #region data

        private void InitializeVariables()
        {
            _CmdBarOpenedOffset = 15;
            _UIDispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
        }

        private void BindAppDataSource()
        {
            if (App.DataSource == null)
            {
                App.DataSource = new DataSource();
            }

            _PageDataSource = App.DataSource;
        }

        private async void LoadRecentData()
        {
            if (_PageDataSource.RecentPhotos?.Count > 0)
            {
                RecentView.ItemsSource = _PageDataSource.RecentPhotos;
                return;
            }

            ShowRecentLoadingView();

            var added = await _PageDataSource.FetchRecentPhotos();

            HideRecentLoadingView();

            if (added > 0)
            {
                RecentView.ItemsSource = _PageDataSource.RecentPhotos;

                if (BackgroundTasks.IsTileTaskActivated())
                {
                    TileDesigner.UpdatePrimary();
                }
                else
                {
                    TileDesigner.ClearPrimary();
                }
            }
            else
            {
                ShowRecentEmptyView();
                RecentView.Visibility = Visibility.Collapsed;
            }
        }

        private async void LoadPexelsData()
        {
            if (_PageDataSource.PexelsPhotos?.Count > 0)
            {
                PexelsView.ItemsSource = _PageDataSource.PexelsPhotos;
                return;
            }

            ShowPexelsLoadingView();

            var added = await _PageDataSource.FetchPexelsPhotos();

            HidePexelsLoadingView();

            if (added > 0)
            {
                PexelsView.ItemsSource = _PageDataSource.PexelsPhotos;

                if (BackgroundTasks.IsTileTaskActivated())
                {
                    TileDesigner.UpdatePrimary();
                }
                else
                {
                    TileDesigner.ClearPrimary();
                }
            }
            else
            {
                ShowPexelsEmptyView();
                PexelsView.Visibility = Visibility.Collapsed;
            }
        }
        
        
        private async void LoadRandomData()
        {
            if (_PageDataSource.RandomPhotos?.Count > 0)
            {
                RandomView.ItemsSource = _PageDataSource.RandomPhotos;
                return;
            }

            ShowRandomLoadingView();

            var added = await _PageDataSource.FetchRandomPhotos();

            HideRandomLoadingView();

            if (added > 0)
            {
                RandomView.ItemsSource = _PageDataSource.RandomPhotos;

                if (BackgroundTasks.IsTileTaskActivated())
                {
                    TileDesigner.UpdatePrimary();
                }
                else
                {
                    TileDesigner.ClearPrimary();
                }
            }
            else
            {
                ShowRandomEmptyView();
                RandomView.Visibility = Visibility.Collapsed;
            }
        }
        
        void ShowRandomLoadingView()
        {
            RandomLoadingView.Visibility = Visibility.Visible;
        }
        
        void ShowPexelsLoadingView()
        {
            PexelsLoadingView.Visibility = Visibility.Visible;
        }

        void ShowSearchLoadingView()
        {
            SearchLoadingView.Visibility = Visibility.Visible;
        }

        void HideSearchLoadingView()
        {
            SearchLoadingView.Visibility = Visibility.Collapsed;
        }

        void HidePexelsLoadingView()
        {
            PexelsLoadingView.Visibility = Visibility.Collapsed;
        }
        
        
        void HideRandomLoadingView()
        {
            RandomLoadingView.Visibility = Visibility.Collapsed;
        }

        void ShowPexelsEmptyView()
        {
            PexelsEmptyView.Visibility = Visibility.Visible;
        }
        
        void ShowRandomEmptyView()
        {
            RandomEmptyView.Visibility = Visibility.Visible;
        }
        
        void ShowSearchEmptyView()
        {
            SearchEmptyView.Visibility = Visibility.Visible;
        }

        void ShowRecentLoadingView()
        {
            RecentLoadingView.Visibility = Visibility.Visible;
        }

        void HideRecentLoadingView()
        {
            RecentLoadingView.Visibility = Visibility.Collapsed;
        }

        void ShowRecentEmptyView()
        {
            RecentEmptyView.Visibility = Visibility.Visible;
        }

        private async void LoadCollections()
        {
            if (_PageDataSource.HomeCollections?.Count > 0)
            {
                CollectionsView.ItemsSource = _PageDataSource.HomeCollections;
                return;
            }

            ShowCollectionsLoadingView();

            var added = await _PageDataSource.FetchRecentCollections();

            HideCollectionsLoadingView();

            if (added > 0)
            {
                CollectionsView.ItemsSource = _PageDataSource.HomeCollections;
                return;
            }

            ShowCollectionsEmptyView();
            CollectionsView.Visibility = Visibility.Collapsed;
        }

        void ShowCollectionsLoadingView()
        {
            CollectionsLoadingView.Visibility = Visibility.Visible;
        }

        void HideCollectionsLoadingView()
        {
            CollectionsLoadingView.Visibility = Visibility.Collapsed;
        }

        void ShowCollectionsEmptyView()
        {
            CollectionsEmptyView.Visibility = Visibility.Visible;
        }

        #endregion data

        #region events

        private void PhotoItem_Loaded(object sender, RoutedEventArgs e)
        {
            var photoItem = (StackPanel) sender;

            var data = (Unsplasharp.Models.Photo) photoItem.DataContext;

            if (data == _LastPhotoSelected)
            {
                photoItem.Fade(1).Start();
                return;
            }

            var delay = GetAnimationDelayPivotIndex();

            photoItem.Offset(0, 100, 0)
                .Then()
                .Fade(1, 500, delay)
                .Offset(0, 0, 500, delay)
                .Start();

            double GetAnimationDelayPivotIndex()
            {
                var step = 100;

                switch (PagePivot.SelectedIndex)
                {
                    case 0:
                        return _RecentAnimationDelay += step;
                    case 1:
                        return _CollectionAnimationDelay += step;
                    case 2:
                        return _PexelsAnimationDelay += step;
                    case 3:
                        return _SearchAnimationDelay += step;
                    case 4:
                        return _RandomAnimationDelay += step;
                    default:
                        return 0;
                }
            }
        }
        
        private void GridView_Loaded_Random(object sender, RoutedEventArgs e)
        {
            _scrollViewer = RandomView.GetScrollViewer();
            _scrollViewer.ViewChanging -= ScrollViewer_ViewChanging;
            _scrollViewer.ViewChanging += ScrollViewer_ViewChanging;
        }

        private void GridView_Loaded_Recent(object sender, RoutedEventArgs e)
        {
            _scrollViewer = RecentView.GetScrollViewer();
            _scrollViewer.ViewChanging -= ScrollViewer_ViewChanging;
            _scrollViewer.ViewChanging += ScrollViewer_ViewChanging;
        }
        
        private void GridView_Loaded_Pexels(object sender, RoutedEventArgs e)
        {
            _scrollViewer = PexelsView.GetScrollViewer();
            _scrollViewer.ViewChanging -= ScrollViewer_ViewChanging;
            _scrollViewer.ViewChanging += ScrollViewer_ViewChanging;
        }
        
        private void GridView_Loaded_Collections(object sender, RoutedEventArgs e)
        {
            _scrollViewer = CollectionsView.GetScrollViewer();
            _scrollViewer.ViewChanging -= ScrollViewer_ViewChanging;
            _scrollViewer.ViewChanging += ScrollViewer_ViewChanging;
        }
        
        private void GridView_Loaded_Search(object sender, RoutedEventArgs e)
        {
            _scrollViewer = SearchPhotosView.GetScrollViewer();
            _scrollViewer.ViewChanging -= ScrollViewer_ViewChanging;
            _scrollViewer.ViewChanging += ScrollViewer_ViewChanging;
        }

        private void ScrollViewer_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            OnScrollViewerViewChanged?.Invoke(sender as ScrollViewer);
        }

        private void UserView_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var ellipse = (Ellipse) sender;
            ellipse.Scale(1.1f, 1.1f).Start();
        }

        private void UserView_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            var ellipse = (Ellipse) sender;
            ellipse.Scale(1f, 1f).Start();
        }

        private void CollectionItem_Loaded(object sender, RoutedEventArgs e)
        {
            var collectionItem = (Grid) sender;

            var data = (Collection) collectionItem.DataContext;

            if (data == _LastCollectionSelected)
            {
                collectionItem.Fade(1).Start();
                _LastCollectionSelected = null;
                return;
            }

            _CollectionAnimationDelay += 100;

            collectionItem.Offset(0, 100, 0)
                .Then()
                .Fade(1, 500, _CollectionAnimationDelay)
                .Offset(0, 0, 500, _CollectionAnimationDelay)
                .Start();
        }

        private void CollectionItem_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            var panel = (Grid) sender;
            var image = (Image) panel.FindName("PhotoImage");

            if (image == null) return;

            image.Scale(1.1f, 1.1f).Start();
        }

        private void CollectionItem_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            var panel = (Grid) sender;
            var image = (Image) panel.FindName("PhotoImage");

            if (image == null) return;

            image.Scale(1f, 1f).Start();
        }

        #endregion events

        #region micro-interactions

        private void Image_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var image = (Image) sender;
            image.Scale(1.1f, 1.1f).Start();
        }

        private void Image_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var image = (Image) sender;
            image.Scale(1f, 1f).Start();
        }

        #endregion micro-interactions

        #region commandbar

        private void CmdRefresh_Tapped(object sender, RoutedEventArgs e)
        {
            switch (PagePivot.SelectedIndex)
            {
                case 0:
                    ReloadRecentData();
                    break;
                case 1:
                    ReloadCollections();
                    break;
                case 2:
                    ReloadPexelsData();
                    break;
                case 3:
                    ReloadSearchData();
                    break;
                case 4:
                    ReloadRandomData();
                    break;
                default:
                    break;
            }
        }

        private async void ReloadRecentData()
        {
            ShowRecentLoadingView();
            await _PageDataSource.ReloadRecentPhotos();
            HideRecentLoadingView();

            if (_PageDataSource.RecentPhotos.Count == 0)
            {
                ShowRecentEmptyView();
            }
        }

        private async void ReloadPexelsData()
        {
            ShowPexelsLoadingView();
            await _PageDataSource.ReloadPexelsPhotos();
            HidePexelsLoadingView();

            if (_PageDataSource.PexelsPhotos.Count == 0)
            {
                ShowPexelsEmptyView();
            }
        }
        
        
        private async void ReloadRandomData()
        {
            ShowRandomLoadingView();
            await _PageDataSource.ReloadRandomPhotos();
            HideRandomLoadingView();

            if (_PageDataSource.RandomPhotos.Count == 0)
            {
                ShowRandomEmptyView();
            }
        }

        private async void ReloadSearchData()
        {
            ShowSearchLoadingView();
            await _PageDataSource.ReloadSearchPhotos(temporaryQuery);
            HideSearchLoadingView();

            if (_PageDataSource.PhotosSearchResults.Count == 0)
            {
                ShowSearchEmptyView();
            }
        }

        private async void ReloadCollections()
        {
            ShowCollectionsLoadingView();
            await _PageDataSource.ReloadRecentCollections();
            HideCollectionsLoadingView();

            if (_PageDataSource.HomeCollections.Count == 0)
            {
                ShowCollectionsEmptyView();
            }
        }

        #endregion commandbar

        #region search

        private void Search_Clicked()
        {
            PagePivot.SelectedIndex = 3;

            FindName("SearchPivotItemContent");
            NavigateBackToGridItem();

            if (_AreSearchResultsActivated)
            {
                ShowSearchResults();
                return;
            }

            HideSearchResults();
        }

        private async void Search(string query)
        {
            if (string.IsNullOrEmpty(query) ||
                string.IsNullOrWhiteSpace(query) ||
                query.Length < 3)
            {
                var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                var message = loader.GetString("SearchQueryMinimum");

                Notify(message);
                return;
            }

            ShowLoadingSearch();

            var results = await _PageDataSource.SearchPhotos(query);

            HideLoadingSearch();

            if (_PageDataSource.PhotosSearchResults.Count > 0)
            {
                ShowSearchResults();
            }
            else
            {
                ShowEmptyView();
            }

            void ShowLoadingSearch()
            {
                SearchLoadingView.Visibility = Visibility.Visible;
            }

            void HideLoadingSearch()
            {
                SearchLoadingView.Visibility = Visibility.Collapsed;
            }

            void ShowEmptyView()
            {
                SearchEmptyView.Visibility = Visibility.Visible;
            }
        }

        async void ShowSearchResults()
        {
            _AreSearchResultsActivated = true;

            SearchPhotosView.ItemsSource = _PageDataSource.PhotosSearchResults;

            await SearchPhotosView.Fade(0, 0).Offset(0, 20, 0).StartAsync();

            SearchPhotosView.Visibility = Visibility.Visible;
            SearchPhotosView.Fade(1).Offset(0, 0).Start();
        }

        async void HideSearchResults()
        {
            await SearchPhotosView.Fade().Offset(0, 20).StartAsync();
            SearchPhotosView.Visibility = Visibility.Collapsed;
        }

        private void Search_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            NavigationViewControl.IsPaneOpen = true;
        }

        #endregion search

        #region notifications

        private void FlyoutNotification_Dismiss(object sender, RoutedEventArgs e)
        {
            HideNotification();
        }

        void Notify(string message)
        {
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
            var timer = new Timer(
                async (object state) =>
                {
                    await _UIDispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { HideNotification(); });
                }, autoEvent, TimeSpan.FromSeconds(5), new TimeSpan());
        }

        async void HideNotification()
        {
            await FlyoutNotification
                .Fade(0)
                .Offset(0, -30)
                .StartAsync();

            FlyoutNotification.Visibility = Visibility.Collapsed;
        }

        #endregion notifications

        #region rightTapped flyout

        void ShowProgress(string message = "")
        {
            ProgressDeterminate.Value = 0;
            FlyoutNotification.Visibility = Visibility.Visible;

            if (string.IsNullOrEmpty(message)) return;
            FlyoutText.Text = message;
        }

        void HideProgress()
        {
            FlyoutNotification.Visibility = Visibility.Collapsed;
        }

        private void HttpProgressCallback(Windows.Web.Http.HttpProgress progress)
        {
            if (progress.TotalBytesToReceive == null) return;

            ProgressDeterminate.Minimum = 0;
            ProgressDeterminate.Maximum = (double) progress.TotalBytesToReceive;
            ProgressDeterminate.Value = progress.BytesReceived;
        }

        private void PhotoItem_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            var panel = (StackPanel) sender;
            var photo = (Unsplasharp.Models.Photo) panel.DataContext;

            _LastPhotoSelected = photo;

            IsPhotoInFavorites(photo);

            PhotoRightTappedFlyout.ShowAt(panel);
        }

        private void IsPhotoInFavorites(Unsplasharp.Models.Photo photo)
        {
            if (App.DataSource.LocalFavorites == null)
            {
                RightCmdRemoveFavorites.Visibility = Visibility.Collapsed;
                // RightCmdAddToFavorites.Visibility = Visibility.Collapsed;
                return;
            }

            if (App.DataSource.LocalFavorites.Contains(photo.Id))
            {
                RightCmdRemoveFavorites.Visibility = Visibility.Visible;
                RightCmdAddToFavorites.Visibility = Visibility.Collapsed;
                return;
            }

            RightCmdRemoveFavorites.Visibility = Visibility.Collapsed;
            RightCmdAddToFavorites.Visibility = Visibility.Visible;
        }

        private void CmdCopyLink_Tapped(object sender, RoutedEventArgs e)
        {
            var successMessage = App.ResourceLoader.GetString("CopyLinkSuccess");

            DataTransfer.Copy(_LastPhotoSelected.Links.Html);
            Notify(successMessage);
        }

        private async void CmdSetAsWallpaper_Tapped(object sender, RoutedEventArgs e)
        {
            var progressMessage = App.ResourceLoader.GetString("SettingWallpaper");
            var successMessage = App.ResourceLoader.GetString("WallpaperSetSuccess");
            var failedMessage = App.ResourceLoader.GetString("WallpaperSetFailed");

            ShowProgress(progressMessage);
            var success = await Wallpaper.SetAsWallpaper(_LastPhotoSelected, HttpProgressCallback);
            HideProgress();

            if (success) Notify(successMessage);
            else Notify(failedMessage);
        }

        private async void CmdSetAsLockscreen_Tapped(object sender, RoutedEventArgs e)
        {
            var progressMessage = App.ResourceLoader.GetString("SettingLockscreen");
            var successMessage = App.ResourceLoader.GetString("LockscreenSetSuccess");
            var failedMessage = App.ResourceLoader.GetString("LockscreenSetFailed");

            ShowProgress(progressMessage);
            var success = await Wallpaper.SetAsLockscreen(_LastPhotoSelected, HttpProgressCallback);
            HideProgress();

            if (success) Notify(successMessage);
            else Notify(failedMessage);
        }

        private async void CmdOpenInBrowser_Tapped(object sender, RoutedEventArgs e)
        {
            if (_LastPhotoSelected == null || _LastPhotoSelected.Links == null) return;

            var tracking = "?utm_source=Hangon&utm_medium=referral&utm_campaign=" + Credentials.UnsplashApplicationId;
            var userUri = new Uri(string.Format("{0}{1}", _LastPhotoSelected.Links.Html, tracking));
            var success = await Windows.System.Launcher.LaunchUriAsync(userUri);
        }

        private void CmdDownloadResolution_Tapped(object sender, RoutedEventArgs e)
        {
            var cmd = (MenuFlyoutItem) sender;
            var resolution = (string) cmd.Tag;
            Download(resolution);
        }

        private async void Download(string size = "")
        {
            ShowProgress();
            var result = false;

            if (string.IsNullOrEmpty(size))
            {
                result = await Wallpaper.SaveToPicturesLibrary(_LastPhotoSelected, HttpProgressCallback);
            }
            else
            {
                string url = getURL();
                result = await Wallpaper.SaveToPicturesLibrary(_LastPhotoSelected, HttpProgressCallback, url);
            }

            HideProgress();

            var successMessage = App.ResourceLoader.GetString("SavePhotoSuccess");
            var failedMessage = App.ResourceLoader.GetString("SavePhotoFailed");

            if (result) Notify(successMessage);
            else Notify(failedMessage);

            string getURL()
            {
                switch (size)
                {
                    case "raw":
                        return _LastPhotoSelected.Urls.Raw;
                    case "full":
                        return _LastPhotoSelected.Urls.Full;
                    case "regular":
                        return _LastPhotoSelected.Urls.Regular;
                    case "small":
                        return _LastPhotoSelected.Urls.Small;
                    default:
                        return _LastPhotoSelected.Urls.Regular;
                }
            }
        }

        private void RightCmdAddToFavorites_Tapped(object sender, RoutedEventArgs e)
        {
            var localFavorites = App.DataSource.LocalFavorites;
            if (localFavorites == null) return;

            var cmd = (MenuFlyoutItem) sender;
            var photo = (Unsplasharp.Models.Photo) cmd.DataContext;

            if (photo == null || string.IsNullOrEmpty(photo.Id))
            {
                DataTransfer.ShowLocalToast(App.ResourceLoader.GetString("PhotoNotFound"));
                return;
            }

            if (localFavorites.Contains(photo.Id))
            {
                DataTransfer.ShowLocalToast(App.ResourceLoader.GetString("PhotoAlreadyInFavorites"));
                return;
            }

            var task = App.DataSource.AddToFavorites(photo);

            var message = App.ResourceLoader.GetString("PhotoSuccessfulAddedToFavorites");
            Notify(message);
        }

        private void RightCmdRemoveFavorites_Tapped(object sender, RoutedEventArgs e)
        {
            var localFavorites = App.DataSource.LocalFavorites;
            if (localFavorites == null) return;

            var cmd = (MenuFlyoutItem) sender;
            var photo = (Unsplasharp.Models.Photo) cmd.DataContext;

            if (photo == null || string.IsNullOrEmpty(photo.Id))
            {
                DataTransfer.ShowLocalToast(App.ResourceLoader.GetString("PhotoNotFound"));
                return;
            }

            var task = App.DataSource.RemoveFromFavorites(photo);

            var message = App.ResourceLoader.GetString("PhotoSuccessfulRemovedFromFavorites");
            Notify(message);
        }

        #endregion rightTapped flyout

        #region update changelog

        private async void ShowLastUpdateChangelog()
        {
            PagePivot.IsEnabled = false;
            await UpdateChangeLogFlyout.Scale(.9f, .9f, 0, 0, 0).Fade(0).StartAsync();
            UpdateChangeLogFlyout.Visibility = Visibility.Visible;

            var x = (float) UpdateChangeLogFlyout.ActualWidth / 2;
            var y = (float) UpdateChangeLogFlyout.ActualHeight / 2;

            await UpdateChangeLogFlyout.Scale(1f, 1f, x, y).Fade(1).StartAsync();
            PagePivot.Blur(10, 500, 500).Start();
        }

        private void ChangelogDismissButton_Tapped(object sender, RoutedEventArgs e)
        {
            HideUpdateChangelog();
        }

        private void CloseChangelogFlyout_Tapped(object sender, TappedRoutedEventArgs e)
        {
            HideUpdateChangelog();
        }

        private async void HideUpdateChangelog()
        {
            var x = (float) UpdateChangeLogFlyout.ActualWidth / 2;
            var y = (float) UpdateChangeLogFlyout.ActualHeight / 2;

            await UpdateChangeLogFlyout.Scale(.9f, .9f, x, y).Fade(0).StartAsync();
            UpdateChangeLogFlyout.Visibility = Visibility.Collapsed;
            PagePivot.Blur(0).Start();
            PagePivot.IsEnabled = true;
        }

        #endregion update changelog
    }
}
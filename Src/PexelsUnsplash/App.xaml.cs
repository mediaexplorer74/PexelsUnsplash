using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Resources;
using Windows.Foundation.Metadata;
using Windows.Globalization;
using Windows.Phone.UI.Input;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Stream.Wall.Models;
using Stream.Wall.Services;
using Stream.Wall.Views;

namespace Stream.Wall {
    sealed partial class App : Application
    {
        public static DataSource DataSource { get; set; }

        public static ResourceLoader ResourceLoader { get; set; }

        public static string DeviceType { get; set; }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            InitializeVariables();
        }

        private void InitializeVariables() {
            ResourceLoader = new ResourceLoader();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;
                rootFrame.Navigated += OnNavigated;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;

                SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;

                if (ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons")) {
                    HardwareButtons.BackPressed += OnBackPressed;
                }

                UpdateBackButtonVisibility();
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(HomePage), e.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }

            //UpdateTitleBarThemeButton();
            UpdateAppTheme();
            UpdateTitleBarTheme();
            HideSystemTray();
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }


        // handle hardware back button press
        void OnBackPressed(object sender, BackPressedEventArgs e) {
            var frame = (Frame)Window.Current.Content;
            if (frame.CanGoBack) {
                e.Handled = true;
                frame.GoBack();
            }
        }

        // handle software back button press
        void OnBackRequested(object sender, BackRequestedEventArgs e) {
            var frame = (Frame)Window.Current.Content;
            if (frame.CanGoBack) {
                e.Handled = true;
                frame.GoBack();
            }
        }

        void OnNavigated(object sender, NavigationEventArgs e) {
            UpdateBackButtonVisibility();
        }

        private void UpdateBackButtonVisibility() {
            var frame = (Frame)Window.Current.Content;

            var visibility = AppViewBackButtonVisibility.Collapsed;

            if (frame.CanGoBack) {
                visibility = AppViewBackButtonVisibility.Visible;
            }

            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = visibility;
        }

        public static void UpdateAppTheme() {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            
            var theme = localSettings.Values.ContainsKey("Theme") ?
                (string)localSettings.Values["Theme"] : "Dark";

            var frame = (Frame)Window.Current.Content;
            if (theme == ApplicationTheme.Light.ToString()) {
                frame.RequestedTheme = ElementTheme.Light;
                return;
            }

            frame.RequestedTheme = ElementTheme.Dark;
        }

        public static void UpdateTitleBarTheme() {
            var titleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

            if (Settings.IsApplicationThemeLight()) {
                titleBar.ButtonInactiveForegroundColor = Colors.Black;
                titleBar.ButtonForegroundColor = Colors.Black;
                return;
            }

            titleBar.ButtonInactiveForegroundColor = Colors.White;
            titleBar.ButtonForegroundColor = Colors.White;
        }

        public static void SetTitleBarTheme(ElementTheme theme) {
            var titleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;

            if (theme == ElementTheme.Light) {
                titleBar.ButtonInactiveForegroundColor = Colors.White;
                titleBar.ButtonForegroundColor = Colors.White;
                return;
            }

            titleBar.ButtonInactiveForegroundColor = Colors.Black;
            titleBar.ButtonForegroundColor = Colors.Black;
        }

        async void HideSystemTray() {
            var statusbar = "Windows.UI.ViewManagement.StatusBar";
            if (ApiInformation.IsTypePresent(statusbar)) {
                await Windows.UI.ViewManagement.StatusBar.GetForCurrentView().HideAsync();
            }
        }

        public static void UpdateLanguage() {
            var lang = Settings.GetAppCurrentLanguage();
            ApplicationLanguages.PrimaryLanguageOverride = lang;
        }
    }
}

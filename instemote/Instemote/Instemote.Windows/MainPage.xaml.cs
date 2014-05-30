using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Instemote
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        bool isUiSetup = false;

        public MainPage()
        {
            this.InitializeComponent();

           
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Ensure the UI is ready
            SetupUI();

            // Make sure we are up to date
            RunStartUp();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {

        }


        #region Page Init

        public void SetupUI()
        {
            if (!isUiSetup)
            {
                App.DataMan.HouseMan.HouseUpdatedHandler += HouseMan_HouseUpdatedHandler;
                App.DataMan.HouseMan.HouseUpdatingHandler += HouseMan_HouseUpdatingHandler;
                App.DataMan.CommandMan.CommandResultHandler += CommandMan_CommandResultHandler;

                SceneList.ItemsSource = App.DataMan.SettingsMan.SceneList;

                isUiSetup = true;
            }
        }

        public void RunStartUp()
        {
            ThreadPool.RunAsync(new WorkItemHandler((IAsyncAction) =>
            {
                // Check for a local connection
                App.DataMan.CommandMan.TestLocal();

                App.DataMan.HouseMan.CheckForSync(false);
            }));
        }

        #endregion

        #region Handlers

        void CommandMan_CommandResultHandler(Backend.Lib.InsteonCommand command, bool hasMore)
        {

        }

        void HouseMan_HouseUpdatingHandler()
        {
            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                // Update the status
                ShowStatusBar("Syncing...");
            }); 
        }

        void HouseMan_HouseUpdatedHandler(Exception e)
        {
            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                // Hide the status bar
                HideStatusBar(null);

                SceneList.ItemsSource = App.DataMan.SettingsMan.SceneList;
            }); 
        }

        #endregion

        #region ProgressBar

        public void ShowStatusBar(string text)
        {
            StatusBar.IsIndeterminate = true;
            StatusBar.Visibility = Visibility.Visible;
        }

        public void HideStatusBar(string text)
        {
            StatusBar.IsIndeterminate = false;
            StatusBar.Visibility = Visibility.Collapsed;
        }

        #endregion

        private void OnButton_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }

        private void OffButton_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }
    }
}

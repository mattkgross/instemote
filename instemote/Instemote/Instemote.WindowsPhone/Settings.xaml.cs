using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.System.Threading;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Instemote
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Settings : Page
    {
        bool PageSetup = false;

        public Settings()
        {
            this.InitializeComponent();

            // Set up scenes
            List<Scene> scenes = App.DataMan.SettingsMan.SceneList;
            List<string> names = new List<string>();
            string CurrentScene = App.DataMan.SettingsMan.CortanaSceneCommand;
            string CurrentGeoFenceScene = App.DataMan.SettingsMan.GeoFenceSceneCommand;
            int SelectIndex = 0;
            int GeoFenseIndex = 0;
            int count = 0;
            foreach(Scene scene in scenes)
            {
                names.Add(scene.Name);

                if(scene.Name.Equals(CurrentScene))
                {
                    SelectIndex = count;
                }

                if (scene.Name.Equals(CurrentGeoFenceScene))
                {
                    GeoFenseIndex = count;
                }
                count++;
            }
            CortanaScene.ItemsSource = names;
            CortanaScene.SelectedIndex = SelectIndex;

            Windows.Phone.UI.Input.HardwareButtons.BackPressed += HardwareButtons_BackPressed;
            Unloaded += Settings_Unloaded;

            // Set the geo fence
            if (App.DataMan.SettingsMan.HomeGeoFenceLat == 0 && App.DataMan.SettingsMan.HomeGeoFenceLong == 0)
            {
                HomeText.Text = "Current Home: Not Set";
            }
            else
            {
                HomeText.Text = "Current Home: " + Math.Round(App.DataMan.SettingsMan.HomeGeoFenceLat, 2) + ", " + Math.Round(App.DataMan.SettingsMan.HomeGeoFenceLong, 2);
            }

            // Set the Geo Fence scene
            GeoFenseScene.ItemsSource = names;
            GeoFenseScene.SelectedIndex = GeoFenseIndex;

            // Set the checkboxes
            LightsOff.IsChecked = App.DataMan.SettingsMan.GeoFenseOffLeave;
            LightsOffAsk.IsEnabled = App.DataMan.SettingsMan.GeoFenseOffLeave;
            LightsOffAsk.IsChecked = App.DataMan.SettingsMan.GeoFenseOffAsk;
            LightsOn.IsChecked = App.DataMan.SettingsMan.GeoFenseOnArrive;
            LightsOnAsk.IsEnabled = App.DataMan.SettingsMan.GeoFenseOnArrive;
            LightsOnAsk.IsChecked = App.DataMan.SettingsMan.GeoFenseOnAsk;

            // set setup
            PageSetup = true;
        }

        void Settings_Unloaded(object sender, RoutedEventArgs e)
        {
            Windows.Phone.UI.Input.HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
        }

        private void HardwareButtons_BackPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
            if (this.Frame.CanGoBack)
            {
                e.Handled = true;
                this.Frame.GoBack();
            }
        }

        private async void SetHome_Click(object sender, RoutedEventArgs e)
        {
            Geolocator geo = new Geolocator();
            HomeText.Text = "Current Home: Working...";
            Geoposition pos = await geo.GetGeopositionAsync();

            App.DataMan.SettingsMan.HomeGeoFenceLat = pos.Coordinate.Point.Position.Latitude;
            App.DataMan.SettingsMan.HomeGeoFenceLong = pos.Coordinate.Point.Position.Longitude;

            HomeText.Text = "Current Home: " + Math.Round(pos.Coordinate.Point.Position.Latitude, 2) + ", " + Math.Round(pos.Coordinate.Point.Position.Longitude, 2);
        }

        //protected override void OnNavigatedTo(NavigationEventArgs e)
        //{
          
        //}

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // We don't need to update, this will happen when we nav to the main page
            //ThreadPool.RunAsync(new WorkItemHandler((IAsyncAction) =>
            //{
            //    App.DataMan.GeoFenceMan.InitGeoFence();
            //}));
        }

        private void CortanaScene_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(PageSetup)
            {
                App.DataMan.SettingsMan.CortanaSceneCommand = (string)CortanaScene.SelectedItem;
            }
        }

        private void LightsOff_Checked(object sender, RoutedEventArgs e)
        {
            if(PageSetup)
            {
                App.DataMan.SettingsMan.GeoFenseOffLeave = (bool)LightsOff.IsChecked;
                LightsOffAsk.IsEnabled = App.DataMan.SettingsMan.GeoFenseOffLeave;
            }
        }

        private void LightsOffAsk_Checked(object sender, RoutedEventArgs e)
        {
            if (PageSetup)
            {
                App.DataMan.SettingsMan.GeoFenseOffAsk = (bool)LightsOffAsk.IsChecked;
            }
        }

        private void LightsOn_Checked(object sender, RoutedEventArgs e)
        {
            if (PageSetup)
            {
                App.DataMan.SettingsMan.GeoFenseOnArrive = (bool)LightsOn.IsChecked;
                LightsOnAsk.IsEnabled = App.DataMan.SettingsMan.GeoFenseOnArrive;
            }
        }

        private void LightsOnAsk_Checked(object sender, RoutedEventArgs e)
        {
            if (PageSetup)
            {
                App.DataMan.SettingsMan.GeoFenseOnAsk = (bool)LightsOnAsk.IsChecked;
            }
        }

        private void GeoFenseScene_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PageSetup)
            {
                App.DataMan.SettingsMan.GeoFenceSceneCommand = (string)GeoFenseScene.SelectedItem;
            }
        }     
    }
}

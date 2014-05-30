using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.System.Threading;
using Windows.UI.Popups;
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
    public sealed partial class LoginPage : Page
    {

        public LoginPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
 
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {

        }

        private async void Button_Tapped(object sender, TappedRoutedEventArgs e)
        {
            string email = Email.Text;
            string password = Password.Password;

            Email.IsEnabled = false;
            Password.IsEnabled = false;
            LoginButton.IsEnabled = false;

            StatusBar statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
            statusBar.ProgressIndicator.Text = "Logging in...";
            statusBar.ProgressIndicator.ShowAsync();

            bool success = false;
            try
            {
                App.DataMan.SettingsMan.UserPassword = password;
                App.DataMan.SettingsMan.UserEmail = email;
                // If we sign in, the login will get the house info
                success = await Task.Run(async () => App.DataMan.RefreshCredentials(false));
            }
            catch(Exception ex)
            {
                success = false;
                App.DataMan.SettingsMan.UserPassword = "";
                App.DataMan.SettingsMan.UserEmail = "";
                Email.IsEnabled = true;
                Password.IsEnabled = true;
                LoginButton.IsEnabled = true;

                if(ex.Message != null && ex.Message.ToLower().Contains("unauthorized"))
                {
                    new MessageDialog("Your credentials don't seem right, why not try again?", "Oops").ShowAsync();
                }
                else
                {
                    App.ShowDialog("We can't connect right now, please try again in a while.", "Connection Error", ex);
                }
            }

            statusBar.ProgressIndicator.HideAsync();

            if(success)
            {
                this.Frame.Navigate(typeof(MainPage));
            }
        }

        private void Email_TextChanged(object sender, TextChangedEventArgs e)
        {
            EnableButtonCheck();
        }

        private void Password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            EnableButtonCheck();
        }

        private void EnableButtonCheck()
        {
            if(!String.IsNullOrWhiteSpace(Password.Password) &&
                !String.IsNullOrWhiteSpace(Email.Text))
            {
                LoginButton.IsEnabled = true;
            }
        }

        private void Password_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if(e.Key == Windows.System.VirtualKey.Enter)
            {
                Button_Tapped(null, null);
            }
        }
    }
}

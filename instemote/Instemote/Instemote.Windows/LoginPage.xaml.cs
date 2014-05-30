using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Threading;
using Windows.UI.Popups;
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
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            this.InitializeComponent();
        }
        
        private async void LoginButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            string email = EmailBox.Text;
            string password = PasswordBox.Password;

            // Disable
            EmailBox.IsEnabled = false;
            PasswordBox.IsEnabled = false;
            LoginButton.IsEnabled = false;
            ProgressBar.IsIndeterminate = true;
            ProgressBar.Visibility = Visibility.Visible;

            // Try to login
            bool success = false;
            Exception exception = null;
            App.DataMan.SettingsMan.UserPassword = password;
            App.DataMan.SettingsMan.UserEmail = email;
            // If we sign in, the login will get the house info
            await ThreadPool.RunAsync(new WorkItemHandler((IAsyncAction) =>
            {
                try
                {
                    success = App.DataMan.RefreshCredentials(false);
                }
                catch(Exception ex)
                {
                    success = false;
                    exception = ex;
                }
            }));
            
            if (exception != null)
            {
                // Rest the state, show the error
                success = false;
                App.DataMan.SettingsMan.UserPassword = "";
                App.DataMan.SettingsMan.UserEmail = "";
                PasswordBox.IsEnabled = true;
                EmailBox.IsEnabled = true;
                LoginButton.IsEnabled = true;

                if (exception.Message != null && exception.Message.ToLower().Contains("unauthorized"))
                {
                    new MessageDialog("Your credentials don't seem right, why not try again?", "Oops").ShowAsync();
                }
                else
                {
                    App.ShowDialog("We can't connect right now, please try again in a while.", "Connection Error", exception);
                }
            }

            // Turn off the bar
            ProgressBar.IsIndeterminate = false;
            ProgressBar.Visibility = Visibility.Collapsed; 

            if (success)
            {
                // Navigate if we are good
                this.Frame.Navigate(typeof(MainPage));
            }
        }
        
        private void ValidateFields()
        {
            LoginButton.IsEnabled = !String.IsNullOrWhiteSpace(EmailBox.Text) && !String.IsNullOrWhiteSpace(PasswordBox.Password);
        }

        private void EmailBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateFields();        
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ValidateFields();                
        }

        private void PasswordBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if(e.Key == Windows.System.VirtualKey.Enter)
            {
                LoginButton_Tapped(null, null);
            }
        }        
    }
}

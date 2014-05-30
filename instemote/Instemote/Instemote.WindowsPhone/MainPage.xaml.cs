using Backend.Lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Devices.Geolocation;
using Windows.Devices.Geolocation.Geofencing;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.SpeechRecognition;
using Windows.Storage;
using Windows.System.Threading;
using Windows.UI;
using Windows.UI.Notifications;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Instemote
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        bool isUiSetup = false;
        SolidColorBrush AccentBrush = null;

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            UpdateCortanaText();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                CheckVoiceActivation(e);

                CheckForGeoFence(e);
            }

            // Make sure the UI is ready
            SetupUI();

            // Make sure we are up to date
            RunStartUp();
        }

        #region UI

        public void SetupUI()
        {
            if (!isUiSetup)
            {
                App.DataMan.HouseMan.HouseUpdatedHandler += HouseMan_HouseUpdatedHandler;
                App.DataMan.HouseMan.HouseUpdatingHandler += HouseMan_HouseUpdatingHandler;
                App.DataMan.CommandMan.CommandResultHandler += CommandMan_CommandResultHandler;

                SceneListBox.ItemsSource = App.DataMan.SettingsMan.SceneList;

                isUiSetup = true;
            }
        }

        void CommandMan_CommandResultHandler(InsteonCommand command, bool hasMore)
        {
            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (command.ResponseException == null)
                {                
                    HideStatusBar("Done!");                
                }
                else
                {
                    App.ShowDialog("Unable to send the command, check your network connection and try again later.", "Connection Error", command.ResponseException);
                    HideStatusBar("Error");  
                }
            });
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

                SceneListBox.ItemsSource = App.DataMan.SettingsMan.SceneList;
            }); 
        }

        public void ShowStatusBar(string text)
        {
            StatusBar statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
            statusBar.ProgressIndicator.Text = text;
            statusBar.ProgressIndicator.ProgressValue = null;
            statusBar.ProgressIndicator.ShowAsync();
        }

        public void HideStatusBar(string text)
        {
            if(!String.IsNullOrWhiteSpace(text))
            {
                StatusBar statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
                statusBar.ProgressIndicator.ProgressValue = 0.0;
                statusBar.ProgressIndicator.Text = text;
                statusBar.ProgressIndicator.ShowAsync();

                ThreadPoolTimer.CreateTimer((ThreadPoolTimer obj) =>
                    {
                        Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                            {
                                statusBar.ProgressIndicator.HideAsync();
                            });
                    }, TimeSpan.FromSeconds(3));

            }
            else
            {
                StatusBar statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
                statusBar.ProgressIndicator.HideAsync();
            }
        }


        #endregion

        #region Handlers

        private void LogOut_Click(object sender, RoutedEventArgs e)
        {
            App.DataMan.SettingsMan.Reset();

            Frame.Navigate(typeof(LoginPage));
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Settings));
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            App.DataMan.HouseMan.CheckForSync(true);
        }

        private void Button_Off(object sender, TappedRoutedEventArgs e)
        {
            Scene scene = (sender as Button).DataContext as Scene;
            InsteonCommand command = new InsteonCommand(InsteonCommand.CommandType.Scene_Off, scene);
            App.DataMan.CommandMan.SendCommand(command);
            ShowStatusBar("Sending...");
        }

        private void Button_On(object sender, TappedRoutedEventArgs e)
        {
            Scene scene = (sender as Button).DataContext as Scene;
            InsteonCommand command = new InsteonCommand(InsteonCommand.CommandType.Scene_On, scene);
            App.DataMan.CommandMan.SendCommand(command);
            ShowStatusBar("Sending...");
        }

        #endregion

        #region StartUp

        public void RunStartUp()
        {           
           ThreadPool.RunAsync(new WorkItemHandler((IAsyncAction) =>
           {
               // Check for a local connection
               App.DataMan.CommandMan.TestLocal();

               App.DataMan.HouseMan.CheckForSync(false);

               App.DataMan.GeoFenceMan.InitGeoFence();
           }));
        }

        #endregion

        #region Cortana

        public async void UpdateCortanaText()
        {
            // Add the voice commands           
            try
            {
                StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///VoiceCommands.xml"));
                await Windows.Media.SpeechRecognition.VoiceCommandManager.InstallCommandSetsFromStorageFileAsync(file);
            }
            catch (Exception e) { App.ShowDialog("We can't update your voice data right now.", "Voice Error", e); }  
        }

        public void CheckVoiceActivation(NavigationEventArgs e)
        {
            if (e != null && e.Parameter != null && e.Parameter.GetType() == typeof(SpeechRecognitionResult))
            {
                try
                {
                    // Get the argument
                    SpeechRecognitionResult speechRecognitionResult = (SpeechRecognitionResult)e.Parameter;
                    string voiceCommandName = speechRecognitionResult.RulePath[0];
                    string textSpoken = speechRecognitionResult.Text.ToLower();
                    
                    switch (voiceCommandName)
                    {
                        case "TurnGeneric":
                        {
                            // Get the default scene
                            Scene CortanaScene = App.DataMan.HouseMan.GetSceneFromName(App.DataMan.SettingsMan.CortanaSceneCommand);

                            if (CortanaScene != null)
                            {
                                if (textSpoken.Contains("off"))
                                {
                                    ShowCommandOverlay(false, CortanaScene.Name);
                                    InsteonCommand command = new InsteonCommand(InsteonCommand.CommandType.Scene_Off, CortanaScene);
                                    App.DataMan.CommandMan.SendCommand(command);
                                    ShowStatusBar("Turning off " + CortanaScene.Name.ToLower() + "...");
                                }
                                else if (textSpoken.Contains("on"))
                                {
                                    ShowCommandOverlay(true, CortanaScene.Name);
                                    InsteonCommand command = new InsteonCommand(InsteonCommand.CommandType.Scene_On, CortanaScene);
                                    App.DataMan.CommandMan.SendCommand(command);
                                    ShowStatusBar("Turning on " + CortanaScene.Name.ToLower() + "...");
                                }
                            }
                            else
                            {
                                HideStatusBar("Scene not set, go to settings");
                            }
                            break;
                        }
                        case "TurnScene":
                        {
                            string sceneName = "";
                            bool TurnOff = false;
                            if(textSpoken.Trim().StartsWith("on "))
                            {
                                int space = textSpoken.IndexOf(" ");
                                sceneName = textSpoken.Substring(space + 1);
                            }
                            else 
                            {
                                TurnOff = true;
                                int space = textSpoken.IndexOf(" ");
                                sceneName = textSpoken.Substring(space + 1);
                            }

                            bool Found = false;
                            bool GiveUp = false;
                            int itteration = 0;
                            Scene askedFor = null;
                            string baseGuess = "";
                            while(!Found && !GiveUp)
                            {
                                askedFor = App.DataMan.HouseMan.GetSceneFromName(sceneName);

                                if(askedFor != null && askedFor.Name.Trim().ToLower().Equals(sceneName.ToLower().Trim()))
                                {
                                    Found = true;
                                    break;
                                }

                                // Not found, try something else
                                switch(itteration)
                                {
                                    case 0:
                                    {
                                        // remove the?
                                        if (sceneName.Contains("the "))
                                        {
                                            int theEnd = sceneName.IndexOf("the ", 0) + 4;
                                            sceneName = sceneName.Substring(theEnd);
                                        }
                                        break;
                                    }
                                    case 1:
                                    {
                                        // remove lights or light
                                        if (sceneName.Contains(" light"))
                                        {
                                            int lightStart = sceneName.IndexOf(" light", 0);
                                            sceneName = sceneName.Substring(0, lightStart);
                                        }
                                        break;
                                    }
                                    case 2:
                                    {
                                        // Set the base guess
                                        baseGuess = sceneName;

                                        // add room
                                        sceneName = baseGuess+" room";
                                        break;
                                    }
                                    case 3:
                                    {
                                        // add light
                                        sceneName = baseGuess+" light";
                                        break;
                                    }
                                    case 4:
                                    {
                                        // add light
                                        sceneName = baseGuess+" lights";
                                        break;
                                    }
                                    default:
                                        GiveUp = true;
                                        break;
                                }
                                itteration++;
                            }

                            if (!Found || askedFor == null)
                            {
                                HideStatusBar("Scene not found");
                            }
                            else
                            {
                                if (TurnOff)
                                {
                                    ShowCommandOverlay(false, askedFor.Name);
                                    InsteonCommand command = new InsteonCommand(InsteonCommand.CommandType.Scene_Off, askedFor);
                                    App.DataMan.CommandMan.SendCommand(command);
                                    ShowStatusBar("Turning off " + askedFor.Name.ToLower() + "...");
                                }
                                else if (textSpoken.Contains("on"))
                                {
                                    ShowCommandOverlay(true, askedFor.Name);
                                    InsteonCommand command = new InsteonCommand(InsteonCommand.CommandType.Scene_On, askedFor);
                                    App.DataMan.CommandMan.SendCommand(command);
                                    ShowStatusBar("Turning on " + askedFor.Name.ToLower() + "...");
                                }
                            }                          
                            
                            break;
                        }
                            
                        default:
                            // There is no match for the voice command name.
                            break;
                    }                    
                }
                catch (Exception ex)
                {
                   // App.ShowDialog("There was an ", "error", ex);
                }
            }
        }

        #endregion

        #region GeoFence

        public void CheckForGeoFence(NavigationEventArgs e)
        {
            // Check for Geo Fence Toast
            if (e != null && e.Parameter != null && typeof(string) == e.Parameter.GetType())
            {
                string parm = (string)e.Parameter;
                InsteonCommand.CommandType commandType;

                if(parm.Equals(GeoFenceManager.HomeToastLaunchOffAsk))
                {
                    commandType = InsteonCommand.CommandType.Scene_Off;
                }
                else if (parm.Equals(GeoFenceManager.HomeToastLaunchOnAsk))
                {
                    commandType = InsteonCommand.CommandType.Scene_On;
                }
                else
                {
                    return;
                }

                Scene scene = App.DataMan.HouseMan.GetSceneFromName(App.DataMan.SettingsMan.GeoFenceSceneCommand);

                if(scene != null)
                {
                    InsteonCommand command = new InsteonCommand(commandType, scene);          
                    App.DataMan.CommandMan.SendCommand(command);
                    ShowCommandOverlay(commandType == InsteonCommand.CommandType.Scene_On, scene.Name);
                }
                else
                {
                    HideStatusBar("Scene not found");
                }               
            }
        }


        #endregion

        #region CommandOverLay

        private void ShowCommandOverlay(bool turningOn, string name)
        {
            SolidColorBrush dark = new SolidColorBrush(Color.FromArgb(152, 152, 152, 152));            
            if(AccentBrush == null)
            {
                AccentBrush = (SolidColorBrush)Bulb1.Fill;
            }

            if(turningOn)
            {
                Bulb1.Fill = AccentBrush;
                Bulb2.Fill = AccentBrush;
                Bulb3.Fill = AccentBrush;
                Bulb4.Fill = AccentBrush;
                Bulb5.Fill = AccentBrush;
                Bulb6.Fill = AccentBrush;
                Bulb7.Fill = AccentBrush;

                CommandText.Text = (name == null && name.Length > 1 ? "Lights" : char.ToUpper(name[0]) + name.Substring(1)) + " On";
            }
            else
            {
                Bulb1.Fill = dark;
                Bulb2.Fill = dark;
                Bulb3.Fill = dark;
                Bulb4.Fill = dark;
                Bulb5.Fill = dark;
                Bulb6.Fill = dark;
                Bulb7.Fill = dark;
                CommandText.Text = (name == null && name.Length > 1 ? "Lights" : char.ToUpper(name[0]) + name.Substring(1)) + " Off";
            }

            this.BottomAppBar.Visibility = Visibility.Collapsed;
            CommandOverLay.Opacity = 0;
            CommandOverLay.Visibility = Visibility.Visible;
            FadeInCommandOverlay.Begin();
        }

        private void FadeOutCommandOverLay_Completed(object sender, object e)
        {
            this.BottomAppBar.Visibility = Visibility.Visible;
            CommandOverLay.Visibility = Visibility.Collapsed;
        }
        private void FadeInCommandOverlay_Completed(object sender, object e)
        {
            FadeOutCommandOverLay.Begin();
        }

        #endregion
    }
}

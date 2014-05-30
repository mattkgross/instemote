using Backend.Lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
#if WINDOWS_PHONE_APP
using Windows.Media.SpeechRecognition;
#endif
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227

namespace Instemote
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : Application
    {


        static object lockObj = new Object();
        static DataManager _DataMan = null;
        public static DataManager DataMan
        {
            get
            {
                if (_DataMan == null)
                {
                    lock (lockObj)
                    {
                        _DataMan = new DataManager();
                    }
                }
                return _DataMan;
            }
        }

#if WINDOWS_PHONE_APP
        private TransitionCollection transitions;
        SpeechRecognitionResult HeldResults = null;
#endif


        public App()
        {
            this.InitializeComponent();
            this.Suspending += this.OnSuspending;
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                // TODO: change this value to a cache size that is appropriate for your application
                rootFrame.CacheSize = 1;

                if (e != null && e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
#if WINDOWS_PHONE_APP
                // Removes the turnstile navigation for startup.
                if (rootFrame.ContentTransitions != null)
                {
                    this.transitions = new TransitionCollection();
                    foreach (var c in rootFrame.ContentTransitions)
                    {
                        this.transitions.Add(c);
                    }
                }

                rootFrame.ContentTransitions = null;
                rootFrame.Navigated += this.RootFrame_FirstNavigated;
#endif

                if (!DataMan.SettingsMan.IsUserLoggedIn)
                {
                    if (!rootFrame.Navigate(typeof(LoginPage), ""))
                    {
                        throw new Exception("Failed to create initial page");
                    }
                }
                else
                {
#if WINDOWS_PHONE_APP
                    if (HeldResults != null)
                    {
                        // Send the voice start if that's what we want.
                        rootFrame.Navigate(typeof(MainPage), HeldResults);
                        HeldResults = null;
                    }
                    else
#endif
                    {
                        rootFrame.Navigate(typeof(MainPage), e != null ? e.Arguments : "");
                    }
                }
            }
            else
            {
                // We are already open, make sure if we have args to handle them.
                if (e != null && e.Kind == ActivationKind.Launch)
                {
                    // Launch home
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
            }

            // Ensure the current window is active
            Window.Current.Activate();
        }

#if WINDOWS_PHONE_APP
        private void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
        {
            var rootFrame = sender as Frame;
            rootFrame.ContentTransitions = this.transitions ?? new TransitionCollection() { new NavigationThemeTransition() };
            rootFrame.Navigated -= this.RootFrame_FirstNavigated;
        }

        protected override void OnActivated(IActivatedEventArgs args)
        {
            base.OnActivated(args);

            Frame rootFrame = Window.Current.Content as Frame;

            // Was the app activated by a voice command?
            if (args.Kind == Windows.ApplicationModel.Activation.ActivationKind.VoiceCommand)
            {
                var commandArgs = args as Windows.ApplicationModel.Activation.VoiceCommandActivatedEventArgs;
                Windows.Media.SpeechRecognition.SpeechRecognitionResult speechRecognitionResult = commandArgs.Result;
                if (rootFrame == null)
                {
                    // This will happen if the app is not running, so hold them and send them later
                    HeldResults = speechRecognitionResult;
                    // Apparently if OnActivated is called OnLaunced isn't?!!? wtf.
                    OnLaunched(null);
                }
                else
                {
                    rootFrame.Navigate(typeof(MainPage), speechRecognitionResult);
                }
            }            
        }
#endif

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            // TODO: Save application state and stop any background activity
            deferral.Complete();
        }


        public static void ShowDialog(string msg, string title, Exception e)
        {
            string body = msg + Environment.NewLine + Environment.NewLine + (e == null ? "" : e.Message);
            new MessageDialog(body, title).ShowAsync();
        }
    }
}
using Backend.Lib;
using Instemote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.ApplicationModel.Background;
using Windows.Devices.Geolocation.Geofencing;
using Windows.UI.Notifications;

namespace GeoFenseBackgroundTask
{
    public sealed class GeoFenseBackgroundTask : IBackgroundTask
    {
        BackgroundTaskDeferral Deferral;
        DataManager DataMan;
        int WaitingCount = 0;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            DataMan = new DataManager();
            Deferral = taskInstance.GetDeferral();

            var reports = GeofenceMonitor.Current.ReadReports();

            foreach(GeofenceStateChangeReport report in reports)
            {
                if(report.Geofence.Id.Equals(GeoFenceManager.HomeGeoFence))
                {
                    bool isEntered = report.NewState == GeofenceState.Entered;
                    bool DoAction = (DataMan.SettingsMan.GeoFenseOffLeave && !isEntered) ||
                                    (DataMan.SettingsMan.GeoFenseOnArrive && isEntered);
                    bool AskUser = (DataMan.SettingsMan.GeoFenseOffAsk && !isEntered) ||
                                   (DataMan.SettingsMan.GeoFenseOnAsk && isEntered);
                    
                    if(DoAction)
                    {
                        if (!AskUser)
                        {
                            Scene scene = DataMan.HouseMan.GetSceneFromName(DataMan.SettingsMan.GeoFenceSceneCommand);

                            if (scene != null)
                            {
                                // Set up a listener for the command, only for the first
                                lock (Deferral)
                                {
                                    if (WaitingCount == 0)
                                    {
                                        DataMan.CommandMan.CommandResultHandler += CommandMan_CommandResultHandler;
                                    }

                                    WaitingCount++;
                                }

                                // Send the command
                                InsteonCommand command = new InsteonCommand(isEntered ? InsteonCommand.CommandType.Scene_On : InsteonCommand.CommandType.Scene_Off, scene);
                                DataMan.CommandMan.SendCommand(command);
                            }                           
                        }

                        // Now inform the user
                        string message = "";
                        string LaunchArg = "";
                        if(AskUser)
                        {
                            message = (isEntered ? "Home? Turn on " : "Turn off ") + "the lights?";
                            LaunchArg = isEntered ? GeoFenceManager.HomeToastLaunchOnAsk : GeoFenceManager.HomeToastLaunchOffAsk;
                        }
                        else
                        {
                            message = "Lights " + (isEntered ? "turned on" : "turned off");
                        }

                        string toast = "<toast launch=\"" + LaunchArg + "\"><visual><binding template=\"ToastText02\"><text id=\"1\">Instemote</text><text id=\"2\">" + message + "</text></binding></visual></toast>";
                        var xmlDoc = new Windows.Data.Xml.Dom.XmlDocument();
                        xmlDoc.LoadXml(toast);
                        var ToastNote = new ToastNotification(xmlDoc);
                        ToastNotificationManager.CreateToastNotifier().Show(ToastNote); 
                    }
                }
            }

            lock(Deferral)
            {
                // Complete if we aren't waiting
                if (WaitingCount == 0)
                {
                    Deferral.Complete();
                }
            }
        }

        void CommandMan_CommandResultHandler(InsteonCommand command, bool hasMore)
        {
            lock (Deferral)
            {
                // Mark complete
                WaitingCount--;

                // Complete if we aren't waiting
                if (WaitingCount == 0)
                {
                    DataMan.CommandMan.CommandResultHandler -= CommandMan_CommandResultHandler;
                    Deferral.Complete();
                }
            }
        }
    }
}

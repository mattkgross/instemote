using Instemote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Devices.Geolocation;
using Windows.Devices.Geolocation.Geofencing;

namespace Backend.Lib
{
    public class GeoFenceManager
    {
        public static double RadiusInMeters = 80;
        public static int DwellSeconds = 5;
        public static string HomeGeoFence = "Home";
        public static string HomeToastLaunchOnAsk = "HomeToastLaunchOnAsk";
        public static string HomeToastLaunchOffAsk = "HomeToastLaunchOffAsk";
        private static string GeoFenseName = "InstemoteGeoFenceBT";

        public DataManager DataMan;

        public GeoFenceManager(DataManager da)
        {
            DataMan = da;
        }

        public async void InitGeoFence()
        {
            if (DataMan.SettingsMan.HomeGeoFenceLat == 0 || DataMan.SettingsMan.HomeGeoFenceLong == 0)
            {
                return;
            }

            try
            {
                bool isHomeSet = false;
                foreach(Geofence geofence in GeofenceMonitor.Current.Geofences)
                {
                    if (((Geocircle)geofence.Geoshape).Center.Latitude == DataMan.SettingsMan.HomeGeoFenceLat &&
                       ((Geocircle)geofence.Geoshape).Center.Longitude == DataMan.SettingsMan.HomeGeoFenceLong &&
                       ((Geocircle)geofence.Geoshape).Radius == RadiusInMeters &&
                        geofence.DwellTime.TotalSeconds == DwellSeconds)
                    {
                        isHomeSet = true;
                        break;
                    }
                }

                if(!isHomeSet)
                {
                    // Set the position
                    BasicGeoposition position = new BasicGeoposition();
                    position.Latitude = DataMan.SettingsMan.HomeGeoFenceLat;
                    position.Longitude = DataMan.SettingsMan.HomeGeoFenceLong;

                    Geocircle geocircle = new Geocircle(position, RadiusInMeters, AltitudeReferenceSystem.Surface);
                    MonitoredGeofenceStates mask = 0;
                    mask |= MonitoredGeofenceStates.Entered;
                    mask |= MonitoredGeofenceStates.Exited;

                    Geofence homeGeoFence = new Geofence(HomeGeoFence, geocircle, mask, false, TimeSpan.FromSeconds(DwellSeconds));

                    // Clear any old, set the new
                    GeofenceMonitor.Current.Geofences.Clear();
                    GeofenceMonitor.Current.Geofences.Add(homeGeoFence);
                }

                // Check for a background agent
                List<KeyValuePair<Guid, IBackgroundTaskRegistration>> tasks = Windows.ApplicationModel.Background.BackgroundTaskRegistration.AllTasks.ToList();

                bool HasTask = false;
                foreach(KeyValuePair<Guid, IBackgroundTaskRegistration> task in tasks)
                {
                    if(task.Value.Name.Equals(GeoFenseName))
                    {
                        HasTask = true;
                        break;
                    }
                }

                if(!HasTask)
                {
                    BackgroundAccessStatus backgroundAccessStatus = await BackgroundExecutionManager.RequestAccessAsync();
                    BackgroundTaskBuilder geofenceTaskBuilder = new BackgroundTaskBuilder();
                    LocationTrigger trigger = new LocationTrigger(LocationTriggerType.Geofence);
                    geofenceTaskBuilder.Name = GeoFenseName;
                    geofenceTaskBuilder.TaskEntryPoint = "GeoFenseBackgroundTask.GeoFenseBackgroundTask";                    
                    geofenceTaskBuilder.SetTrigger(trigger);
                    BackgroundTaskRegistration geofenceTask = geofenceTaskBuilder.Register();
                }
            }
            catch (Exception e)
            {

            }
        }
    }
}

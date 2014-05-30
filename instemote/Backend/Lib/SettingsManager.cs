using Insteon;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Instemote
{
    public class SettingsManager
    {
        public SettingsManager()
        {

        }

        public bool IsUserLoggedIn
        {
            get
            {
                return !String.IsNullOrWhiteSpace(UserEmail) && !String.IsNullOrWhiteSpace(UserPassword);
            }
        }

        public string UserEmail
        {
            get
            {
                return (string)Windows.Storage.ApplicationData.Current.RoamingSettings.Values["UserEmail"];
            }
            set
            {
                Windows.Storage.ApplicationData.Current.RoamingSettings.Values["UserEmail"] = value;
            }
        }

        public string UserPassword
        {
            get
            {
                return (string)Windows.Storage.ApplicationData.Current.RoamingSettings.Values["UserPassword"];
            }
            set
            {
                Windows.Storage.ApplicationData.Current.RoamingSettings.Values["UserPassword"] = value;
            }
        }

        public string RemoteIP
        {
            get
            {
                return (string)Windows.Storage.ApplicationData.Current.RoamingSettings.Values["RemoteIP"];
            }
            set
            {
                Windows.Storage.ApplicationData.Current.RoamingSettings.Values["RemoteIP"] = value;
            }
        }

        public string RemotePort
        {
            get
            {
                return (string)Windows.Storage.ApplicationData.Current.RoamingSettings.Values["RemotePort"];
            }
            set
            {
                Windows.Storage.ApplicationData.Current.RoamingSettings.Values["RemotePort"] = value;
            }
        }

        public string LocalIP
        {
            get
            {
                return (string)Windows.Storage.ApplicationData.Current.RoamingSettings.Values["LocalIP"];
            }
            set
            {
                Windows.Storage.ApplicationData.Current.RoamingSettings.Values["LocalIP"] = value;
            }
        }

        public string LocalPort
        {
            get
            {
                return (string)Windows.Storage.ApplicationData.Current.RoamingSettings.Values["LocalPort"];
            }
            set
            {
                Windows.Storage.ApplicationData.Current.RoamingSettings.Values["LocalPort"] = value;
            }
        }

        public string HouseID
        {
            get
            {
                return (string)Windows.Storage.ApplicationData.Current.RoamingSettings.Values["HouseID"];
            }
            set
            {
                Windows.Storage.ApplicationData.Current.RoamingSettings.Values["HouseID"] = value;
            }
        }

        public string HouseName
        {
            get
            {
                return (string)Windows.Storage.ApplicationData.Current.RoamingSettings.Values["HouseName"];
            }
            set
            {
                Windows.Storage.ApplicationData.Current.RoamingSettings.Values["HouseName"] = value;
            }
        }

        public string HubPassword
        {
            get
            {
                return (string)Windows.Storage.ApplicationData.Current.RoamingSettings.Values["HubPassword"];
            }
            set
            {
                Windows.Storage.ApplicationData.Current.RoamingSettings.Values["HubPassword"] = value;
            }
        }

        public string HubUserName
        {
            get
            {
                return (string)Windows.Storage.ApplicationData.Current.RoamingSettings.Values["HubUserName"];
            }
            set
            {
                Windows.Storage.ApplicationData.Current.RoamingSettings.Values["HubUserName"] = value;
            }
        }

        public void Reset()
        {
            UserEmail = "";
            UserPassword = "";
            RemoteIP = "";
            RemotePort = "";
            LocalIP = "";
            LocalPort = "";
            HouseID = "";
            CortanaSceneCommand = "";
            HouseName = "";
            HubPassword = "";
            HubUserName = "";
        }

        #region House

        public DateTimeOffset LastHomeSync
        {
            get
            {
                if (Windows.Storage.ApplicationData.Current.LocalSettings.Values.ContainsKey("LastHomeSync"))
                {
                    return (DateTimeOffset)Windows.Storage.ApplicationData.Current.LocalSettings.Values["LastHomeSync"];
                }
                else
                {
                    return DateTimeOffset.Now.Subtract(TimeSpan.FromDays(50));
                }
            }
            set
            {
                Windows.Storage.ApplicationData.Current.LocalSettings.Values["LastHomeSync"] = value;
            }
        }

        public List<Scene> SceneList
        {
            get
            {
                if (Windows.Storage.ApplicationData.Current.LocalSettings.Values.ContainsKey("SceneList"))
                {
                    return JsonConvert.DeserializeObject<List<Scene>>((string)Windows.Storage.ApplicationData.Current.LocalSettings.Values["SceneList"]);
                }
                else
                {
                    return new List<Scene>();
                }
            }
            set
            {
                string json = JsonConvert.SerializeObject(value);
                Windows.Storage.ApplicationData.Current.LocalSettings.Values["SceneList"] = json;
            }
        }

        public List<Device> DeviceList
        {
            get
            {
                if (Windows.Storage.ApplicationData.Current.LocalSettings.Values.ContainsKey("DeviceList"))
                {
                    return JsonConvert.DeserializeObject<List<Device>>((string)Windows.Storage.ApplicationData.Current.LocalSettings.Values["DeviceList"]);
                }
                else
                {
                    return new List<Device>();
                }
            }
            set
            {
                string json = JsonConvert.SerializeObject(value);
                Windows.Storage.ApplicationData.Current.LocalSettings.Values["DeviceList"] = json;
            }
        }

        #endregion

        #region Settings

        public string CortanaSceneCommand
        {
            get
            {
                return (string)Windows.Storage.ApplicationData.Current.RoamingSettings.Values["CortanaSceneCommand"];
            }
            set
            {
                Windows.Storage.ApplicationData.Current.RoamingSettings.Values["CortanaSceneCommand"] = value;
            }
        }

        public string GeoFenceSceneCommand
        {
            get
            {
                return (string)Windows.Storage.ApplicationData.Current.RoamingSettings.Values["GeoFenceSceneCommand"];
            }
            set
            {
                Windows.Storage.ApplicationData.Current.RoamingSettings.Values["GeoFenceSceneCommand"] = value;
            }
        }

        public double HomeGeoFenceLat
        {
            get
            {
                if (Windows.Storage.ApplicationData.Current.RoamingSettings.Values.ContainsKey("HomeGeoFenceLat"))
                {
                    return (double)Windows.Storage.ApplicationData.Current.RoamingSettings.Values["HomeGeoFenceLat"];
                }
                else
                {
                    return 0.0;
                }
            }
            set
            {
                Windows.Storage.ApplicationData.Current.RoamingSettings.Values["HomeGeoFenceLat"] = value;
            }
        }

        public double HomeGeoFenceLong
        {
            get
            {
                if (Windows.Storage.ApplicationData.Current.RoamingSettings.Values.ContainsKey("HomeGeoFenceLong"))
                {
                    return (double)Windows.Storage.ApplicationData.Current.RoamingSettings.Values["HomeGeoFenceLong"];
                }
                else
                {
                    return 0.0;
                }
            }
            set
            {
                Windows.Storage.ApplicationData.Current.RoamingSettings.Values["HomeGeoFenceLong"] = value;
            }
        }

        public bool GeoFenseOffLeave
        {
            get
            {
                if (Windows.Storage.ApplicationData.Current.RoamingSettings.Values.ContainsKey("GeoFenseOffLeave"))
                {
                    return (bool)Windows.Storage.ApplicationData.Current.RoamingSettings.Values["GeoFenseOffLeave"];
                }
                else
                {
                    return false;
                }
            }
            set
            {
                Windows.Storage.ApplicationData.Current.RoamingSettings.Values["GeoFenseOffLeave"] = value;
            }
        }

        public bool GeoFenseOffAsk
        {
            get
            {
                if (Windows.Storage.ApplicationData.Current.RoamingSettings.Values.ContainsKey("GeoFenseOffAsk"))
                {
                    return (bool)Windows.Storage.ApplicationData.Current.RoamingSettings.Values["GeoFenseOffAsk"];
                }
                else
                {
                    return false;
                }
            }
            set
            {
                Windows.Storage.ApplicationData.Current.RoamingSettings.Values["GeoFenseOffAsk"] = value;
            }
        }

        public bool GeoFenseOnArrive
        {
            get
            {
                if (Windows.Storage.ApplicationData.Current.RoamingSettings.Values.ContainsKey("GeoFenseOnArrive"))
                {
                    return (bool)Windows.Storage.ApplicationData.Current.RoamingSettings.Values["GeoFenseOnArrive"];
                }
                else
                {
                    return false;
                }
            }
            set
            {
                Windows.Storage.ApplicationData.Current.RoamingSettings.Values["GeoFenseOnArrive"] = value;
            }
        }

        public bool GeoFenseOnAsk
        {
            get
            {
                if (Windows.Storage.ApplicationData.Current.RoamingSettings.Values.ContainsKey("GeoFenseOnAsk"))
                {
                    return (bool)Windows.Storage.ApplicationData.Current.RoamingSettings.Values["GeoFenseOnAsk"];
                }
                else
                {
                    return false;
                }
            }
            set
            {
                Windows.Storage.ApplicationData.Current.RoamingSettings.Values["GeoFenseOnAsk"] = value;
            }
        }

        #endregion
    }
}

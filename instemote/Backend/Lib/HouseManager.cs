using Instemote;
using Insteon;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.System.Threading;

namespace Backend.Lib
{
    public class HouseManager
    {
        public DataManager DataMan;

        public delegate void HouseUpdated(Exception e);
        public event HouseUpdated HouseUpdatedHandler;

        public delegate void HouseUpdating();
        public event HouseUpdating HouseUpdatingHandler;

        public HouseManager(DataManager da)
        {
            DataMan = da;
        }

        public void CheckForSync(bool force)
        {
            if (force || DateTimeOffset.Now.Subtract(DataMan.SettingsMan.LastHomeSync).Days > 3)
            {
                ThreadPool.RunAsync(new WorkItemHandler((IAsyncAction) =>
                {
                    if (HouseUpdatingHandler != null)
                    {
                        HouseUpdatingHandler();
                    }

                    // This will also update the house
                    DataMan.RefreshCredentials(true);
                }));
            }
        }

        public void GetHouseConfig()
        {
            WebRequest myReq = (WebRequest)WebRequest.Create("https://connect.insteon.com/api-usr/UserConfig/GetHouseConfig?HouseID=" + DataMan.SettingsMan.HouseID);
            myReq.Headers["Authorization"] = DataMan.SettingsMan.UserEmail + ":" + DataMan.SettingsMan.UserPassword;

            // Make the request
            Exception ex = null;
            myReq.BeginGetResponse(new AsyncCallback((IAsyncResult result) =>
            {
                try
                {
                    HttpWebRequest request = result.AsyncState as HttpWebRequest;
                    if (request != null)
                    {
                        using (HttpWebResponse webResponse = (HttpWebResponse)request.EndGetResponse(result))
                        {
                            using (StreamReader reader = new StreamReader(webResponse.GetResponseStream()))
                            {
                                string temp = DataMan.CleanUpJson(reader.ReadToEnd());
                                JObject response = JObject.Parse(temp);

                                DataMan.SettingsMan.HouseName = (string)response["HouseName"];
                                DataMan.SettingsMan.HubPassword = (string)response["HubPassword"];
                                DataMan.SettingsMan.HubUserName = (string)response["HubUserName"];

                                ParseDevices(response["DeviceList"]);
                                ParseScenes(response["SceneList"]);
                            }
                        }
                    }
                }
                catch (Exception r)
                {
                    ex = r;
                }

                if(ex == null)
                {
                    DataMan.SettingsMan.LastHomeSync = DateTimeOffset.Now;
                }

                if (HouseUpdatedHandler != null)
                {
                    HouseUpdatedHandler(ex);
                }
            }), myReq);
        }

        public void ParseScenes(JToken JsonList)
        {
            List<Scene> Scenes = new List<Scene>();
            foreach (JToken SceneJson in JsonList)
            {
                Scene scene = new Scene();
                scene.Name = (string)SceneJson["SceneName"];
                scene.ID = (string)SceneJson["SceneID"];
                scene.GroupID = (string)SceneJson["Group"];

                Scenes.Add(scene);                
            }

            DataMan.SettingsMan.SceneList = Scenes;
        }

        public void ParseDevices(JToken JsonList)
        {
            List<Device> Devices = new List<Device>();

            foreach (JToken SceneJson in JsonList)
            {
                if (((string)SceneJson["DeviceName"]).ToLower().Contains("motion"))
                {
                    // This is a motion
                    continue;
                }

                Device device = new Device();
                device.Name = (string)SceneJson["DeviceName"];
                device.ID = (string)SceneJson["DeviceID"];
                device.GroupID = (string)SceneJson["Group"];
                device.InsteonID = (string)SceneJson["InsteonID"];

                Devices.Add(device);
            }

            DataMan.SettingsMan.DeviceList = Devices;
        }

        public Scene GetSceneFromName(string name)
        {            
            List<Scene> SceneList = DataMan.SettingsMan.SceneList;

            if(name != null)
            {
                name = name.ToLower().Trim();

                foreach (Scene s in SceneList)
                {
                    if (s.Name.ToLower().Trim().Equals(name))
                    {
                        return s;
                    }
                }
            }          

            return SceneList.Count > 0 ? SceneList[0] : null;
        }
    }
}

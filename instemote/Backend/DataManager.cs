using Backend.Lib;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.System.Threading;
using Windows.Web.Http;

namespace Instemote
{
    public class DataManager
    {
        public SettingsManager SettingsMan;
        public HouseManager HouseMan;
        public CommandManager CommandMan;
        public GeoFenceManager GeoFenceMan;
      
        public DataManager()
        {
            SettingsMan = new SettingsManager();
            CommandMan = new CommandManager(this);
            HouseMan = new HouseManager(this);
            GeoFenceMan = new GeoFenceManager(this);
        }


        #region HouseStuff

        public bool RefreshCredentials(bool UpdateHouse)
        {
            string email = SettingsMan.UserEmail;
            string password = SettingsMan.UserPassword;

            WebRequest myReq = (WebRequest)WebRequest.Create("https://connect.insteon.com/api-usr/NetlincService/GetuserResInfo");
            myReq.Headers["Authorization"] = email + ":" + password;

            // Make the request
            Exception ex = null;
            using (AutoResetEvent are = new AutoResetEvent(false))
            {
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
                                    string temp = CleanUpJson(reader.ReadToEnd());
                                    JObject response = JObject.Parse(temp);

                                    SettingsMan.RemoteIP = (string)response["remote_ip"];
                                    SettingsMan.RemotePort = (string)response["remote_port"];
                                    SettingsMan.LocalIP = (string)response["local_ip"];
                                    SettingsMan.LocalPort = (string)response["local_port"];
                                    SettingsMan.HouseID = (string)response["HouseID"];
                                    SettingsMan.UserEmail = email;
                                    SettingsMan.UserPassword = password;
                                }
                            }
                        }
                    }
                    catch (Exception r)
                    {
                        ex = r;
                    }
                    are.Set();
                }), myReq);
                are.WaitOne();
            }

            if(ex != null)
            {
                throw ex;
            }

            if (UpdateHouse)
            {
                ThreadPool.RunAsync(new WorkItemHandler((IAsyncAction) =>
                {
                    HouseMan.GetHouseConfig();
                }));
            }

            return true;     
        }

      
        public string CleanUpJson(string input)
        {
            if (input.StartsWith("["))
            {
                input = input.Substring(1);
            }

            if (input.EndsWith("]"))
            {
                input = input.Substring(0, input.Length - 1);
            }

            return input;
        }

        #endregion
    }
}

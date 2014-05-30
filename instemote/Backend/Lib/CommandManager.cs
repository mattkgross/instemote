using Instemote;
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
    public class CommandManager
    {
        public delegate void CommandResult(InsteonCommand command, bool hasMore);
        public event CommandResult CommandResultHandler;

        public List<InsteonCommand> Commands = new List<InsteonCommand>();
        public Random random = new Random((int)DateTime.UtcNow.Ticks);

        DataManager DataMan;
        public bool UsingLocalAddress = false;

        public CommandManager(DataManager da)
        {
            DataMan = da;
        }

        public void SendCommand(InsteonCommand command)
        {
            lock(Commands)
            {
                // Add the command
                Commands.Insert(0, command);

                if(Commands.Count > 10)
                {
                    Commands.RemoveAt(10);
                }

                if(Commands.Count == 1)
                {
                    // We need to start processing
                    ProcessRequests();
                }
            }
        }

        public void ProcessRequests()
        {
            ThreadPool.RunAsync(new WorkItemHandler((IAsyncAction) =>
            {
                while(true)
                {
                    InsteonCommand command = null;

                    lock(Commands)
                    {
                        if(Commands.Count == 0)
                        {
                            // We are done!
                            break;
                        }
                        else
                        {
                            command = Commands[Commands.Count - 1];
                            Commands.RemoveAt(Commands.Count - 1);
                        }
                    }

                    if(command != null)
                    {
                        // Send the command
                        SendHubRequest(command);

                        // See if there was a problem
                        if (command.ResponseException != null)
                        {
                            // If the exception was that we couldn't connect, try the remote ip.
                            if (command.ResponseException.GetType() == typeof(WebException))
                            {
                                // Check for bad creds
                                if (((WebException)command.ResponseException).Message.ToLower().Contains("unauthorized"))
                                {
                                    // Try to get the new creds
                                    DataMan.RefreshCredentials(true);
                                }
                                else
                                {
                                    if (UsingLocalAddress)
                                    {
                                        // Try it again remotely
                                        UsingLocalAddress = false;
                                        command.ResponseException = null;
                                        command.Response = "";
                                        command.ForceRemote = false;
                                        command.ForceLocal = false;

                                        // Try sending it again
                                        SendHubRequest(command);
                                    }
                                }                                
                            }
                        }

                        bool HasMore = false;
                        lock (Commands)
                        {
                            if (Commands.Count > 0)
                                HasMore = true;
                        }

                        if(CommandResultHandler != null)
                            CommandResultHandler(command, HasMore);
                    }
                }
            }));
        }

        private void SendHubRequest(InsteonCommand command)
        {            
            // Setup
            bool RequestIsLocal = (command.ForceLocal || command.ForceRemote) ? (command.ForceLocal || !command.ForceRemote) : UsingLocalAddress;
            string ip = RequestIsLocal ? DataMan.SettingsMan.LocalIP : DataMan.SettingsMan.RemoteIP;
            string port = RequestIsLocal ? DataMan.SettingsMan.LocalPort : DataMan.SettingsMan.RemotePort;

            // Create the request
            WebRequest webRequest = (WebRequest)WebRequest.Create("http://" + ip + ":" + port + "/" + command.GetArgument());
            webRequest.Headers["Cache-Control"] = "no-cache";
            webRequest.Headers[HttpRequestHeader.IfModifiedSince] = DateTime.UtcNow.ToString();
            webRequest.Credentials = new NetworkCredential(DataMan.SettingsMan.HubUserName, DataMan.SettingsMan.HubPassword);

            // Make the request
            using (AutoResetEvent are = new AutoResetEvent(false))
            {
                webRequest.BeginGetResponse(new AsyncCallback((IAsyncResult result) =>
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
                                    command.Response = reader.ReadToEnd();
                                    if (!String.IsNullOrWhiteSpace(command.Response))
                                    {
                                        throw new Exception("Unexpected Response");
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        command.ResponseException = ex;
                    }

                    // Set the wait
                    are.Set();

                }), webRequest);

                // Wait for the event
                are.WaitOne();
            }
        }  
   
        public void TestLocal()
        {
            ThreadPool.RunAsync(new WorkItemHandler((IAsyncAction) =>
            {
                // Test to see if a local connect is available.
                // Create the request
                WebRequest webRequest = (WebRequest)WebRequest.Create("http://" + DataMan.SettingsMan.LocalIP + ":" + DataMan.SettingsMan.LocalPort);
                webRequest.Headers["Cache-Control"] = "no-cache";
                webRequest.Credentials = new NetworkCredential(DataMan.SettingsMan.HubUserName, DataMan.SettingsMan.HubPassword);

                // Make the request
                webRequest.BeginGetResponse(new AsyncCallback((IAsyncResult result) =>
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
                                    UsingLocalAddress = true;
                                    string response = reader.ReadToEnd();
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        if (!String.IsNullOrWhiteSpace(e.Message) &&
                            e.Message.Contains(""))
                        {
                            UsingLocalAddress = false;
                        }
                    }
                }), webRequest);
                
            }));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Lib
{
    public class InsteonCommand
    {
        private static int idCont = 0;
        public int Id;

        public bool ForceLocal = false;
        public bool ForceRemote = false;

        public CommandType Type = CommandType.Scene_Off;
        public string RequestId = "";

        // Response stuff
        public Exception ResponseException;
        public string Response = "";

        public InsteonCommand(CommandType type)
        {
            Type = type;
            idCont++;
            Id = idCont;
        }

        public InsteonCommand(CommandType type, Scene s) : this(type)
        {
            RequestId = s.GroupID;
        }   

        public string GetArgument()
        {
            switch (Type)
            {
                case CommandType.Scene_On:
                    return "0?11" + RequestId + "=I=0=0";
                case CommandType.Scene_Off:
                    return "0?13" + RequestId + "=I=0=0";
                case CommandType.Device_On:
                    return "0?11" + RequestId + "=I=0=0";
                case CommandType.Device_Off:
                    return "0?11" + RequestId + "=I=0=0";
                default:
                    return "";
            }
        }
        public enum CommandType
        {
            Scene_On,
            Scene_Off,
            Device_On,
            Device_Off,
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SCSV.AlarmInteract
{
    public class Local
    {
        public string user;
        public string pwd;
    }

    public class CMS
    {
        public string ip;
        public int port;
    }

    public class Devices
    {
        public string display;
        public bool dws;
        public string[] cameras;
    }

    public class CameraAttr
    {
        public bool dws = false;

        public int code;
        public int handle;
        public string camera;

        public string monitor;
        public int index = 0;
    }

    public class DWS
    {
        public string sn;
        public Monitor[] monitor;
    }

    public class Monitor
    {
        public string sn;
        public int index = 0;
    }

    public enum EventType
    {
        /// <summary>
        /// 呼叫
        /// </summary>
        Call = 1,
        /// <summary>
        /// 通话
        /// </summary>
        Connect,
        /// <summary>
        /// 挂断
        /// </summary>
        HangUp
    }

    public class Msg: EventArgs
    {
        public EventType type;
        public string display;
    }

    public class tool
    {
        public static object BytesToStruct(byte[] bytes, Type strcutType)
        {
            int size = Marshal.SizeOf(strcutType);
            IntPtr buffer = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.Copy(bytes, 0, buffer, size);
                return Marshal.PtrToStructure(buffer, strcutType);
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
    }
}

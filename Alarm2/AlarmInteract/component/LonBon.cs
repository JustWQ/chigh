using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SCSV.AlarmInteract.component
{
    class LonBon: AlarmInteractClass
    {
        private LonBonDLL.ACTION_CALLBACK _call_back;

        public override bool Start(string append)
        {
            // 解析地址盒地址
            string addr = null;
            JObject jsonOjb = JObject.Parse(append);
            if (jsonOjb != null)
            {
                addr = jsonOjb["addrMgr"].ToString();
            }

            // 解析出错
            if (string.IsNullOrEmpty(addr))
            {
                Logger.Log.ErrorFormat("地址盒地址解析错误, append：{0}\r\n", append);
                return false;
            }

            // 初始化LonBon SDK
            _call_back = new LonBonDLL.ACTION_CALLBACK(func);
            int status = LonBonDLL.lb_initialServer(addr, 0);
            if (status != 0)
            {
                Logger.Log.ErrorFormat("初始化LonBon SDK失败, LonBon：{0} status： {1}\r\n", addr, status);
                return false;
            }

            // 注册回调函数
            status = LonBonDLL.lb_CallActionNotify(_call_back, IntPtr.Zero);
            if (status != 0)
            {
                Logger.Log.ErrorFormat("注册回调函数失败 {0}\r\n", status);
                return false;
            }

            return true;
        }

        public override void Stop()
        {
            
        }

        private void func(lb_event_message userEvent, IntPtr wParam, IntPtr userData)
        {
            // 事件参数
            action_param param = new action_param();
            byte[] pByte = new byte[1024];
            Marshal.Copy(wParam, pByte, 0, Marshal.SizeOf(param));
            param = (action_param)BytesToStruct(pByte, param.GetType());
            Logger.Log.InfoFormat("Event: {0}, Sender: {1}, Receiver: {2}\r\n", userEvent, param.sender, param.receiver);

            int displayNum = 0;

            // 取分机号
            if (param.sender % 1000 != 0)
            {
                displayNum = param.sender;
            }
            else if (param.receiver % 1000 != 0)
            {
                displayNum = param.receiver;
            }
            else
            {
                Logger.Log.Info("不存在分机设备");
                return;
            }

            switch (userEvent)
            {
                // 紧急报警
                case lb_event_message.LBTCP_EVENT_EXTNEMGY:
                    EventHandle(EventType.Call, displayNum);
                    break;
                // 普通呼叫
                case lb_event_message.LBTCP_EVENT_CALLIN:
                    EventHandle(EventType.Call, displayNum);
                    break;
                // 监听接通
                case lb_event_message.LBTCP_EVENT_LSTN_CONNECT:
                // 对讲接通
                case lb_event_message.LBTCP_EVENT_TALK_CONNECT:
                    EventHandle(EventType.Connect, displayNum);
                    break;
                // 通话挂断
                case lb_event_message.LBTCP_EVENT_TALK_DISCONNECT:
                    EventHandle(EventType.HangUp, displayNum);
                    break;
            }
        }

        private void EventHandle(EventType type, int display)
        {
            Msg msg = new Msg();
            msg.type = type;
            msg.display = display.ToString();

            // 通知主线程
            handler?.Invoke(this, msg);
        }

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

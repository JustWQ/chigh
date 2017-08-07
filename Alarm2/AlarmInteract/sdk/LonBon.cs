using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace SCSV.AlarmInteract
{
    public enum lb_event_message
    {
        LBTCP_EVENT_NONE = 0,

        LBTCP_EVENT_PROCESSING,             // 1-呼出处理中
        LBTCP_EVENT_RINGBACK,               // 2-呼出振铃
        LBTCP_EVENT_CALLIN,                 // 3-普通呼入
        LBTCP_EVENT_EXTNEMGY,               // 4-紧急报警
        LBTCP_EVENT_EXTNNSALM,              // 5-喧哗报警
        LBTCP_EVENT_EXTNRMALARM,            // 6-防拆报警
        LBTCP_EVENT_EXTNWARDALARM,          // 7-病区门口机报警
        LBTCP_EVENT_EXTNINFUSALM,           // 8-输液报警

        LBTCP_EVENT_LSTN_CONNECT,           // 9-监听接通
        LBTCP_EVENT_TALK_CONNECT,           // 10-对讲接通
        LBTCP_EVENT_TALK_DISCONNECT,        // 11-通话挂断
        LBTCP_EVENT_CALLIN_DISCONNECT,      // 12-呼入/呼出挂断
        LBTCP_EVENT_CALLOUT_FAIL,           // 13-呼出失败
        LBTCP_EVENT_DR1_OPEN,               // 14-门磁1断开提示
        LBTCP_EVENT_DR1_CLOSE,              // 15-门磁1闭合提示
        LBTCP_EVENT_DR2_OPEN,               // 16-门磁2断开提示
        LBTCP_EVENT_DR2_CLOSE,              // 17-门磁2闭合提示
        LBTCP_EVENT_BC2MST_START,           // 18-对主机喊话广播开始
        LBTCP_EVENT_BC2EXTN_START,          // 19-对分机喊话广播开始
        LBTCP_EVENT_BC2EXTNFILE_START,      // 20-文件广播开始
        LBTCP_EVENT_BC2EXTNEXAD_START,      // 21-外接音源广播开始
        LBTCP_EVENT_BC_STOP,                // 22-广播结束

        LBTCP_EVENT_MLTK_CONFERENCE_START,  // 23-会议模式开始
        LBTCP_EVENT_MLTK_CONDUCT_START,     // 24-指挥模式开始
        LBTCP_EVENT_MLTK_STOP,              // 25-多方通话结束
        LBTCP_EVENT_MLTK_FAIL,              // 26-多方通话失败
    }

    public struct action_param
    {
        public int sender;                 // 发送端
        public int receiver;               // 接收端
        public string acceptBc;            // 广播接收端
        public string SessionId;           // 会话标识
        public int broadId;                // 广播组序(标识)/门磁编号
        public string rdFile;              // 录音文件名
        public int atmTerNum;				// Atm编号
    }

public class LonBonDLL
    {
        // 视频回调
        public delegate void ACTION_CALLBACK(lb_event_message userEvent, IntPtr wParam, IntPtr userData);

        // 初始化
        [DllImport(@".\lb_sdk_universal.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int lb_initialServer(string serverIp, int svrPort);
        // 请求视频流
        [DllImport(@".\lb_sdk_universal.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int lb_CallActionNotify(ACTION_CALLBACK callback, IntPtr userData);
    }
}

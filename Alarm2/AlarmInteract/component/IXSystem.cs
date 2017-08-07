using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SCSV.AlarmInteract.component
{
    class IXSystem : AlarmInteractClass
    {
        private TcpListener _server;
        private CancellationTokenSource _cts;
        /// <summary>  
        /// 接收缓冲区大小  
        /// </summary>  
        public Int32 ReceiveBufferSize = 1024;

        public override bool Start(string append)
        {
            // 解析配置参数
            string ip = null;
            int port = 0;
            JObject jsonOjb = JObject.Parse(append);
            if (jsonOjb != null)
            {
                ip = jsonOjb["ip"]?.ToString();
                port = Convert.ToInt32(jsonOjb["port"]);
                master = jsonOjb["master"]?.ToString();
            }

            // 解析出错
            if (string.IsNullOrEmpty(ip) || port == 0)
            {
                Logger.Log.ErrorFormat("配置参数解析错误, append：{0}\r\n", append);
                return false;
            }

            // 启动监听
            //_server = new TcpListener(IPAddress.Parse(ip), port);
            _server = new TcpListener(IPAddress.Any, port);
            if (_server == null)
            {
                Logger.Log.ErrorFormat("{0}:{1} 启动监听失败，检查端口占用\r\n", ip, port);
                return false;
            }

            _server.Start();

            // 准备接受连接请求
            _cts = new CancellationTokenSource();
            Task t = new Task(() => StartReceiveConnect(_cts.Token), _cts.Token);
            t.Start();
            t.ContinueWith((Task obj) =>
            {
                Logger.Log.Info("IXSystem监听任务退出");
            });

            Logger.Log.InfoFormat("IXSystem服务启动： {0}:{1} {2}", ip, port, master);
            return true;
        }

        public override void Stop()
        {
            _cts.Cancel();
            _server.Stop();
        }

        private void StartReceiveConnect(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    TcpClient client = _server.AcceptTcpClient();
                    Task t = new Task(() => StartReceiveMsg(client, _cts.Token), _cts.Token);
                    t.Start();
                    t.ContinueWith(TaskEnded);
                }
                catch (Exception e)
                {
                    Logger.Log.ErrorFormat("{0}\r\n", e.Message.ToString());
                }
            }
        }

        private void StartReceiveMsg(TcpClient client, CancellationToken ct)
        {
            string from = client.Client.RemoteEndPoint.ToString();
            Logger.Log.InfoFormat("Ready receive data from {0}", from);

            //利用TcpClient对象GetStream方法得到网络流
            NetworkStream stream = client.GetStream();
            stream.ReadTimeout = 600000;
            Byte[] data;

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    Read(stream, out data);
                    Logger.Log.InfoFormat("Received msg from {0}: {1}", from, BitConverter.ToString(data));
                    // 回应ACK
                    Byte[] ack = {0x41, 0x43, 0x4B};
                    stream.Write(ack, 0, ack.Length);

                    // 解析消息内容
                    IXMessage msg = new IXMessage();
                    msg = (IXMessage)tool.BytesToStruct(data, msg.GetType());
                    EventHandle(msg);
                }
                catch (Exception e)
                {
                    Logger.Log.ErrorFormat("{0}：{1}", from, e.Message);
                    stream.Close();
                    break;
                }
            }
        }

        private void TaskEnded(Task obj)
        {
            Logger.Log.Info("Client 连接关闭");
        }

        /// <summary>  
        /// 异步接收  
        /// </summary>  
        /// <param name="data">接收到的字节数组</param>  
        public void Read(NetworkStream Stream, out Byte[] data)
        {
            // 用户定义对象  
            AsyncReadStateObject State = new AsyncReadStateObject
            {   // 将事件状态设置为非终止状态，导致线程阻止  
                eventDone = new ManualResetEvent(false),
                stream = Stream,
                exception = null,
                numberOfBytesRead = -1
            };
            
            Byte[] Buffer = new Byte[ReceiveBufferSize];
            using (MemoryStream memStream = new MemoryStream(ReceiveBufferSize))
            {
                Int32 TotalBytes = 100;       // 总共需要接收的字节数  
                Int32 ReceivedBytes = 0;    // 当前已接收的字节数  
                while (true)
                {
                    // 将事件状态设置为非终止状态，导致线程阻止  
                    State.eventDone.Reset();

                    // 异步读取网络数据流  
                    Stream.BeginRead(Buffer, 0, Buffer.Length, new AsyncCallback(AsyncReadCallback), State);

                    // 等待操作完成信号  
                    if (State.eventDone.WaitOne(Stream.ReadTimeout, false))
                    {   // 接收到信号  
                        if (State.exception != null) throw State.exception;

                        if (State.numberOfBytesRead == 0)
                        {   // 连接已经断开  
                            throw new SocketException();
                        }
                        else if (State.numberOfBytesRead > 0)
                        {
                            memStream.Write(Buffer, 0, State.numberOfBytesRead);
                            ReceivedBytes += State.numberOfBytesRead;
                            if (ReceivedBytes >= TotalBytes) break;
                        }
                    }
                    else
                    {   // 超时异常  
                        throw new TimeoutException();
                    }
                }

                // 将流内容写入字节数组  
                data = (memStream.Length > 0) ? memStream.ToArray() : null;
            }
        }

        /// <summary>  
        /// 异步读取回调函数  
        /// </summary>  
        /// <param name="ar">异步操作结果</param>  
        private static void AsyncReadCallback(IAsyncResult ar)
        {
            AsyncReadStateObject State = ar.AsyncState as AsyncReadStateObject;
            try
            {   // 异步写入结束  
                State.numberOfBytesRead = State.stream.EndRead(ar);
            }

            catch (Exception e)
            {   // 异步连接异常  
                State.exception = e;
            }

            finally
            {   // 将事件状态设置为终止状态，线程继续  
                State.eventDone.Set();
            }
        }

        private void EventHandle(IXMessage ix)
        {
            Msg msg = new Msg();
            // 默认取SrcID
            string display = Encoding.ASCII.GetString(ix.SrcID).TrimEnd('\0');
            if (display.CompareTo(master) == 0)
            {
                display = Encoding.ASCII.GetString(ix.DstID).TrimEnd('\0');
            }


            // 通话
            if (ix.EventType[0] == 0x30 && ix.EventType[1] == 0x30)
            {
                if (ix.EventType[2] == 0x30 && (ix.EventType[3] == 0x30))
                {
                    msg.type = EventType.Call;
                }
                else if (ix.EventType[2] == 0x30 && (ix.EventType[3] == 0x31))
                {
                    msg.type = EventType.Connect;
                }
                else if (ix.EventType[2] == 0x31 && (ix.EventType[3] == 0x31))
                {
                    msg.type = EventType.HangUp;
                }
                else
                {
                    Logger.Log.ErrorFormat("忽略事件：事件（{0}），分机（{1}）", BitConverter.ToString(ix.EventType), display);
                    return;
                }
            }
            else
            {
                return;
            }

            // 通知主线程
            msg.display = display;
            handler?.Invoke(this, msg);
        }
    }

    /// <summary>  
    /// 异步读状态对象  
    /// </summary>  
    internal class AsyncReadStateObject
    {
        public ManualResetEvent eventDone;
        public NetworkStream stream;
        public Exception exception;
        public Int32 numberOfBytesRead;
    }

    internal struct IXMessage
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] extend1;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] EventType;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public byte[] extend2;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public byte[] SrcID;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public byte[] extend3;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public byte[] DstID;
    }
}

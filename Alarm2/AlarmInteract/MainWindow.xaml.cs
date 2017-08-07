using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SCSV.AlarmInteract.component;
using sv_base_liveLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;

namespace SCSV.AlarmInteract
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public  partial class MainWindow : Window
    {
        private sv_base_live_ctrl _ocx;
        private AlarmInteractClass _sdk;
        private Local _local;
        private CMS _cms;
        private Dictionary<string, Devices> _devices;
        private DWS _dws;
        private List<CameraAttr> _stream = new List<CameraAttr>();
        private string _protocol;
        private string _append;
        private string _sn;
        private string _session;
        private string _ccs_ip;
        private int _ccs_port;
        private bool _login = false;
        private List<VideoHost> LV;//播放器集合
        public static int row = 3;
        public  static int col = 3;            //
        int i=0;//屏幕状态字 0：小屏。1：全屏
        int x;
        int y;
        consoleDesk d;
        static Inforxaml ifx;
        bool full = false;
        VideoHost v = new VideoHost();
        public static  System.Windows.Forms.Timer myTimer = new System.Windows.Forms.Timer();
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;           
        }

        public static void ShowStatus(string status, bool success = true)
        {
            ifx.txStatus.Text = status;
            ifx.txStatus.Foreground = success ? Brushes.ForestGreen : Brushes.Red;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                // 卸载控件
                if (_ocx != null)
                {
                    // 关闭视频
                    // RealStop();

                    // 注销
                    if (_login)
                    {
                        _ocx.Logout();
                    }

                    _ocx.Uninit();
                }

                // 卸载sdk组件
                if (_sdk != null)
                {
                    _sdk.Stop();
                }
            }
            catch (Exception ex)
            {
                Logger.Log.ErrorFormat("{0}\r\n", ex.ToString());
            }

            Thread.Sleep(1000);
            Logger.Log.Info("服务退出\r\n");
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {


            d = new consoleDesk();
            d.WindowStartupLocation = WindowStartupLocation.Manual;
            d.Left = SystemParameters.PrimaryScreenWidth - d.Width;
            d.Top = SystemParameters.PrimaryScreenHeight - d.Height;
            d.readbutton += ShowVideoHost;
            //d.Owner = this;
            // d.closefather += closethis;
            d.Show();
            //bAddConsole(new Warn() { ExtensionNum = "4", ExtensionStatus = 7, ExtensionTime = DateTime.Now.ToString() });
            ifx = new Inforxaml();
            ifx.Width = this.Width;
            ifx.Height = this.Height / 14;
            ifx.WindowStartupLocation = WindowStartupLocation.Manual;
            ifx.Left = 0;
            ifx.Top = 0;
            ifx.Owner = this;
            ifx.Closing += MainWindow_Closing;
            ifx.closefather += closethis;
            ifx.Show();
            this.Show();
            ifx.txStatus.Text = "平台服务加载中...";
            // 初始化标志
            bool flag = false;
            string errMsg = "";
            try
            {
                do
                {
                    // 加载配置文件
                    if (!LoadConfig())
                    {
                        errMsg = "加载配置文件失败\r\n";
                        break;
                    }

                    int status = 0;
                    _ocx = new sv_base_live_ctrl();
                    _ocx.StatusEvent += _ocx_StatusEvent;
                    _ocx.StreamPlayEvt += _ocx_StreamPlayEvt;

                    // 实例化组件
                    if (string.Compare(_protocol, "LonBon") == 0)
                    {
                        _sdk = new LonBon();
                    }
                    else if (string.Compare(_protocol, "IXSystem") == 0)
                    {
                        _sdk = new IXSystem();
                    }
                    // 配置错误
                    else
                    {
                        errMsg = string.Format("厂商协议配置错误，{0}\r\n", _protocol);
                        break;
                    }

                    // 事件接受器
                    _sdk.handler += MsgHandler;

                    // 启动组件
                    if (!_sdk.Start(_append))
                    {
                        errMsg = string.Format("初始化SDK失败, {0}！\r\n", _append);
                        break;
                    }

                    // 登录中心
                    HttpProc http = new HttpProc();
                    string url = string.Format("http://{0}:{1}/cms/user_login.xml?userName={2}&passwd={3}&centerIp={4}",
                        _cms.ip, _cms.port, _local.user, _local.pwd, _cms.ip);

                    string rsp = http.Post(url, "");

                    // 解析处理结果
                    if (!ParseRsp(rsp))
                    {
                        errMsg = string.Format("登录中心失败：{0}\r\n{1}\r\n", url, rsp);
                        break;
                    }
                    Logger.Log.InfoFormat("登录中心成功：\r\nStandardNumber：{0}\r\nSession：{1}\r\nCCS：{2}:{3}\r\n",
                        _sn, _session, _ccs_ip, _ccs_port);

                    // 初始化控件
                    _ocx.Init();
                    // 登录
                    status = _ocx.Login(_sn, _ccs_ip, _ccs_port, _session, 0);
                    if (status < 0)
                    {
                        errMsg = string.Format("登录CCS失败： {0}\r\n", status);
                        break;
                    }

                    flag = true;
                } while (false);

                // 初始化失败
                if (!flag)
                {
                    Close();
                }
                else
                {
                    ShowStatus("平台服务准备就绪，等待呼叫请求中...");
                }
            }
            catch (Exception ex)
            {
                errMsg = string.Format("初始化出现异常： {0}\r\n", ex.ToString());
            }

            if (errMsg.Length > 0)
            {
                Logger.Log.Error(errMsg);
                ShowStatus(errMsg, false);
            }
            ShowVideoHost();
        }

        private void ShowVideoHost()
        {

            grid1.Children.Clear();
            grid1.RowDefinitions.Clear();
            grid1.ColumnDefinitions.Clear();
            for (int i = 0; i < row; i++)
            {
                RowDefinition rowd = new RowDefinition();
                grid1.RowDefinitions.Add(rowd);
            }
            for (int i = 0; i < col; i++)
            {
                ColumnDefinition colu = new ColumnDefinition();
                grid1.ColumnDefinitions.Add(colu);
            }
            LV = new List<VideoHost>();
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    VideoHost videoHost = new VideoHost();
                    videoHost.MouseDoubleClickEvent += VideoHost_MouseDoubleClickEvent;
                    videoHost.Margin = new Thickness(2);
                    videoHost.Background = new SolidColorBrush(Color.FromRgb(240, 255, 240));
                    grid1.Children.Add(videoHost);
                    Grid.SetColumn(videoHost, j);
                    Grid.SetRow(videoHost, i);
                    LV.Add(videoHost);
                    videoHost.Child = new System.Windows.Forms.UserControl();
                }

            }
            Show();
        }

        private void VideoHost_MouseDoubleClickEvent(object sender, System.Windows.Forms.MouseEventArgs e)
        {

            try
            {
                if (i == 0)
                {
                    x = Grid.GetColumn((VideoHost)sender);
                    y = Grid.GetRow((VideoHost)sender);
                    i++;
                }

                if (full)
                {
                    Grid.SetRow((VideoHost)sender, y);
                    Grid.SetColumn((VideoHost)sender, x);
                    Grid.SetRowSpan((VideoHost)sender, 1);
                    Grid.SetColumnSpan((VideoHost)sender, 1);
                    foreach (var item in LV)
                    {
                        item.Visibility = Visibility.Visible;
                    }

                    full = false;
                    i--;
                }
                else
                {
                    Grid.SetRow((VideoHost)sender, 0);
                    Grid.SetColumn((VideoHost)sender, 0);
                    Grid.SetRowSpan((VideoHost)sender, row);
                    Grid.SetColumnSpan((VideoHost)sender, col);
                    foreach (var item in LV)
                    {
                        item.Visibility = Visibility.Hidden;

                    }
                    ((VideoHost)sender).Visibility = Visibility.Visible;
                    full = true;
                }

                ifx.txStatus.Visibility = Visibility.Visible;
                myTimer.Dispose();
                myTimer.Tick += new EventHandler(Inforxaml_Hidden);
                myTimer.Enabled = false;
                myTimer.Interval = 4000;
                myTimer.Start();

            }
            catch { }

        }

        public void closethis()
        {
            this.Close();
        }
        private void _ocx_StreamPlayEvt(int stream_handle, int err_code)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => {
                // 查找播放对象
                var stream = _stream.Find(item => (item.handle == stream_handle));
                if (stream == null)
                {
                    Logger.Log.ErrorFormat("对象检索失败：{0}", stream_handle);
                    return;
                }

                if (err_code != 0)
                {
                    Logger.Log.ErrorFormat("视频播放失败：{0}-{1} {2}", stream.camera, stream_handle, err_code);
                    _ocx.StopStream(stream_handle);
                    _stream.Remove(stream);

                    return;
                }

                Logger.Log.InfoFormat("视频播放成功：{0}-{1}", stream.camera, stream_handle);
            }));
        }

        private void _ocx_StatusEvent(int lOrder, int lCode, int lStatus, int lParam, string sParam)
        {
            // 接口报错
            if (lStatus != 0)
            {
                Logger.Log.ErrorFormat("ocx接口调用错误： Order:{0}, Code:{1}, Status:{2}\r\n", lOrder, lCode, lStatus);
            }

            // 登录
            if (lOrder == 2001)
            {
                Logger.Log.Info("登录CCS成功，服务监听中……\r\n");
                _login = true;
            }
            // 视频请求
            else if (lOrder == 3001)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    // 查找播放对象
                    var stream = _stream.Find(item => (item.code == lCode));
                    if (stream == null)
                    {
                        Logger.Log.ErrorFormat("对象检索失败：{0}", lCode);
                        return;
                    }

                    // 视频请求成功
                    if (lStatus == 0)
                    {
                        // 记录播放句柄
                        stream.handle = lParam;
                    }
                    // 视频请求失败
                    else
                    {
                        _stream.Remove(stream);
                        Logger.Log.ErrorFormat("视频播放失败：{0}-{1} {2}", stream.camera, lParam, lStatus);
                    }
                }));
            }
            // 视频关闭
            else if (lOrder == 3002)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    foreach (var item in LV)
                    {
                        item.Child.BackColor = System.Drawing.Color.White;
                    }

                }));
            }
            // 视频投墙
            else if (lOrder == 6002)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    // 查找播放对象
                    var stream = _stream.Find(item => (item.code == lCode));
                    if (stream == null)
                    {
                        Logger.Log.ErrorFormat("对象检索失败：{0}", lCode);
                        return;
                    }

                    // 视频请求成功
                    if (lStatus != 0)
                    {
                        _stream.Remove(stream);
                        Logger.Log.ErrorFormat("视频投墙失败：{0}-{1}", stream.camera, lStatus);
                    }
                }));
            }
        }

        private bool LoadConfig()
        {
            string config = AppDomain.CurrentDomain.BaseDirectory + "Config.json";

            if (!File.Exists(config))
                return false;

            try
            {
                string strJson = File.ReadAllText(config);
                JObject jsonOjb = JObject.Parse(strJson);

                // 用户配置
                JToken token = jsonOjb["local"];
                if (token != null)
                {
                    _local = JsonConvert.DeserializeObject<Local>(token.ToString());

                    // 加密
                    MD5 md5 = new MD5CryptoServiceProvider();
                    byte[] pwd = md5.ComputeHash(Encoding.Default.GetBytes(_local.pwd));
                    _local.pwd = BitConverter.ToString(pwd).Replace("-", "").ToLower();
                }

                // cms配置
                token = jsonOjb["cms"];
                if (token != null)
                {
                    _cms = JsonConvert.DeserializeObject<CMS>(token.ToString());
                }

                // 厂商协议
                token = jsonOjb["protocol"];
                if (token != null)
                {
                    _protocol = token.ToString();
                    // 取附加参数
                    var obj = jsonOjb["append"][_protocol];
                    if (obj != null)
                    {
                        _append = obj.ToString();
                    }
                }

                // 设备信息
                token = jsonOjb["devices"];
                if (token != null)
                {
                    var enumDev = JsonConvert.DeserializeObject<IEnumerable<Devices>>(token.ToString());
                    _devices = enumDev.ToDictionary(device => device.display, device => device);
                }

                // DWS配置
                token = jsonOjb["dws"];
                if (token != null)
                {
                    _dws = JsonConvert.DeserializeObject<DWS>(token.ToString());
                }
            }
            catch (Exception ex)
            {
                Logger.Log.ErrorFormat("读取配置信息出错\r\n", ex);
                return false;
            }

            return true;
        }

        private bool ParseRsp(string data)
        {
            try
            {
                do
                {
                    XmlDocument xml = new XmlDocument();
                    xml.LoadXml(data);
                    XmlNode node = xml.SelectSingleNode("Response");

                    // 获取属性
                    if (node.Attributes["Code"].Value != "200")
                    {
                        // 登录失败
                        Logger.Log.ErrorFormat("登录失败，{0}\r\n", node.Attributes["Message"].Value);
                    }

                    XmlNodeList child = node.ChildNodes;
                    foreach (XmlNode item in child)
                    {
                        switch (item.Name)
                        {
                            // 用户Session
                            case "SessionId":
                                {
                                    _session = item.InnerText;
                                    break;
                                }
                            // StandardNumber                                
                            case "User":
                                {
                                    _sn = item.Attributes["StandardNumber"].Value;
                                    break;
                                }
                            // // CCS信息
                            case "CCS":
                                {
                                    XmlNodeList ccs = item.ChildNodes;
                                    foreach (XmlNode item0 in ccs)
                                    {
                                        // ip
                                        if (item0.Name == "IP")
                                        {
                                            _ccs_ip = item0.InnerText;
                                        }
                                        // port
                                        else if (item0.Name == "Port")
                                        {
                                            _ccs_port = Convert.ToInt32(item0.InnerText);
                                        }

                                        continue;
                                    }
                                    break;
                                }
                        }
                        continue;
                    }
                } while (false);
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
            }

            if (_session == string.Empty || _ccs_ip == string.Empty || _ccs_port == 0)
            {
                Logger.Log.ErrorFormat("Session: {0}, CCS IP: {1}, CCS Port: {2}\r\n", _session, _ccs_ip, _ccs_port);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 打开视频
        /// </summary>
        /// <param name="displayNum"></param>
        private void RealPaly(string displayNum)
        {
            // 取绑定摄像头
            if (!_devices.ContainsKey(displayNum) || _devices[displayNum].cameras.Length == 0)
            {
                ShowStatus(string.Format("未找到分机【{0}】绑定信息", displayNum), false);
                Logger.Log.ErrorFormat("分机没有绑定摄像头，{0}", displayNum);
                return;
            }

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                // 分机配置
                var displayCfg = _devices[displayNum];
                var cameras = displayCfg.cameras;

                // 播放实时视频
                // 窗口句柄
                int[] winHandles= new int[] { };
                for (int i = 0; i < row*col; i++)
                {
                    winHandles[i] = LV[i].Child.Handle.ToInt32();
                }

                // 播放视频
                for (int i = 0; i < cameras.Length && i < winHandles.Length; ++i)
                {
                    int status = _ocx.StartLive(cameras[i], winHandles[i], 0);
                    if (status > 0)
                    {
                        // 记录播放属性
                        _stream.Add(new CameraAttr
                        {
                            code = status,
                            camera = cameras[i]
                        });

                        Logger.Log.InfoFormat("请求视频：{0}", cameras[i]);
                    }
                }

                // 启用视频投墙
                if (displayCfg.dws && _dws != null && _dws.monitor.Length != 0)
                {
                    // 监视器通道
                    var monitors = _dws.monitor;
                    // 视频投墙
                    for (int i = 0; i < cameras.Length && i < monitors.Length; ++i)
                    {
                        int status = _ocx.DisplayWallRealPlay(_dws.sn, monitors[i].sn, cameras[i], monitors[i].index);
                        if (status > 0)
                        {
                            // 记录播放属性
                            _stream.Add(new CameraAttr
                            {
                                dws = true,
                                code = status,
                                camera = cameras[i],
                                monitor = monitors[i].sn,
                                index = monitors[i].index
                            });

                            Logger.Log.InfoFormat("视频投墙：camera({0} dws({1}) monitor({2}) index({3})", cameras[i], _dws.sn, monitors[i].sn, monitors[i].index);
                        }
                    }
                }
            }));
        }
        
        /// <summary>
        /// 关闭视频
        /// </summary>
        /// <param name="displayNum"></param>
        private void RealStop(string displayNum = null)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                // 取分机信息
                if (!string.IsNullOrEmpty(displayNum) && !_devices.ContainsKey(displayNum))
                {
                    ShowStatus(string.Format("未找到分机【{0}】绑定信息", displayNum), false);
                    Logger.Log.ErrorFormat("分机没有绑定摄像头，{0}", displayNum);
                    return;
                }

                foreach (var item in _stream)
                {
                    // 关闭视频
                    _ocx.StopStream(item.handle);
                    Logger.Log.InfoFormat("关闭视频：{0}", item.camera);

                    // 视频投墙
                    //if (_devices[displayNum].dws)
                    //{
                    //    _ocx.DisplayWallRealStop(_dws.sn, item.monitor, item.index);
                    //    Logger.Log.InfoFormat("停止投墙：camera({0} dws({1}) monitor({2}) index({3})", item.camera, _dws.sn, item.monitor, item.index);
                    //}
                }

                _stream.Clear();
            }));
        }

        public void MsgHandler(object sender, Msg e)
        {
            Logger.Log.InfoFormat("接收到事件：分机（{0}），事件（{1}）", e.display, e.type);
            switch(e.type)
            {
                case EventType.Call:
                    ShowStatus(string.Format("分机【{0}】呼叫中...", e.display));
                    break;
                case EventType.Connect:
                    ShowStatus(string.Format("分机【{0}】通话中...", e.display));
                    foreach (var item in LV)
                    {
                        item.Visibility = Visibility.Visible;
                    }
                    RealPaly(e.display);
                    break;
                case EventType.HangUp:
                    ShowStatus(string.Format("分机【{0}】已结束通话，等待新的呼叫请求...", e.display));
                    RealStop(e.display);
                    foreach (var item in LV)
                    {
                        item.Visibility = Visibility.Collapsed;
                    }
                    break;
                default:
                    break;
            }
        }
        private void Inforxaml_Hidden(object sender, EventArgs e)
        {
            ifx.txStatus.Visibility = Visibility.Hidden;
        }

        #region windowsMinimize最小化窗口事件
        bool shouldClose;
        WindowState lastWindowState = WindowState.Maximized;
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (!shouldClose)
            {
                e.Cancel = true;
                try
                {

                    //d.Hide();
                    ifx.Visibility=Visibility.Hidden;
                    this.Hide();
                    
                }
                catch
                {
                }

            }
        }

        private void OnNotificationAreaIconDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                Open();
            }
        }

        private void OnMenuItemOpenClick(object sender, EventArgs e)
        {
            Open();
        }

        private void Open()
        {
            try
            {
                
                Show();
                //d.Show();
                ifx.Visibility=Visibility.Visible;
                this.WindowState = lastWindowState;
            }
            catch
            {
            }
        }

        private void OnMenuItemExitClick(object sender, EventArgs e)
        {
            shouldClose = true;
            Close();
        }
        #endregion
    }
}

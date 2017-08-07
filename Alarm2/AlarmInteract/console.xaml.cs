using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SCSV.AlarmInteract
{
    /// <summary>
    /// console.xaml 的交互逻辑
    /// </summary>
    /// 
    public partial class consoleDesk : Window
    {

        public  List<Warn> wl = new List<Warn>();
        //public delegate void childclose();
        //public event childclose closefather;
        public delegate void buttonClick();
        public event buttonClick readbutton;
        public int temp;
        public double ScreenHeight
        {
            get;
            set;
        }
        public double MinScreenHeight
        {
            get;
            set;
        }
        public consoleDesk()
        {
            
            InitializeComponent();
            MinScreenHeight = SystemParameters.PrimaryScreenWidth - 2;
            DmouseLeave.To = MinScreenHeight;
            ScreenHeight = SystemParameters.PrimaryScreenWidth - this.Width;
            DmouseEnter.To = ScreenHeight;
            AddWarn();          
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
           // closefather();
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
        public void AddWarn()
        {
                wl.AddRange( new List<Warn> {
                new Warn() { ExtensionNum = "1", ExtensionStatus = 13, ExtensionTime = DateTime.Now.ToString() },
                new Warn() { ExtensionNum = "2", ExtensionStatus = 10, ExtensionTime = DateTime.Now.ToString() },
                new Warn() { ExtensionNum = "3", ExtensionStatus = 6, ExtensionTime = DateTime.Now.ToString() }
            });
           
            add();
        }
        public void add()
        {
            list_wating.Children.Clear();
            for (int i = 0; i < wl.Count; i++)
            {
                Button t = new Button();
                //t. = TextAlignment.Center;
                t.Opacity = 1;
                t.Height = this.Height / 5;
                t.Width = 260;
                t.FontSize = 20;
                t.MouseDoubleClick += Textbox_Click;
                t.Content = wl[i].ExtensionNum +" " +wl[i].ExtensionTime+ " " + wl[i].ExtensionStatus;
                list_wating.Children.Add(t);
            }
        }
        int ClickTimes=1;//是否处理完成        
        private void Textbox_Click(object sender, RoutedEventArgs e)
        {
            if (ClickTimes==1)
            {
                ((Button)sender).Background = new SolidColorBrush(Color.FromRgb(255,255, 0));
                string[] s = ((Button)sender).Content.ToString().Split(' ');
                string num = s[0];
                int w = Convert.ToInt32(s[2]);
                MainWindow.ShowStatus(num + "的详细信息如下");
                foreach (var item in list_wating.Children)
                {
                    ((Button)item).IsEnabled = false;
                }
                ((Button)sender).IsEnabled = true;

                if (w > 9 && w <=12)
                {
                    MainWindow.col = 4;
                    MainWindow.row = 3;
                }
                if (w <=9)
                {
                    MainWindow.col = 3;
                    MainWindow.row = 3;
                }
                if (w > 12)
                {
                    MainWindow.col = 4;
                    MainWindow.row = 4;
                }
                readbutton();
                ClickTimes = 2;
                return;
            }
            if (ClickTimes == 2)
            {
                MessageBoxResult dr = MessageBox.Show("已经完成报警检查？", "提示", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                if (dr == MessageBoxResult.OK)
                {
                    wl.RemoveAt(list_wating.Children.IndexOf((Button)sender));
                    add();
                    foreach (var item in list_wating.Children)
                    {
                        ((Button)item).IsEnabled = true;
                    }
                    ClickTimes=1;
                    MainWindow.ShowStatus("等待报警处理");
                }
                else
                {
                    return;
                }

                
            }
            //temp = list_wating.SelectedIndex;
            //if (finish)
            //{


            //        MessageBoxResult dr = MessageBox.Show("已经完成报警检查？", "提示", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            //        if (dr == MessageBoxResult.OK)
            //        {


            //            if (temp == -1 || lasttemp == -1)
            //            {

            //            }
            //            else
            //            {
            //                wl.Remove(wl[lasttemp]);
            //                add();
            //                finish = false;
            //            }
            //        }
            //        else
            //        {
            //          return;
            //        }


            //}
            //else
            //{

            //    lasttemp = temp;
            //    finish = true;
            //    ((Button)sender).Background = new SolidColorBrush(Color.FromRgb(240, 0, 0));
            //    string[] s = ((Button)sender).Content.ToString().Split(' ');
            //    string num = s[0];
            //    MainWindow.ShowStatus(num + "正在处理中");
            //}

        }





        #region 
        //private void Button_Click(object sender, RoutedEventArgs e)
        //{
        //   wl.Add(new Warn() { ExtensionNum = "565", ExtensionStatus = "正在处理中", ExtensionTime = DateTime.Now.ToString() });
        //    add();
        //}
        #endregion

        
    }
}































































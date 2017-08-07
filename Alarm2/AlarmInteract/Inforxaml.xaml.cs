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
    /// Inforxaml.xaml 的交互逻辑
    /// </summary>
    public partial class Inforxaml : Window
    {
        public delegate void childclose();
        public event childclose closefather;
       
        public Inforxaml()
        {
            InitializeComponent();
            //txStatus.Text = "平台服务加载中...";


        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            closefather();
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

        private void TextBlock_MouseEnter(object sender, MouseEventArgs e)
        {
            MainWindow.myTimer.Dispose();
            txStatus.Visibility = Visibility.Visible;
        }

        private void txStatus_MouseLeave(object sender, MouseEventArgs e)
        {
            MainWindow.myTimer.Start();
        }
    }
}



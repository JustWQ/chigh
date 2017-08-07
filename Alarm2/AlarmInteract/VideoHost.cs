using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows;

namespace SCSV.AlarmInteract
{
    public static class Utility
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr h, int msg, IntPtr lp, IntPtr wp);

        public static void DragMove(IntPtr hwnd)
        {
            const int WM_SYSCOMMAND = 0x112;
            const int WM_LBUTTONUP = 0x202;

            SendMessage(hwnd, WM_SYSCOMMAND, (IntPtr)0xf012, IntPtr.Zero);
            SendMessage(hwnd, WM_LBUTTONUP, IntPtr.Zero, IntPtr.Zero);
        }
    }

    public class VideoHost : WindowsFormsHost
    {
        public System.Windows.FrameworkElement DragWindow { get; set; }
        public event EventHandler<System.Windows.Forms.MouseEventArgs> MouseDoubleClickEvent;
        private bool _drag;
        private System.Drawing.Point _lastPt;

        public VideoHost()
        {
            ChildChanged += OnChildChanged;
        }

        private void OnChildChanged(object sender, ChildChangedEventArgs childChangedEventArgs)
        {
            var previousChild = childChangedEventArgs.PreviousChild as System.Windows.Forms.Control;
            if (previousChild != null)
            {
                previousChild.MouseDown -= OnMouseDown;
            }
            if (Child != null)
            {
                Child.MouseDown += OnMouseDown;
                Child.MouseUp += Child_MouseUp;
                Child.MouseMove += Child_MouseMove;
                Child.MouseDoubleClick += Child_MouseDoubleClick;
                Child.DragDrop += new System.Windows.Forms.DragEventHandler(Child_DragDrop);
            }
        }

        void Child_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
        {

        }

        private void Child_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (MouseDoubleClickEvent != null)
            {
                MouseDoubleClickEvent(this, e);
            }
        }

        private void Child_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            /*
            MouseButton? wpfButton = ConvertToWpf(e.Button);
            if (!wpfButton.HasValue)
                return;
            
            RaiseEvent(new System.Windows.Input.MouseButtonEventArgs(Mouse.PrimaryDevice, 0, wpfButton.Value)
            {
                RoutedEvent = Mouse.MouseMoveEvent,
                Source = this,
            });
            */

            if (!_drag)
                return;

            if (_lastPt.X == 0 && _lastPt.Y == 0)
                return;

            if (Math.Abs(_lastPt.X - e.Location.X) < 5 && Math.Abs(_lastPt.Y - e.Location.Y) < 5)
                return;
            //if (DragWindow == null)
            //{
            //    DragWindow = ((System.Windows.Forms.Control)sender).Capture = false;
            //}

            if (e.Button == MouseButtons.Left && DragWindow != null)
            {
                // it is necessary to release mouse capture, so thatExtendApplication.Current.DefaultWindow
                // WPF window will be able to capture mouse input
                ((System.Windows.Forms.Control)sender).Capture = false;
                // use helper to acquire window handle
                //var helper = new System.Windows.Interop. WindowInteropHelper(
                //    DragWindow);

                IntPtr hwnd = ((System.Windows.Interop.HwndSource)PresentationSource.FromVisual(DragWindow)).Handle;
                // System.Windows.Interop.HwndSource hwndSource = System.Windows.PresentationSource.FromVisual(DragWindow) as System.Windows.Interop.HwndSource;
                //if (hwndSource != null)
                //{
                System.Drawing.Rectangle fff = System.Windows.Forms.Screen.GetWorkingArea(new System.Drawing.Point(0, 0));

                Utility.DragMove(this.Handle);
                System.Windows.DragDropEffects allowedEffects = System.Windows.DragDropEffects.Move;
                if (System.Windows.DragDrop.DoDragDrop(this, this, allowedEffects) != System.Windows.DragDropEffects.None)
                {

                }

                //}
            }
        }

        private void Child_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            _drag = false;

            MouseButton? wpfButton = ConvertToWpf(e.Button);
            if (!wpfButton.HasValue)
                return;

            RaiseEvent(new System.Windows.Input.MouseButtonEventArgs(Mouse.PrimaryDevice, 0, wpfButton.Value)
            {
                RoutedEvent = Mouse.MouseUpEvent,
                Source = this,
            });
        }

        private void OnMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            _drag = true;
            _lastPt = e.Location;

            MouseButton? wpfButton = ConvertToWpf(e.Button);
            if (!wpfButton.HasValue)
                return;

            RaiseEvent(new System.Windows.Input.MouseButtonEventArgs(Mouse.PrimaryDevice, 0, wpfButton.Value)
            {
                RoutedEvent = Mouse.MouseDownEvent,
                Source = this,
            });

        }

        private MouseButton? ConvertToWpf(System.Windows.Forms.MouseButtons winformButton)
        {
            switch (winformButton)
            {
                case MouseButtons.Left:
                    return MouseButton.Left;
                case MouseButtons.None:
                    return null;
                case MouseButtons.Right:
                    return MouseButton.Right;
                case MouseButtons.Middle:
                    return MouseButton.Middle;
                case MouseButtons.XButton1:
                    return MouseButton.XButton1;
                case MouseButtons.XButton2:
                    return MouseButton.XButton2;
                default:
                    throw new ArgumentOutOfRangeException("winformButton");
            }
        }
    }
}

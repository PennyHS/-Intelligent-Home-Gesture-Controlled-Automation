using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

using System.Runtime.InteropServices; 
using System.Windows.Shapes;
using System.Windows.Ink;
using Leap;
using System.IO.Ports;
using System.Diagnostics;

namespace WpfApplication1
{
    public partial class MainWindow : Window
    {
        Controller leap = new Controller();
        float windowWidth = 1400;
        float windowHeight = 800;
        DrawingAttributes touchIndicator = new DrawingAttributes();
          

         SerialPort _serialPort = new SerialPort("COM8");
       
        public MainWindow()
        {
            InitializeComponent();
            CompositionTarget.Rendering += Update;
            touchIndicator.Width = 30;
            touchIndicator.Height = 30;
            touchIndicator.StylusTip = StylusTip.Ellipse;

            init();
        }

        protected void Update(object sender, EventArgs e)
        {
            paintCanvas.Strokes.Clear();
            windowWidth = (float)this.Width;
            windowHeight = (float)this.Height;

            Leap.Frame frame = leap.Frame();

            InteractionBox interactionBox = leap.Frame().InteractionBox;


            foreach (Pointable pointable in leap.Frame().Pointables)
            {
                Leap.Vector normalizedPosition =
                    interactionBox.NormalizePoint(pointable.StabilizedTipPosition);
                float tx = normalizedPosition.x * windowWidth;
                float ty = windowHeight - normalizedPosition.y * windowHeight;

                int alpha = 255;
                if (pointable.TouchDistance > 0 && pointable.TouchZone != Pointable.Zone.ZONENONE)
                {
                    alpha = 255 - (int)(255 * pointable.TouchDistance);
                    touchIndicator.Color = Color.FromArgb((byte)alpha, 0x0, 0xff, 0x0);
                }
                else if (pointable.TouchDistance <= 0)
                {
                    alpha = -(int)(255 * pointable.TouchDistance);
                    touchIndicator.Color = Color.FromArgb((byte)alpha, 0xff, 0x0, 0x0);
                }
                else
                {
                    alpha = 50;
                    touchIndicator.Color = Color.FromArgb((byte)alpha, 0x0, 0x0, 0xff);
                }
                StylusPoint touchPoint = new StylusPoint(tx, ty);
                StylusPointCollection tips =
                    new StylusPointCollection(new StylusPoint[] { touchPoint });
                Stroke touchStroke = new Stroke(tips, touchIndicator);
                paintCanvas.Strokes.Add(touchStroke);
                MouseHelper.SetCursorPosition((int)tx, (int)ty);

                float distance = pointable.TouchDistance;

                if (distance == -1)
                {
                    sendDoubleClick();

                }

            }
        }

        private void sendDoubleClick()
        {
            (new Thread(new ThreadStart(delegate()
            {
                MouseHelper.MouseEvent(MouseHelper.MouseEventFlags.LeftDown);
                Thread.Sleep(100);
                MouseHelper.MouseEvent(MouseHelper.MouseEventFlags.LeftUp);
                Thread.Sleep(100);
                MouseHelper.MouseEvent(MouseHelper.MouseEventFlags.LeftDown);
                Thread.Sleep(100);
                MouseHelper.MouseEvent(MouseHelper.MouseEventFlags.LeftUp);
            }))).Start();
        }

        private void init()
        {
            _serialPort.BaudRate = 9600;

            try
            {
                if (!_serialPort.IsOpen)
                    _serialPort.Open();
            } 

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            } 
        }

        private void LIGHT_Click(object sender, RoutedEventArgs e)
        {
            if (_serialPort.IsOpen)
                _serialPort.Write("L");
        }

        private void TV_Click(object sender, RoutedEventArgs e)
        {
            if (_serialPort.IsOpen)
                _serialPort.Write("0");
            _serialPort.Close();
            Window1 window1 = new Window1();
            window1.Show();
        }

        private void FAN_Click(object sender, RoutedEventArgs e)
        {
           // Process.Start( "http://www.google.com");
        }

        private void CLOSE_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        }
        }

    class MouseHelper

    {
        [StructLayout(LayoutKind.Sequential)]
        public struct MousePoint
        {
            public int X;
            public int Y;

            public MousePoint(int x, int y)
            {
                X = x;
                Y = y;
            }

        }

        [Flags]
        public enum MouseEventFlags
        {
            LeftDown = 0x00000002,
            LeftUp = 0x00000004,
            MiddleDown = 0x00000020,
            MiddleUp = 0x00000040,
            Move = 0x00000001,
            Absolute = 0x00008000,
            RightDown = 0x00000008,
            RightUp = 0x00000010,
            Wheel = 0x00000800,
            XDown = 0x00000080,
            XUp = 0x00000100
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(long dwFlags, long dx, long dy, long cButtons, long dwExtraInfo);

        [DllImport("user32.dll", EntryPoint = "SetCursorPos")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out MousePoint lpMousePoint);

        public static void SetCursorPosition(int X, int Y)
        {
            SetCursorPos(X, Y);
        }

        public static void SetCursorPosition(MousePoint point)
        {
            SetCursorPos(point.X, point.Y);
        }

        public static MousePoint GetCursorPosition()
        {
            MousePoint currentMousePoint;
            var gotPoint = GetCursorPos(out currentMousePoint);
            if (!gotPoint) { currentMousePoint = new MousePoint(0, 0); }
            return currentMousePoint;
        }

        public static void MouseEvent(MouseEventFlags value)
        {
            MouseEvent(value, 0);
        }

        public static void MouseEvent(MouseEventFlags value, long data)
        {
            MousePoint position = GetCursorPosition();

            mouse_event((int)value, position.X, position.Y, data, 0);
        }
    }

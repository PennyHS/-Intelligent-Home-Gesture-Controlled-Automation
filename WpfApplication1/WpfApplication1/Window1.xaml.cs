using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
 





using System.Windows.Shapes;
using Leap;
using System.IO.Ports;
using System.Diagnostics;
using System.Windows.Ink;

namespace WpfApplication1
{
    public partial class Window1 : Window, ILeapEventDelegate
    { 
        private Controller controller = new Controller();

        private LeapEventListener listener;
       // private Boolean isClosing = false;
        SerialPort _serialPort2 = new SerialPort("COM8");
        public Window1()
        {
            InitializeComponent();
            this.controller = new Controller();
            this.listener = new LeapEventListener(this);
            controller.AddListener(listener);

            initt();
        }

                delegate void LeapEventDelegate(string EventName);
        public void LeapEventNotification(string EventName)
        {
            if (this.CheckAccess())
            {
                switch (EventName)
                {
                    case "onInit":
                        break;
                    case "onConnect":
                        connectHandler();
                        break;
                    case "onFrame":
                        detectGesture(this.controller.Frame());
                        break;
                }
            }
            else
            {
                Dispatcher.Invoke(new LeapEventDelegate(LeapEventNotification), new object[] { EventName });
            }
        }

        private void initt()
        {
            _serialPort2.BaudRate = 38400;

            try
            {
                if (!_serialPort2.IsOpen)
                    _serialPort2.Open();
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void connectHandler()
        {
            this.controller.EnableGesture(Gesture.GestureType.TYPE_CIRCLE);
            this.controller.EnableGesture(Gesture.GestureType.TYPE_KEY_TAP);
            this.controller.EnableGesture(Gesture.GestureType.TYPE_SCREEN_TAP);
            this.controller.EnableGesture(Gesture.GestureType.TYPE_SWIPE);
        }

        public void detectGesture(Leap.Frame frame)
        {
            GestureList gestures = frame.Gestures(); //returns a list of gestures

            for (int i = 0; i < gestures.Count(); i++) //enumrate all the gestures detected in a frame
            { 
                Gesture gesture = gestures[i];

                switch (gesture.Type)
                { 
                    case Gesture.GestureType.TYPE_CIRCLE:
                        richTextBox1.AppendText("circle" + Environment.NewLine);
                        break;
                    case Gesture.GestureType.TYPE_KEY_TAP:
                        //richTextBox1.AppendText("key tap" + Environment.NewLine);
                        
                        break;
                    case Gesture.GestureType.TYPE_SCREEN_TAP:
                        richTextBox1.AppendText("screen tap" + Environment.NewLine);
                        Button_Click(null, new RoutedEventArgs());
                        break;
                    case Gesture.GestureType.TYPE_SWIPE:
                     {
                           // richTextBox1.AppendText("swipe" + Environment.NewLine);
                         SwipeGesture Swipe = new SwipeGesture(gesture);

                         // Compare directions and give preference to the greatest linear movement.
                         float fAbsX = Math.Abs(Swipe.Direction.x);
                         float fAbsY = Math.Abs(Swipe.Direction.y);
                         float fAbsZ = Math.Abs(Swipe.Direction.z);

                         // Was X the greatest?
			if (fAbsX > fAbsY && fAbsX > fAbsZ)
			{

                if (Swipe.Direction.x > 0)
				{
                    richTextBox1.AppendText("Swipe Right" + Environment.NewLine);
                    if (_serialPort2.IsOpen)
                        _serialPort2.Write("1");
                    Task.Delay(10).Wait();
                    
				}
				else
				{
                    richTextBox1.AppendText("Swipe Left" + Environment.NewLine);


                    Task.Delay(10).Wait();
				}
			}
            else if (fAbsY > fAbsX && fAbsY > fAbsZ)
			{
				if (Swipe.Direction.y > 0)
				{
                    richTextBox1.AppendText("Swipe up" + Environment.NewLine);
                    if (_serialPort2.IsOpen)
                        _serialPort2.Write("3");
                    Task.Delay(10).Wait();
				}
				else
				{
                    richTextBox1.AppendText("Swipe down" + Environment.NewLine);
                    if (_serialPort2.IsOpen)
                        _serialPort2.Write("4");
                    Task.Delay(10).Wait();
				}

			}
                            break;

                     } 
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (_serialPort2.IsOpen)
                _serialPort2.Write("0");  
            _serialPort2.Close();
            this.Close();
        }

        private void TileLayoutControl_TileClick(object sender, DevExpress.Xpf.LayoutControl.TileClickEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }
    }
     public interface ILeapEventDelegate
    {
        void LeapEventNotification(string EventName);
    }

    public class LeapEventListener : Listener
    {
        ILeapEventDelegate eventDelegate;

        public LeapEventListener(ILeapEventDelegate delegateObject)
        {
            this.eventDelegate = delegateObject;
        }
        public override void OnInit(Controller controller)
        {
            this.eventDelegate.LeapEventNotification("onInit");
        }
        public override void OnConnect(Controller controller)
        {
            this.eventDelegate.LeapEventNotification("onConnect");
        }
        public override void OnFrame(Controller controller)
        {
            this.eventDelegate.LeapEventNotification("onFrame");
        }
        public override void OnExit(Controller controller)
        {
            this.eventDelegate.LeapEventNotification("onExit");
        }
        public override void OnDisconnect(Controller controller)
        {
            this.eventDelegate.LeapEventNotification("onDisconnect");
        }
    }
}






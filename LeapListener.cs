using Leap;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;

namespace MouseForm
{
    class Cursor
    {
        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y); //cur pos
        [DllImport("user32.dll")]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy,uint cButtons, uint dwExtraInfo);//click
        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;
        private const int APPCOMMAND_VOLUME_MUTE = 0x80000;
        private const int APPCOMMAND_VOLUME_UP = 0xA0000;
        private const int APPCOMMAND_VOLUME_DOWN = 0x90000;
       
        public static void MoveCursor(int x, int y)
        {
            SetCursorPos(x, y);

        }
    }
  class LeapListener : Listener{
      int i = 0;
      int x = 0;
      int y = 0;

      double MouseDown = 0;
        public override void OnInit(Controller cntrlr){
              Console.WriteLine("Initialized");
          }

        public override void OnConnect(Controller cntrlr){
            Console.WriteLine("Connected");
            cntrlr.EnableGesture(Gesture.GestureType.TYPECIRCLE);
            cntrlr.EnableGesture(Gesture.GestureType.TYPESWIPE);
       //   cntrlr.EnableGesture(Gesture.GestureType.TYPESCREENTAP);
       //   cntrlr.EnableGesture(Gesture.GestureType.TYPEKEYTAP);
        }

        public override void OnDisconnect(Controller cntrlr){
            Console.WriteLine("Disconnected");
        }

        public override void OnExit(Controller cntrlr){
            Console.WriteLine("Exited");
        }

        private long currentTime;
        private long previousTime;
        private long timeDifference;
        private long VolTime = 0;
        private long swipeTime = 0;
        const int TipPause = 50000;
        bool oskIsOpened = false;

        public override void OnFrame(Controller cntrlr){

            Frame currentFrame = cntrlr.Frame();

            currentTime = currentFrame.Timestamp;
            timeDifference = currentTime - previousTime;
            
            if (timeDifference > 1000)
            {
                if (!currentFrame.Hands.IsEmpty){ //van kéz az aktuális Frame-ben

                 
                   
                    Hand RightHand = currentFrame.Hands.Rightmost;
                    Hand LeftHand = currentFrame.Hands.Leftmost;
                    Finger finger = RightHand.Fingers.Frontmost;

                    Leap.Screen screen = cntrlr.LocatedScreens.ClosestScreenHit(finger);  
                    if (screen.IsValid)
                    {


                        var tipVel = (int)finger.TipVelocity.Magnitude; 

                        if (tipVel > 25)
                        {
                            var xScreenIntersect = screen.Intersect(finger, true).x;
                            var yScreenIntersect = screen.Intersect(finger, true).y;
                         
                            if (xScreenIntersect.ToString() != "NaN")
                            {
                                int x = (int)(xScreenIntersect * screen.WidthPixels);
                                int y = (int)(screen.HeightPixels - (yScreenIntersect * screen.HeightPixels));
                                if (RightHand.Fingers.Count < 3)
                                {
                                    Cursor.MoveCursor(x, y);
                                }

                                GestureList gestures = currentFrame.Gestures();
                                for (int i = 0; i < gestures.Count; i++)
                                {
                                    Gesture gesture = gestures[i];

                                    switch (gesture.Type)
                                    {
                                        case Gesture.GestureType.TYPECIRCLE:
                                            CircleGesture circle = new CircleGesture(gesture);

                                            String clockwiseness;
                                            if (circle.Pointable.Direction.AngleTo(circle.Normal) <= Math.PI / 4)
                                            {
                                                //Clockwise if angle < 90°
                                                clockwiseness = "clockwise";
                                            }
                                            else
                                            {
                                                clockwiseness = "counterclockwise";
                                            }
                                            if (currentFrame.Hands.Count == 1 && currentFrame.Fingers.Count==1)
                                            {
                                                if (currentTime - VolTime > TipPause)
                                                {
                                                    VolTime = currentTime;
                                                    if (clockwiseness.Equals("clockwise")) { Cursor.keybd_event((byte)Keys.VolumeUp, 0, 0, 0); }
                                                    if (clockwiseness.Equals("counterclockwise")) { Cursor.keybd_event((byte)Keys.VolumeDown, 0, 0, 0); }
                                                }
                                            }
                                            float sweptAngle = 0;

                                           
                                            if (circle.State != Gesture.GestureState.STATESTART)
                                            {
                                                CircleGesture previousUpdate = new CircleGesture(cntrlr.Frame(1).Gesture(circle.Id));
                                                sweptAngle = (circle.Progress - previousUpdate.Progress) * 360;
                                            }

                                                Console.WriteLine("Circle id: " + circle.Id
                                                               + ", " + circle.State
                                                               + ", progress: " + circle.Progress
                                                               + ", radius: " + circle.Radius
                                                               + ", angle: " + sweptAngle
                                                               + ", " + clockwiseness);
                                         break;

                                        case Gesture.GestureType.TYPESWIPE:
                                            if (currentFrame.Fingers.Count >= 5 && currentFrame.Hands.Count == 1)
                                            {
                                                if (currentTime - swipeTime > TipPause)
                                                {
                                                    swipeTime = currentTime;
                                                    SwipeGesture swipe = new SwipeGesture(gesture);
                                                    Console.WriteLine("Swipe id: " + swipe.Id
                                                                   + ", " + swipe.State
                                                                   + ", position: " + swipe.Position
                                                                   + ", direction: " + swipe.Direction
                                                                   + ", speed: " + swipe.Speed);

                                                    if (swipe.Direction.x < 0)
                                                    {
                                                        System.Windows.Forms.SendKeys.SendWait("%{LEFT}"); // alt+leftarrow
                                                        Console.WriteLine("back");
                                                    }
                                                    if (swipe.Direction.x > 0)
                                                    {
                                                        System.Windows.Forms.SendKeys.SendWait("%{RIGHT}"); // alt+rightarrow
                                                        Console.WriteLine("fwd");
                                                    }
                                                }
                                            }

                                            break;
                                        case Gesture.GestureType.TYPEKEYTAP:
                                            KeyTapGesture keytap = new KeyTapGesture(gesture);
                                            Console.WriteLine("Tap id: " + keytap.Id
                                                           + ", " + keytap.State
                                                           + ", position: " + keytap.Position
                                                           + ", direction: " + keytap.Direction);
                                            break;
                                        case Gesture.GestureType.TYPESCREENTAP:
                                            ScreenTapGesture screentap = new ScreenTapGesture(gesture);
                                            Console.WriteLine("Tap id: " + screentap.Id
                                                           + ", " + screentap.State
                                                           + ", position: " + screentap.Position
                                                           + ", direction: " + screentap.Direction);
                                            break;
                                        default:
                                            Console.WriteLine("Unknown gesture");
                                            break;
                                    }
                                }
                            }

                        }

                        if (currentFrame.Hands.Count == 2 && LeftHand.Fingers.Count<=2)
                        {
                            if (Extensions.IsTapping(LeftHand.Fingers.Frontmost))
                            {
                                //Mouse.LeftClick();
                                if ((currentTime - MouseDown)/2 > TipPause)
                                {
                                    MouseDown = currentTime;
                                    Console.WriteLine("Left " + i);
                                    Cursor.mouse_event(0x02 | 0x04, (uint)x, (uint)y, 0, 0);
                                }
                                i = i + 1;
                                return;
                            }

                            if (Extensions.IsTapping(LeftHand.Fingers.Rightmost))
                            {
                                //Mouse.RightClick();
                                if ((currentTime - MouseDown)/2 > TipPause)
                                {
                                    MouseDown = currentTime;
                                    Console.WriteLine("Right " + i);
                                    Cursor.mouse_event(0x08 | 0x10, (uint)x, (uint)y, 0, 0);
                                }
                                i = i + 1;
                                return;
                            }
                        }

                        if (currentFrame.Hands.Count == 2 && LeftHand.Fingers.Count >= 4 && RightHand.Fingers.Count<=2)
                        {
                       
                                //scrl dwn
                                if (LeftHand.Direction.y > 0.5)
                                {

                                    Console.WriteLine("scrl up" + i);

                                    //Cursor.keybd_event(38, 0, 0, 0); // 40 == up arw key
                                   
                                        System.Windows.Forms.SendKeys.SendWait("{UP}");
                                    
                                    i = i + 1;
                                }

                                if (LeftHand.Direction.y < 0)
                                {

                                    Console.WriteLine("scrl down " + i);
                                    //   Cursor.keybd_event(40, 0, 0, 0); // 38 == dwn arw key

                                    System.Windows.Forms.SendKeys.SendWait("{DOWN}");
                                    
                                    i = i + 1;
                                }
                                return;
                          }

                        if (currentFrame.Hands.Count == 2 && LeftHand.Fingers.Count >= 4 && RightHand.Fingers.Count>=4)
                        {
                            if (oskIsOpened == false)
                            {
                                Process.Start(@"C:\Windows\System32\osk.exe");
                                oskIsOpened = true;
                            }
                        }
                        
                    }

                }

                previousTime = currentTime;
            }
        }
    }
}

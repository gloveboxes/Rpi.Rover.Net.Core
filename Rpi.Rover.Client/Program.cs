using System;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Emmellsoft.IoT.Rpi.SenseHat;
using RichardsTech.Sensors;

namespace Rpi.Rover.Client
{
    class Program
    {
        enum Motor
        {
            Stop,
            Forward,
            LeftForward,
            RightForward,
            LeftBackward,
            RightBackward,
            Backward,
            SharpLeft,
            SharpRight
        }

        static MyTcpClient client = new MyTcpClient("rpirover.local");
        static ISenseHat senseHat = SenseHatFactory.GetSenseHat();
        static bool motorRunning = false;  // false = off;

        static void Main(string[] args)
        {
            while (true)
            {
                if (!senseHat.Sensors.ImuSensor.Update())
                {
                    continue;
                }

                if (!senseHat.Sensors.Acceleration.HasValue)
                {
                    continue;
                }

                // if (!senseHat.Joystick.Update())
                // {
                //     // client.Connect(Motor.RightForward.ToString());
                //     continue;
                // }
                // UpdatePosition();

                Image colors = CreateGravityBlobScreen(senseHat.Sensors.Acceleration.Value);

                senseHat.Display.CopyColorsToScreen(colors);

                senseHat.Display.Update();

                SetMotorDirection(senseHat.Sensors.Acceleration.Value);

                Thread.Sleep(50);
            }

            client.Close();
        }

        private static void SetMotorDirection(Vector3 vector)
        {

            if (vector.X < -0.55)
            {
                client.Connect(((int)Motor.SharpLeft).ToString());
                motorRunning = true;
            }
            else if (vector.X > 0.55)
            {
                client.Connect(((int)Motor.SharpRight).ToString());
                motorRunning = true;
            }
            else if (vector.X < -0.25)
            {
                client.Connect(((int)Motor.RightForward).ToString());
                motorRunning = true;
            }
            else if (vector.X > 0.25)
            {
                client.Connect(((int)Motor.LeftForward).ToString());
                motorRunning = true;
            }
            else if (vector.Y < -0.25)
            {
                client.Connect(((int)Motor.Forward).ToString());
                motorRunning = true;
            }
            else if (vector.Y > 0.25)
            {
                client.Connect(((int)Motor.Backward).ToString());
                motorRunning = true;
            }
            else
            {
                client.Connect(((int)Motor.Stop).ToString());
                motorRunning = true;
            }
        }

        private static Image CreateGravityBlobScreen(Vector3 vector)
        {
            double x0 = (vector.X + 1) * 5.5 - 2;
            double y0 = (vector.Y + 1) * 5.5 - 2;
            double distScale = 4;

            var screen = new Image(8, 8);

            bool isUpsideDown = vector.Z < 0;

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    double dx = x0 - x;
                    double dy = y0 - y;

                    double dist = Math.Sqrt(dx * dx + dy * dy) / distScale;
                    if (dist > 1)
                    {
                        dist = 1;
                    }

                    int colorIntensity = (int)Math.Round(255 * (1 - dist));
                    if (colorIntensity > 255)
                    {
                        colorIntensity = 255;
                    }

                    screen[x, y] = isUpsideDown
                        ? Color.FromArgb(255, (byte)colorIntensity, 0, 0)
                        : Color.FromArgb(255, 0, (byte)colorIntensity, 0);
                }
            }

            return screen;
        }

        private static void UpdatePosition()
        {
            bool _lastPressingEnter = false;

            if (senseHat.Joystick.LeftKey == KeyState.Pressed)
            {
                client.Connect(((int)Motor.LeftForward).ToString());
                motorRunning = true;
            }
            else if (senseHat.Joystick.RightKey == KeyState.Pressed)
            {
                client.Connect(((int)Motor.RightForward).ToString());
                motorRunning = true;
            }

            if (senseHat.Joystick.UpKey == KeyState.Pressed)
            {
                client.Connect(((int)Motor.Forward).ToString());
                motorRunning = true;
            }
            else if (senseHat.Joystick.DownKey == KeyState.Pressed)
            {
                client.Connect(((int)Motor.Backward).ToString());
                motorRunning = true;
            }

            // Is the enter (middle) key currently being pressed?
            bool currentPressingEnter = senseHat.Joystick.EnterKey == KeyState.Pressing;

            // Has its state been changed since the last check?
            if (_lastPressingEnter != currentPressingEnter)
            {
                // Remember the current state for the next check.
                _lastPressingEnter = currentPressingEnter;

                if (!motorRunning)
                {
                    client.Connect(((int)Motor.Forward).ToString());
                }
                else
                {
                    client.Connect(((int)Motor.Stop).ToString());
                }

                motorRunning = !motorRunning;

            }
        }
    }
}

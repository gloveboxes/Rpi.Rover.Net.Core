using System;
using System.Device.Gpio;
using System.Diagnostics;
using System.IO;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Net;
using System.Text;


namespace Rpi.listen
{
    class Program
    {
        enum MotorMap : byte
        {
            TwoPlus = 21,
            TwoMinus = 26,
            OnePlus = 19,
            OneMinus = 20
        }

        enum MotorControl
        {
            Stop,
            Forward,
            LeftForward,
            RightForward,
            LeftBackward,
            RightBackward,
            Backward,
            SharpLeft,
            SharpRight,
            ShutDown,
            Unknown
        }

        static RoverServer tcp = new RoverServer();
        static GpioController controller = new GpioController();

        static Motor left = new Motor(controller, (int)MotorMap.TwoPlus, (int)MotorMap.TwoMinus);
        static Motor right = new Motor(controller, (int)MotorMap.OnePlus, (int)MotorMap.OneMinus);

        static MqttClient client = new MqttClient("localhost");


        static void Main(string[] args)
        {
            client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;


            string clientId = Guid.NewGuid().ToString();
            client.Connect(clientId);

            // subscribe to the topic "/home/temperature" with QoS 2 
            client.Subscribe(new string[] { "/rover/motor" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });

            tcp.Listen(roverActions);
        }

        static void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            string action = Encoding.UTF8.GetString(e.Message, 0, e.Message.Length);
            roverActions(action);
        }

        static void roverActions(string action)
        {
            int cmd;

            // Console.WriteLine(action);
            if (int.TryParse(action, out cmd))
            {
                switch ((MotorControl)cmd)
                {
                    case MotorControl.Stop: // stop
                        left.Stop();
                        right.Stop();
                        break;
                    case MotorControl.Forward: // forward
                        left.Forward();
                        right.Forward();
                        break;
                    case MotorControl.LeftForward: // left
                        left.Stop();
                        right.Forward();
                        break;
                    case MotorControl.RightForward: // right
                        left.Forward();
                        right.Stop();
                        break;
                    case MotorControl.LeftBackward: // leftbackward
                        left.Stop();
                        right.Backward();
                        break;
                    case MotorControl.RightBackward: // right backward
                        left.Backward();
                        right.Stop();
                        break;
                    case MotorControl.Backward:
                        left.Backward();
                        right.Backward();
                        break;
                    case MotorControl.SharpLeft: // sharpleft
                        left.Forward();
                        right.Backward();
                        break;
                    case MotorControl.SharpRight: //sharpright
                        left.Backward();
                        right.Forward();
                        break;
                    case MotorControl.ShutDown:
                        ShutDown();
                        break;
                }
            }
        }

        static void ShutDown()
        {
            string result;

            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = @"/bin/bash";
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            start.Arguments = "-c \"sudo halt\"";

            try
            {
                using (Process process = Process.Start(start))
                {
                    using (StreamReader reader = process.StandardOutput)
                    {
                        result = reader.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}

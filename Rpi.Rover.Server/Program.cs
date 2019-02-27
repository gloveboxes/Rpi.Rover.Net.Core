using System;
using System.Device.Gpio;

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

        static Controller tcp = new Controller();
        static GpioController controller = new GpioController();

        static Motor left = new Motor(controller, (int)MotorMap.TwoPlus, (int)MotorMap.TwoMinus);
        static Motor right = new Motor(controller, (int)MotorMap.OnePlus, (int)MotorMap.OneMinus);

        static void Main(string[] args)
        {
            tcp.Listen(roverActions);
        }

        static void roverActions(string action)
        {
            int _action;
            // Console.WriteLine(action);
            if (int.TryParse(action, out _action))
            {
                switch (_action)
                {
                    case 0: // stop
                        left.Stop();
                        right.Stop();
                        break;
                    case 1: // forward
                        left.Forward();
                        right.Forward();
                        break;
                    case 2: // left
                        left.Stop();
                        right.Forward();
                        break;
                    case 3: // right
                        left.Forward();
                        right.Stop();
                        break;
                    case 4: // leftbackward
                        left.Stop();
                        right.Backward();
                        break;
                    case 5: // right backward
                        left.Backward();
                        right.Stop();
                        break;
                    case 6:
                        left.Backward();
                        right.Backward();
                        break;
                    case 7: // sharpleft
                        left.Forward();
                        right.Backward();
                        break;
                    case 8: //sharpright
                        left.Backward();
                        right.Forward();
                        break;

                }
            }
        }
    }
}

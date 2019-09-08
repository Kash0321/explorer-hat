using System;
using System.Threading;
using System.Threading.Tasks;

namespace ExplorerHat.LightingSample
{
    class Program
    {
        const int MOTOR_TIME = 2000;

        static void Main(string[] args)
        {
            using (var hat = new Iot.Device.ExplorerHat.ExplorerHat())
            {
                hat.Lights.Blue.On();
                hat.Lights.Yellow.On();
                hat.Lights.Green.On();
                hat.Motors.Forwards();
                Thread.Sleep(MOTOR_TIME);
                hat.Lights.Blue.Off();
                hat.Lights.Yellow.Off();
                hat.Lights.Green.Off();
                hat.Motors.Stop();

                hat.Lights.Blue.On();
                hat.Lights.Yellow.On();
                hat.Lights.Red.On();
                hat.Motors.Backwards();
                Thread.Sleep(MOTOR_TIME);
                hat.Lights.Blue.Off();
                hat.Lights.Yellow.Off();
                hat.Lights.Red.Off();
                hat.Motors.Stop();

                hat.Lights.Blue.On();
                hat.Lights.Green.On();
                hat.Motors.One.Speed = 0.5d;
                Thread.Sleep(MOTOR_TIME);
                hat.Lights.Blue.Off();
                hat.Lights.Green.Off();
                hat.Motors.Stop();

                hat.Lights.Blue.On();
                hat.Lights.Red.On();
                hat.Motors.One.Speed = -0.5d;
                Thread.Sleep(MOTOR_TIME);
                hat.Lights.Blue.Off();
                hat.Lights.Red.Off();
                hat.Motors.Stop();

                hat.Lights.Yellow.On();
                hat.Lights.Green.On();
                hat.Motors.Two.Speed = 0.5d;
                Thread.Sleep(MOTOR_TIME);
                hat.Lights.Yellow.Off();
                hat.Lights.Green.Off();
                hat.Motors.Stop();

                hat.Lights.Yellow.On();
                hat.Lights.Red.On();
                hat.Motors.Two.Speed = -0.5d;
                Thread.Sleep(MOTOR_TIME);
                hat.Lights.Yellow.Off();
                hat.Lights.Red.Off();
                hat.Motors.Stop();
            }
        }
    }
}

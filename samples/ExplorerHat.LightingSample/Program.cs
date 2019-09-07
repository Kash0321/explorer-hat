using System;
using System.Threading;
using System.Threading.Tasks;

namespace ExplorerHat.LightingSample
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var hat = new Iot.Device.ExplorerHat.ExplorerHat())
            {
                Task.Run(() =>
                {
                    while (true)
                    {
                        hat.Lights.Blue.On();
                        Thread.Sleep(500);
                        hat.Lights.Blue.Off();
                        Thread.Sleep(200);
                    }
                });

                while (true)
                {
                    hat.Motors.Forwards(0.50d);
                    Thread.Sleep(10000);
                    hat.Motors.Stop();


                    hat.Motors.Backwards();
                    Thread.Sleep(10000);
                    hat.Motors.Stop();
                }
            }
        }
    }
}

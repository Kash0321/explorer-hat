using System;
using System.Threading;

namespace ExplorerHat.LightingSample
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var hat = new Iot.Device.ExplorerHat.ExplorerHat())
            {
                while (true)
                {
                    hat.Light.Blue.On();
                    Thread.Sleep(500);
                    hat.Light.Blue.Off();
                    Thread.Sleep(200);
                }
            }
        }
    }
}

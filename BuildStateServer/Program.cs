using System;

namespace BuildStateServer
{
    internal class Program
    {
        private static void Main(string[] args)
        {

            try
            {
                Console.WriteLine("Initializing Build State Monitor");
                ServerApplication.Start();
                Console.ReadLine();
            }
            catch (Exception exception)
            {
                Console.WriteLine("Build State Monitor encountered an exception" + exception);
                Console.ReadLine();
            }

            ServerApplication.Stop();

            
        }
    }
}
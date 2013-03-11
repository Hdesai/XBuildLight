using System;

namespace BuildClient
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Initializing Build Client");
            ClientApplication.Start();
            Console.WriteLine("Build Client is running");
            Console.ReadLine();
            ClientApplication.Stop();

        }
    }
}
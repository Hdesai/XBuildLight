using System;
using System.Threading;

namespace BuildClient
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Initializing Build Client");
            var client=new ClientApplication();
            client.Start(); 
            Console.WriteLine("Build Client is running");
            Console.ReadLine();
            client.Stop();

        }
    }
}
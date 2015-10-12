using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildClient
{
    public class BinaryConsoleTraceListener : ConsoleTraceListener
    {
        public override void WriteLine(string message)
        {
            byte[] arr = System.Text.Encoding.ASCII.GetBytes(message);
            var sb = new System.Text.StringBuilder();
            foreach (byte b in arr)
            {
                sb.Append(Convert.ToString((int) b, 2));
            }
            string binary = sb.ToString();
            base.WriteLine(binary);
        }
    }
}
using System;
using System.Text;

namespace PCController.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine("helloworld");
            UDPManager happy = new UDPManager();
            var happy2 = happy.GetUDPManager("127.0.0.1", 5000, 660);

            string message = "happy\r";

            byte[] inputBytes = Encoding.ASCII.GetBytes(message); // new byte array and feed it the input string
            happy2.SendMessage(inputBytes);
        }
    }
}

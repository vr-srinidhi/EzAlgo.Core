using System;
using KiteConnect;

namespace ExAlgo.Core.AccessGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            Kite kite = new Kite("fm1sxbj5od62i9z5", Debug: true);
            kite.GetLoginURL();
            var user = kite.GenerateSession("jQcJN8isackRDxbjGykiBRWOfZFhVPjc", "u58eyhqq0wwm2jgpx9wm0c3l8f6h28k4");
            System.Console.WriteLine(user.AccessToken);
        }
    }
}

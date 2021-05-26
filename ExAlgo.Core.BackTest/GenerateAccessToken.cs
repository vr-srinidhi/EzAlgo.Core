using System;
using KiteConnect;
using NUnit.Framework;

namespace ExAlgo.Core.BackTest
{
    public class GenerateAccessToken
    {
        [Test]
        public void GenerateAcessToken()
        {
            Kite kite = new Kite("fm1sxbj5od62i9z5", Debug: true);
            kite.GetLoginURL();
            var user = kite.GenerateSession("vfacCZOMVNHoLpmHK5HoY7ksDymlGWCt", "u58eyhqq0wwm2jgpx9wm0c3l8f6h28k4");
            System.Console.WriteLine(user.AccessToken);
        }
    }
}

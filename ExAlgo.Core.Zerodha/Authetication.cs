using System;
using KiteConnect;

namespace ExAlgo.Core.Zerodha
{
    public class Authetication : IAuthetication
    {
        private readonly IConfiguration _configuration;

        public Authetication(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public Kite Authorize()
        {
            Kite kite = new Kite(_configuration.ZerodhaApikey, Debug: true);
            kite.SetAccessToken(_configuration.ZerodhaAccessToken);
            return kite;
        }

    }

    public interface IAuthetication
    {
        Kite Authorize();
    }
}

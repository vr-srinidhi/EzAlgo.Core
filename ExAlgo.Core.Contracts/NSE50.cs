using System;
using System.Collections.Generic;

namespace ExAlgo.Core.Contracts
{
    public static class NSE
    {
        private static Dictionary<string, string> _nse50;
        private static Dictionary<string, string> _nseIndices;

        public static Dictionary<string, string> NationalStockExchange50 => _nse50;
        public static Dictionary<string, string> NseIndices => _nseIndices;

        static NSE()
        {
            _nse50 = new Dictionary<string, string>();
            _nse50.Add("3861249", "ADANIPORTS");
            _nse50.Add("60417", "ASIANPAINT");
            _nse50.Add("1510401", "AXISBANK");
            _nse50.Add("4267265", "BAJAJ-AUTO");
            _nse50.Add("4268801", "BAJAJFINSV");
            _nse50.Add("81153", "BAJFINANCE");
            _nse50.Add("2714625", "BHARTIARTL");
            _nse50.Add("134657", "BPCL");
            _nse50.Add("140033", "BRITANNIA");
            _nse50.Add("177665", "CIPLA");
            _nse50.Add("5215745", "COALINDIA");
            _nse50.Add("2800641", "DIVISLAB");
            _nse50.Add("225537", "DRREDDY");
            _nse50.Add("232961", "EICHERMOT");
            _nse50.Add("1207553", "GAIL");
            _nse50.Add("315393", "GRASIM");
            _nse50.Add("1850625", "HCLTECH");
            _nse50.Add("340481", "HDFC");
            _nse50.Add("341249", "HDFCBANK");
            _nse50.Add("119553", "HDFCLIFE");
            _nse50.Add("345089", "HEROMOTOCO");
            _nse50.Add("348929", "HINDALCO");
            _nse50.Add("356865", "HINDUNILVR");
            _nse50.Add("1270529", "ICICIBANK");
            _nse50.Add("1346049", "INDUSINDBK");
            _nse50.Add("408065", "INFY");
            _nse50.Add("415745", "IOC");
            _nse50.Add("424961", "ITC");
            _nse50.Add("3001089", "JSWSTEEL");
            _nse50.Add("492033", "KOTAKBANK");
            _nse50.Add("2939649", "LT");
            _nse50.Add("519937", "M&M");
            _nse50.Add("2815745", "MARUTI");
            _nse50.Add("4598529", "NESTLEIND");
            _nse50.Add("2977281", "NTPC");
            _nse50.Add("633601", "ONGC");
            _nse50.Add("3834113", "POWERGRID");
            _nse50.Add("738561", "RELIANCE");
            _nse50.Add("5582849", "SBILIFE");
            _nse50.Add("779521", "SBIN");
            _nse50.Add("794369", "SHREECEM");
            _nse50.Add("857857", "SUNPHARMA");
            _nse50.Add("884737", "TATAMOTORS");
            _nse50.Add("895745", "TATASTEEL");
            _nse50.Add("2953217", "TCS");
            _nse50.Add("3465729", "TECHM");
            _nse50.Add("897537", "TITAN");
            _nse50.Add("2952193", "ULTRACEMCO");
            _nse50.Add("2889473", "UPL");
            _nse50.Add("969473", "WIPRO");


            _nseIndices = new Dictionary<string, string>();
            _nseIndices.Add("256265", "NIFTY50");
            _nseIndices.Add("260105", "NIFTYBANK");
            _nseIndices.Add("261641", "NIFTYENERGY");
            _nseIndices.Add("257801", "NIFTYFINSERVICE");
            _nseIndices.Add("261897", "NIFTYFMCG");
            _nseIndices.Add("261385", "NIFTYINFRA");
            _nseIndices.Add("259849", "NIFTYIT");
            _nseIndices.Add("263945", "NIFTYMEDIA");
            _nseIndices.Add("263689", "NIFTYMETAL");
            _nseIndices.Add("262409", "NIFTYPHARMA");
            


        }
    }
}

using System;
using System.Configuration;

namespace ExAlgo.Core.Zerodha
{
    public class Configuration : IConfiguration
    {
        public string ZerodhaUserId => "017957";
        public string ZerodhaPass => "shri@19871!";
        public string ZerodhaPin => "123456";
        public string ZerodhaApikey => "fm1sxbj5od62i9z5";
        public string ZerodhaApiSecret => "u58eyhqq0wwm2jgpx9wm0c3l8f6h28k4";
        public string ZerodhaRequestToken => "1rdNgiUF0bL79887KtqJ7U5gpVXGJ7cV";
        public string ZerodhaAccessToken => "NjXZlZevmqh4IXK3JLwknCV90WFtLNOf";
        public string ZerodhaPublicToken => "NjXZlZevmqh4IXK3JLwknCV90WFtLNOf";
    }

    public interface IConfiguration
    {
        string ZerodhaUserId { get; }
        string ZerodhaPass { get; }
        string ZerodhaPin { get; }
        string ZerodhaApikey { get; }
        string ZerodhaApiSecret { get; }
        string ZerodhaRequestToken { get; }
        string ZerodhaAccessToken { get; }
        string ZerodhaPublicToken { get; }
    }
}


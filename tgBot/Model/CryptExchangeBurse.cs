using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace myApi.Model
{
    public class CryptExchangeBurse
    {
        public string name { get; set; }
        public List<tickers> tickers { get; set; }
    }
    public class tickers
    {
        
        public market market { get; set; }
        public converted_last converted_Last { get; set; }

    }
    public class market 
    {
        public string name { get; set; }
        public string identifier { get; set; }
    }
    public class converted_last
    {
        public double btc { get; set; }
        public double usd { get; set; }
    }
}

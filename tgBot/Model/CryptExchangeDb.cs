using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace myApi.Model
{
    public class CryptExchangeDb
    {
        public string ID { get; set; }
        public string Exchange { get; set; }
    }
   
}

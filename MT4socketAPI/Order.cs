using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTsocketAPI.MT4
{
    public class Order
    {
        public long TICKET { get; set; }
        public int MAGIC { get; set; }
        public string SYMBOL { get; set; }
        public double LOTS { get; set; }
        public string TYPE { get; set; }
        public double PRICE_OPEN { get; set; }
        public string OPEN_TIME { get; set; }
        public double STOP_LOSS { get; set; }
        public double SWAP { get; set; }
        public double COMMISSION { get; set; }
        public double TAKE_PROFIT { get; set; }
        public double PROFIT { get; set; }
        public string COMMENT { get; set; }
        public string EXPIRATION { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}

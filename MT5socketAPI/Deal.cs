using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTsocketAPI.MT5
{
    public class Deal
    {
        public string TIME { get; set; }
        public long DEAL { get; set; }
        public string SYMBOL { get; set; }
        public long ORDER { get; set; }
        public long POSITION { get; set; }
        public string TYPE { get; set; }
        public string REASON { get; set; }
        public string DIRECTION { get; set; }
        public double PRICE { get; set; }
        public double VOLUME { get; set; }
        public double SL { get; set; }
        public double TP { get; set; }
        public double COMMISSION { get; set; }
        public double PROFIT { get; set; }
        public int MAGIC { get; set; }
        public string COMMENT { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

    }

    public class OrderDeal
    {
        public string TIME { get; set; }
        public long TICKET { get; set; }
        public string SYMBOL { get; set; }
        public string TYPE { get; set; }
        public double VOLUME { get; set; }
        public double PRICE { get; set; }
        public int MAGIC { get; set; }
        public string COMMENT { get; set; }
        public List<Deal> DEALS { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}

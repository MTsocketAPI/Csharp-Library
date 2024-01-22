using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTsocketAPI.MT4
{
    public class Quote
    {
        public string SYMBOL { get; set; }
        public double ASK { get; set; }
        public double BID { get; set; }
        public int FLAGS { get; set; }
        public string TIME { get; set; }
        public int VOLUME { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

    }

    public class OHLC_Msg
    {
        //public string MSG { get; set; }
        public string SYMBOL { get; set; }
        public string PERIOD { get; set; }
        public List<Rates> OHLC { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}

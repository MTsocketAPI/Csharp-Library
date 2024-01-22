using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTsocketAPI.MT4
{
    public class Asset
    {
        //public string MSG { get; set; }
        public string NAME { get; set; }
        public string TIME { get; set; }
        public int DIGITS { get; set; }
        public int SPREAD_FLOAT { get; set; }
        public int SPREAD { get; set; }
        public int TRADE_CALC_MODE { get; set; }
        public int TRADE_MODE { get; set; }
        public int START_TIME { get; set; }
        public int EXPIRATION_TIME { get; set; }
        public int TRADE_STOPS_LEVEL { get; set; }
        public int TRADE_FREEZE_LEVEL { get; set; }
        public int TRADE_EXEMODE { get; set; }
        public int SWAP_MODE { get; set; }
        public int SWAP_ROLLOVER3DAYS { get; set; }
        public double POINT { get; set; }
        public double TRADE_TICK_SIZE { get; set; }
        public double TRADE_CONTRACT_SIZE { get; set; }
        public double VOLUME_MIN { get; set; }
        public double VOLUME_MAX { get; set; }
        public double VOLUME_STEP { get; set; }
        public double SWAP_LONG { get; set; }
        public double SWAP_SHORT { get; set; }
        public double MARGIN_INITIAL { get; set; }
        public double MARGIN_MAINTENANCE { get; set; }
        public string CURRENCY_BASE { get; set; }
        public string CURRENCY_PROFIT { get; set; }
        public string CURRENCY_MARGIN { get; set; }
        public string DESCRIPTION { get; set; }
        public string PATH { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}

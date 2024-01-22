using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTsocketAPI.MT4
{
    public class AccountStatus
    {
        public string COMPANY { get; set; }
        public string CURRENCY { get; set; }
        public string NAME { get; set; }
        public string SERVER { get; set; }
        public int LOGIN { get; set; }
        public int TRADE_MODE { get; set; }
        public int LEVERAGE { get; set; }
        public int LIMIT_ORDERS { get; set; }
        public int MARGIN_SO_MODE { get; set; }
        public int TRADE_ALLOWED { get; set; }
        public int TRADE_EXPERT { get; set; }
        public double BALANCE { get; set; }
        public double CREDIT { get; set; }
        public double PROFIT { get; set; }
        public double EQUITY { get; set; }
        public double MARGIN { get; set; }
        public double MARGIN_FREE { get; set; }
        public double MARGIN_LEVEL { get; set; }
        public double MARGIN_SO_CAL { get; set; }
        public double MARGIN_SO_SO { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}

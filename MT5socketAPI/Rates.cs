using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTsocketAPI.MT5
{
    public class Rates
    {
        public string TIME { get; set; }
        public double OPEN { get; set; }
        public double HIGH { get; set; }
        public double LOW { get; set; }
        public double CLOSE { get; set; }
        public int TICK_VOLUME { get; set; }
        public int SPREAD { get; set; }
        public int REAL_VOLUME { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

    }

	public class MarketBook
	{
		public double PRICE { get; set; }
		public int VOLUME { get; set; }
		public double VOLUMEREAL { get; set; }
		public string TYPE { get; set; }
	}

	public class MarketDepth
	{
		//public string MSG { get; set; }
		public string SYMBOL { get; set; }
		public List<MarketBook> MARKET_BOOK { get; set; }
		public override string ToString()
		{
			return JsonConvert.SerializeObject(this);
		}
	}
}

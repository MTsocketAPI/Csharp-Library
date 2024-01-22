using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTsocketAPI.MT4
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
}

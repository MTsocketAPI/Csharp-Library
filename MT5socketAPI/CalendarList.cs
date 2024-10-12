using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTsocketAPI.MT5
{
	// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
	public class Event
	{
		public string TIME { get; set; }
		public int EVENT_COUNTRY_ID { get; set; }
		public int EVENT_DIGITS { get; set; }
		public string EVENT_CODE { get; set; }
		public string EVENT_FREQUENCY { get; set; }
		public int EVENT_ID { get; set; }
		public string EVENT_IMPORTANCE { get; set; }
		public string EVENT_MULTIPLIER { get; set; }
		public string EVENT_NAME { get; set; }
		public string EVENT_SECTOR { get; set; }
		public string EVENT_SOURCE_URL { get; set; }
		public string EVENT_TIME_MODE { get; set; }
		public string EVENT_TYPE { get; set; }
		public string EVENT_UNIT { get; set; }
		public double ACTUAL_VALUE { get; set; }
		public double FORECAST_VALUE { get; set; }
		public double PREVIOUS_VALUE { get; set; }
		public string IMPACT_TYPE { get; set; }
		public int REVISION { get; set; }
		public string PERIOD { get; set; }
		public double? REVISED_VALUE { get; set; }
	}

	public class CalendarList
	{
		public string MSG { get; set; }
		public List<Event> EVENTS { get; set; }
		public int ERROR_ID { get; set; }
		public string ERROR_DESCRIPTION { get; set; }
	}
}

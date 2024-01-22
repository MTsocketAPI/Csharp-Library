using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTsocketAPI.MT4
{
    public  class TerminalInfo
    {
        //public string MSG { get; set; }
        public string LANGUAGE { get; set; }
        public string COMPANY { get; set; }
        public string NAME { get; set; }
        public string PATH { get; set; }
        public string DATA_PATH { get; set; }
        public string COMMONDATA_PATH { get; set; }
        public int BUILD { get; set; }
        public int COMMUNITY_ACCOUNT { get; set; }
        public int COMMUNITY_CONNECTION { get; set; }
        public int CONNECTED { get; set; }
        public int DLLS_ALLOWED { get; set; }
        public int TRADE_ALLOWED { get; set; }
        public int EMAIL_ENABLED { get; set; }
        public int FTP_ENABLED { get; set; }
        public int NOTIFICATIONS_ENABLED { get; set; }
        public int MAXBARS { get; set; }
        public int MQID { get; set; }
        public int CODEPAGE { get; set; }
        public int CPU_CORES { get; set; }
        public int DISK_SPACE { get; set; }
        public int MEMORY_PHYSICAL { get; set; }
        public int MEMORY_TOTAL { get; set; }
        public int MEMORY_AVAILABLE { get; set; }
        public int MEMORY_USED { get; set; }
        public int SCREEN_DPI { get; set; }
        public int PING_LAST { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}

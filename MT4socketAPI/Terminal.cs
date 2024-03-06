using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Text;

namespace MTsocketAPI.MT4
{
    public enum OrderType
    {
        ORDER_TYPE_BUY,
        ORDER_TYPE_SELL,
        ORDER_TYPE_BUY_LIMIT,
        ORDER_TYPE_SELL_LIMIT,
        ORDER_TYPE_BUY_STOP,
        ORDER_TYPE_SELL_STOP
    }

    public enum TimeFrame
    {
        PERIOD_M1,
        PERIOD_M5,
        PERIOD_M15,
        PERIOD_M30,
        PERIOD_H1,
        PERIOD_H4,
        PERIOD_D1,
        PERIOD_W1,
        PERIOD_MN1
    }

    public enum MA_Method
    {
        MODE_SMA,
        MODE_EMA,
        MODE_SMMA,
        MODE_LWMA
    }

    public enum Applied_Price
    {
        PRICE_CLOSE,
        PRICE_OPEN,
        PRICE_HIGH,
        PRICE_LOW,
        PRICE_MEDIAN,
        PRICE_TYPICAL,
        PRICE_WEIGHTED
    }
    public class Terminal
    {
        public string host = "localhost";
        public int cmd_port = 77;
        public int data_port = 78;
        static int bufferLen = 8192;

        //public int Product = 0;

        TcpClient tcpClient_cmd;
        TcpClient tcpClient_data;

        public event EventHandler OnConnect;
        public event EventHandler OnDisconnect;
        public event EventHandler<Quote> OnPrice;
        public event EventHandler<OHLC_Msg> OnOHLC;
        
        public string Version { get; set; }

        public JObject SendCommand(JObject cmd)
        {
            JObject jresult;

            string responseData = string.Empty;

            try
            {
                byte[] data = Encoding.ASCII.GetBytes(cmd.ToString(Formatting.None) + "\r\n");

                NetworkStream stream = tcpClient_cmd.GetStream();

                stream.Write(data, 0, data.Length);

                data = new byte[bufferLen];

                int bytes;
                do
                {
                    bytes = stream.Read(data, 0, bufferLen);
                    responseData += Encoding.ASCII.GetString(data, 0, bytes);
                } while (stream.DataAvailable);

                jresult = JsonConvert.DeserializeObject<JObject>(responseData);

                //stream.Close();
                //tcpClient_cmd.Close();

                return jresult;
            }
            catch (Exception ex)
            {
                throw new FormatException(ex.Message);
            }
        }

        private void ListenMTData()
        {
            Thread listen = new Thread(() => ListenMTDataStream());
            listen.IsBackground = true;
            listen.Start();
        }

        private void ListenMTDataStream()
        {
            int bytes;
            byte[] data = new byte[bufferLen];

            NetworkStream stream = tcpClient_data.GetStream();

            do
            {
                string responseData = string.Empty;

                do
                {
                    bytes = stream.Read(data, 0, data.Length);
                    responseData += Encoding.ASCII.GetString(data, 0, bytes);
                } while (stream.DataAvailable);

                responseData.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList().ForEach(
                    line => {
                    JObject jresult = JObject.Parse(line);
                    if (jresult["MSG"].ToString() == "TRACK_PRICES")
                    {
                        Quote price = JsonConvert.DeserializeObject<Quote>(line);
                        if (OnPrice != null) OnPrice(this, price);
                    }
                    else if (jresult["MSG"].ToString() == "TRACK_OHLC")
                    {
                        OHLC_Msg price = JsonConvert.DeserializeObject<OHLC_Msg>(line);
                        if (OnOHLC != null) OnOHLC(this, price);
                    }
                });
            } while (true);
        }

        public bool Connect(string host = "localhost", int cmd_port = 77, int data_port = 78)
        {
            try
            {
                tcpClient_cmd = new TcpClient(host, cmd_port);
                tcpClient_data = new TcpClient(host, data_port);

                ListenMTData();

                JObject json_cmd = new JObject();
                json_cmd["MSG"] = "VERSION";
                JObject res = SendCommand(json_cmd);

                if (res["ERROR_ID"].ToString() == "0")
                {
                    Version = res["NUMBER"].ToString();

                    if (Convert.ToDouble(Version) < 4.1)
                    {
                        throw new Exception("This API version needs at least MTsocketAPI 4.1 version");
                    }
                }
                else
                {
                    throw new Exception("Error with the command sent. ERROR_ID: " + res["ERROR_ID"] + " ERROR_DESCRIPTION: " + res["ERROR_DESCRIPTION"]);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            if (OnConnect != null) OnConnect(this, new EventArgs());
            return true;
        }

        public TerminalInfo GetTerminalInfo()
        {
            try
            {
                JObject json_cmd = new JObject();
                json_cmd["MSG"] = "TERMINAL_INFO";
                JObject res = SendCommand(json_cmd);

                if (res["ERROR_ID"].ToString() == "0")
                {
                    return JsonConvert.DeserializeObject<TerminalInfo>(res.ToString());
                }
                else
                {
                    throw new Exception("Error with the command sent. ERROR_ID: " + res["ERROR_ID"] + " ERROR_DESCRIPTION: " + res["ERROR_DESCRIPTION"]);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public AccountStatus GetAccountStatus()
        {
            try
            {
                JObject json_cmd = new JObject();
                json_cmd["MSG"] = "ACCOUNT_STATUS";
                JObject res = SendCommand(json_cmd);

                if (res["ERROR_ID"].ToString() == "0")
                {
                    return JsonConvert.DeserializeObject<AccountStatus>(res.ToString());
                }
                else
                {
                    throw new Exception("Error with the command sent. ERROR_ID: " + res["ERROR_ID"] + " ERROR_DESCRIPTION: " + res["ERROR_DESCRIPTION"]);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<Order> GetOrderList()
        {
            try
            {
                JObject json_cmd = new JObject();
                json_cmd["MSG"] = "ORDER_LIST";
                JObject res = SendCommand(json_cmd);

                if (res["ERROR_ID"].ToString() == "0")
                {
                    return JsonConvert.DeserializeObject<List<Order>>(res["TRADES"].ToString());
                }
                else
                {
                    throw new Exception("Error with the command sent. ERROR_ID: " + res["ERROR_ID"] + " ERROR_DESCRIPTION: " + res["ERROR_DESCRIPTION"]);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public double MA_Indicator(string Symbol, TimeFrame tf, int MA_Period, int MA_Shift, MA_Method MA_Method, Applied_Price Applied_Price, int Shift)
        {
            try
            {
                JObject json_cmd = new JObject();
                json_cmd["MSG"] = "MA_INDICATOR";
                json_cmd["SYMBOL"] = Symbol;
                json_cmd["TIMEFRAME"] = tf.ToString();
                json_cmd["MA_PERIOD"] = MA_Period;
                json_cmd["MA_SHIFT"] = MA_Shift;
                json_cmd["MA_METHOD"] = MA_Method.ToString();
                json_cmd["APPLIED_PRICE"] = Applied_Price.ToString();
                json_cmd["SHIFT"] = Shift;

                JObject res = SendCommand(json_cmd);

                if (res["ERROR_ID"].ToString() == "0")
                {
                    return Convert.ToDouble(res["DATA_VALUE"]);
                }
                else
                {
                    throw new Exception("Error with the command sent. ERROR_ID: " + res["ERROR_ID"] + " ERROR_DESCRIPTION: " + res["ERROR_DESCRIPTION"]);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public double ATR_Indicator(string Symbol, TimeFrame tf, int Period, int Shift)
        {
            try
            {
                JObject json_cmd = new JObject();
                json_cmd["MSG"] = "ATR_INDICATOR";
                json_cmd["SYMBOL"] = Symbol;
                json_cmd["TIMEFRAME"] = tf.ToString();
                json_cmd["PERIOD"] = Period;
                json_cmd["SHIFT"] = Shift;

                JObject res = SendCommand(json_cmd);

                if (res["ERROR_ID"].ToString() == "0")
                {
                    return Convert.ToDouble(res["DATA_VALUE"]);
                }
                else
                {
                    throw new Exception("Error with the command sent. ERROR_ID: " + res["ERROR_ID"] + " ERROR_DESCRIPTION: " + res["ERROR_DESCRIPTION"]);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public double Custom_Indicator(string Symbol, TimeFrame tf, string Indicator_Name, int Mode, int Shift, List<double> Param = null)
        {
            try
            {
                JObject json_cmd = new JObject();
                json_cmd["MSG"] = "CUSTOM_INDICATOR";
                json_cmd["SYMBOL"] = Symbol;
                json_cmd["TIMEFRAME"] = tf.ToString();
                json_cmd["INDICATOR_NAME"] = Indicator_Name;
                json_cmd["MODE"] = Mode;
                json_cmd["SHIFT"] = Shift;

                if (Param != null)
                {
                    switch (Param.Count)
                    {
                        case 4:
                            json_cmd["PARAM1"] = Convert.ToDouble(Param[0]);
                            json_cmd["PARAM2"] = Convert.ToDouble(Param[1]);
                            json_cmd["PARAM3"] = Convert.ToDouble(Param[2]);
                            json_cmd["PARAM4"] = Convert.ToDouble(Param[3]);
                            break;
                        case 3:
                            json_cmd["PARAM1"] = Convert.ToDouble(Param[0]);
                            json_cmd["PARAM2"] = Convert.ToDouble(Param[1]);
                            json_cmd["PARAM3"] = Convert.ToDouble(Param[2]);
                            break;
                        case 2:
                            json_cmd["PARAM1"] = Convert.ToDouble(Param[0]);
                            json_cmd["PARAM2"] = Convert.ToDouble(Param[1]);
                            break;
                        case 1:
                            json_cmd["PARAM1"] = Convert.ToDouble(Param[0]);
                            break;
                    }
                }

                JObject res = SendCommand(json_cmd);

                if (res["ERROR_ID"].ToString() == "0")
                {
                    return Convert.ToDouble(res["DATA_VALUE"]);
                }
                else
                {
                    throw new Exception("Error with the command sent. ERROR_ID: " + res["ERROR_ID"] + " ERROR_DESCRIPTION: " + res["ERROR_DESCRIPTION"]);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public long SendOrder(string Symbol, double Volume, OrderType Type, double Price = 0, double SL = 0, double TP = 0, double Slippage = 0, string Comment = "", int MagicNr = 0, string Expiration = "1970/01/01 00:00")
        {
            try
            {
                JObject json_cmd = new JObject();
                json_cmd["MSG"] = "ORDER_SEND";
                json_cmd["SYMBOL"] = Symbol;
                json_cmd["VOLUME"] = Volume;
                json_cmd["TYPE"] = Type.ToString();
                if (SL > 0) json_cmd["SL"] = SL;
                if (TP > 0) json_cmd["TP"] = TP;
                if (Price > 0) json_cmd["PRICE"] = Price;
                if (Slippage > 0) json_cmd["SLIPPAGE"] = Slippage;
                if (Comment != "") json_cmd["COMMENT"] = Comment;
                if (MagicNr > 0) json_cmd["MAGICNR"] = MagicNr;
                if (Expiration != "1970/01/01 00:00") json_cmd["EXPIRATION"] = Expiration;

                JObject res = SendCommand(json_cmd);

                if (res["ERROR_ID"].ToString() == "0")
                {
                    return Convert.ToInt32(res["TICKET"].ToString());
                }
                else
                {
                    throw new Exception("Error with the command sent. ERROR_ID: " + res["ERROR_ID"] + " ERROR_DESCRIPTION: " + res["ERROR_DESCRIPTION"]);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public bool OrderModify(long Ticket, double SL = 0, double TP = 0)
        {
            try
            {
                JObject json_cmd = new JObject();
                json_cmd["MSG"] = "ORDER_MODIFY";
                json_cmd["TICKET"] = Ticket;
                if (SL > 0) json_cmd["SL"] = SL;
                if (TP != 0) json_cmd["TP"] = TP;

                JObject res = SendCommand(json_cmd);

                if (res["ERROR_ID"].ToString() == "0")
                {
                    return true;
                }
                else
                {
                    throw new Exception("Error with the command sent. ERROR_ID: " + res["ERROR_ID"] + " ERROR_DESCRIPTION: " + res["ERROR_DESCRIPTION"]);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string OrderClose(long Ticket, double Volume = 0, double Price = 0, double Slippage = 0)
        {
            try
            {
                JObject json_cmd = new JObject();
                json_cmd["MSG"] = "ORDER_CLOSE";
                json_cmd["TICKET"] = Ticket;
                if (Volume > 0) json_cmd["VOLUME"] = Volume;
                if (Price != 0) json_cmd["PRICE"] = Price;
                if (Slippage != 0) json_cmd["SLIPPAGE"] = Slippage;

                JObject res = SendCommand(json_cmd);

                if (res["ERROR_ID"].ToString() == "0")
                {
                    return res["TYPE"].ToString();//JsonConvert.DeserializeObject<AccountStatus>(res.ToString());
                }
                else
                {
                    throw new Exception("Error with the command sent. ERROR_ID: " + res["ERROR_ID"] + " ERROR_DESCRIPTION: " + res["ERROR_DESCRIPTION"]);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<Asset> GetSymbolList()
        {
            try
            {
                JObject json_cmd = new JObject();
                json_cmd["MSG"] = "SYMBOL_LIST";

                JObject res = SendCommand(json_cmd);

                if (res["ERROR_ID"].ToString() == "0")
                {
                    return JsonConvert.DeserializeObject<List<Asset>>(res["SYMBOLS"].ToString());
                }
                else
                {
                    throw new Exception("Error with the command sent. ERROR_ID: " + res["ERROR_ID"] + " ERROR_DESCRIPTION: " + res["ERROR_DESCRIPTION"]);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Asset GetSymbolInfo(string Symbol)
        {
            try
            {
                JObject json_cmd = new JObject();
                json_cmd["MSG"] = "SYMBOL_INFO";
                json_cmd["SYMBOL"] = Symbol;
                JObject res = SendCommand(json_cmd);

                if (res["ERROR_ID"].ToString() == "0")
                {
                    return JsonConvert.DeserializeObject<Asset>(res.ToString());
                }
                else
                {
                    throw new Exception("Error with the command sent. ERROR_ID: " + res["ERROR_ID"] + " ERROR_DESCRIPTION: " + res["ERROR_DESCRIPTION"]);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Quote GetQuote(string Symbol)
        {
            try
            {
                JObject json_cmd = new JObject();
                json_cmd["MSG"] = "QUOTE";
                json_cmd["SYMBOL"] = Symbol;
                JObject res = SendCommand(json_cmd);

                if (res["ERROR_ID"].ToString() == "0")
                {
                    return JsonConvert.DeserializeObject<Quote>(res.ToString());
                }
                else
                {
                    throw new Exception("Error with the command sent. ERROR_ID: " + res["ERROR_ID"] + " ERROR_DESCRIPTION: " + res["ERROR_DESCRIPTION"]);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<Rates> PriceHistory(string Symbol, TimeFrame tf, DateTime From, DateTime To)
        {
            try
            {
                JObject json_cmd = new JObject();
                json_cmd["MSG"] = "PRICE_HISTORY";
                json_cmd["SYMBOL"] = Symbol;
                json_cmd["TIMEFRAME"] = tf.ToString();
                json_cmd["FROM_DATE"] = From.ToString("yyyy.MM.dd HH:mm");
                json_cmd["TO_DATE"] = To.ToString("yyyy.MM.dd HH:mm");

                JObject res = SendCommand(json_cmd);

                if (res["ERROR_ID"].ToString() == "0")
                {
                    return JsonConvert.DeserializeObject<List<Rates>>(res["RATES"].ToString());
                }
                else
                {
                    throw new Exception("Error with the command sent. ERROR_ID: " + res["ERROR_ID"] + " ERROR_DESCRIPTION: " + res["ERROR_DESCRIPTION"]);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool TrackPrices(List<Asset> symbols)
        {
            try
            {
                JObject json_cmd = new JObject();

                json_cmd["MSG"] = "TRACK_PRICES";

                JArray ja = new JArray();

                symbols = symbols.Where(x => x.TRADE_MODE > 0).ToList();  //Avoid disabled symbols

                foreach (Asset symbol in symbols)
                    ja.Add(symbol.NAME);

                json_cmd["SYMBOLS"] = ja;

                JObject res = SendCommand(json_cmd);

                if (res["ERROR_ID"].ToString() == "0")
                {
                    return true;
                }
                else
                {
                    throw new Exception("Error with the command sent. ERROR_ID: " + res["ERROR_ID"] + " ERROR_DESCRIPTION: " + res["ERROR_DESCRIPTION"]);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool TrackPrices(List<string> symbols)
        {
            try
            {
                JObject json_cmd = new JObject();
                json_cmd["MSG"] = "TRACK_PRICES";

                JArray ja = new JArray();
                foreach (string symbol in symbols)
                    ja.Add(symbol);

                json_cmd["SYMBOLS"] = ja;

                JObject res = SendCommand(json_cmd);

                if (res["ERROR_ID"].ToString() == "0")
                {
                    return true;
                }
                else
                {
                    throw new Exception("Error with the command sent. ERROR_ID: " + res["ERROR_ID"] + " ERROR_DESCRIPTION: " + res["ERROR_DESCRIPTION"]);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool TrackOHLC(List<string> symbols, TimeFrame tf)
        {
            try
            {
                JObject json_cmd = new JObject();
                json_cmd["MSG"] = "TRACK_OHLC";
                json_cmd["TIMEFRAME"] = tf.ToString();

                JArray ja = new JArray();
                foreach (string symbol in symbols)
                    ja.Add(symbol);

                json_cmd["SYMBOLS"] = ja;

                JObject res = SendCommand(json_cmd);

                if (res["ERROR_ID"].ToString() == "0")
                {
                    return true;
                }
                else
                {
                    throw new Exception("Error with the command sent. ERROR_ID: " + res["ERROR_ID"] + " ERROR_DESCRIPTION: " + res["ERROR_DESCRIPTION"]);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
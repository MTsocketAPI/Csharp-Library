using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Text;

namespace MTsocketAPI.MT5
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

    public enum TradeRetCode : long
    {
        TRADE_RETCODE_REQUOTE = 10004,
        TRADE_RETCODE_REJECT = 1006,
        TRADE_RETCODE_CANCEL = 10007,
        TRADE_RETCODE_PLACED = 10008,
        TRADE_RETCODE_DONE = 10009,
        TRADE_RETCODE_DONE_PARTIAL = 10010,
        TRADE_RETCODE_ERROR = 10011,
        TRADE_RETCODE_TIMEOUT = 10012,
        TRADE_RETCODE_INVALID = 10013,
        TRADE_RETCODE_INVALID_VOLUME = 10014,
        TRADE_RETCODE_INVALID_PRICE = 10015,
        TRADE_RETCODE_INVALID_STOPS = 10016,
        TRADE_RETCODE_TRADE_DISABLED = 10017,
        TRADE_RETCODE_MARKET_CLOSED = 10018,
        TRADE_RETCODE_NO_MONEY = 10019,
        TRADE_RETCODE_PRICE_CHANGED = 10020,
        TRADE_RETCODE_PRICE_OFF = 10021,
        TRADE_RETCODE_INVALID_EXPIRATION = 10022,
        TRADE_RETCODE_ORDER_CHANGED = 10023,
        TRADE_RETCODE_TOO_MANY_REQUESTS = 10024,
        TRADE_RETCODE_NO_CHANGES = 10025,
        TRADE_RETCODE_SERVER_DISABLES_AT = 10026,
        TRADE_RETCODE_CLIENT_DISABLES_AT = 10027,
        TRADE_RETCODE_LOCKED = 10028,
        TRADE_RETCODE_FROZEN = 10029,
        TRADE_RETCODE_INVALID_FILL = 10030,
        TRADE_RETCODE_CONNECTION = 10031,
        TRADE_RETCODE_ONLY_REAL = 10032,
        TRADE_RETCODE_LIMIT_ORDERS = 10033,
        TRADE_RETCODE_LIMIT_VOLUME = 10034,
        TRADE_RETCODE_INVALID_ORDER = 10035,
        TRADE_RETCODE_POSITION_CLOSED = 10036,
        TRADE_RETCODE_INVALID_CLOSE_VOLUME = 10038,
        TRADE_RETCODE_CLOSE_ORDER_EXIST = 10039,
        TRADE_RETCODE_LIMIT_POSITIONS = 10040,
        TRADE_RETCODE_REJECT_CANCEL = 10041,
        TRADE_RETCODE_LONG_ONLY = 10042,
        TRADE_RETCODE_SHORT_ONLY = 10043,
        TRADE_RETCODE_CLOSE_ONLY = 10044,
        TRADE_RETCODE_FIFO_CLOSE = 10045,
        TRADE_RETCODE_HEDGE_PROHIBITED = 10046
    }
    public enum TimeFrame
    {
        PERIOD_M1,
        PERIOD_M2,
        PERIOD_M3,
        PERIOD_M4,
        PERIOD_M5,
        PERIOD_M6,
        PERIOD_M10,
        PERIOD_M12,
        PERIOD_M15,
        PERIOD_M20,
        PERIOD_M30,
        PERIOD_H1,
        PERIOD_H2,
        PERIOD_H3,
        PERIOD_H4,
        PERIOD_H6,
        PERIOD_H8,
        PERIOD_H12,
        PERIOD_D1,
        PERIOD_W1,
        PERIOD_MN1
    }

    public enum TradeHistoryMode
    {
        POSITIONS,
        DEALS,
        ORDERS,
        ORDERS_DEALS
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
        public Terminal() { }

        public string host = "127.0.0.1";
        public int cmd_port = 77;
        public int data_port = 78;
        static int bufferLen = 65536;

        TcpClient tcpClient_cmd;
        TcpClient tcpClient_data;

        public event EventHandler OnConnect;
        public event EventHandler OnDisconnect;
        public event EventHandler<Quote> OnPrice;
        public event EventHandler<OHLC_Msg> OnOHLC;
        public event EventHandler<OrderEvent> OnOrderEvent;

        /// <summary>
        /// MTsocketAPI version
        /// </summary>
        public string Version { get; set; }
        private object sendCmdLock = new object();
        /// <summary>
        /// Send RAW JSON command to MTsocketAPI
        /// </summary>
        /// <param name="cmd">JSON command</param>
        /// <returns>JSON reply</returns>
        public JObject SendCommand(JObject cmd)
        {
            lock (sendCmdLock)
            {
                try
                {
                    //System.IO.File.AppendAllText("log.txt", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff") +  " - IN: " + cmd.ToString(Formatting.None) + Environment.NewLine);
                    byte[] data = Encoding.ASCII.GetBytes(cmd.ToString(Formatting.None) + "\r\n");

                    NetworkStream stream = tcpClient_cmd.GetStream();
                    stream.ReadTimeout = 3000;
                    stream.Write(data, 0, data.Length);

                    data = new byte[bufferLen];

                    string responseData = string.Empty;

                    int bytes;
                    do
                    {
                        bytes = stream.Read(data, 0, bufferLen);
                        responseData += Encoding.ASCII.GetString(data, 0, bytes);
                        //System.Threading.Thread.Sleep(1000);
                        //System.IO.File.AppendAllText("log.txt", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff") + " - loop: " + responseData + Environment.NewLine);
                    } while (stream.DataAvailable || !responseData.EndsWith("\r\n"));

                    //System.IO.File.AppendAllText("log.txt", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff") + " - OU: " + responseData + Environment.NewLine);

                    JObject? jresult = JsonConvert.DeserializeObject<JObject>(responseData);

                    if (jresult != null)
                        return jresult;
                    else
                        throw new Exception("Error with deserialization in SendCommand");
                }
                catch (Exception)
                {
                    throw;
                }
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
                    try
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        responseData += Encoding.ASCII.GetString(data, 0, bytes);
                    }
                    catch (Exception ex)
                    {

                    }
                    
                } while (stream.DataAvailable);

                try
                {
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
                        else if (jresult["MSG"].ToString() == "TRACK_TRADE_EVENTS")
                        {
                            OrderEvent ordEvent = JsonConvert.DeserializeObject<OrderEvent>(line);
                            if (OnOrderEvent != null) OnOrderEvent(this, ordEvent);
                        }
                    });
                }
                catch (Exception ex)
                {

                }
                
            } while (true);
        }

        /// <summary>
        /// Connect to MTsocketAPI
        /// </summary>
        /// <param name="host">Hostname or IP Address</param>
        /// <param name="cmd_port">MTsocketAPI command port</param>
        /// <param name="data_port">MTsocketAPI data port</param>
        /// <returns>True = connect successful, False = connect fail</returns>
        public bool Connect(string host = "127.0.0.1", int cmd_port = 77, int data_port = 78)
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

                    if (Convert.ToDouble(Version) < 5.21)
                    {
                        throw new Exception("This API version needs at least MTsocketAPI 5.21 version");
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

        /// <summary>
        /// Get MT5 Terminal Information
        /// </summary>
        /// <returns>Terminal Info object</returns>
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
                    throw new Exception("Error with the GetTerminalInfo command. ERROR_ID: " + res["ERROR_ID"] + " ERROR_DESCRIPTION: " + res["ERROR_DESCRIPTION"]);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Get MT5 Account Status Info
        /// </summary>
        /// <returns>AccountStatus object</returns>
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
                    throw new Exception("Error with the GetAccountStatus command. ERROR_ID: " + res["ERROR_ID"] + " ERROR_DESCRIPTION: " + res["ERROR_DESCRIPTION"]);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Get Pending Orders
        /// </summary>
        /// <returns>List of pending orders</returns>
        public List<Position> GetPendingOrders()
        {
            try
            {
                JObject json_cmd = new JObject();
                json_cmd["MSG"] = "ORDER_LIST";
                JObject res = SendCommand(json_cmd);

                if (res["ERROR_ID"].ToString() == "0")
                {
                    List<Position> pending = JsonConvert.DeserializeObject<List<Position>>(res["PENDING"].ToString());
                    return pending;
                }
                else
                {
                    throw new Exception("Error with the GetPendingOrders command. ERROR_ID: " + res["ERROR_ID"] + " ERROR_DESCRIPTION: " + res["ERROR_DESCRIPTION"]);
                }
            }
            catch (Exception)
            {
                throw;
            }
            
        }

        /// <summary>
        /// Get Opened Positions
        /// </summary>
        /// <returns>List of opened positions</returns>
        public List<Position> GetOpenedOrders()
        {
            try
            {
                JObject json_cmd = new JObject();
                json_cmd["MSG"] = "ORDER_LIST";
                JObject res = SendCommand(json_cmd);

                if (res["ERROR_ID"].ToString() == "0")
                {
                    List<Position> opened = JsonConvert.DeserializeObject<List<Position>>(res["OPENED"].ToString());
                    //List<Order> pending = JsonConvert.DeserializeObject<List<Order>>(res["PENDING"].ToString());
                    return opened;
                }
                else
                {
                    throw new Exception("Error with the GetOpenedOrders command. ERROR_ID: " + res["ERROR_ID"] + " ERROR_DESCRIPTION: " + res["ERROR_DESCRIPTION"]);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Get Moving Average values
        /// </summary>
        /// <param name="Symbol">Symbol</param>
        /// <param name="tf">TimeFrame</param>
        /// <param name="MA_Period">Moving Average period</param>
        /// <param name="MA_Shift">Moving Average shift</param>
        /// <param name="MA_Method">Moving Average method</param>
        /// <param name="Applied_Price">Appliced Price</param>
        /// <param name="Shift">Shift</param>
        /// <param name="Num">Number of elements</param>
        /// <returns>List of MA values</returns>
        /// <exception cref="Exception"></exception>
        public List<double> MA_Indicator(string Symbol, TimeFrame tf, int MA_Period, int MA_Shift, MA_Method MA_Method, Applied_Price Applied_Price, int Shift, int Num = 1)
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
                json_cmd["NUM"] = Num;

                JObject res = SendCommand(json_cmd);

                if (res["ERROR_ID"].ToString() == "0")
                {
                    return JsonConvert.DeserializeObject<List<double>>(res["DATA_VALUES"].ToString());
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

        /// <summary>
        /// Get data from a the ATR indicator using the iATR function. More info: <see href="https://docs.mql4.com/indicators/iatr"/>
        /// </summary>
        /// <param name="Symbol">Symbol name</param>
        /// <param name="tf">TimeFrame</param>
        /// <param name="Period">ATR Period</param>
        /// <param name="Shift">Shift</param>
        /// <param name="Num">Number of elements</param>
        public List<double> ATR_Indicator(string Symbol, TimeFrame tf, int Period, int Shift, int Num = 1)
        {
            try
            {
                JObject json_cmd = new JObject();
                json_cmd["MSG"] = "ATR_INDICATOR";
                json_cmd["SYMBOL"] = Symbol;
                json_cmd["TIMEFRAME"] = tf.ToString();
                json_cmd["PERIOD"] = Period;
                json_cmd["SHIFT"] = Shift;
                json_cmd["NUM"] = Num;

                JObject res = SendCommand(json_cmd);

                if (res["ERROR_ID"].ToString() == "0")
                {
                    return JsonConvert.DeserializeObject<List<double>>(res["DATA_VALUES"].ToString());
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

        /// <summary>
        /// Get data from a custom indicator using the Metatrader iCustom function. More Info: <see href="https://www.mql5.com/es/docs/indicators/icustom"/>
        /// </summary>
        /// <param name="Symbol">Symbol name</param>
        /// <param name="tf">TimeFrame</param>
        /// <param name="Indicator_Name">Indicator Name</param>
        /// <param name="Mode">Mode</param>
        /// <param name="Shift">Shift</param>
        /// <param name="Num">Number of elements</param>
        /// <param name="Params">Parameters</param>
        public List<double> Custom_Indicator(string Symbol, TimeFrame tf, string Indicator_Name, int Mode, int Shift, int Num = 1, List<string> Params = null)
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
                json_cmd["NUM"] = Num;

                int i = 1;

                foreach (var param in Params)
                {
                    double valorD;
                    int valorI;
                    if (int.TryParse(param, out valorI))
                        json_cmd["PARAM" + i.ToString()] = valorI;
                    else if (double.TryParse(param, out valorD))
                        json_cmd["PARAM" + i.ToString()] = valorD;
                    else
                        json_cmd["PARAM" + i.ToString()] = param.ToString();

                    i++;
                }

                JObject res = SendCommand(json_cmd);

                if (res["ERROR_ID"].ToString() == "0")
                {
                    //return Convert.ToDouble(res["DATA_VALUES"]);
                    return JsonConvert.DeserializeObject<List<double>>(res["DATA_VALUES"].ToString());
                }
                else
                {
                    throw new Exception("Error with the Custom_Indicator command. ERROR_ID: " + res["ERROR_ID"] + " ERROR_DESCRIPTION: " + res["ERROR_DESCRIPTION"]);
                }
            }
            catch (Exception)
            {
                throw;
            }
            
        }

        /// <summary>
        /// Get Position History
        /// </summary>
        /// <param name="FromDate">From date</param>
        /// <param name="ToDate">To date</param>
        /// <returns>List of positions</returns>
        public List<Position> GetTradeHistoryPositions(DateTime FromDate, DateTime ToDate)
        {
            try
            {
                JObject json_cmd = new JObject();
                json_cmd["MSG"] = "TRADE_HISTORY";
                json_cmd["MODE"] = TradeHistoryMode.POSITIONS.ToString();
                json_cmd["FROM_DATE"] = FromDate.ToString("yyyy.MM.dd HH:mm:ss");
                json_cmd["TO_DATE"] = ToDate.ToString("yyyy.MM.dd HH:mm:ss");

                JObject res = SendCommand(json_cmd);

                if (res["ERROR_ID"].ToString() == "0")
                {
                    return JsonConvert.DeserializeObject<List<Position>>(res[TradeHistoryMode.POSITIONS.ToString()].ToString());
                }
                else
                {
                    throw new Exception("Error with the command TRADE_HISTORY. ERROR_ID: " + res["ERROR_ID"] + " ERROR_DESCRIPTION: " + res["ERROR_DESCRIPTION"]);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Get the Deals History
        /// </summary>
        /// <param name="FromDate"></param>
        /// <param name="ToDate"></param>
        /// <returns>List of deals</returns>
        public List<Deal> GetTradeHistoryDeals(DateTime FromDate, DateTime ToDate)
        {
            try
            {
                JObject json_cmd = new JObject();
                json_cmd["MSG"] = "TRADE_HISTORY";
                json_cmd["MODE"] = TradeHistoryMode.DEALS.ToString();
                json_cmd["FROM_DATE"] = FromDate.ToString("yyyy.MM.dd HH:mm:ss");
                json_cmd["TO_DATE"] = ToDate.ToString("yyyy.MM.dd HH:mm:ss");

                JObject res = SendCommand(json_cmd);

                if (res["ERROR_ID"].ToString() == "0")
                {
                    return JsonConvert.DeserializeObject<List<Deal>>(res[TradeHistoryMode.DEALS.ToString()].ToString());
                }
                else
                {
                    throw new Exception("Error with the command TRADE_HISTORY. ERROR_ID: " + res["ERROR_ID"] + " ERROR_DESCRIPTION: " + res["ERROR_DESCRIPTION"]);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Get the Order History
        /// </summary>
        /// <param name="FromDate">From date</param>
        /// <param name="ToDate">To date</param>
        /// <returns>List of orders</returns>
        public List<Order> GetTradeHistoryOrders(DateTime FromDate, DateTime ToDate)
        {
            try
            {
                JObject json_cmd = new JObject();
                json_cmd["MSG"] = "TRADE_HISTORY";
                json_cmd["MODE"] = TradeHistoryMode.ORDERS.ToString();
                json_cmd["FROM_DATE"] = FromDate.ToString("yyyy.MM.dd HH:mm:ss");
                json_cmd["TO_DATE"] = ToDate.ToString("yyyy.MM.dd HH:mm:ss");

                JObject res = SendCommand(json_cmd);

                if (res["ERROR_ID"].ToString() == "0")
                {
                    return JsonConvert.DeserializeObject<List<Order>>(res[TradeHistoryMode.ORDERS.ToString()].ToString());
                }
                else
                {
                    throw new Exception("Error with the command TRADE_HISTORY. ERROR_ID: " + res["ERROR_ID"] + " ERROR_DESCRIPTION: " + res["ERROR_DESCRIPTION"]);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Get the Orders & Deals History
        /// </summary>
        /// <param name="FromDate">From date</param>
        /// <param name="ToDate">To date</param>
        /// <returns>List of Orders & Deals</returns>
        public List<Order_Deal> GetTradeHistoryOrdersDeals(DateTime FromDate, DateTime ToDate)
        {
            try
            {
                JObject json_cmd = new JObject();
                json_cmd["MSG"] = "TRADE_HISTORY";
                json_cmd["MODE"] = TradeHistoryMode.ORDERS_DEALS.ToString();
                json_cmd["FROM_DATE"] = FromDate.ToString("yyyy.MM.dd HH:mm:ss");
                json_cmd["TO_DATE"] = ToDate.ToString("yyyy.MM.dd HH:mm:ss");

                JObject res = SendCommand(json_cmd);

                if (res["ERROR_ID"].ToString() == "0")
                {
                    return JsonConvert.DeserializeObject<List<Order_Deal>>(res[TradeHistoryMode.ORDERS_DEALS.ToString()].ToString());
                }
                else
                {
                    throw new Exception("Error with the command TRADE_HISTORY. ERROR_ID: " + res["ERROR_ID"] + " ERROR_DESCRIPTION: " + res["ERROR_DESCRIPTION"]);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Get the price history (Open, High, Low, Close) for a specified Symbol and TimeFrame.
        /// </summary>
        /// <param name="Symbol">Symbol</param>
        /// <param name="tf">TimeFrame</param>
        /// <param name="From">From date</param>
        /// <param name="To">To date</param>
        public List<Rates> PriceHistory(string Symbol, TimeFrame tf, DateTime FromDate, DateTime ToDate)
        {
            try
            {
                JObject json_cmd = new JObject();
                json_cmd["MSG"] = "PRICE_HISTORY";
                json_cmd["SYMBOL"] = Symbol;
                json_cmd["TIMEFRAME"] = tf.ToString();
                json_cmd["FROM_DATE"] = FromDate.ToString("yyyy.MM.dd HH:mm");
                json_cmd["TO_DATE"] = ToDate.ToString("yyyy.MM.dd HH:mm");

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

        /// <summary>
        /// Send market or limit orders.
        /// </summary>
        /// <param name="Symbol">Symbol</param>
        /// <param name="Volume">Volume size</param>
        /// <param name="Type">OrderType enum</param>
        /// <param name="Price">Desired price. 0 for broker's best price (optional)</param>
        /// <param name="SL">Stop Loss price. 0 for no Stop Loss (optional)</param>
        /// <param name="TP">Take Profit price. 0 for no Take Profit (optional)</param>
        /// <param name="Slippage">Slippage. 0 for default broker's Slippage (optional)</param>
        /// <param name="Comment">Order Comment (optional)</param>
        /// <param name="MagicNr">Magic Number (optional)</param>
        /// <param name="Expiration">Order Expiration Date. Only for limit or stop orders (optional)</param>
        public TradeResult SendOrder(string Symbol, double Volume, OrderType Type, double Price = 0, double SL = 0, double TP = 0, double Slippage = 0, string Comment = "", int MagicNr = 0, string Expiration = "1970/01/01 00:00")
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
                    return JsonConvert.DeserializeObject<TradeResult>(res.ToString());
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

        /// <summary>
        /// Change SL or TP for market or limit orders. More Info: <see href="https://www.mtsocketapi.com/doc5/API/ORDER_MODIFY.html"/>
        /// </summary>
        /// <param name="Ticket">Ticket Number</param>
        /// <param name="SL">Stop loss price (optional)</param>
        /// <param name="TP">Take profit price (optional)</param>
        public TradeResult OrderModify(long Ticket, double SL = 0, double TP = 0)
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
                    return JsonConvert.DeserializeObject<TradeResult>(res.ToString());
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

        /// <summary>
        /// Close partially or fully an order using the ticket number. 
        /// Also it closes stop or limit orders. More Info: <see href="https://www.mtsocketapi.com/doc5/API/ORDER_CLOSE.html"/>
        /// </summary>
        /// <param name="Ticket">Ticket Number</param>
        /// <param name="Volume">Volume size (optional)</param>
        /// <param name="Price">Desired Price (optional)</param>
        /// <param name="Slippage">Max lippage (optional)</param>
        public TradeResult OrderClose(long Ticket, double Volume = 0, double Price = 0, double Slippage = 0)
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
                    return JsonConvert.DeserializeObject<TradeResult>(res.ToString());
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

        /// <summary>
        /// Get MT5 Symbol List
        /// </summary>
        /// <returns>Asset List</returns>
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

        /// <summary>
        /// Get detailed information from a symbol
        /// </summary>
        /// <param name="Symbol">Symbol</param>
        /// <returns>Asset object</returns>
        /// <exception cref="Exception"></exception>
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

        /// <summary>
        /// Get last quote from a symbol
        /// </summary>
        /// <param name="Symbol">Symbol</param>
        /// <returns>Quote object</returns>
        /// <exception cref="Exception"></exception>
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
        
        /// <summary>
        /// Start streaming prices from a list of symbols
        /// </summary>
        /// <param name="symbols">Symbol List</param>
        /// <returns>True if the command was executed successfully. Streaming data will be received on the data port</returns>
        /// <exception cref="Exception"></exception>
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

        /// <summary>
        /// Start streaming prices from a list of symbols
        /// </summary>
        /// <param name="symbols">Asset List</param>
        /// <returns>True if the command was executed successfully. Streaming data will be received on the data port</returns>
        /// <exception cref="Exception"></exception>
        public bool TrackPrices(List<Asset> symbols)
        {
            try
            {
                JObject json_cmd = new JObject();
                json_cmd["MSG"] = "TRACK_PRICES";

                symbols = symbols.Where(x => x.TRADE_MODE > 0).ToList(); //Avoid disabled symbols

                JArray ja = new JArray();
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

        /// <summary>
        /// Start streaming OHLC data from a list of symbols
        /// </summary>
        /// <param name="symbols">Symbol list</param>
        /// <param name="tf">TimeFrame</param>
        /// <returns>True if the command was executed successfully</returns>
        /// <exception cref="Exception"></exception>
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

        /// <summary>
        /// Start streaming OHLC data from a list of symbols
        /// </summary>
        /// <param name="symbols">Asset list</param>
        /// <param name="tf">TimeFrame</param>
        /// <returns>True if the command was executed successfully</returns>
        /// <exception cref="Exception"></exception>
        public bool TrackOHLC(List<Asset> symbols, TimeFrame tf)
        {
            try
            {
                JObject json_cmd = new JObject();
                json_cmd["MSG"] = "TRACK_OHLC";
                json_cmd["TIMEFRAME"] = tf.ToString();

                JArray ja = new JArray();

                symbols = symbols.Where(x => x.TRADE_MODE > 0).ToList();
                
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

        /// <summary>
        /// Start streaming order events
        /// </summary>
        /// <param name="enable">True to start streaming order events. False to disable streaming order events</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public bool TrackOrderEvent(bool enable)
        {
            try
            {
                JObject json_cmd = new JObject();
                json_cmd["MSG"] = "TRACK_TRADE_EVENTS";
                json_cmd["ENABLED"] = enable;

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

        /// <summary>
        /// Get the status of the order events
        /// </summary>
        /// <returns>True if the streaming of order events is enabled. False if is disabled</returns>
        public bool TrackOrderEvent()
        {
            try
            {
                JObject json_cmd = new JObject();
                json_cmd["MSG"] = "TRACK_TRADE_EVENTS";

                JObject res = SendCommand(json_cmd);

                if (res["ERROR_ID"].ToString() == "0")
                {
                    return Convert.ToBoolean(res["ENABLED"]);
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
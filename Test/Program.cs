using MTsocketAPI.MT5;

namespace Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            
            try
            {
				//Console.WriteLine("#### MTsocketAPI C# Library ####");
                
				MTsocketAPI.MT5.Terminal mt5 = new MTsocketAPI.MT5.Terminal();
                mt5.Connect();

                
                
                List<MTsocketAPI.MT5.OHLC_Req> ohlc_list = new List<MTsocketAPI.MT5.OHLC_Req>();
				MTsocketAPI.MT5.OHLC_Req req = new MTsocketAPI.MT5.OHLC_Req();
                req.SYMBOL = "EURUSD";
                req.TIMEFRAME = TimeFrame.PERIOD_M1;
				ohlc_list.Add(req);
				MTsocketAPI.MT5.OHLC_Req req2 = new MTsocketAPI.MT5.OHLC_Req();
				req2.SYMBOL = "GBPUSD";
				req2.TIMEFRAME = TimeFrame.PERIOD_M1;
                req2.DEPTH = 3;
				ohlc_list.Add(req2);
                
                bool result = mt5.TrackOHLC(ohlc_list);
                //double valor = mt4.Custom_Indicator("EURUSD", MTsocketAPI.MT4.TimeFrame.PERIOD_M5, "Momentum", 0, 0, new List<string> { "14", "0" });
                //var history = mt4.GetOrderHistory(DateTime.Now.AddDays(-10), DateTime.Now.AddDays(1));
                
                /*
                Terminal mt5 = new Terminal();
				//mt5.OnPrice += Mt5_OnPrice;
				mt5.OnMarketDepth += Mt5_OnMarketDepth;
                if (!mt5.Connect())
                {
                    Console.WriteLine("Connect failed. Please check that MTsocketAPI is running");
                    return;
                }

                mt5.TrackMarketDepth(new List<string>() { "EURUSD", "GBPUSD" });
                */
				//var result = mt5.GetSymbolInfo("EURUSD");

                //CalendarList pepito = mt5.CalendarList(DateTime.Now.AddDays(-3), DateTime.Now.AddDays(-2));

				//List<double> AlligatorLine1 = mt5.Custom_Indicator("EURUSD", TimeFrame.PERIOD_M15, "Examples\\Alligator", 0, 1, new List<string> { "13", "8", "8", "5", "5", "3", "MODE_SMMA", "PRICE_MEDIAN" });
				//List<double> AlligatorLine2 = mt5.Custom_Indicator("EURUSD", TimeFrame.PERIOD_M15, "Examples\\Alligator", 1, 1, new List<string> { "13", "8", "8", "5", "5", "3", "MODE_SMMA", "PRICE_MEDIAN" });
				//List<double> AlligatorLine3 = mt5.Custom_Indicator("EURUSD", TimeFrame.PERIOD_M15, "Examples\\Alligator", 2, 1, new List<string> { "13", "8", "8", "5", "5", "3", "MODE_SMMA", "PRICE_MEDIAN" });

    //            Console.WriteLine("AlligatorLine1: " + AlligatorLine1[0]);
				//Console.WriteLine("AlligatorLine2: " + AlligatorLine2[0]);
				//Console.WriteLine("AlligatorLine3: " + AlligatorLine3[0]);

				//mt5.TrackPrices(new List<string>() { "EURUSD","GBPUSD" });

				//Console.ReadKey();
				//List<Asset> list = mt5.GetSymbolList();
				//List<string> symbols = mt5.GetSymbolList().Where(x => x.TRADE_MODE > 0).Select(x => x.NAME.ToString()).ToList();
				//mt5.TrackPrices(symbols);
				//mt5.TrackPrices(mt5.GetSymbolList());

                /*
                mt5Master.OnConnect += Tt_OnConnect;
                mt5Master.OnPrice += Tt_OnPrice;
                mt5Master.OnOHLC += Tt_OnOHLC;
                mt5Master.OnOrderEvent += Tt_OnOrderEvent;

                if (!mt5Master.Connect())
                {
                    Console.WriteLine("Error connecting to MTsocketAPI. Please check if MT5 is running and MTsocketAPI started successfully");
                    return;
                }

                if (!mt5Slave.Connect("localhost",70,71))
                {
                    Console.WriteLine("Error connecting to MTsocketAPI. Please check if MT5 is running and MTsocketAPI started successfully");
                    return;
                }

                mt5Master.TrackOrderEvent(true);
                /*
                List<Asset> list = mt5.GetSymbolList();
                List<string> symbols = new List<string>();

                Console.WriteLine("Symbol list: ");

                foreach (var item in list)
                {
                    symbols.Add(item.NAME);
                    Console.WriteLine("Name: " + item.NAME + " Description: " + item.DESCRIPTION);
                }

                Quote quote = mt5.GetQuote(symbols.First());

                Console.WriteLine(quote.SYMBOL + " Time: " + quote.TIME + " Ask: " + quote.ASK + " Bid: " + quote.BID);

                Asset asset = mt5.GetSymbolInfo(symbols.First());

                Console.WriteLine(asset.ToString());

                DateTime serverTime = Convert.ToDateTime(quote.TIME);

                List<double> atr = mt5.ATR_Indicator(symbols.First(), TimeFrame.PERIOD_M1, 14, 0, 20);
                List<double> indicator = mt5.Custom_Indicator(symbols.First(), TimeFrame.PERIOD_M1, "Examples\\Alligator", 0, 0, 10, new List<string> { "13", "8", "8", "5", "5", "3", "MODE_SMMA", "PRICE_MEDIAN" });
                AccountStatus account = mt5.GetAccountStatus();

                Console.WriteLine(account.ToString());

                List<Position> positions = mt5.GetOpenedOrders();
                List<Position> pending = mt5.GetPendingOrders();
                
                TerminalInfo tinfo = mt5.GetTerminalInfo();

                Console.WriteLine(tinfo.ToString());

                List<Deal> hdeals = mt5.GetTradeHistoryDeals(serverTime.AddDays(-10), serverTime);
                List<Order> horders = mt5.GetTradeHistoryOrders(serverTime.AddDays(-10), serverTime);
                List<Order_Deal> horderdeals = mt5.GetTradeHistoryOrdersDeals(serverTime.AddDays(-10), serverTime);
                List<Position> hpos = mt5.GetTradeHistoryPositions(serverTime.AddDays(-10), serverTime);

                List<double> ma = mt5.MA_Indicator(asset.NAME, TimeFrame.PERIOD_M1, 14, 0, MA_Method.MODE_SMA, Applied_Price.PRICE_CLOSE, 0);
                List<Rates> rates = mt5.PriceHistory(asset.NAME, TimeFrame.PERIOD_D1, serverTime.AddDays(-10), serverTime);

                //Actions
                Console.WriteLine("\nPress any key to send one buy order to MT5...");
                Console.ReadKey(true);
                TradeResult buy = mt5.SendOrder(asset.NAME, asset.VOLUME_MIN, OrderType.ORDER_TYPE_BUY);
                Console.WriteLine("Result:" + buy.ToString());
                Console.WriteLine("\nPress any key to set SL and TP...");
                Console.ReadKey(true);
                TradeResult Modify = mt5.OrderModify(buy.ORDER, buy.PRICE - asset.SPREAD * 10 * asset.POINT, buy.PRICE + asset.SPREAD * 10 * asset.POINT);
                Console.WriteLine("Result:" + Modify.ToString());
                Console.WriteLine("\nPress any key to close the order...");
                Console.ReadKey(true);
                TradeResult close = mt5.OrderClose(buy.ORDER);
                Console.WriteLine("Result:" + close.ToString());
                */
                //bool result = mt5.TrackOHLC(new List<string>() { asset.NAME },TimeFrame.PERIOD_M1);
                //bool result2 = mt5.TrackPrices(new List<string>() { asset.NAME });
                //bool result3 = mt5.TrackOrderEvent(true);

                Console.ReadKey();

                Console.WriteLine("Finished!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return;
            }

            //do
            //{
            //    System.Threading.Thread.Sleep(1000);
            //} while (true);
        }

		private static void Mt5_OnMarketDepth(object? sender, MarketDepth e)
		{
			Console.WriteLine("Market Depth: " + e.ToString());
		}

		private static void Mt5_OnPrice(object? sender, Quote e)
        {
            Console.WriteLine("Quote: " + e.ToString());
        }
        /*
private static void Mt5_OnPrice(object? sender, Quote e)
{
   Console.WriteLine("Quote: " + e.ToString());
}

static Dictionary<long,long> opened = new Dictionary<long,long>();

private static void Tt_OnOrderEvent(object? sender, OrderEvent e)
{
   if (e.TRADE_RESULT != null)
   {
       //Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff") + " - " + e.ToString());

       if (e.TRADE_REQUEST != null)
       {
           if (e.TRADE_REQUEST.ACTION == "TRADE_ACTION_DEAL") //Open or close position
           {
               if (e.TRADE_REQUEST.POSITION == 0)
               {
                   TradeResult tr = mt5Slave.SendOrder(e.TRADE_REQUEST.SYMBOL, e.TRADE_REQUEST.VOLUME, Enum.Parse<OrderType>( e.TRADE_REQUEST.TYPE));
                   opened.Add(e.TRADE_REQUEST.ORDER, tr.ORDER);
                   Console.WriteLine("New position: " + e.TRADE_REQUEST.ORDER + " Symbol: " + e.TRADE_REQUEST.SYMBOL + " Volume: " + e.TRADE_REQUEST.VOLUME);
               }
               else
               {
                   mt5Slave.OrderClose(opened.FirstOrDefault(t => t.Key == e.TRADE_REQUEST.POSITION).Value);
                   Console.WriteLine("Position closed: " + e.TRADE_REQUEST.POSITION + " Symbol: " + e.TRADE_REQUEST.SYMBOL + " Volume: " + e.TRADE_REQUEST.VOLUME);
               }
           }
           else if (e.TRADE_REQUEST.ACTION == "TRADE_ACTION_SLTP") //Update SL or TP
           {
               mt5Slave.OrderModify(opened.FirstOrDefault(t => t.Key == e.TRADE_REQUEST.POSITION).Value, e.TRADE_REQUEST.SL, e.TRADE_REQUEST.TP);
               Console.WriteLine("SL or TP updated!");
           }
       }
   }
}

private static void Tt_OnOHLC(object? sender, OHLC_Msg e)
{
   Console.WriteLine($"OHLC: {e.OHLC[0].TIME} {e.SYMBOL} Open: {e.OHLC[0].OPEN} High: {e.OHLC[0].HIGH} Low: {e.OHLC[0].LOW} Close:{e.OHLC[0].CLOSE}");
}

private static void Tt_OnPrice(object? sender, Quote e)
{
   Console.WriteLine($"Time: {e.TIME} Symbol: {e.SYMBOL} Ask: {e.ASK} Bid: {e.BID}");
}

private static void Tt_OnConnect(object? sender, EventArgs e)
{
   Console.WriteLine($"Connected!");
}*/
    }
}
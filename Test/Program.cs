using MTsocketAPI.MT5;
using System.Security.Cryptography;

namespace Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("#### MTsocketAPI C# Library ####");

                MTsocketAPI.MT5.Terminal mt5 = new MTsocketAPI.MT5.Terminal();

                mt5.OnConnect += Tt_OnConnect;
                mt5.OnPrice += Tt_OnPrice;
                mt5.OnOHLC += Tt_OnOHLC;
                mt5.OnOrderEvent += Tt_OnOrderEvent;

                if (!mt5.Connect())
                {
                    Console.WriteLine("Error connecting to MTsocketAPI. Please check if MT5 is running and MTsocketAPI started successfully");
                    return;
                }

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

                //bool result = mt5.TrackOHLC(new List<string>() { asset.NAME },TimeFrame.PERIOD_M1);
                //bool result2 = mt5.TrackPrices(new List<string>() { asset.NAME });
                //bool result3 = mt5.TrackOrderEvent(true);

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

        private static void Tt_OnOrderEvent(object? sender, OrderEvent e)
        {
            Console.WriteLine(e.TRADE_TRANSACTION.ToString());
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
        }
    }
}
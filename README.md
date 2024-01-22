# MTsocketAPI C# Libraries

Libraries created to help developers to easily use the MTsocketAPI using C#

Basic Usage:

```csharp
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

                if (!mt5.Connect())
                {
                    Console.WriteLine("Error connecting to MTsocketAPI. Please check if MT5 is running and MTsocketAPI started successfully");
                    return;
                }

                Quote quote = mt5.GetQuote("EURUSD");

                Console.WriteLine(quote.SYMBOL + " Time: " + quote.TIME + " Ask: " + quote.ASK + " Bid: " + quote.BID);

                Asset asset = mt5.GetSymbolInfo("EURUSD");

                Console.WriteLine(asset.ToString());

                Console.WriteLine("\nPress any key to send one buy order to MT5...");
                Console.ReadKey(true);
                TradeResult buy = mt5.SendOrder(asset.NAME, asset.VOLUME_MIN, OrderType.ORDER_TYPE_BUY);
                Console.WriteLine("Result:" + buy.ToString());
               
                Console.WriteLine("\nPress any key to close the order...");
                Console.ReadKey(true);
                TradeResult close = mt5.OrderClose(buy.ORDER);
                Console.WriteLine("Result:" + close.ToString());

                Console.WriteLine("Finished!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return;
            }
        }
    }
}
```

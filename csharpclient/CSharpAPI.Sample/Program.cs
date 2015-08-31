using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using IBApi;
using System.Threading;
using IBSamples;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Samples
{
    public class Sample
    {
        public static int Main(string[] args)
        {
            Console.WriteLine("Thread {0}: Starting...", Thread.CurrentThread.ManagedThreadId);
            using (var ibClient = new IbClient())
            {
                try
                {
                    ProcessErrors(ibClient);
                    ProcessTickPrices(ibClient);
                    ProcessTickSizes(ibClient);

                    Console.WriteLine("Connecting to IB...");
                    ibClient.Connect("127.0.0.1", 7496, 0);


                    var cts = new CancellationTokenSource();
                    ibClient.Request.ReqMktData(1001, ContractSamples.getEurUsdForex(), "", false, GetFakeParameters(3), cts.Token);
                    Console.ReadLine();
                    //ibClient.Request.CancelMktData(1001);
                    cts.Cancel();

                    Console.ReadLine();
                    Console.WriteLine("Disconnecting...");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return 0;
        }


        private static async void ProcessTickPrices(IbClient ibClient)
        {
            while (true)
            {
                var tickPrice = await ibClient.Response.TickPriceAsync();
                Console.WriteLine("Thread {0}: Tick Price. Ticker Id: {1}, Field: {2}, Price: {3}, CanAutoExecute: {4}", Thread.CurrentThread.ManagedThreadId, tickPrice.TickerId, tickPrice.Field, tickPrice.Price, tickPrice.CanAutoExecute);                
            }
        }

        private static async void ProcessTickSizes(IbClient ibClient)
        {
            while (true)
            {
                var tickSize = await ibClient.Response.TickSizeAsync();
                Console.WriteLine("Thread {0}: Tick Size. Ticker Id: {1}, Field: {2}, Size: {3}", Thread.CurrentThread.ManagedThreadId, tickSize.TickerId, tickSize.Field, tickSize.Size);
            }
        }

        private static async void ProcessErrors(IbClient ibClient)
        {
            while (true)
            {
                var error = await ibClient.Response.ErrorAsync();
                Console.WriteLine("Thread {0}: Error - {1}", Thread.CurrentThread.ManagedThreadId, error.ErrorMessage);
                if (error.Exception != null) Console.WriteLine(error.Exception);
            }
        }


        /*****************************************************************/
        /* Below are few quick-to-test examples on the IB API functions.
         * Process methods, like the ones above, will need to implemented to process responses generated from these request methods. 
         * See the IB API Reference to determine which methods will receive responses for a particular request.
         */
        /*****************************************************************/
        private static void TestIbMethods(IbClient ibClient)
        {
            /***************************************************/
            /*** Real time market data operations  - Tickers ***/
            /***************************************************/
            /*** Requesting real time market data ***/
            //ibClient.Request.ReqMarketDataType(2);
            //ibClient.Request.ReqMktData(1001, ContractSamples.getEurUsdForex(), "", false, GetFakeParameters(3));
            //ibClient.Request.ReqMktData(1002, ContractSamples.getOption(), "", false, GetFakeParameters(3));
            //ibClient.Request.ReqMktData(1003, ContractSamples.getEuropeanStock(), "", false, GetFakeParameters(3));
            //Thread.Sleep(2000);
            /*** Canceling the market data subscription ***/
            //ibClient.Request.CancelMktData(1001);
            //ibClient.Request.CancelMktData(1002);
            //ibClient.Request.CancelMktData(1003);

            /********************************************************/
            /*** Real time market data operations  - Market Depth ***/
            /********************************************************/
            /*** Requesting the Deep Book ***/
            //ibClient.Request.ReqMktDepth(2001, ContractSamples.getEurGbpForex(), 5, GetFakeParameters(2));
            //Thread.Sleep(2000);
            /*** Canceling the Deep Book request ***/
            //ibClient.Request.CancelMktDepth(2001);

            /**********************************************************/
            /*** Real time market data operations  - Real Time Bars ***/
            /**********************************************************/
            /*** Requesting real time bars ***/
            //ibClient.Request.ReqRealTimeBars(3001, ContractSamples.getEurGbpForex(), -1, "MIDPOINT", true, GetFakeParameters(4));
            //Thread.Sleep(2000);
            /*** Canceling real time bars ***/
            //ibClient.Request.CancelRealTimeBars(3001);

            /**********************************/
            /*** Historical Data operations ***/
            /**********************************/
            /*** Requesting historical data ***/
            //ibClient.Request.ReqHistoricalData(4001, ContractSamples.getEurGbpForex(), "20130722 23:59:59", "1 D", "1 min", "MIDPOINT", 1, 1, GetFakeParameters(4));
            //ibClient.Request.ReqHistoricalData(4002, ContractSamples.getEuropeanStock(), "20131009 23:59:59", "10 D", "1 min", "TRADES", 1, 1, null);
            /*** Canceling historical data requests ***/
            //ibClient.Request.CancelHistoricalData(4001);
            //ibClient.Request.CancelHistoricalData(4002);

            /****************************/
            /*** Contract information ***/
            /****************************/
            //ibClient.Request.ReqContractDetails(6001, ContractSamples.GetbyIsin());
            //ibClient.Request.ReqContractDetails(210, ContractSamples.getOptionForQuery());
            //ibClient.Request.ReqContractDetails(211, ContractSamples.GetBondForQuery());
        }


        private static List<TagValue> GetFakeParameters(int numParams)
        {
            List<TagValue> fakeParams = new List<TagValue>();
            for (int i = 0; i < numParams; i++)
                fakeParams.Add(new TagValue("tag" + i, i.ToString()));
            return fakeParams;
        }
        
    }
}
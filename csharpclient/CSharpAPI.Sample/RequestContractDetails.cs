/* Copyright (C) 2013 Interactive Brokers LLC. All rights reserved.  This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IBApi;
using System.Threading;

namespace Samples
{
    public class RequestContractDetails
    {
        public static int Main(string[] args)
        {
            Console.WriteLine("Thread {0}: Starting...", Thread.CurrentThread.ManagedThreadId);
            using (var ibClient = new IbClient())
            {
                try
                {
                    ProcessContractDetails(ibClient);

                    ibClient.Connect("127.0.0.1", 7496, 0);

                    //We can request the whole option's chain by giving a brief description of the contract
                    //i.e. we only specify symbol, currency, secType and exchange (SMART)
                    Contract optionContract = ContractSamples.getOptionForQuery();

                    var endContractDetailsTask = ibClient.Response.ContractDetailsEndAsync();
                    ibClient.Request.ReqContractDetails(1, optionContract);

                    endContractDetailsTask.Wait();
                    Console.WriteLine("Finished receiving all matching contracts.");
                    Thread.Sleep(1000);
                    Console.WriteLine("Disconnecting...");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return 0;
        }


        public static async void ProcessContractDetails(IbClient ibClient)
        {
            while (true)
            {
                var contractDetailsMsg = await ibClient.Response.ContractDetailsAsync();
                var contractDetails = contractDetailsMsg.ContractDetails;
                Console.WriteLine("/*******Incoming Contract Details - RequestId " + contractDetailsMsg.ReqId + "************/");
                Console.WriteLine(contractDetails.Summary.Symbol + " " + contractDetails.Summary.SecType + " @ " + contractDetails.Summary.Exchange);
                Console.WriteLine("Expiry: " + contractDetails.Summary.Expiry + ", Right: " + contractDetails.Summary.Right);
                Console.WriteLine("Strike: " + contractDetails.Summary.Strike + ", Multiplier: " + contractDetails.Summary.Multiplier);
                Console.WriteLine("/*******     End     *************/\n");
            }
        }
    }
}

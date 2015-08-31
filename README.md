# tws-api-tap
Interactive Brokers Trader Workstation (TWS) API Client implemented using C# Task-based Asynchronous Pattern. Forked from TWS API Version 9.71.

### Summary

The goal of this library is to make the TWS API more accessible to new programmers while also offering component-level accessibility for more advanced usage.


Why [Task-based Asynchronous Pattern (TAP)](https://msdn.microsoft.com/en-us/library/hh873175(v=vs.110).aspx)? 

The intention of having an explicitly defined, and Microsoft recommended, asynchronous pattern as the interface to TWS is to clarify the API Client's behavior and minimize boilerplate code so you can spend more time focusing on your business logic. You'll get documented [TAP consumption patterns](https://msdn.microsoft.com/en-us/library/hh873173(v=vs.110).aspx) defining the execution flows and discussing the synchronization implications of asynchronous programming using TAP so you don't need to spend time trying to understand the layers of indirection underlying EWrapper. In fact, this library removes EWrapper and the obligatory method implementations that come with it so you only need to handle messages you care about. You'll get to use [async and await](https://msdn.microsoft.com/en-us/library/hh191443.aspx) to simplify your calling code. And you'll have straightforward mechanisms for controlling execution flow and handling synchronization.


### Examples
Get the current time synchronously by waiting on the Task to return a result:
``` C#
public static void Main()
{
	using (var ibClient = new IbClient())
	{
		ibClient.Connect("127.0.0.1", 7496, 0);	
		var t = ibClient.Response.CurrentTimeAsync(); // capture Task
		ibClient.Request.ReqCurrentTime(); // request current time from server
		Console.WriteLine("Server time is {0}.", t.Result); // wait for Task to return a Result
	}
}
```

Process tick prices until enter is pressed:
``` C#
public static void Main()
{
	using (var ibClient = new IbClient())
	{
		ProcessTickPrices(ibClient); // process tick prices asynchronously
		ibClient.Connect("127.0.0.1", 7496, 0);
		ibClient.Request.ReqMktData(1001, ContractSamples.getEurUsdForex(), "", false, GetFakeParameters(3));
		Console.ReadLine(); // wait here and process tick prices
		ibClient.Request.CancelMktData(1001);
	}
}

private static async void ProcessTickPrices(IbClient ibClient)
{
	while (true)
	{
		var tickPrice = await ibClient.Response.TickPriceAsync();
		Console.WriteLine("Tick Price. Ticker Id: {1}, Field: {2}, Price: {3}, CanAutoExecute: {4}", Thread.CurrentThread.ManagedThreadId, tickPrice.TickerId, tickPrice.Field, tickPrice.Price, tickPrice.CanAutoExecute);
	}
}
```


### Future State
Considering using cancellation tokens to cancel data requests instead of having to call an associated cancel request method with the original ticker id. Cancellation-tokens branch has proof of concept code.

This:
``` C#
var cts = new CancellationTokenSource();
ibClient.Request.ReqMktData(1001, ContractSamples.getEurUsdForex(), "", false, GetFakeParameters(3), cts.Token);
cts.Cancel(); // cancel market data request
```

Instead of this:
``` C#
ibClient.Request.ReqMktData(1001, ContractSamples.getEurUsdForex(), "", false, GetFakeParameters(3), cts.Token);
ibClient.Request.CancelMktData(1001);
```



### Warning
This software is provided __AS-IS__ with __NO WARRANTY__, express or
implied. Your use of this software is at your own risk. It may contain any number
of bugs, known or unknown, which might cause you to lose money if you use it.

This code is not sanctioned or supported by Interactive Brokers.

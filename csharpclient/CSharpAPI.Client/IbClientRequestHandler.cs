using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBApi
{
    /// <summary>
    /// Sends requests to IB server. This class is not thread-safe.
    /// </summary>
    public class IbClientRequestHandler
    {
        private readonly IIbClientConnection ibClientConnection;

        public IbClientRequestHandler(IIbClientConnection ibClientConnection)
        {
            this.ibClientConnection = ibClientConnection;
        }


        #region Request Methods

        /// <summary>
        /// Changes the TWS/GW log level.
        /// </summary>
        /// <param name="logLevel">
        /// Valid values are:
        /// 1 = SYSTEM
        /// 2 = ERROR
        /// 3 = WARNING
        /// 4 = INFORMATION
        /// 5 = DETAIL
        /// </param>
        public void SetServerLogLevel(int logLevel)
        {
            CheckConnection();
            const int VERSION = 1;

            List<byte> paramsList = new List<byte>();
            paramsList.AddParameter(OutgoingMessages.ChangeServerLog);
            paramsList.AddParameter(VERSION);
            paramsList.AddParameter(logLevel);

            ibClientConnection.IbWriter.Send(paramsList, EClientErrors.FAIL_SEND_SERVER_LOG_LEVEL);
        }

        /// <summary>
        /// Requests the server's current time.
        /// </summary>
        public void ReqCurrentTime()
        {
            int VERSION = 1;
            CheckConnection();
            CheckServerVersion(MinServerVer.CURRENT_TIME, " It does not support current time requests.");

            List<byte> paramsList = new List<byte>();
            paramsList.AddParameter(OutgoingMessages.RequestCurrentTime);
            paramsList.AddParameter(VERSION);//version
            ibClientConnection.IbWriter.Send(paramsList, EClientErrors.FAIL_SEND_REQCURRTIME);
        }

        /// <summary>
        /// Requests real time market data. This function will return the product's market data. It is important to notice that only real time data can be delivered via the API.
        /// </summary>
        /// <param name="tickerId">The request's identifier.</param>
        /// <param name="contract">
        /// The Contract for which the data is being requested:
        /// - 100 	Option Volume (currently for stocks)
        /// - 101 	Option Open Interest (currently for stocks) 
        /// - 104 	Historical Volatility (currently for stocks)
        /// - 106 	Option Implied Volatility (currently for stocks)
        /// - 162 	Index Future Premium
        /// - 165 	Miscellaneous Stats
        /// - 221 	Mark Price (used in TWS P&L computations)
        /// - 225 	Auction values (volume, price and imbalance)
        /// - 233 	RTVolume - contains the last trade price, last trade size, last trade time, total volume, VWAP, and single trade flag.
        /// - 236 	Shortable
        /// - 256 	Inventory
        /// - 258 	Fundamental Ratios
        /// - 411 	Realtime Historical Volatility
        /// - 456 	IBDividends
        /// </param>
        /// <param name="genericTickList">Comma separated ids of the available generic ticks.</param>
        /// <param name="snapshot">When set to true, it will provide a single snapshot of the available data. Set to false if you want to receive continuous updates.</param>
        /// <param name="mktDataOptions"></param>
        public void ReqMktData(int tickerId, Contract contract, string genericTickList, bool snapshot, List<TagValue> mktDataOptions)
        {
            CheckConnection();
            if (snapshot) CheckServerVersion(tickerId, MinServerVer.SNAPSHOT_MKT_DATA, " It does not support snapshot market data requests.");
            if (contract.UnderComp != null) CheckServerVersion(tickerId, MinServerVer.UNDER_COMP, " It does not support delta-neutral orders");
            if (contract.ConId > 0) CheckServerVersion(tickerId, MinServerVer.CONTRACT_CONID, " It does not support ConId parameter");
            if (!Util.StringIsEmpty(contract.TradingClass)) CheckServerVersion(tickerId, MinServerVer.TRADING_CLASS, " It does not support trading class parameter in reqMktData.");

            int version = 11;
            List<byte> paramsList = new List<byte>();
            paramsList.AddParameter(OutgoingMessages.RequestMarketData);
            paramsList.AddParameter(version);
            paramsList.AddParameter(tickerId);
            if (ibClientConnection.ServerVersion >= MinServerVer.CONTRACT_CONID) paramsList.AddParameter(contract.ConId);
            paramsList.AddParameter(contract.Symbol);
            paramsList.AddParameter(contract.SecType);
            paramsList.AddParameter(contract.Expiry);
            paramsList.AddParameter(contract.Strike);
            paramsList.AddParameter(contract.Right);
            if (ibClientConnection.ServerVersion >= 15) paramsList.AddParameter(contract.Multiplier);
            paramsList.AddParameter(contract.Exchange);
            if (ibClientConnection.ServerVersion >= 14) paramsList.AddParameter(contract.PrimaryExch);
            paramsList.AddParameter(contract.Currency);
            if (ibClientConnection.ServerVersion >= 2) paramsList.AddParameter(contract.LocalSymbol);
            if (ibClientConnection.ServerVersion >= MinServerVer.TRADING_CLASS) paramsList.AddParameter(contract.TradingClass);
            if (ibClientConnection.ServerVersion >= 8 && Constants.BagSecType.Equals(contract.SecType))
            {
                if (contract.ComboLegs == null)
                {
                    paramsList.AddParameter(0);
                }
                else
                {
                    paramsList.AddParameter(contract.ComboLegs.Count);
                    for (int i = 0; i < contract.ComboLegs.Count; i++)
                    {
                        ComboLeg leg = contract.ComboLegs[i];
                        paramsList.AddParameter(leg.ConId);
                        paramsList.AddParameter(leg.Ratio);
                        paramsList.AddParameter(leg.Action);
                        paramsList.AddParameter(leg.Exchange);
                    }
                }
            }
            if (ibClientConnection.ServerVersion >= MinServerVer.UNDER_COMP)
            {
                if (contract.UnderComp != null)
                {
                    paramsList.AddParameter(true);
                    paramsList.AddParameter(contract.UnderComp.ConId);
                    paramsList.AddParameter(contract.UnderComp.Delta);
                    paramsList.AddParameter(contract.UnderComp.Price);
                }
                else
                {
                    paramsList.AddParameter(false);
                }
            }
            if (ibClientConnection.ServerVersion >= 31)
            {
                paramsList.AddParameter(genericTickList);
            }
            if (ibClientConnection.ServerVersion >= MinServerVer.SNAPSHOT_MKT_DATA)
            {
                paramsList.AddParameter(snapshot);
            }
            if (ibClientConnection.ServerVersion >= MinServerVer.LINKING)
            {
                paramsList.AddParameter(TagValueListToString(mktDataOptions));
            }
            ibClientConnection.IbWriter.Send(tickerId, paramsList, EClientErrors.FAIL_SEND_REQMKT);
        }

        /// <summary>
        /// Cancels a RT Market Data request.
        /// </summary>
        /// <param name="tickerId">Request's identifier.</param>
        public void CancelMktData(int tickerId)
        {
            CheckConnection();
            ibClientConnection.IbWriter.SendCancelRequest(OutgoingMessages.CancelMarketData, 1, tickerId, EClientErrors.FAIL_SEND_CANMKT);
        }

        /// <summary>
        /// Calculate the volatility for an option. Request the calculation of the implied volatility based on hypothetical option and its underlying prices. The calculation will be return in TickOptionComputationAsync callback.
        /// </summary>
        /// <param name="reqId">Unique identifier of the request.</param>
        /// <param name="contract">The option's contract for which the volatility wants to be calculated.</param>
        /// <param name="optionPrice">Hypothetical option price.</param>
        /// <param name="underPrice">Hypothetical option's underlying price.</param>
        /// <param name="impliedVolatilityOptions"></param>
        public void CalculateImpliedVolatility(int reqId, Contract contract, double optionPrice, double underPrice, List<TagValue> impliedVolatilityOptions)
        {
            CheckConnection();
            CheckServerVersion(MinServerVer.REQ_CALC_IMPLIED_VOLAT, " It does not support calculate implied volatility.");
            if (!Util.StringIsEmpty(contract.TradingClass)) CheckServerVersion(MinServerVer.TRADING_CLASS, "");
            const int version = 3;
            List<byte> paramsList = new List<byte>();
            paramsList.AddParameter(OutgoingMessages.ReqCalcImpliedVolat);
            paramsList.AddParameter(version);
            paramsList.AddParameter(reqId);
            paramsList.AddParameter(contract.ConId);
            paramsList.AddParameter(contract.Symbol);
            paramsList.AddParameter(contract.SecType);
            paramsList.AddParameter(contract.Expiry);
            paramsList.AddParameter(contract.Strike);
            paramsList.AddParameter(contract.Right);
            paramsList.AddParameter(contract.Multiplier);
            paramsList.AddParameter(contract.Exchange);
            paramsList.AddParameter(contract.PrimaryExch);
            paramsList.AddParameter(contract.Currency);
            paramsList.AddParameter(contract.LocalSymbol);
            if (ibClientConnection.ServerVersion >= MinServerVer.TRADING_CLASS) paramsList.AddParameter(contract.TradingClass);
            paramsList.AddParameter(optionPrice);
            paramsList.AddParameter(underPrice);

            if (ibClientConnection.ServerVersion >= MinServerVer.LINKING)
            {
                int tagValuesCount = impliedVolatilityOptions == null ? 0 : impliedVolatilityOptions.Count;
                paramsList.AddParameter(tagValuesCount);
                paramsList.AddParameter(TagValueListToString(impliedVolatilityOptions));
            }

            ibClientConnection.IbWriter.Send(reqId, paramsList, EClientErrors.FAIL_SEND_REQCALCIMPLIEDVOLAT);
        }

        /// <summary>
        /// Cancels an option's implied volatility calculation request.
        /// </summary>
        /// <param name="reqId">The identifier of the implied volatility's calculation request.</param>
        public void CancelCalculateImpliedVolatility(int reqId)
        {
            CheckConnection();
            CheckServerVersion(MinServerVer.CANCEL_CALC_IMPLIED_VOLAT, " It does not support calculate implied volatility cancellation.");
            ibClientConnection.IbWriter.SendCancelRequest(OutgoingMessages.CancelImpliedVolatility, 1, reqId, EClientErrors.FAIL_SEND_CANCALCIMPLIEDVOLAT);
        }

        /// <summary>
        /// Calculates an option's price. Calculates an option's price based on the provided volatility and its underlying's price. The calculation will be return in TickOptionComputationAsync callback.
        /// </summary>
        /// <param name="reqId">Request's unique identifier.</param>
        /// <param name="contract">The option's contract for which the price wants to be calculated.</param>
        /// <param name="volatility">Hypothetical volatility.</param>
        /// <param name="underPrice">Hypothetical underlying's price.</param>
        /// <param name="optionPriceOptions"></param>
        public void CalculateOptionPrice(int reqId, Contract contract, double volatility, double underPrice, List<TagValue> optionPriceOptions)
        {
            CheckConnection();
            CheckServerVersion(MinServerVer.REQ_CALC_OPTION_PRICE, " It does not support calculation price requests.");
            if (!Util.StringIsEmpty(contract.TradingClass)) CheckServerVersion(MinServerVer.REQ_CALC_OPTION_PRICE, " It does not support tradingClass parameter in calculateOptionPrice.");

            const int version = 3;
            List<byte> paramsList = new List<byte>();
            paramsList.AddParameter(OutgoingMessages.ReqCalcOptionPrice);
            paramsList.AddParameter(version);
            paramsList.AddParameter(reqId);
            paramsList.AddParameter(contract.ConId);
            paramsList.AddParameter(contract.Symbol);
            paramsList.AddParameter(contract.SecType);
            paramsList.AddParameter(contract.Expiry);
            paramsList.AddParameter(contract.Strike);
            paramsList.AddParameter(contract.Right);
            paramsList.AddParameter(contract.Multiplier);
            paramsList.AddParameter(contract.Exchange);
            paramsList.AddParameter(contract.PrimaryExch);
            paramsList.AddParameter(contract.Currency);
            paramsList.AddParameter(contract.LocalSymbol);
            if (ibClientConnection.ServerVersion >= MinServerVer.TRADING_CLASS) paramsList.AddParameter(contract.TradingClass);
            paramsList.AddParameter(volatility);
            paramsList.AddParameter(underPrice);

            if (ibClientConnection.ServerVersion >= MinServerVer.LINKING)
            {
                int tagValuesCount = optionPriceOptions == null ? 0 : optionPriceOptions.Count;
                paramsList.AddParameter(tagValuesCount);
                paramsList.AddParameter(TagValueListToString(optionPriceOptions));
            }

            ibClientConnection.IbWriter.Send(reqId, paramsList, EClientErrors.FAIL_SEND_REQCALCOPTIONPRICE);
        }

        /// <summary>
        /// Cancels an option's price calculation request.
        /// </summary>
        /// <param name="reqId">The identifier of the option's price's calculation request.</param>
        public void CancelCalculateOptionPrice(int reqId)
        {
            CheckConnection();
            CheckServerVersion(MinServerVer.CANCEL_CALC_OPTION_PRICE, " It does not support calculate option price cancellation.");
            ibClientConnection.IbWriter.SendCancelRequest(OutgoingMessages.CancelOptionPrice, 1, reqId, EClientErrors.FAIL_SEND_CANCALCOPTIONPRICE);
        }

        /// <summary>
        /// Indicates the TWS to switch to "frozen" market data. The API can receive frozen market data from Trader Workstation. Frozen market data is the last data recorded in our system. During normal trading hours, the API receives real-time market data. If you use this function, you are telling TWS to automatically switch to frozen market data after the close. Then, before the opening of the next trading day, market data will automatically switch back to real-time market data.
        /// </summary>
        /// <param name="marketDataType">Set to 1 for real time streaming, set to 2 for frozen market data.</param>
        public void ReqMarketDataType(int marketDataType)
        {
            CheckConnection();
            CheckServerVersion(MinServerVer.REQ_MARKET_DATA_TYPE, " It does not support market data type requests.");
            const int VERSION = 1;
            List<byte> paramsList = new List<byte>();
            paramsList.AddParameter(OutgoingMessages.RequestMarketDataType);
            paramsList.AddParameter(VERSION);
            paramsList.AddParameter(marketDataType);
            ibClientConnection.IbWriter.Send(paramsList, EClientErrors.FAIL_SEND_REQMARKETDATATYPE);
        }

        /// <summary>
        /// Places an order.
        /// </summary>
        /// <param name="id">The order's unique identifier. Use a sequential id starting with the id received at the nextValidId method.</param>
        /// <param name="contract">The order's contract.</param>
        /// <param name="order">The order.</param>
        public void PlaceOrder(int id, Contract contract, Order order)
        {
            CheckConnection();

            if (!VerifyOrder(order, id, StringsAreEqual(Constants.BagSecType, contract.SecType))) return;
            if (!VerifyOrderContract(contract, id)) return;

            int MsgVersion = (ibClientConnection.ServerVersion < MinServerVer.NOT_HELD) ? 27 : 43;
            List<byte> paramsList = new List<byte>();

            paramsList.AddParameter(OutgoingMessages.PlaceOrder);
            paramsList.AddParameter(MsgVersion);
            paramsList.AddParameter(id);

            if (ibClientConnection.ServerVersion >= MinServerVer.PLACE_ORDER_CONID)
            {
                paramsList.AddParameter(contract.ConId);
            }
            paramsList.AddParameter(contract.Symbol);
            paramsList.AddParameter(contract.SecType);
            paramsList.AddParameter(contract.Expiry);
            paramsList.AddParameter(contract.Strike);
            paramsList.AddParameter(contract.Right);
            if (ibClientConnection.ServerVersion >= 15)
            {
                paramsList.AddParameter(contract.Multiplier);
            }
            paramsList.AddParameter(contract.Exchange);
            if (ibClientConnection.ServerVersion >= 14)
            {
                paramsList.AddParameter(contract.PrimaryExch);
            }
            paramsList.AddParameter(contract.Currency);
            if (ibClientConnection.ServerVersion >= 2)
            {
                paramsList.AddParameter(contract.LocalSymbol);
            }
            if (ibClientConnection.ServerVersion >= MinServerVer.TRADING_CLASS)
            {
                paramsList.AddParameter(contract.TradingClass);
            }
            if (ibClientConnection.ServerVersion >= MinServerVer.SEC_ID_TYPE)
            {
                paramsList.AddParameter(contract.SecIdType);
                paramsList.AddParameter(contract.SecId);
            }

            // paramsList.AddParameter main order fields
            paramsList.AddParameter(order.Action);
            paramsList.AddParameter(order.TotalQuantity);
            paramsList.AddParameter(order.OrderType);
            if (ibClientConnection.ServerVersion < MinServerVer.ORDER_COMBO_LEGS_PRICE)
            {
                paramsList.AddParameter(order.LmtPrice == Double.MaxValue ? 0 : order.LmtPrice);
            }
            else
            {
                paramsList.AddParameterMax(order.LmtPrice);
            }
            if (ibClientConnection.ServerVersion < MinServerVer.TRAILING_PERCENT)
            {
                paramsList.AddParameter(order.AuxPrice == Double.MaxValue ? 0 : order.AuxPrice);
            }
            else
            {
                paramsList.AddParameterMax(order.AuxPrice);
            }

            // paramsList.AddParameter extended order fields
            paramsList.AddParameter(order.Tif);
            paramsList.AddParameter(order.OcaGroup);
            paramsList.AddParameter(order.Account);
            paramsList.AddParameter(order.OpenClose);
            paramsList.AddParameter(order.Origin);
            paramsList.AddParameter(order.OrderRef);
            paramsList.AddParameter(order.Transmit);
            if (ibClientConnection.ServerVersion >= 4)
            {
                paramsList.AddParameter(order.ParentId);
            }

            if (ibClientConnection.ServerVersion >= 5)
            {
                paramsList.AddParameter(order.BlockOrder);
                paramsList.AddParameter(order.SweepToFill);
                paramsList.AddParameter(order.DisplaySize);
                paramsList.AddParameter(order.TriggerMethod);
                if (ibClientConnection.ServerVersion < 38)
                {
                    // will never happen
                    paramsList.AddParameter(/* order.ignoreRth */ false);
                }
                else
                {
                    paramsList.AddParameter(order.OutsideRth);
                }
            }

            if (ibClientConnection.ServerVersion >= 7)
            {
                paramsList.AddParameter(order.Hidden);
            }

            // paramsList.AddParameter combo legs for BAG requests
            bool isBag = StringsAreEqual(Constants.BagSecType, contract.SecType);
            if (ibClientConnection.ServerVersion >= 8 && isBag)
            {
                if (contract.ComboLegs == null)
                {
                    paramsList.AddParameter(0);
                }
                else
                {
                    paramsList.AddParameter(contract.ComboLegs.Count);

                    ComboLeg comboLeg;
                    for (int i = 0; i < contract.ComboLegs.Count; i++)
                    {
                        comboLeg = (ComboLeg)contract.ComboLegs[i];
                        paramsList.AddParameter(comboLeg.ConId);
                        paramsList.AddParameter(comboLeg.Ratio);
                        paramsList.AddParameter(comboLeg.Action);
                        paramsList.AddParameter(comboLeg.Exchange);
                        paramsList.AddParameter(comboLeg.OpenClose);

                        if (ibClientConnection.ServerVersion >= MinServerVer.SSHORT_COMBO_LEGS)
                        {
                            paramsList.AddParameter(comboLeg.ShortSaleSlot);
                            paramsList.AddParameter(comboLeg.DesignatedLocation);
                        }
                        if (ibClientConnection.ServerVersion >= MinServerVer.SSHORTX_OLD)
                        {
                            paramsList.AddParameter(comboLeg.ExemptCode);
                        }
                    }
                }
            }

            // add order combo legs for BAG requests
            if (ibClientConnection.ServerVersion >= MinServerVer.ORDER_COMBO_LEGS_PRICE && isBag)
            {
                if (order.OrderComboLegs == null)
                {
                    paramsList.AddParameter(0);
                }
                else
                {
                    paramsList.AddParameter(order.OrderComboLegs.Count);

                    for (int i = 0; i < order.OrderComboLegs.Count; i++)
                    {
                        OrderComboLeg orderComboLeg = order.OrderComboLegs[i];
                        paramsList.AddParameterMax(orderComboLeg.Price);
                    }
                }
            }

            if (ibClientConnection.ServerVersion >= MinServerVer.SMART_COMBO_ROUTING_PARAMS && isBag)
            {
                List<TagValue> smartComboRoutingParams = order.SmartComboRoutingParams;
                int smartComboRoutingParamsCount = smartComboRoutingParams == null ? 0 : smartComboRoutingParams.Count;
                paramsList.AddParameter(smartComboRoutingParamsCount);
                if (smartComboRoutingParamsCount > 0)
                {
                    for (int i = 0; i < smartComboRoutingParamsCount; ++i)
                    {
                        TagValue tagValue = smartComboRoutingParams[i];
                        paramsList.AddParameter(tagValue.Tag);
                        paramsList.AddParameter(tagValue.Value);
                    }
                }
            }

            if (ibClientConnection.ServerVersion >= 9)
            {
                // paramsList.AddParameter deprecated sharesAllocation field
                paramsList.AddParameter("");
            }

            if (ibClientConnection.ServerVersion >= 10)
            {
                paramsList.AddParameter(order.DiscretionaryAmt);
            }

            if (ibClientConnection.ServerVersion >= 11)
            {
                paramsList.AddParameter(order.GoodAfterTime);
            }

            if (ibClientConnection.ServerVersion >= 12)
            {
                paramsList.AddParameter(order.GoodTillDate);
            }

            if (ibClientConnection.ServerVersion >= 13)
            {
                paramsList.AddParameter(order.FaGroup);
                paramsList.AddParameter(order.FaMethod);
                paramsList.AddParameter(order.FaPercentage);
                paramsList.AddParameter(order.FaProfile);
            }
            if (ibClientConnection.ServerVersion >= 18)
            { // institutional short sale slot fields.
                paramsList.AddParameter(order.ShortSaleSlot);      // 0 only for retail, 1 or 2 only for institution.
                paramsList.AddParameter(order.DesignatedLocation); // only populate when order.shortSaleSlot = 2.
            }
            if (ibClientConnection.ServerVersion >= MinServerVer.SSHORTX_OLD)
            {
                paramsList.AddParameter(order.ExemptCode);
            }
            if (ibClientConnection.ServerVersion >= 19)
            {
                paramsList.AddParameter(order.OcaType);
                if (ibClientConnection.ServerVersion < 38)
                {
                    // will never happen
                    paramsList.AddParameter( /* order.rthOnly */ false);
                }
                paramsList.AddParameter(order.Rule80A);
                paramsList.AddParameter(order.SettlingFirm);
                paramsList.AddParameter(order.AllOrNone);
                paramsList.AddParameterMax(order.MinQty);
                paramsList.AddParameterMax(order.PercentOffset);
                paramsList.AddParameter(order.ETradeOnly);
                paramsList.AddParameter(order.FirmQuoteOnly);
                paramsList.AddParameterMax(order.NbboPriceCap);
                paramsList.AddParameterMax(order.AuctionStrategy);
                paramsList.AddParameterMax(order.StartingPrice);
                paramsList.AddParameterMax(order.StockRefPrice);
                paramsList.AddParameterMax(order.Delta);
                // Volatility orders had specific watermark price attribs in server version 26
                double lower = (ibClientConnection.ServerVersion == 26 && order.OrderType.Equals("VOL"))
                     ? Double.MaxValue
                     : order.StockRangeLower;
                double upper = (ibClientConnection.ServerVersion == 26 && order.OrderType.Equals("VOL"))
                     ? Double.MaxValue
                     : order.StockRangeUpper;
                paramsList.AddParameterMax(lower);
                paramsList.AddParameterMax(upper);
            }

            if (ibClientConnection.ServerVersion >= 22)
            {
                paramsList.AddParameter(order.OverridePercentageConstraints);
            }

            if (ibClientConnection.ServerVersion >= 26)
            { // Volatility orders
                paramsList.AddParameterMax(order.Volatility);
                paramsList.AddParameterMax(order.VolatilityType);
                if (ibClientConnection.ServerVersion < 28)
                {
                    bool isDeltaNeutralTypeMKT = (String.Compare("MKT", order.DeltaNeutralOrderType, true) == 0);
                    paramsList.AddParameter(isDeltaNeutralTypeMKT);
                }
                else
                {
                    paramsList.AddParameter(order.DeltaNeutralOrderType);
                    paramsList.AddParameterMax(order.DeltaNeutralAuxPrice);

                    if (ibClientConnection.ServerVersion >= MinServerVer.DELTA_NEUTRAL_CONID && !String.IsNullOrEmpty(order.DeltaNeutralOrderType))
                    {
                        paramsList.AddParameter(order.DeltaNeutralConId);
                        paramsList.AddParameter(order.DeltaNeutralSettlingFirm);
                        paramsList.AddParameter(order.DeltaNeutralClearingAccount);
                        paramsList.AddParameter(order.DeltaNeutralClearingIntent);
                    }

                    if (ibClientConnection.ServerVersion >= MinServerVer.DELTA_NEUTRAL_OPEN_CLOSE && !String.IsNullOrEmpty(order.DeltaNeutralOrderType))
                    {
                        paramsList.AddParameter(order.DeltaNeutralOpenClose);
                        paramsList.AddParameter(order.DeltaNeutralShortSale);
                        paramsList.AddParameter(order.DeltaNeutralShortSaleSlot);
                        paramsList.AddParameter(order.DeltaNeutralDesignatedLocation);
                    }
                }
                paramsList.AddParameter(order.ContinuousUpdate);
                if (ibClientConnection.ServerVersion == 26)
                {
                    // Volatility orders had specific watermark price attribs in server version 26
                    double lower = order.OrderType.Equals("VOL") ? order.StockRangeLower : Double.MaxValue;
                    double upper = order.OrderType.Equals("VOL") ? order.StockRangeUpper : Double.MaxValue;
                    paramsList.AddParameterMax(lower);
                    paramsList.AddParameterMax(upper);
                }
                paramsList.AddParameterMax(order.ReferencePriceType);
            }

            if (ibClientConnection.ServerVersion >= 30)
            { // TRAIL_STOP_LIMIT stop price
                paramsList.AddParameterMax(order.TrailStopPrice);
            }

            if (ibClientConnection.ServerVersion >= MinServerVer.TRAILING_PERCENT)
            {
                paramsList.AddParameterMax(order.TrailingPercent);
            }

            if (ibClientConnection.ServerVersion >= MinServerVer.SCALE_ORDERS)
            {
                if (ibClientConnection.ServerVersion >= MinServerVer.SCALE_ORDERS2)
                {
                    paramsList.AddParameterMax(order.ScaleInitLevelSize);
                    paramsList.AddParameterMax(order.ScaleSubsLevelSize);
                }
                else
                {
                    paramsList.AddParameter("");
                    paramsList.AddParameterMax(order.ScaleInitLevelSize);

                }
                paramsList.AddParameterMax(order.ScalePriceIncrement);
            }

            if (ibClientConnection.ServerVersion >= MinServerVer.SCALE_ORDERS3 && order.ScalePriceIncrement > 0.0 && order.ScalePriceIncrement != Double.MaxValue)
            {
                paramsList.AddParameterMax(order.ScalePriceAdjustValue);
                paramsList.AddParameterMax(order.ScalePriceAdjustInterval);
                paramsList.AddParameterMax(order.ScaleProfitOffset);
                paramsList.AddParameter(order.ScaleAutoReset);
                paramsList.AddParameterMax(order.ScaleInitPosition);
                paramsList.AddParameterMax(order.ScaleInitFillQty);
                paramsList.AddParameter(order.ScaleRandomPercent);
            }

            if (ibClientConnection.ServerVersion >= MinServerVer.SCALE_TABLE)
            {
                paramsList.AddParameter(order.ScaleTable);
                paramsList.AddParameter(order.ActiveStartTime);
                paramsList.AddParameter(order.ActiveStopTime);
            }

            if (ibClientConnection.ServerVersion >= MinServerVer.HEDGE_ORDERS)
            {
                paramsList.AddParameter(order.HedgeType);
                if (!String.IsNullOrEmpty(order.HedgeType))
                {
                    paramsList.AddParameter(order.HedgeParam);
                }
            }

            if (ibClientConnection.ServerVersion >= MinServerVer.OPT_OUT_SMART_ROUTING)
            {
                paramsList.AddParameter(order.OptOutSmartRouting);
            }

            if (ibClientConnection.ServerVersion >= MinServerVer.PTA_ORDERS)
            {
                paramsList.AddParameter(order.ClearingAccount);
                paramsList.AddParameter(order.ClearingIntent);
            }

            if (ibClientConnection.ServerVersion >= MinServerVer.NOT_HELD)
            {
                paramsList.AddParameter(order.NotHeld);
            }

            if (ibClientConnection.ServerVersion >= MinServerVer.UNDER_COMP)
            {
                if (contract.UnderComp != null)
                {
                    UnderComp underComp = contract.UnderComp;
                    paramsList.AddParameter(true);
                    paramsList.AddParameter(underComp.ConId);
                    paramsList.AddParameter(underComp.Delta);
                    paramsList.AddParameter(underComp.Price);
                }
                else
                {
                    paramsList.AddParameter(false);
                }
            }

            if (ibClientConnection.ServerVersion >= MinServerVer.ALGO_ORDERS)
            {
                paramsList.AddParameter(order.AlgoStrategy);
                if (!String.IsNullOrEmpty(order.AlgoStrategy))
                {
                    List<TagValue> algoParams = order.AlgoParams;
                    int algoParamsCount = algoParams == null ? 0 : algoParams.Count;
                    paramsList.AddParameter(algoParamsCount);
                    if (algoParamsCount > 0)
                    {
                        for (int i = 0; i < algoParamsCount; ++i)
                        {
                            TagValue tagValue = (TagValue)algoParams[i];
                            paramsList.AddParameter(tagValue.Tag);
                            paramsList.AddParameter(tagValue.Value);
                        }
                    }
                }
            }

            if (ibClientConnection.ServerVersion >= MinServerVer.ALGO_ID)
            {
                paramsList.AddParameter(order.AlgoId);
            }

            if (ibClientConnection.ServerVersion >= MinServerVer.WHAT_IF_ORDERS)
            {
                paramsList.AddParameter(order.WhatIf);
            }

            if (ibClientConnection.ServerVersion >= MinServerVer.LINKING)
            {
                //int orderOptionsCount = order.OrderMiscOptions == null ? 0 : order.OrderMiscOptions.Count;
                //paramsList.AddParameter(orderOptionsCount);
                paramsList.AddParameter(TagValueListToString(order.OrderMiscOptions));
            }

            ibClientConnection.IbWriter.Send(id, paramsList, EClientErrors.FAIL_SEND_ORDER);
        }

        /// <summary>
        /// Cancels an active order.
        /// </summary>
        /// <param name="orderId">The order's client id</param>
        public void CancelOrder(int orderId)
        {
            CheckConnection();
            ibClientConnection.IbWriter.SendCancelRequest(OutgoingMessages.CancelOrder, 1, orderId, EClientErrors.FAIL_SEND_CORDER);
        }

        /// <summary>
        /// Requests all open orders places by this specific API client (identified by the API client id).
        /// </summary>
        public void ReqOpenOrders()
        {
            int VERSION = 1;
            CheckConnection();
            List<byte> paramsList = new List<byte>();
            paramsList.AddParameter(OutgoingMessages.RequestOpenOrders);
            paramsList.AddParameter(VERSION);
            ibClientConnection.IbWriter.Send(paramsList, EClientErrors.FAIL_SEND_OORDER);
        }

        /// <summary>
        /// Requests all open orders submitted by any API client as well as those directly placed in the TWS. The existing orders will be received via the openOrder and orderStatus events.
        /// </summary>
        public void ReqAllOpenOrders()
        {
            int VERSION = 1;
            CheckConnection();
            List<byte> paramsList = new List<byte>();
            paramsList.AddParameter(OutgoingMessages.RequestAllOpenOrders);
            paramsList.AddParameter(VERSION);
            ibClientConnection.IbWriter.Send(paramsList, EClientErrors.FAIL_SEND_OORDER);
        }

        /// <summary>
        /// Requests all order placed on the TWS directly. Only the orders created after this request has been made will be returned.
        /// </summary>
        /// <param name="autoBind">If set to true, the newly created orders will be implicitely associated with this client.</param>
        public void ReqAutoOpenOrders(bool autoBind)
        {
            int VERSION = 1;
            CheckConnection();
            List<byte> paramsList = new List<byte>();
            paramsList.AddParameter(OutgoingMessages.RequestAutoOpenOrders);
            paramsList.AddParameter(VERSION);
            paramsList.AddParameter(autoBind);
            ibClientConnection.IbWriter.Send(paramsList, EClientErrors.FAIL_SEND_OORDER);
        }

        /// <summary>
        /// Requests the next valid order id.
        /// </summary>
        /// <param name="numIds">Deprecate</param>
        public void ReqIds(int numIds)
        {
            CheckConnection();
            const int VERSION = 1;

            List<byte> paramsList = new List<byte>();
            paramsList.AddParameter(OutgoingMessages.RequestIds);
            paramsList.AddParameter(VERSION);
            paramsList.AddParameter(numIds);
            ibClientConnection.IbWriter.Send(paramsList, EClientErrors.FAIL_GENERIC);
        }

        /// <summary>
        /// Exercises your options.
        /// </summary>
        /// <param name="tickerId">Exercise request's identifier.</param>
        /// <param name="contract">The option Contract to be exercised.</param>
        /// <param name="exerciseAction">Set to 1 to exercise the option, set to 2 to let the option lapse.</param>
        /// <param name="exerciseQuantity">Number of contracts to be exercised.</param>
        /// <param name="account">Destination account.</param>
        /// <param name="ovrd">Specifies whether your setting will override the system's natural action. For example, if your action is "exercise" and the option is not in-the-money, by natural action the option would not exercise. If you have override set to "yes" the natural action would be overridden and the out-of-the money option would be exercised. Set to 1 to override, set to 0 not to.</param>
        public void ExerciseOptions(int tickerId, Contract contract, int exerciseAction, int exerciseQuantity, string account, int ovrd)
        {
            //WARN needs to be tested!
            CheckConnection();
            CheckServerVersion(21, " It does not support options exercise from the API.");
            if ((!Util.StringIsEmpty(contract.TradingClass) || contract.ConId > 0))
            {
                CheckServerVersion(MinServerVer.TRADING_CLASS, " It does not support conId not tradingClass parameter when exercising options.");
            }

            int VERSION = 2;
            List<byte> paramsList = new List<byte>();
            paramsList.AddParameter(OutgoingMessages.ExerciseOptions);
            paramsList.AddParameter(VERSION);
            paramsList.AddParameter(tickerId);

            if (ibClientConnection.ServerVersion >= MinServerVer.TRADING_CLASS)
            {
                paramsList.AddParameter(contract.ConId);
            }
            paramsList.AddParameter(contract.Symbol);
            paramsList.AddParameter(contract.SecType);
            paramsList.AddParameter(contract.Expiry);
            paramsList.AddParameter(contract.Strike);
            paramsList.AddParameter(contract.Right);
            paramsList.AddParameter(contract.Multiplier);
            paramsList.AddParameter(contract.Exchange);
            paramsList.AddParameter(contract.Currency);
            paramsList.AddParameter(contract.LocalSymbol);
            if (ibClientConnection.ServerVersion >= MinServerVer.TRADING_CLASS)
            {
                paramsList.AddParameter(contract.TradingClass);
            }
            paramsList.AddParameter(exerciseAction);
            paramsList.AddParameter(exerciseQuantity);
            paramsList.AddParameter(account);
            paramsList.AddParameter(ovrd);

            ibClientConnection.IbWriter.Send(paramsList, EClientErrors.FAIL_GENERIC);
        }

        /// <summary>
        /// Cancels all the active orders. This method will cancel ALL open orders included those placed directly via the TWS.
        /// </summary>
        public void ReqGlobalCancel()
        {
            CheckConnection();

            CheckServerVersion(MinServerVer.REQ_GLOBAL_CANCEL, "It does not support global cancel requests.");

            const int VERSION = 1;

            List<byte> paramsList = new List<byte>();
            paramsList.AddParameter(OutgoingMessages.RequestGlobalCancel);
            paramsList.AddParameter(VERSION);
            ibClientConnection.IbWriter.Send(paramsList, EClientErrors.FAIL_SEND_REQGLOBALCANCEL);
        }

        /// <summary>
        /// Subscribes to an specific account's information and portfolio. Through this method, a single account's subscription can be started/stopped. 
        /// As a result from the subscription, the account's information, portfolio and last update time will be received at UpdateAccountValueAsync, 
        /// UpdateAccountPortfolioAsync, UpdateAccountTimeAsync respectively. Only one account can be subscribed at a time. A second subscription request 
        /// for another account when the previous one is still active will cause the first one to be canceled in favour of the second one. Consider user 
        /// ReqPositions if you want to retrieve all your accounts' portfolios directly.
        /// </summary>
        /// <param name="subscribe">Set to true to start the subscription and to false to stop it.</param>
        /// <param name="acctCode">The account id (i.e. U123456) for which the information is requested.</param>
        public void ReqAccountUpdates(bool subscribe, string acctCode)
        {
            int VERSION = 2;
            CheckConnection();
            List<byte> paramsList = new List<byte>();
            paramsList.AddParameter(OutgoingMessages.RequestAccountData);
            paramsList.AddParameter(VERSION);
            paramsList.AddParameter(subscribe);
            if (ibClientConnection.ServerVersion >= 9) paramsList.AddParameter(acctCode);
            ibClientConnection.IbWriter.Send(paramsList, EClientErrors.FAIL_SEND_REQACCOUNTDATA);
        }

        /// <summary>
        /// Requests a specific account's summary.This method will subscribe to the account summary as presented in the TWS' Account Summary tab.
        /// </summary>
        /// <param name="reqId">The unique request identifier.</param>
        /// <param name="group">Set to "All" to return account summary data for all accounts, or set to a specific Advisor Account Group name that has already been created in TWS Global Configuration.</param>
        /// <param name="tags">
        /// A comma separated list with the desired tags:
        /// - AccountType
        /// - NetLiquidation,
        /// - TotalCashValue — Total cash including futures pnl
        /// - SettledCash — For cash accounts, this is the same as TotalCashValue
        /// - AccruedCash — Net accrued interest
        /// - BuyingPower — The maximum amount of marginable US stocks the account can buy
        /// - EquityWithLoanValue — Cash + stocks + bonds + mutual funds
        /// - PreviousEquityWithLoanValue,
        /// - GrossPositionValue — The sum of the absolute value of all stock and equity option positions
        /// - RegTEquity,
        /// - RegTMargin,
        /// - SMA — Special Memorandum Account
        /// - InitMarginReq,
        /// - MaintMarginReq,
        /// - AvailableFunds,
        /// - ExcessLiquidity,
        /// - Cushion — Excess liquidity as a percentage of net liquidation value
        /// - FullInitMarginReq,
        /// - FullMaintMarginReq,
        /// - FullAvailableFunds,
        /// - FullExcessLiquidity,
        /// - LookAheadNextChange — Time when look-ahead values take effect
        /// - LookAheadInitMarginReq,
        /// - LookAheadMaintMarginReq,
        /// - LookAheadAvailableFunds,
        /// - LookAheadExcessLiquidity,
        /// - HighestSeverity — A measure of how close the account is to liquidation
        /// - DayTradesRemaining — The Number of Open/Close trades a user could put on before Pattern Day Trading is detected. A value of "-1" means that the user can put on unlimited day trades.
        /// - Leverage — GrossPositionValue / NetLiquidation
        /// </param>        
        /// <response>AccountSummary, AccountSummaryEnd</response>
        public void ReqAccountSummary(int reqId, string group, string tags)
        {
            int VERSION = 1;
            CheckConnection();
            CheckServerVersion(reqId, MinServerVer.ACCT_SUMMARY, " It does not support account summary requests.");

            List<byte> paramsList = new List<byte>();
            paramsList.AddParameter(OutgoingMessages.RequestAccountSummary);
            paramsList.AddParameter(VERSION);
            paramsList.AddParameter(reqId);
            paramsList.AddParameter(group);
            paramsList.AddParameter(tags);
            ibClientConnection.IbWriter.Send(reqId, paramsList, EClientErrors.FAIL_SEND_REQACCOUNTDATA);
        }

        /// <summary>
        /// Cancels the account's summary request. After requesting an account's summary, invoke this function to cancel it.
        /// </summary>
        /// <param name="reqId">The identifier of the previously performed account request.</param>
        public void CancelAccountSummary(int reqId)
        {
            CheckConnection();
            CheckServerVersion(MinServerVer.ACCT_SUMMARY, " It does not support account summary cancellation.");
            ibClientConnection.IbWriter.SendCancelRequest(OutgoingMessages.CancelAccountSummary, 1, reqId, EClientErrors.FAIL_SEND_CANACCOUNTDATA);
        }

        /// <summary>
        /// Requests all positions from all accounts.
        /// </summary>
        public void ReqPositions()
        {
            CheckConnection();
            CheckServerVersion(MinServerVer.ACCT_SUMMARY, " It does not support position requests.");

            const int VERSION = 1;
            List<byte> paramsList = new List<byte>();
            paramsList.AddParameter(OutgoingMessages.RequestPositions);
            paramsList.AddParameter(VERSION);
            ibClientConnection.IbWriter.Send(paramsList, EClientErrors.FAIL_SEND_REQPOSITIONS);
        }

        /// <summary>
        /// Cancels all account's positions request.
        /// </summary>
        public void CancelPositions()
        {
            CheckConnection();
            CheckServerVersion(MinServerVer.ACCT_SUMMARY, " It does not support position cancellation.");
            ibClientConnection.IbWriter.SendCancelRequest(OutgoingMessages.CancelPositions, 1, EClientErrors.FAIL_SEND_CANPOSITIONS);
        }

        /// <summary>
        /// Requests all the day's executions matching the filter. Only the current day's executions can be retrieved. Along with the executions, 
        /// the CommissionReport will also be returned. The execution details will arrive at ExecDetailsAsync.
        /// </summary>
        /// <param name="reqId">The request's unique identifier.</param>
        /// <param name="filter">The filter criteria used to determine which execution reports are returned.</param>
        public void ReqExecutions(int reqId, ExecutionFilter filter)
        {
            CheckConnection();

            int VERSION = 3;

            List<byte> paramsList = new List<byte>();
            paramsList.AddParameter(OutgoingMessages.RequestExecutions);
            paramsList.AddParameter(VERSION);//version

            if (ibClientConnection.ServerVersion >= MinServerVer.EXECUTION_DATA_CHAIN)
            {
                paramsList.AddParameter(reqId);
            }

            //Send the execution rpt filter data
            if (ibClientConnection.ServerVersion >= 9)
            {
                paramsList.AddParameter(filter.ClientId);
                paramsList.AddParameter(filter.AcctCode);

                // Note that the valid format for time is "yyyymmdd-hh:mm:ss"
                paramsList.AddParameter(filter.Time);
                paramsList.AddParameter(filter.Symbol);
                paramsList.AddParameter(filter.SecType);
                paramsList.AddParameter(filter.Exchange);
                paramsList.AddParameter(filter.Side);
            }
            ibClientConnection.IbWriter.Send(reqId, paramsList, EClientErrors.FAIL_SEND_EXEC);
        }

        /// <summary>
        /// Requests contract information. This method will provide all the contracts matching the contract provided. It can also be used 
        /// to retrieve complete options and futures chains. This information will be returned at ContractDetails.
        /// </summary>
        /// <param name="reqId">The unique request identifier.</param>
        /// <param name="contract">The contract used as sample to query the available contracts. Typically, it will contain the Contract::Symbol, Contract::Currency, Contract::SecType, Contract::Exchange</param>
        public void ReqContractDetails(int reqId, Contract contract)
        {
            CheckConnection();

            if (!String.IsNullOrEmpty(contract.SecIdType) || !String.IsNullOrEmpty(contract.SecId))
            {
                CheckServerVersion(reqId, MinServerVer.SEC_ID_TYPE, " It does not support secIdType not secId attributes");
            }

            if (!String.IsNullOrEmpty(contract.TradingClass))
            {
                CheckServerVersion(reqId, MinServerVer.TRADING_CLASS, " It does not support the TradingClass parameter when requesting contract details.");
            }

            int VERSION = 7;

            List<byte> paramsList = new List<byte>();
            paramsList.AddParameter(OutgoingMessages.RequestContractData);
            paramsList.AddParameter(VERSION);//version
            if (ibClientConnection.ServerVersion >= MinServerVer.CONTRACT_DATA_CHAIN)
            {
                paramsList.AddParameter(reqId);
            }
            if (ibClientConnection.ServerVersion >= MinServerVer.CONTRACT_CONID)
            {
                paramsList.AddParameter(contract.ConId);
            }
            paramsList.AddParameter(contract.Symbol);
            paramsList.AddParameter(contract.SecType);
            paramsList.AddParameter(contract.Expiry);
            paramsList.AddParameter(contract.Strike);
            paramsList.AddParameter(contract.Right);
            if (ibClientConnection.ServerVersion >= 15)
            {
                paramsList.AddParameter(contract.Multiplier);
            }
            paramsList.AddParameter(contract.Exchange);
            paramsList.AddParameter(contract.Currency);
            paramsList.AddParameter(contract.LocalSymbol);
            if (ibClientConnection.ServerVersion >= MinServerVer.TRADING_CLASS)
            {
                paramsList.AddParameter(contract.TradingClass);
            }
            if (ibClientConnection.ServerVersion >= 31)
            {
                paramsList.AddParameter(contract.IncludeExpired);
            }
            if (ibClientConnection.ServerVersion >= MinServerVer.SEC_ID_TYPE)
            {
                paramsList.AddParameter(contract.SecIdType);
                paramsList.AddParameter(contract.SecId);
            }
            ibClientConnection.IbWriter.Send(reqId, paramsList, EClientErrors.FAIL_SEND_REQCONTRACT);
        }

        /// <summary>
        /// Requests the contract's market depth (order book).
        /// </summary>
        /// <param name="tickerId">The request's identifier.</param>
        /// <param name="contract">The Contract for which the depth is being requested.</param>
        /// <param name="numRows">The number of rows on each side of the order book.</param>
        /// <param name="mktDepthOptions"></param>
        public void ReqMktDepth(int tickerId, Contract contract, int numRows, List<TagValue> mktDepthOptions)
        {
            CheckConnection();

            if (!String.IsNullOrEmpty(contract.TradingClass) || contract.ConId > 0)
            {
                CheckServerVersion(tickerId, MinServerVer.TRADING_CLASS, " It does not support ConId nor TradingClass parameters in reqMktDepth.");
            }

            const int VERSION = 5;
            List<byte> paramsList = new List<byte>();
            paramsList.AddParameter(OutgoingMessages.RequestMarketDepth);
            paramsList.AddParameter(VERSION);
            paramsList.AddParameter(tickerId);

            // paramsList.AddParameter contract fields
            if (ibClientConnection.ServerVersion >= MinServerVer.TRADING_CLASS)
            {
                paramsList.AddParameter(contract.ConId);
            }
            paramsList.AddParameter(contract.Symbol);
            paramsList.AddParameter(contract.SecType);
            paramsList.AddParameter(contract.Expiry);
            paramsList.AddParameter(contract.Strike);
            paramsList.AddParameter(contract.Right);
            if (ibClientConnection.ServerVersion >= 15)
            {
                paramsList.AddParameter(contract.Multiplier);
            }
            paramsList.AddParameter(contract.Exchange);
            paramsList.AddParameter(contract.Currency);
            paramsList.AddParameter(contract.LocalSymbol);
            if (ibClientConnection.ServerVersion >= MinServerVer.TRADING_CLASS)
            {
                paramsList.AddParameter(contract.TradingClass);
            }
            if (ibClientConnection.ServerVersion >= 19)
            {
                paramsList.AddParameter(numRows);
            }
            if (ibClientConnection.ServerVersion >= MinServerVer.LINKING)
            {
                //int tagValuesCount = mktDepthOptions == null ? 0 : mktDepthOptions.Count;
                //paramsList.AddParameter(tagValuesCount);
                paramsList.AddParameter(TagValueListToString(mktDepthOptions));
            }
            ibClientConnection.IbWriter.Send(paramsList, EClientErrors.FAIL_SEND_REQMKTDEPTH);
        }

        /// <summary>
        /// Cancel's market depth's request.
        /// </summary>
        /// <param name="tickerId">Request's identifier.</param>
        public void CancelMktDepth(int tickerId)
        {
            CheckConnection();
            ibClientConnection.IbWriter.SendCancelRequest(OutgoingMessages.CancelMarketDepth, 1, tickerId, EClientErrors.FAIL_SEND_CANMKTDEPTH);
        }

        /// <summary>
        /// Subscribes to IB's News Bulletins.
        /// </summary>
        /// <param name="allMessages">If set to true, will return all the existing bulletins for the current day, set to false to receive only the new bulletins.</param>
        public void ReqNewsBulletins(bool allMessages)
        {
            CheckConnection();

            const int VERSION = 1;
            List<byte> paramsList = new List<byte>();
            paramsList.AddParameter(OutgoingMessages.RequestNewsBulletins);
            paramsList.AddParameter(VERSION);
            paramsList.AddParameter(allMessages);
            ibClientConnection.IbWriter.Send(paramsList, EClientErrors.FAIL_GENERIC);
        }

        /// <summary>
        /// Cancels IB's news bulletin subscription.
        /// </summary>
        public void CancelNewsBulletin()
        {
            CheckConnection();
            ibClientConnection.IbWriter.SendCancelRequest(OutgoingMessages.CancelNewsBulletin, 1, EClientErrors.FAIL_SEND_CORDER);
        }

        /// <summary>
        /// Requests the accounts to which the logged user has access to.
        /// </summary>
        public void ReqManagedAccts()
        {
            CheckConnection();
            const int VERSION = 1;
            List<byte> paramsList = new List<byte>();
            paramsList.AddParameter(OutgoingMessages.RequestManagedAccounts);
            paramsList.AddParameter(VERSION);
            ibClientConnection.IbWriter.Send(paramsList, EClientErrors.FAIL_GENERIC);
        }

        /// <summary>
        /// Requests the FA configuration. A Financial Advisor can define three different configurations:
        /// 1. Groups: offer traders a way to create a group of accounts and apply a single allocation method to all accounts in the group.
        /// 2. Profiles: let you allocate shares on an account-by-account basis using a predefined calculation value.
        /// 3. Account Aliases: let you easily identify the accounts by meaningful names rather than account numbers.
        /// More information at https://www.interactivebrokers.com/en/?f=%2Fen%2Fsoftware%2Fpdfhighlights%2FPDF-AdvisorAllocations.php%3Fib_entity%3Dllc
        /// </summary>
        /// <param name="faDataType">The configuration to change. Set to 1, 2 or 3 as defined above.</param>
        public void RequestFA(int faDataType)
        {
            CheckConnection();
            const int VERSION = 1;
            List<byte> paramsList = new List<byte>();
            paramsList.AddParameter(OutgoingMessages.RequestFA);
            paramsList.AddParameter(VERSION);
            paramsList.AddParameter(faDataType);
            ibClientConnection.IbWriter.Send(paramsList, EClientErrors.FAIL_SEND_FA_REQUEST);
        }

        /// <summary>
        /// Replaces Financial Advisor's settings. A Financial Advisor can define three different configurations:
        /// 1. Groups: offer traders a way to create a group of accounts and apply a single allocation method to all accounts in the group.
        /// 2. Profiles: let you allocate shares on an account-by-account basis using a predefined calculation value.
        /// 3. Account Aliases: let you easily identify the accounts by meaningful names rather than account numbers.
        /// More information at https://www.interactivebrokers.com/en/?f=%2Fen%2Fsoftware%2Fpdfhighlights%2FPDF-AdvisorAllocations.php%3Fib_entity%3Dllc
        /// </summary>
        /// <param name="faDataType">The configuration to change. Set to 1, 2 or 3 as defined above.</param>
        /// <param name="xml">The xml-formatted configuration string</param>
        /// <remarks>WARNING: IB has not tested this yet!</remarks>
        public void ReplaceFA(int faDataType, string xml)
        {
            CheckConnection();

            List<byte> paramsList = new List<byte>();
            paramsList.AddParameter(OutgoingMessages.ReplaceFA);
            paramsList.AddParameter(1);
            paramsList.AddParameter(faDataType);
            paramsList.AddParameter(xml);
            ibClientConnection.IbWriter.Send(paramsList, EClientErrors.FAIL_SEND_FA_REPLACE);
        }

        /// <summary>
        /// Requests all possible parameters which can be used for a scanner subscription.
        /// </summary>
        public void ReqScannerParameters()
        {
            CheckConnection();
            const int VERSION = 1;
            List<byte> paramsList = new List<byte>();
            paramsList.AddParameter(OutgoingMessages.RequestScannerParameters);
            paramsList.AddParameter(VERSION);
            ibClientConnection.IbWriter.Send(paramsList, EClientErrors.FAIL_SEND_REQSCANNERPARAMETERS);
        }

        /// <summary>
        /// Starts a subscription to market scan results based on the provided parameters.
        /// </summary>
        /// <param name="reqId">The request's identifier.</param>
        /// <param name="subscription">Summary of the scanner subscription including its filters.</param>
        /// <param name="scannerSubscriptionOptions"></param>
        public void ReqScannerSubscription(int reqId, ScannerSubscription subscription, List<TagValue> scannerSubscriptionOptions)
        {
            CheckConnection();
            const int VERSION = 4;
            List<byte> paramsList = new List<byte>();
            paramsList.AddParameter(OutgoingMessages.RequestScannerSubscription);
            paramsList.AddParameter(VERSION);
            paramsList.AddParameter(reqId);
            paramsList.AddParameterMax(subscription.NumberOfRows);
            paramsList.AddParameter(subscription.Instrument);
            paramsList.AddParameter(subscription.LocationCode);
            paramsList.AddParameter(subscription.ScanCode);
            paramsList.AddParameterMax(subscription.AbovePrice);
            paramsList.AddParameterMax(subscription.BelowPrice);
            paramsList.AddParameterMax(subscription.AboveVolume);
            paramsList.AddParameterMax(subscription.MarketCapAbove);
            paramsList.AddParameterMax(subscription.MarketCapBelow);
            paramsList.AddParameter(subscription.MoodyRatingAbove);
            paramsList.AddParameter(subscription.MoodyRatingBelow);
            paramsList.AddParameter(subscription.SpRatingAbove);
            paramsList.AddParameter(subscription.SpRatingBelow);
            paramsList.AddParameter(subscription.MaturityDateAbove);
            paramsList.AddParameter(subscription.MaturityDateBelow);
            paramsList.AddParameterMax(subscription.CouponRateAbove);
            paramsList.AddParameterMax(subscription.CouponRateBelow);
            paramsList.AddParameter(subscription.ExcludeConvertible);
            if (ibClientConnection.ServerVersion >= 25)
            {
                paramsList.AddParameterMax(subscription.AverageOptionVolumeAbove);
                paramsList.AddParameter(subscription.ScannerSettingPairs);
            }
            if (ibClientConnection.ServerVersion >= 27)
            {
                paramsList.AddParameter(subscription.StockTypeFilter);
            }

            if (ibClientConnection.ServerVersion >= MinServerVer.LINKING)
            {
                //int tagValuesCount = scannerSubscriptionOptions == null ? 0 : scannerSubscriptionOptions.Count;
                //paramsList.AddParameter(tagValuesCount);
                paramsList.AddParameter(TagValueListToString(scannerSubscriptionOptions));
            }

            ibClientConnection.IbWriter.Send(paramsList, EClientErrors.FAIL_SEND_REQSCANNER);
        }

        /// <summary>
        /// Cancels Scanner Subscription.
        /// </summary>
        /// <param name="tickerId">The subscription's unique identifier.</param>
        public void CancelScannerSubscription(int tickerId)
        {
            CheckConnection();
            ibClientConnection.IbWriter.SendCancelRequest(OutgoingMessages.CancelScannerSubscription, 1, tickerId, EClientErrors.FAIL_SEND_CANSCANNER);
        }

        /// <summary>
        /// Requests contracts' historical data. When requesting historical data, a finishing time and date is required along with a duration string. 
        /// For example, having endDateTime = 20130701 23:59:59 GMT and durationStr = 3 D will return three days of data counting backwards 
        /// from July 1st 2013 at 23:59:59 GMT resulting in all the available bars of the last three days until the date and time specified. 
        /// It is possible to specify a timezone optionally. The resulting bars will be returned in HistoricalDataAsync.
        /// </summary>
        /// <param name="tickerId">The request's unique identifier.</param>
        /// <param name="contract">The contract for which we want to retrieve the data.</param>
        /// <param name="endDateTime">Request's ending time with format yyyyMMdd HH:mm:ss {TMZ}.</param>
        /// <param name="durationString">
        /// The amount of time for which the data needs to be retrieved:
        /// - " S (seconds)
        /// - " D (days)
        /// - " W (weeks)
        /// - " M (months)
        /// - " Y (years)
        /// </param>
        /// <param name="barSizeSetting">
        /// The size of the bar:
        /// - 1 sec
        /// - 5 secs
        /// - 15 secs
        /// - 30 secs
        /// - 1 min
        /// - 2 mins
        /// - 3 mins
        /// - 5 mins
        /// - 15 mins
        /// - 30 mins
        /// - 1 hour
        /// - 1 day
        /// </param>
        /// <param name="whatToShow">
        /// The kind of information being retrieved:
        /// - TRADES
        /// - MIDPOINT
        /// - BID
        /// - ASK
        /// - BID_ASK
        /// - HISTORICAL_VOLATILITY
        /// - OPTION_IMPLIED_VOLATILITY
        /// </param>
        /// <param name="useRTH">Set to 0 to obtain the data which was also generated ourside of the Regular Trading Hours, set to 1 to obtain only the RTH data.</param>
        /// <param name="formatDate">Set to 1 to obtain the bars' time as yyyyMMdd HH:mm:ss, set to 2 to obtain it like system time format in seconds.</param>
        /// <param name="chartOptions"></param>
        public void ReqHistoricalData(int tickerId, Contract contract, string endDateTime, string durationString, string barSizeSetting, string whatToShow, int useRTH, int formatDate, List<TagValue> chartOptions)
        {
            CheckConnection();
            CheckServerVersion(tickerId, 16);
            if (!String.IsNullOrEmpty(contract.TradingClass) || contract.ConId > 0)
            {
                CheckServerVersion(tickerId, MinServerVer.TRADING_CLASS, " It does not support conId nor trading class parameters when requesting historical data.");
            }

            const int VERSION = 6;
            List<byte> paramsList = new List<byte>();
            paramsList.AddParameter(OutgoingMessages.RequestHistoricalData);
            paramsList.AddParameter(VERSION);
            paramsList.AddParameter(tickerId);
            if (ibClientConnection.ServerVersion >= MinServerVer.TRADING_CLASS) paramsList.AddParameter(contract.ConId);
            paramsList.AddParameter(contract.Symbol);
            paramsList.AddParameter(contract.SecType);
            paramsList.AddParameter(contract.Expiry);
            paramsList.AddParameter(contract.Strike);
            paramsList.AddParameter(contract.Right);
            paramsList.AddParameter(contract.Multiplier);
            paramsList.AddParameter(contract.Exchange);
            paramsList.AddParameter(contract.PrimaryExch);
            paramsList.AddParameter(contract.Currency);
            paramsList.AddParameter(contract.LocalSymbol);
            if (ibClientConnection.ServerVersion >= MinServerVer.TRADING_CLASS)
            {
                paramsList.AddParameter(contract.TradingClass);
            }
            paramsList.AddParameter(contract.IncludeExpired ? 1 : 0);
            paramsList.AddParameter(endDateTime);
            paramsList.AddParameter(barSizeSetting);
            paramsList.AddParameter(durationString);
            paramsList.AddParameter(useRTH);
            paramsList.AddParameter(whatToShow);
            paramsList.AddParameter(formatDate);
            if (StringsAreEqual(Constants.BagSecType, contract.SecType))
            {
                if (contract.ComboLegs == null)
                {
                    paramsList.AddParameter(0);
                }
                else
                {
                    paramsList.AddParameter(contract.ComboLegs.Count);

                    ComboLeg comboLeg;
                    for (int i = 0; i < contract.ComboLegs.Count; i++)
                    {
                        comboLeg = (ComboLeg)contract.ComboLegs[i];
                        paramsList.AddParameter(comboLeg.ConId);
                        paramsList.AddParameter(comboLeg.Ratio);
                        paramsList.AddParameter(comboLeg.Action);
                        paramsList.AddParameter(comboLeg.Exchange);
                    }
                }
            }

            if (ibClientConnection.ServerVersion >= MinServerVer.LINKING)
            {
                paramsList.AddParameter(TagValueListToString(chartOptions));
            }

            ibClientConnection.IbWriter.Send(paramsList, EClientErrors.FAIL_SEND_REQHISTDATA);
        }

        /// <summary>
        /// Cancels a historical data request.
        /// </summary>
        /// <param name="reqId">The request's identifier.</param>
        public void CancelHistoricalData(int reqId)
        {
            CheckConnection();
            CheckServerVersion(24, " It does not support historical data cancelations.");
            const int VERSION = 1;
            //No server version validation takes place here since minimum is already higher
            ibClientConnection.IbWriter.SendCancelRequest(OutgoingMessages.CancelOptionPrice, VERSION, reqId, EClientErrors.FAIL_SEND_CANHISTDATA);
        }

        /// <summary>
        /// Requests real time bars. Currently, only 5 seconds bars are provided. This request ius suject to the same pacing as any historical data request: no more than 60 API queries in more than 600 seconds.
        /// </summary>
        /// <param name="tickerId">The request's unique identifier.</param>
        /// <param name="contract">The Contract for which the depth is being requested.</param>
        /// <param name="barSize">Currently being ignored.</param>
        /// <param name="whatToShow">
        /// The nature of the data being retrieved:
        /// - TRADES
        /// - MIDPOINT
        /// - BID
        /// - ASK
        /// </param>
        /// <param name="useRTH">Set to 0 to obtain the data which was also generated ourside of the Regular Trading Hours, set to 1 to obtain only the RTH data.</param>
        /// <param name="realTimeBarsOptions"></param>
        public void ReqRealTimeBars(int tickerId, Contract contract, int barSize, string whatToShow, bool useRTH, List<TagValue> realTimeBarsOptions)
        {
            CheckConnection();
            CheckServerVersion(tickerId, MinServerVer.REAL_TIME_BARS, " It does not support real time bars.");

            if (!String.IsNullOrEmpty(contract.TradingClass) || contract.ConId > 0)
            {
                CheckServerVersion(tickerId, MinServerVer.TRADING_CLASS, " It does not support ConId nor TradingClass parameters in reqRealTimeBars.");
            }

            const int VERSION = 3;
            List<byte> paramsList = new List<byte>();
            paramsList.AddParameter(OutgoingMessages.RequestRealTimeBars);
            paramsList.AddParameter(VERSION);
            paramsList.AddParameter(tickerId);

            // paramsList.AddParameter contract fields
            if (ibClientConnection.ServerVersion >= MinServerVer.TRADING_CLASS)
            {
                paramsList.AddParameter(contract.ConId);
            }
            paramsList.AddParameter(contract.Symbol);
            paramsList.AddParameter(contract.SecType);
            paramsList.AddParameter(contract.Expiry);
            paramsList.AddParameter(contract.Strike);
            paramsList.AddParameter(contract.Right);
            paramsList.AddParameter(contract.Multiplier);
            paramsList.AddParameter(contract.Exchange);
            paramsList.AddParameter(contract.PrimaryExch);
            paramsList.AddParameter(contract.Currency);
            paramsList.AddParameter(contract.LocalSymbol);
            if (ibClientConnection.ServerVersion >= MinServerVer.TRADING_CLASS)
            {
                paramsList.AddParameter(contract.TradingClass);
            }
            paramsList.AddParameter(barSize);  // this parameter is not currently used
            paramsList.AddParameter(whatToShow);
            paramsList.AddParameter(useRTH);
            if (ibClientConnection.ServerVersion >= MinServerVer.LINKING)
            {
                paramsList.AddParameter(TagValueListToString(realTimeBarsOptions));
            }
            ibClientConnection.IbWriter.Send(paramsList, EClientErrors.FAIL_SEND_REQRTBARS);
        }

        /// <summary>
        /// Cancels Real Time Bars' subscription.
        /// </summary>
        /// <param name="tickerId">The request's identifier.</param>
        public void CancelRealTimeBars(int tickerId)
        {
            CheckConnection();
            ibClientConnection.IbWriter.SendCancelRequest(OutgoingMessages.CancelRealTimeBars, 1, tickerId, EClientErrors.FAIL_SEND_CANRTBARS);
        }

        /// <summary>
        /// Requests the contract's Reuters' global fundamental data. Reuters funalmental data will be returned at FundamentalDataAsync.
        /// </summary>
        /// <param name="reqId">The request's unique identifier.</param>
        /// <param name="contract">The contract's description for which the data will be returned.</param>
        /// <param name="reportType">
        /// There are three available report types: 
        /// - ReportSnapshot: Company overview
        /// - ReportsFinSummary: Financial summary
        /// - ReportRatios:	Financial ratios
        /// - ReportsFinStatements:	Financial statements
        /// - RESC: Analyst estimates
        /// - CalendarReport: Company calendar
        /// </param>
        /// <param name="fundamentalDataOptions"></param>
        public void ReqFundamentalData(int reqId, Contract contract, String reportType, List<TagValue> fundamentalDataOptions)
        {
            CheckConnection();
            CheckServerVersion(reqId, MinServerVer.FUNDAMENTAL_DATA, " It does not support Fundamental Data requests.");
            if (!String.IsNullOrEmpty(contract.TradingClass) || contract.ConId > 0 || !String.IsNullOrEmpty(contract.Multiplier))
            {
                CheckServerVersion(reqId, MinServerVer.TRADING_CLASS, "");
            }

            const int VERSION = 3;
            List<byte> paramsList = new List<byte>();
            paramsList.AddParameter(OutgoingMessages.RequestFundamentalData);
            paramsList.AddParameter(VERSION);
            paramsList.AddParameter(reqId);
            if (ibClientConnection.ServerVersion >= MinServerVer.TRADING_CLASS)
            {
                //WARN: why are we checking the trading class and multiplier above never send them?
                paramsList.AddParameter(contract.ConId);
            }
            paramsList.AddParameter(contract.Symbol);
            paramsList.AddParameter(contract.SecType);
            paramsList.AddParameter(contract.Exchange);
            paramsList.AddParameter(contract.PrimaryExch);
            paramsList.AddParameter(contract.Currency);
            paramsList.AddParameter(contract.LocalSymbol);
            paramsList.AddParameter(reportType);

            if (ibClientConnection.ServerVersion >= MinServerVer.LINKING)
            {
                int tagValuesCount = fundamentalDataOptions == null ? 0 : fundamentalDataOptions.Count;
                paramsList.AddParameter(tagValuesCount);
                paramsList.AddParameter(TagValueListToString(fundamentalDataOptions));
            }

            ibClientConnection.IbWriter.Send(reqId, paramsList, EClientErrors.FAIL_SEND_REQFUNDDATA);
        }

        /// <summary>
        /// Cancels Fundamental data request.
        /// </summary>
        /// <param name="reqId">The request's idenfier.</param>
        public void CancelFundamentalData(int reqId)
        {
            CheckConnection();
            CheckServerVersion(MinServerVer.FUNDAMENTAL_DATA, " It does not support fundamental data requests.");
            ibClientConnection.IbWriter.SendCancelRequest(OutgoingMessages.CancelFundamentalData, 1, reqId, EClientErrors.FAIL_SEND_CANFUNDDATA);
        }

        public void QueryDisplayGroups(int requestId)
        {
            CheckConnection();
            CheckServerVersion(MinServerVer.LINKING, " It does not support queryDisplayGroups request.");
            const int VERSION = 1;
            List<byte> paramsList = new List<byte>();
            paramsList.AddParameter(OutgoingMessages.QueryDisplayGroups);
            paramsList.AddParameter(VERSION);
            paramsList.AddParameter(requestId);
            ibClientConnection.IbWriter.Send(paramsList, EClientErrors.FAIL_SEND_QUERYDISPLAYGROUPS);
        }

        public void SubscribeToGroupEvents(int requestId, int groupId)
        {
            CheckConnection();
            CheckServerVersion(MinServerVer.LINKING, " It does not support subscribeToGroupEvents request.");
            const int VERSION = 1;
            List<byte> paramsList = new List<byte>();
            paramsList.AddParameter(OutgoingMessages.SubscribeToGroupEvents);
            paramsList.AddParameter(VERSION);
            paramsList.AddParameter(requestId);
            paramsList.AddParameter(groupId);
            ibClientConnection.IbWriter.Send(paramsList, EClientErrors.FAIL_SEND_SUBSCRIBETOGROUPEVENTS);
        }

        public void UpdateDisplayGroup(int requestId, string contractInfo)
        {
            CheckConnection();
            CheckServerVersion(MinServerVer.LINKING, " It does not support updateDisplayGroup request.");
            const int VERSION = 1;
            List<byte> paramsList = new List<byte>();
            paramsList.AddParameter(OutgoingMessages.UpdateDisplayGroup);
            paramsList.AddParameter(VERSION);
            paramsList.AddParameter(requestId);
            paramsList.AddParameter(contractInfo);
            ibClientConnection.IbWriter.Send(paramsList, EClientErrors.FAIL_SEND_UPDATEDISPLAYGROUP);
        }

        public void UnsubscribeFromGroupEvents(int requestId)
        {
            CheckConnection();
            CheckServerVersion(MinServerVer.LINKING, " It does not support unsubscribeFromGroupEvents request.");
            const int VERSION = 1;
            List<byte> paramsList = new List<byte>();
            paramsList.AddParameter(OutgoingMessages.UnsubscribeFromGroupEvents);
            paramsList.AddParameter(VERSION);
            paramsList.AddParameter(requestId);
            ibClientConnection.IbWriter.Send(paramsList, EClientErrors.FAIL_SEND_UNSUBSCRIBEFROMGROUPEVENTS);
        }

        #endregion


        #region Helper Methods

        private void CheckConnection()
        {
            if (!ibClientConnection.IsConnected)
            {
                throw new Exception(String.Format("Code: {0}, Msg: {1}", EClientErrors.NOT_CONNECTED.Code, EClientErrors.NOT_CONNECTED.Message));
            }
        }


        private void CheckServerVersion(int requestId, int requiredVersion)
        {
            CheckServerVersion(requestId, requiredVersion, "");
        }


        private void CheckServerVersion(int requiredVersion, string updatetail)
        {
            CheckServerVersion(IncomingMessage.NotValid, requiredVersion, updatetail);
        }


        private void CheckServerVersion(int tickerId, int requiredVersion, string updateTail)
        {
            if (ibClientConnection.ServerVersion < requiredVersion)
            {
                throw new Exception(String.Format("Id: {0}, Code: {1}, Msg: {2}", tickerId, EClientErrors.UPDATE_TWS.Code, EClientErrors.UPDATE_TWS.Message + updateTail));
            }
        }


        private string TagValueListToString(List<TagValue> tagValues)
        {
            StringBuilder tagValuesStr = new StringBuilder();
            int tagValuesCount = tagValues == null ? 0 : tagValues.Count;

            for (int i = 0; i < tagValuesCount; i++)
            {
                TagValue tagValue = tagValues[i];
                tagValuesStr.Append(tagValue.Tag).Append("=").Append(tagValue.Value).Append(";");
            }
            return tagValuesStr.ToString();
        }

        private bool VerifyOrder(Order order, int id, bool isBagOrder)
        {
            if (ibClientConnection.ServerVersion < MinServerVer.SCALE_ORDERS)
            {
                if (order.ScaleInitLevelSize != Int32.MaxValue || order.ScalePriceIncrement != Double.MaxValue)
                {
                    throw new Exception(String.Format("Id: {0}, Code: {1}, Msg: {2}", 
                        id, 
                        EClientErrors.UPDATE_TWS.Code, 
                        EClientErrors.UPDATE_TWS.Message + "  It does not support Scale orders."));
                }
            }
            if (ibClientConnection.ServerVersion < MinServerVer.WHAT_IF_ORDERS)
            {
                if (order.WhatIf)
                {
                    throw new Exception(String.Format("Id: {0}, Code: {1}, Msg: {2}", 
                        id, 
                        EClientErrors.UPDATE_TWS.Code, 
                        EClientErrors.UPDATE_TWS.Message + "  It does not support what-if orders."));
                }
            }

            if (ibClientConnection.ServerVersion < MinServerVer.SCALE_ORDERS2)
            {
                if (order.ScaleSubsLevelSize != Int32.MaxValue)
                {
                    throw new Exception(String.Format("Id: {0}, Code: {1}, Msg: {2}", 
                        id, 
                        EClientErrors.UPDATE_TWS.Code, 
                        EClientErrors.UPDATE_TWS.Message + "  It does not support Subsequent Level Size for Scale orders."));
                }
            }

            if (ibClientConnection.ServerVersion < MinServerVer.ALGO_ORDERS)
            {
                if (!String.IsNullOrEmpty(order.AlgoStrategy))
                {
                    throw new Exception(String.Format("Id: {0}, Code: {1}, Msg: {2}", 
                        id, 
                        EClientErrors.UPDATE_TWS.Code, 
                        EClientErrors.UPDATE_TWS.Message + "  It does not support algo orders."));
                }
            }

            if (ibClientConnection.ServerVersion < MinServerVer.NOT_HELD)
            {
                if (order.NotHeld)
                {
                    throw new Exception(String.Format("Id: {0}, Code: {1}, Msg: {2}", 
                        id, 
                        EClientErrors.UPDATE_TWS.Code, 
                        EClientErrors.UPDATE_TWS.Message + "  It does not support notHeld parameter."));
                }
            }

            if (ibClientConnection.ServerVersion < MinServerVer.SSHORTX)
            {
                if (order.ExemptCode != -1)
                {
                    throw new Exception(String.Format("Id: {0}, Code: {1}, Msg: {2}", 
                        id, 
                        EClientErrors.UPDATE_TWS.Code, 
                        EClientErrors.UPDATE_TWS.Message + "  It does not support exemptCode parameter."));
                }
            }

            if (ibClientConnection.ServerVersion < MinServerVer.HEDGE_ORDERS)
            {
                if (!String.IsNullOrEmpty(order.HedgeType))
                {
                    throw new Exception(String.Format("Id: {0}, Code: {1}, Msg: {2}",
                        id,
                        EClientErrors.UPDATE_TWS.Code,
                        EClientErrors.UPDATE_TWS.Message + "  It does not support hedge orders."));
                }
            }

            if (ibClientConnection.ServerVersion < MinServerVer.OPT_OUT_SMART_ROUTING)
            {
                if (order.OptOutSmartRouting)
                {
                    throw new Exception(String.Format("Id: {0}, Code: {1}, Msg: {2}",
                        id,
                        EClientErrors.UPDATE_TWS.Code,
                        EClientErrors.UPDATE_TWS.Message + "  It does not support optOutSmartRouting parameter."));
                }
            }

            if (ibClientConnection.ServerVersion < MinServerVer.DELTA_NEUTRAL_CONID)
            {
                if (order.DeltaNeutralConId > 0
                        || !String.IsNullOrEmpty(order.DeltaNeutralSettlingFirm)
                        || !String.IsNullOrEmpty(order.DeltaNeutralClearingAccount)
                        || !String.IsNullOrEmpty(order.DeltaNeutralClearingIntent))
                {
                    throw new Exception(String.Format("Id: {0}, Code: {1}, Msg: {2}",
                        id,
                        EClientErrors.UPDATE_TWS.Code,
                        EClientErrors.UPDATE_TWS.Message + "  It does not support deltaNeutral parameters: ConId, SettlingFirm, ClearingAccount, ClearingIntent"));
                }
            }

            if (ibClientConnection.ServerVersion < MinServerVer.DELTA_NEUTRAL_OPEN_CLOSE)
            {
                if (!String.IsNullOrEmpty(order.DeltaNeutralOpenClose)
                        || order.DeltaNeutralShortSale
                        || order.DeltaNeutralShortSaleSlot > 0
                        || !String.IsNullOrEmpty(order.DeltaNeutralDesignatedLocation)
                        )
                {
                    throw new Exception(String.Format("Id: {0}, Code: {1}, Msg: {2}",
                        id,
                        EClientErrors.UPDATE_TWS.Code,
                        EClientErrors.UPDATE_TWS.Message + "  It does not support deltaNeutral parameters: OpenClose, ShortSale, ShortSaleSlot, DesignatedLocation"));
                }
            }

            if (ibClientConnection.ServerVersion < MinServerVer.SCALE_ORDERS3)
            {
                if (order.ScalePriceIncrement > 0 && order.ScalePriceIncrement != Double.MaxValue)
                {
                    if (order.ScalePriceAdjustValue != Double.MaxValue ||
                        order.ScalePriceAdjustInterval != Int32.MaxValue ||
                        order.ScaleProfitOffset != Double.MaxValue ||
                        order.ScaleAutoReset ||
                        order.ScaleInitPosition != Int32.MaxValue ||
                        order.ScaleInitFillQty != Int32.MaxValue ||
                        order.ScaleRandomPercent)
                    {
                        throw new Exception(String.Format("Id: {0}, Code: {1}, Msg: {2}",
                            id,
                            EClientErrors.UPDATE_TWS.Code,
                            EClientErrors.UPDATE_TWS.Message + "  It does not support Scale order parameters: PriceAdjustValue, PriceAdjustInterval, ProfitOffset, AutoReset, InitPosition, InitFillQty and RandomPercent"));
                    }
                }
            }

            if (ibClientConnection.ServerVersion < MinServerVer.ORDER_COMBO_LEGS_PRICE && isBagOrder)
            {
                if (order.OrderComboLegs.Count > 0)
                {
                    OrderComboLeg orderComboLeg;
                    for (int i = 0; i < order.OrderComboLegs.Count; ++i)
                    {
                        orderComboLeg = (OrderComboLeg)order.OrderComboLegs[i];
                        if (orderComboLeg.Price != Double.MaxValue)
                        {
                            throw new Exception(String.Format("Id: {0}, Code: {1}, Msg: {2}",
                                id,
                                EClientErrors.UPDATE_TWS.Code,
                                EClientErrors.UPDATE_TWS.Message + "  It does not support per-leg prices for order combo legs."));
                        }
                    }
                }
            }

            if (ibClientConnection.ServerVersion < MinServerVer.TRAILING_PERCENT)
            {
                if (order.TrailingPercent != Double.MaxValue)
                {
                    throw new Exception(String.Format("Id: {0}, Code: {1}, Msg: {2}",
                        id,
                        EClientErrors.UPDATE_TWS.Code,
                        EClientErrors.UPDATE_TWS.Message + "  It does not support trailing percent parameter."));
                }
            }

            if (ibClientConnection.ServerVersion < MinServerVer.ALGO_ID && !String.IsNullOrEmpty(order.AlgoId))
            {
                throw new Exception(String.Format("Id: {0}, Code: {1}, Msg: {2}",
                        id,
                        EClientErrors.UPDATE_TWS.Code,
                        EClientErrors.UPDATE_TWS.Message + " It does not support algoId parameter"));
            }

            if (ibClientConnection.ServerVersion < MinServerVer.SCALE_TABLE)
            {
                if (!String.IsNullOrEmpty(order.ScaleTable) || !String.IsNullOrEmpty(order.ActiveStartTime) || !String.IsNullOrEmpty(order.ActiveStopTime))
                {
                    throw new Exception(String.Format("Id: {0}, Code: {1}, Msg: {2}",
                        id,
                        EClientErrors.UPDATE_TWS.Code,
                        EClientErrors.UPDATE_TWS.Message + "  It does not support scaleTable, activeStartTime nor activeStopTime parameters."));
                }
            }

            return true;
        }

        private bool VerifyOrderContract(Contract contract, int id)
        {
            if (ibClientConnection.ServerVersion < MinServerVer.SSHORT_COMBO_LEGS)
            {
                if (contract.ComboLegs.Count > 0)
                {
                    ComboLeg comboLeg;
                    for (int i = 0; i < contract.ComboLegs.Count; ++i)
                    {
                        comboLeg = (ComboLeg)contract.ComboLegs[i];
                        if (comboLeg.ShortSaleSlot != 0 ||
                            !String.IsNullOrEmpty(comboLeg.DesignatedLocation))
                        {
                            throw new Exception(String.Format("Id: {0}, Code: {1}, Msg: {2}",
                                id,
                                EClientErrors.UPDATE_TWS.Code,
                                EClientErrors.UPDATE_TWS.Message + "  It does not support SSHORT flag for combo legs."));
                        }
                    }
                }
            }

            if (ibClientConnection.ServerVersion < MinServerVer.UNDER_COMP)
            {
                if (contract.UnderComp != null)
                {
                    throw new Exception(String.Format("Id: {0}, Code: {1}, Msg: {2}",
                                id,
                                EClientErrors.UPDATE_TWS.Code,
                                EClientErrors.UPDATE_TWS.Message + "  It does not support delta-neutral orders."));
                }
            }

            if (ibClientConnection.ServerVersion < MinServerVer.PLACE_ORDER_CONID)
            {
                if (contract.ConId > 0)
                {
                    throw new Exception(String.Format("Id: {0}, Code: {1}, Msg: {2}",
                                id,
                                EClientErrors.UPDATE_TWS.Code,
                                EClientErrors.UPDATE_TWS.Message + "  It does not support conId parameter."));
                }
            }

            if (ibClientConnection.ServerVersion < MinServerVer.SEC_ID_TYPE)
            {
                if (!String.IsNullOrEmpty(contract.SecIdType) || !String.IsNullOrEmpty(contract.SecId))
                {
                    throw new Exception(String.Format("Id: {0}, Code: {1}, Msg: {2}",
                                id,
                                EClientErrors.UPDATE_TWS.Code,
                                EClientErrors.UPDATE_TWS.Message + "  It does not support secIdType and secId parameters."));
                }
            }
            if (ibClientConnection.ServerVersion < MinServerVer.SSHORTX)
            {
                if (contract.ComboLegs.Count > 0)
                {
                    ComboLeg comboLeg;
                    for (int i = 0; i < contract.ComboLegs.Count; ++i)
                    {
                        comboLeg = (ComboLeg)contract.ComboLegs[i];
                        if (comboLeg.ExemptCode != -1)
                        {
                            throw new Exception(String.Format("Id: {0}, Code: {1}, Msg: {2}",
                                id,
                                EClientErrors.UPDATE_TWS.Code,
                                EClientErrors.UPDATE_TWS.Message + "  It does not support exemptCode parameter."));
                        }
                    }
                }
            }
            if (ibClientConnection.ServerVersion < MinServerVer.TRADING_CLASS)
            {
                if (!String.IsNullOrEmpty(contract.TradingClass))
                {
                    throw new Exception(String.Format("Id: {0}, Code: {1}, Msg: {2}",
                                id,
                                EClientErrors.UPDATE_TWS.Code,
                                EClientErrors.UPDATE_TWS.Message + "  It does not support tradingClass parameters in placeOrder."));
                }
            }
            return true;
        }


        private bool StringsAreEqual(string a, string b)
        {
            // compare strings ignoring case
            return String.Compare(a, b, true) == 0;
        }

        #endregion
    }
}

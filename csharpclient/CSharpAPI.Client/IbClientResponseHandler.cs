using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IBApi
{
    /// <summary>
    /// Handles responses from IB Server. This class is not thread-safe.
    /// </summary>
    public class IbClientResponseHandler
    {
        private CancellationTokenSource processMessagesCts;
        private CancellationToken processMessagesCT;
        private readonly IIbClientConnection ibClientConnection;
        public bool IsProcessing { get; private set; }

        public IbClientResponseHandler(IIbClientConnection ibClientConnection)
        {
            this.ibClientConnection = ibClientConnection;
            IsProcessing = false;
        }


        public void Start()
        {
            if (IsProcessing) throw new Exception("Response handler is processing. Must stop before starting again.");
            if (!ibClientConnection.IsConnected) throw new Exception(String.Format("Code: {0}, Msg: {1}", EClientErrors.NOT_CONNECTED.Code, EClientErrors.NOT_CONNECTED.Message));

            // start thread to process incoming messages from IB Server
            processMessagesCts = new CancellationTokenSource();
            processMessagesCT = processMessagesCts.Token;
            Task.Factory.StartNew(ProcessIncomingMessages, processMessagesCT, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            IsProcessing = true;
        }


        public void Stop()
        {
            if (!IsProcessing) throw new Exception("Response handler is not processing. Must start before stopping.");
            
            // cancel task processing incoming messages
            processMessagesCts.Cancel();
            IsProcessing = false;
        }

        
        private void ProcessIncomingMessages()
        {
            int incomingMessageType = -1;
            try
            {
                while (!processMessagesCT.IsCancellationRequested)
                {
                    incomingMessageType = ibClientConnection.IbReader.ReadInt();
                    Console.WriteLine("Received message of type {0}", incomingMessageType);
                    ProcessMessage(incomingMessageType);
                }
            }
            catch (Exception e)
            {
                Stop();
                throw new Exception(String.Format("Exception processing incoming message of type {0}. Message processing has been stopped.", incomingMessageType), e);
            }
        }


        private void ProcessMessage(int incomingMessageType)
        {
            if (incomingMessageType == IncomingMessage.NotValid) return;

            switch (incomingMessageType)
            {
                case IncomingMessage.TickPrice:
                    {
                        TickPriceMessageHandler();
                        break;
                    }
                case IncomingMessage.TickSize:
                    {
                        TickSizeMessageHandler();
                        break;
                    }
                case IncomingMessage.TickString:
                    {
                        TickStringMessageHandler();
                        break;
                    }
                case IncomingMessage.TickGeneric:
                    {
                        TickGenericMessageHandler();
                        break;
                    }
                case IncomingMessage.TickEFP:
                    {
                        TickEFPMessageHandler();
                        break;
                    }
                case IncomingMessage.TickSnapshotEnd:
                    {
                        TickSnapshotEndMessageHandler();
                        break;
                    }
                case IncomingMessage.Error:
                    {
                        ErrorMessageHandler();
                        break;
                    }
                case IncomingMessage.CurrentTime:
                    {
                        CurrentTimeMessageHandler();
                        break;
                    }
                case IncomingMessage.ManagedAccounts:
                    {
                        ManagedAccountsMessageHandler();
                        break;
                    }
                case IncomingMessage.NextValidId:
                    {
                        NextValidIdMessageHandler();
                        break;
                    }
                case IncomingMessage.DeltaNeutralValidation:
                    {
                        DeltaNeutralValidationMessageHandler();
                        break;
                    }
                case IncomingMessage.TickOptionComputation:
                    {
                        TickOptionComputationMessageHandler();
                        break;
                    }
                case IncomingMessage.AccountSummary:
                    {
                        AccountSummaryMessageHandler();
                        break;
                    }
                case IncomingMessage.AccountSummaryEnd:
                    {
                        AccountSummaryEndMessageHandler();
                        break;
                    }
                case IncomingMessage.AccountValue:
                    {
                        AccountValueMessageHandler();
                        break;
                    }
                case IncomingMessage.PortfolioValue:
                    {
                        PortfolioValueMessageHandler();
                        break;
                    }
                case IncomingMessage.AccountUpdateTime:
                    {
                        AccountUpdateTimeMessageHandler();
                        break;
                    }
                case IncomingMessage.AccountDownloadEnd:
                    {
                        AccountDownloadEndMessageHandler();
                        break;
                    }
                case IncomingMessage.OrderStatus:
                    {
                        OrderStatusMessageHandler();
                        break;
                    }
                case IncomingMessage.OpenOrder:
                    {
                        OpenOrderMessageHandler();
                        break;
                    }
                case IncomingMessage.OpenOrderEnd:
                    {
                        OpenOrderEndMessageHandler();
                        break;
                    }
                case IncomingMessage.ContractData:
                    {
                        ContractDataMessageHandler();
                        break;
                    }
                case IncomingMessage.ContractDataEnd:
                    {
                        ContractDataEndMessageHandler();
                        break;
                    }
                case IncomingMessage.ExecutionData:
                    {
                        ExecutionDataMessageHandler();
                        break;
                    }
                case IncomingMessage.ExecutionDataEnd:
                    {
                        ExecutionDataEndMessageHandler();
                        break;
                    }
                case IncomingMessage.CommissionsReport:
                    {
                        CommissionReportMessageHandler();
                        break;
                    }
                case IncomingMessage.FundamentalData:
                    {
                        FundamentalDataMessageHandler();
                        break;
                    }
                case IncomingMessage.HistoricalData:
                    {
                        HistoricalDataMessageHandler();
                        break;
                    }
                case IncomingMessage.MarketDataType:
                    {
                        MarketDataTypeMessageHandler();
                        break;
                    }
                case IncomingMessage.MarketDepth:
                    {
                        MarketDepthMessageHandler();
                        break;
                    }
                case IncomingMessage.MarketDepthL2:
                    {
                        MarketDepthL2MessageHandler();
                        break;
                    }
                case IncomingMessage.NewsBulletins:
                    {
                        NewsBulletinsMessageHandler();
                        break;
                    }
                case IncomingMessage.Position:
                    {
                        PositionMessageHandler();
                        break;
                    }
                case IncomingMessage.PositionEnd:
                    {
                        PositionEndMessageHandler();
                        break;
                    }
                case IncomingMessage.RealTimeBars:
                    {
                        RealTimeBarsMessageHandler();
                        break;
                    }
                case IncomingMessage.ScannerParameters:
                    {
                        ScannerParametersMessageHandler();
                        break;
                    }
                case IncomingMessage.ScannerData:
                    {
                        ScannerDataMessageHandler();
                        break;
                    }
                case IncomingMessage.ReceiveFA:
                    {
                        ReceiveFAMessageHandler();
                        break;
                    }
                case IncomingMessage.BondContractData:
                    {
                        BondContractDataMessageHandler();
                        break;
                    }
                case IncomingMessage.VerifyMessageApi:
                    {
                        VerifyMessageApiMessageHandler();
                        break;
                    }
                case IncomingMessage.VerifyCompleted:
                    {
                        VerifyCompletedMessageHandler();
                        break;
                    }
                case IncomingMessage.DisplayGroupList:
                    {
                        DisplayGroupListMessageHandler();
                        break;
                    }
                case IncomingMessage.DisplayGroupUpdated:
                    {
                        DisplayGroupUpdatedMessageHandler();
                        break;
                    }
                default:
                    {
                        ReportError(new Error(EClientErrors.UNKNOWN_ID.Code, EClientErrors.UNKNOWN_ID.Message));
                        break;
                    }
            }
        }


        #region Async Methods

        private ConcurrentQueue<TaskCompletionSource<TickPriceMsg>> tickPriceTcsQueue = new ConcurrentQueue<TaskCompletionSource<TickPriceMsg>>();
        public Task<TickPriceMsg> TickPriceAsync()
        {
            return CreateTcsAndEnqueue<TickPriceMsg>(tickPriceTcsQueue);
        }

        private ConcurrentQueue<TaskCompletionSource<TickSizeMsg>> tickSizeTcsQueue = new ConcurrentQueue<TaskCompletionSource<TickSizeMsg>>();
        public Task<TickSizeMsg> TickSizeAsync()
        {
            return CreateTcsAndEnqueue<TickSizeMsg>(tickSizeTcsQueue);
        }

        private ConcurrentQueue<TaskCompletionSource<TickStringMsg>> tickStringTcsQueue = new ConcurrentQueue<TaskCompletionSource<TickStringMsg>>();
        public Task<TickStringMsg> TickStringAsync()
        {
            return CreateTcsAndEnqueue<TickStringMsg>(tickStringTcsQueue);
        }

        private ConcurrentQueue<TaskCompletionSource<TickGenericMsg>> tickGenericTcsQueue = new ConcurrentQueue<TaskCompletionSource<TickGenericMsg>>();
        public Task<TickGenericMsg> TickGenericAsync()
        {
            return CreateTcsAndEnqueue<TickGenericMsg>(tickGenericTcsQueue);
        }

        private ConcurrentQueue<TaskCompletionSource<TickEFPMsg>> tickEFPTcsQueue = new ConcurrentQueue<TaskCompletionSource<TickEFPMsg>>();
        public Task<TickEFPMsg> TickEFPAsync()
        {
            return CreateTcsAndEnqueue<TickEFPMsg>(tickEFPTcsQueue);
        }

        private ConcurrentQueue<TaskCompletionSource<int>> tickSnapshotEndTcsQueue = new ConcurrentQueue<TaskCompletionSource<int>>();
        public Task<int> TickSnapshotEndAsync()
        {
            return CreateTcsAndEnqueue<int>(tickSnapshotEndTcsQueue);
        }

        private ConcurrentQueue<TaskCompletionSource<Error>> errorTcsQueue = new ConcurrentQueue<TaskCompletionSource<Error>>();
        public Task<Error> ErrorAsync()
        {
            return CreateTcsAndEnqueue<Error>(errorTcsQueue);
        }

        private ConcurrentQueue<TaskCompletionSource<long>> currentTimeTcsQueue = new ConcurrentQueue<TaskCompletionSource<long>>();
        public Task<long> CurrentTimeAsync()
        {
            return CreateTcsAndEnqueue<long>(currentTimeTcsQueue);
        }

        private ConcurrentQueue<TaskCompletionSource<string>> managedAccountsTcsQueue = new ConcurrentQueue<TaskCompletionSource<string>>();
        public Task<string> ManagedAccountsAsync()
        {
            return CreateTcsAndEnqueue<string>(managedAccountsTcsQueue);
        }

        private ConcurrentQueue<TaskCompletionSource<int>> nextValidIdTcsQueue = new ConcurrentQueue<TaskCompletionSource<int>>();
        public Task<int> NextValidIdAsync()
        {
            return CreateTcsAndEnqueue<int>(nextValidIdTcsQueue);
        }

        private ConcurrentQueue<TaskCompletionSource<DeltaNeutralValidationMsg>> deltaNeutralValidationTcsQueue = new ConcurrentQueue<TaskCompletionSource<DeltaNeutralValidationMsg>>();
        public Task<DeltaNeutralValidationMsg> DeltaNeutralValidationAsync()
        {
            return CreateTcsAndEnqueue<DeltaNeutralValidationMsg>(deltaNeutralValidationTcsQueue);
        }

        private ConcurrentQueue<TaskCompletionSource<TickOptionComputationMsg>> tickOptionComputationTcsQueue = new ConcurrentQueue<TaskCompletionSource<TickOptionComputationMsg>>();
        public Task<TickOptionComputationMsg> TickOptionComputationAsync()
        {
            return CreateTcsAndEnqueue<TickOptionComputationMsg>(tickOptionComputationTcsQueue);
        }

        private ConcurrentQueue<TaskCompletionSource<AccountSummaryMsg>> accountSummaryTcsQueue = new ConcurrentQueue<TaskCompletionSource<AccountSummaryMsg>>();
        public Task<AccountSummaryMsg> AccountSummaryAsync()
        {
            return CreateTcsAndEnqueue<AccountSummaryMsg>(accountSummaryTcsQueue);
        }

        private ConcurrentQueue<TaskCompletionSource<int>> accountSummaryEndTcsQueue = new ConcurrentQueue<TaskCompletionSource<int>>();
        public Task<int> AccountSummaryEndAsync()
        {
            return CreateTcsAndEnqueue<int>(accountSummaryEndTcsQueue);
        }

        private ConcurrentQueue<TaskCompletionSource<UpdateAccountValueMsg>> updateAccountValueTcsQueue = new ConcurrentQueue<TaskCompletionSource<UpdateAccountValueMsg>>();
        public Task<UpdateAccountValueMsg> UpdateAccountValueAsync()
        {
            return CreateTcsAndEnqueue<UpdateAccountValueMsg>(updateAccountValueTcsQueue);
        }

        private ConcurrentQueue<TaskCompletionSource<UpdatePortfolioMsg>> updatePortfolioTcsQueue = new ConcurrentQueue<TaskCompletionSource<UpdatePortfolioMsg>>();
        public Task<UpdatePortfolioMsg> UpdatePortfolioAsync()
        {
            return CreateTcsAndEnqueue<UpdatePortfolioMsg>(updatePortfolioTcsQueue);
        }

        private ConcurrentQueue<TaskCompletionSource<string>> updateAccountTimeTcsQueue = new ConcurrentQueue<TaskCompletionSource<string>>();
        public Task<string> UpdateAccountTimeAsync()
        {
            return CreateTcsAndEnqueue<string>(updateAccountTimeTcsQueue);
        }

        private ConcurrentQueue<TaskCompletionSource<string>> accountDownloadEndTcsQueue = new ConcurrentQueue<TaskCompletionSource<string>>();
        public Task<string> AccountDownloadEndAsync()
        {
            return CreateTcsAndEnqueue<string>(accountDownloadEndTcsQueue);
        }

        private ConcurrentQueue<TaskCompletionSource<OrderStatusMsg>> orderStatusTcsQueue = new ConcurrentQueue<TaskCompletionSource<OrderStatusMsg>>();
        public Task<OrderStatusMsg> OrderStatusAsync()
        {
            return CreateTcsAndEnqueue<OrderStatusMsg>(orderStatusTcsQueue);
        }

        private ConcurrentQueue<TaskCompletionSource<OpenOrderMsg>> openOrderTcsQueue = new ConcurrentQueue<TaskCompletionSource<OpenOrderMsg>>();
        public Task<OpenOrderMsg> OpenOrderAsync()
        {
            return CreateTcsAndEnqueue<OpenOrderMsg>(openOrderTcsQueue);
        }

        private ConcurrentQueue<TaskCompletionSource<object>> openOrderEndTcsQueue = new ConcurrentQueue<TaskCompletionSource<object>>();
        public Task<object> OpenOrderEndAsync()
        {
            return CreateTcsAndEnqueue<object>(openOrderEndTcsQueue);
        }

        private ConcurrentQueue<TaskCompletionSource<ContractDetailsMsg>> contractDetailsTcsQueue = new ConcurrentQueue<TaskCompletionSource<ContractDetailsMsg>>();
        public Task<ContractDetailsMsg> ContractDetailsAsync()
        {
            return CreateTcsAndEnqueue<ContractDetailsMsg>(contractDetailsTcsQueue);
        }

        private ConcurrentQueue<TaskCompletionSource<int>> contractDetailsEndTcsQueue = new ConcurrentQueue<TaskCompletionSource<int>>();
        public Task<int> ContractDetailsEndAsync()
        {
            return CreateTcsAndEnqueue<int>(contractDetailsEndTcsQueue);
        }

        private ConcurrentQueue<TaskCompletionSource<ExecDetailsMsg>> execDetailsTcsQueue = new ConcurrentQueue<TaskCompletionSource<ExecDetailsMsg>>();
        public Task<ExecDetailsMsg> ExecDetailsAsync()
        {
            return CreateTcsAndEnqueue<ExecDetailsMsg>(execDetailsTcsQueue);
        }

        private ConcurrentQueue<TaskCompletionSource<int>> execDetailsEndTcsQueue = new ConcurrentQueue<TaskCompletionSource<int>>();
        public Task<int> ExecDetailsEndAsync()
        {
            return CreateTcsAndEnqueue<int>(execDetailsEndTcsQueue);
        }

        private ConcurrentQueue<TaskCompletionSource<CommissionReportMsg>> commissionReportTcsQueue = new ConcurrentQueue<TaskCompletionSource<CommissionReportMsg>>();
        public Task<CommissionReportMsg> CommissionReportAsync()
        {
            return CreateTcsAndEnqueue<CommissionReportMsg>(commissionReportTcsQueue);
        }

        private ConcurrentQueue<TaskCompletionSource<FundamentalDataMsg>> fundamentalDataTcsQueue = new ConcurrentQueue<TaskCompletionSource<FundamentalDataMsg>>();
        public Task<FundamentalDataMsg> FundamentalDataAsync()
        {
            return CreateTcsAndEnqueue<FundamentalDataMsg>(fundamentalDataTcsQueue);
        }

        private ConcurrentQueue<TaskCompletionSource<HistoricalDataMsg>> historicalDataTcsQueue = new ConcurrentQueue<TaskCompletionSource<HistoricalDataMsg>>();
        public Task<HistoricalDataMsg> HistoricalDataAsync()
        {
            return CreateTcsAndEnqueue<HistoricalDataMsg>(historicalDataTcsQueue);
        }

        private ConcurrentQueue<TaskCompletionSource<HistoricalDataEndMsg>> historicalDataEndTcsQueue = new ConcurrentQueue<TaskCompletionSource<HistoricalDataEndMsg>>();
        public Task<HistoricalDataEndMsg> HistoricalDataEndAsync()
        {
            return CreateTcsAndEnqueue<HistoricalDataEndMsg>(historicalDataEndTcsQueue);
        }

        private ConcurrentQueue<TaskCompletionSource<MarketDataTypeMsg>> marketDataTypeTcsQueue = new ConcurrentQueue<TaskCompletionSource<MarketDataTypeMsg>>();
        public Task<MarketDataTypeMsg> MarketDataTypeAsync()
        {
            return CreateTcsAndEnqueue<MarketDataTypeMsg>(marketDataTypeTcsQueue);
        }

        private ConcurrentQueue<TaskCompletionSource<UpdateMktDepthMsg>> updateMktDepthTcsQueue = new ConcurrentQueue<TaskCompletionSource<UpdateMktDepthMsg>>();
        public Task<UpdateMktDepthMsg> UpdateMktDepthAsync()
        {
            return CreateTcsAndEnqueue<UpdateMktDepthMsg>(updateMktDepthTcsQueue);
        }

        private ConcurrentQueue<TaskCompletionSource<UpdateMktDepthL2Msg>> updateMktDepthL2TcsQueue = new ConcurrentQueue<TaskCompletionSource<UpdateMktDepthL2Msg>>();
        public Task<UpdateMktDepthL2Msg> UpdateMktDepthL2Async()
        {
            return CreateTcsAndEnqueue<UpdateMktDepthL2Msg>(updateMktDepthL2TcsQueue);
        }

        private ConcurrentQueue<TaskCompletionSource<UpdateNewsBulletinMsg>> updateNewsBulletinTcsQueue = new ConcurrentQueue<TaskCompletionSource<UpdateNewsBulletinMsg>>();
        public Task<UpdateNewsBulletinMsg> UpdateNewsBulletinAsync()
        {
            return CreateTcsAndEnqueue<UpdateNewsBulletinMsg>(updateNewsBulletinTcsQueue);
        }

        private ConcurrentQueue<TaskCompletionSource<PositionMsg>> positionTcsQueue = new ConcurrentQueue<TaskCompletionSource<PositionMsg>>();
        public Task<PositionMsg> PositionAsync()
        {
            return CreateTcsAndEnqueue<PositionMsg>(positionTcsQueue);
        }

        private ConcurrentQueue<TaskCompletionSource<object>> positionEndTcsQueue = new ConcurrentQueue<TaskCompletionSource<object>>();
        public Task<object> PositionEndAsync()
        {
            return CreateTcsAndEnqueue<object>(positionEndTcsQueue);
        }

        private ConcurrentQueue<TaskCompletionSource<RealtimeBarMsg>> realtimeBarTcsQueue = new ConcurrentQueue<TaskCompletionSource<RealtimeBarMsg>>();
        public Task<RealtimeBarMsg> RealtimeBarAsync()
        {
            return CreateTcsAndEnqueue<RealtimeBarMsg>(realtimeBarTcsQueue);
        }

        private ConcurrentQueue<TaskCompletionSource<string>> scannerParametersTcsQueue = new ConcurrentQueue<TaskCompletionSource<string>>();
        public Task<string> ScannerParametersAsync()
        {
            return CreateTcsAndEnqueue<string>(scannerParametersTcsQueue);
        }

        private ConcurrentQueue<TaskCompletionSource<ScannerDataMsg>> scannerDataTcsQueue = new ConcurrentQueue<TaskCompletionSource<ScannerDataMsg>>();
        public Task<ScannerDataMsg> ScannerDataAsync()
        {
            return CreateTcsAndEnqueue<ScannerDataMsg>(scannerDataTcsQueue);
        }

        private ConcurrentQueue<TaskCompletionSource<int>> scannerDataEndTcsQueue = new ConcurrentQueue<TaskCompletionSource<int>>();
        public Task<int> ScannerDataEndAsync()
        {
            return CreateTcsAndEnqueue<int>(scannerDataEndTcsQueue);
        }

        private ConcurrentQueue<TaskCompletionSource<ReceiveFAMsg>> receiveFATcsQueue = new ConcurrentQueue<TaskCompletionSource<ReceiveFAMsg>>();
        public Task<ReceiveFAMsg> ReceiveFAAsync()
        {
            return CreateTcsAndEnqueue<ReceiveFAMsg>(receiveFATcsQueue);
        }

        private ConcurrentQueue<TaskCompletionSource<BondContractDetailsMsg>> bondContractDetailsTcsQueue = new ConcurrentQueue<TaskCompletionSource<BondContractDetailsMsg>>();
        public Task<BondContractDetailsMsg> BondContractDetailsAsync()
        {
            return CreateTcsAndEnqueue<BondContractDetailsMsg>(bondContractDetailsTcsQueue);
        }

        private ConcurrentQueue<TaskCompletionSource<string>> verifyMessageApiTcsQueue = new ConcurrentQueue<TaskCompletionSource<string>>();
        public Task<string> VerifyMessageApiAsync()
        {
            return CreateTcsAndEnqueue<string>(verifyMessageApiTcsQueue);
        }

        private ConcurrentQueue<TaskCompletionSource<VerifyCompletedMsg>> verifyCompletedTcsQueue = new ConcurrentQueue<TaskCompletionSource<VerifyCompletedMsg>>();
        public Task<VerifyCompletedMsg> VerifyCompletedAsync()
        {
            return CreateTcsAndEnqueue<VerifyCompletedMsg>(verifyCompletedTcsQueue);
        }

        private ConcurrentQueue<TaskCompletionSource<DisplayGroupListMsg>> displayGroupListTcsQueue = new ConcurrentQueue<TaskCompletionSource<DisplayGroupListMsg>>();
        public Task<DisplayGroupListMsg> DisplayGroupListAsync()
        {
            return CreateTcsAndEnqueue<DisplayGroupListMsg>(displayGroupListTcsQueue);
        }

        private ConcurrentQueue<TaskCompletionSource<DisplayGroupUpdatedMsg>> displayGroupUpdatedTcsQueue = new ConcurrentQueue<TaskCompletionSource<DisplayGroupUpdatedMsg>>();
        public Task<DisplayGroupUpdatedMsg> DisplayGroupUpdatedAsync()
        {
            return CreateTcsAndEnqueue<DisplayGroupUpdatedMsg>(displayGroupUpdatedTcsQueue);
        }

        #endregion


        #region Message Handlers

        private void TickPriceMessageHandler()
        {
            TickPriceMsg tickPrice;
            int msgVersion = ibClientConnection.IbReader.ReadInt();
            tickPrice.TickerId = ibClientConnection.IbReader.ReadInt();
            tickPrice.Field = ibClientConnection.IbReader.ReadInt(); // TickType
            tickPrice.Price = ibClientConnection.IbReader.ReadDouble();
            int size = 0;
            if (msgVersion >= 2) size = ibClientConnection.IbReader.ReadInt();
            if (msgVersion >= 3) tickPrice.CanAutoExecute = ibClientConnection.IbReader.ReadInt();
            else tickPrice.CanAutoExecute = 0;

            DequeueTcsAndSetResult<TickPriceMsg>(tickPrice, tickPriceTcsQueue);

            if (msgVersion >= 2)
            {
                int sizeTickType = -1;//not a tick
                switch (tickPrice.Field)
                {
                    case 1:
                        sizeTickType = 0;//BID_SIZE
                        break;
                    case 2:
                        sizeTickType = 3;//ASK_SIZE
                        break;
                    case 4:
                        sizeTickType = 5;//LAST_SIZE
                        break;
                }
                if (sizeTickType != -1)
                {
                    TickSizeMsg tickSize;
                    tickSize.TickerId = tickPrice.TickerId;
                    tickSize.Field = sizeTickType;
                    tickSize.Size = size;
                    DequeueTcsAndSetResult<TickSizeMsg>(tickSize, tickSizeTcsQueue);
                }
            }
        }

        private void TickSizeMessageHandler()
        {
            TickSizeMsg tickSize;
            int msgVersion = ibClientConnection.IbReader.ReadInt();
            tickSize.TickerId = ibClientConnection.IbReader.ReadInt();
            tickSize.Field = ibClientConnection.IbReader.ReadInt();
            tickSize.Size = ibClientConnection.IbReader.ReadInt();
            DequeueTcsAndSetResult<TickSizeMsg>(tickSize, tickSizeTcsQueue);
        }

        private void TickStringMessageHandler()
        {
            TickStringMsg tickString;
            int msgVersion = ibClientConnection.IbReader.ReadInt();
            tickString.TickerId = ibClientConnection.IbReader.ReadInt();
            tickString.TickType = ibClientConnection.IbReader.ReadInt();
            tickString.Value = ibClientConnection.IbReader.ReadString();
            DequeueTcsAndSetResult<TickStringMsg>(tickString, tickStringTcsQueue);
        }

        private void TickGenericMessageHandler()
        {
            TickGenericMsg tickGeneric;
            int msgVersion = ibClientConnection.IbReader.ReadInt();
            tickGeneric.TickerId = ibClientConnection.IbReader.ReadInt();
            tickGeneric.Field = ibClientConnection.IbReader.ReadInt();
            tickGeneric.Value = ibClientConnection.IbReader.ReadDouble();
            DequeueTcsAndSetResult<TickGenericMsg>(tickGeneric, tickGenericTcsQueue);
        }

        private void TickEFPMessageHandler()
        {
            TickEFPMsg tickEFP;
            int msgVersion = ibClientConnection.IbReader.ReadInt();
            tickEFP.TickerId = ibClientConnection.IbReader.ReadInt();
            tickEFP.TickType = ibClientConnection.IbReader.ReadInt();
            tickEFP.BasisPoints = ibClientConnection.IbReader.ReadDouble();
            tickEFP.FormattedBasisPoints = ibClientConnection.IbReader.ReadString();
            tickEFP.ImpliedFuture = ibClientConnection.IbReader.ReadDouble();
            tickEFP.HoldDays = ibClientConnection.IbReader.ReadInt();
            tickEFP.FutureExpiry = ibClientConnection.IbReader.ReadString();
            tickEFP.DividendImpact = ibClientConnection.IbReader.ReadDouble();
            tickEFP.DividendsToExpiry = ibClientConnection.IbReader.ReadDouble();
            DequeueTcsAndSetResult<TickEFPMsg>(tickEFP, tickEFPTcsQueue);
        }

        private void TickSnapshotEndMessageHandler()
        {
            int msgVersion = ibClientConnection.IbReader.ReadInt();
            int requestId = ibClientConnection.IbReader.ReadInt();
            DequeueTcsAndSetResult<int>(requestId, tickSnapshotEndTcsQueue);
        }

        private void ErrorMessageHandler()
        {
            var error = new Error();
            int msgVersion = ibClientConnection.IbReader.ReadInt();
            if (msgVersion < 2)
            {
                error.ErrorMessage = ibClientConnection.IbReader.ReadString();
            }
            else
            {
                error.RequestId = ibClientConnection.IbReader.ReadInt();
                error.ErrorCode = ibClientConnection.IbReader.ReadInt();
                error.ErrorMessage = ibClientConnection.IbReader.ReadString();
            }
            DequeueTcsAndSetResult<Error>(error, errorTcsQueue);
        }

        private void CurrentTimeMessageHandler()
        {
            int msgVersion = ibClientConnection.IbReader.ReadInt();
            DequeueTcsAndSetResult<long>(ibClientConnection.IbReader.ReadLong(), currentTimeTcsQueue);
        }

        private void ManagedAccountsMessageHandler()
        {
            int msgVersion = ibClientConnection.IbReader.ReadInt();
            string accountsList = ibClientConnection.IbReader.ReadString();
            DequeueTcsAndSetResult<string>(accountsList, managedAccountsTcsQueue);
        }

        private void NextValidIdMessageHandler()
        {
            int msgVersion = ibClientConnection.IbReader.ReadInt();
            int orderId = ibClientConnection.IbReader.ReadInt();
            DequeueTcsAndSetResult<int>(orderId, nextValidIdTcsQueue);
        }

        private void DeltaNeutralValidationMessageHandler()
        {
            DeltaNeutralValidationMsg dnv;
            int msgVersion = ibClientConnection.IbReader.ReadInt();
            dnv.ReqId = ibClientConnection.IbReader.ReadInt();
            dnv.UnderComp = new UnderComp();
            dnv.UnderComp.ConId = ibClientConnection.IbReader.ReadInt();
            dnv.UnderComp.Delta = ibClientConnection.IbReader.ReadDouble();
            dnv.UnderComp.Price = ibClientConnection.IbReader.ReadDouble();
            DequeueTcsAndSetResult<DeltaNeutralValidationMsg>(dnv, deltaNeutralValidationTcsQueue);
        }

        private void TickOptionComputationMessageHandler()
        {
            TickOptionComputationMsg toc;
            int msgVersion = ibClientConnection.IbReader.ReadInt();
            toc.TickerId = ibClientConnection.IbReader.ReadInt(); // req id
            toc.Field = ibClientConnection.IbReader.ReadInt(); // tick type
            toc.ImpliedVolatility = ibClientConnection.IbReader.ReadDouble();
            if (toc.ImpliedVolatility < 0) toc.ImpliedVolatility = Double.MaxValue;
            toc.Delta = ibClientConnection.IbReader.ReadDouble();
            if (Math.Abs(toc.Delta) > 1) toc.Delta = Double.MaxValue;
            toc.OptPrice = Double.MaxValue;
            toc.PvDividend = Double.MaxValue;
            toc.Gamma = Double.MaxValue;
            toc.Vega = Double.MaxValue;
            toc.Theta = Double.MaxValue;
            toc.UndPrice = Double.MaxValue;
            if (msgVersion >= 6 || toc.Field == TickType.MODEL_OPTION)
            {
                toc.OptPrice = ibClientConnection.IbReader.ReadDouble();
                if (toc.OptPrice < 0)
                { // -1 is the "not yet computed" indicator
                    toc.OptPrice = Double.MaxValue;
                }
                toc.PvDividend = ibClientConnection.IbReader.ReadDouble();
                if (toc.PvDividend < 0)
                { // -1 is the "not yet computed" indicator
                    toc.PvDividend = Double.MaxValue;
                }
            }
            if (msgVersion >= 6)
            {
                toc.Gamma = ibClientConnection.IbReader.ReadDouble();
                if (Math.Abs(toc.Gamma) > 1)
                { // -2 is the "not yet computed" indicator
                    toc.Gamma = Double.MaxValue;
                }
                toc.Vega = ibClientConnection.IbReader.ReadDouble();
                if (Math.Abs(toc.Vega) > 1)
                { // -2 is the "not yet computed" indicator
                    toc.Vega = Double.MaxValue;
                }
                toc.Theta = ibClientConnection.IbReader.ReadDouble();
                if (Math.Abs(toc.Theta) > 1)
                { // -2 is the "not yet computed" indicator
                    toc.Theta = Double.MaxValue;
                }
                toc.UndPrice = ibClientConnection.IbReader.ReadDouble();
                if (toc.UndPrice < 0)
                { // -1 is the "not yet computed" indicator
                    toc.UndPrice = Double.MaxValue;
                }
            }
            DequeueTcsAndSetResult<TickOptionComputationMsg>(toc, tickOptionComputationTcsQueue);
        }

        private void AccountSummaryMessageHandler()
        {
            AccountSummaryMsg accountSummary;
            int msgVersion = ibClientConnection.IbReader.ReadInt();
            accountSummary.ReqId = ibClientConnection.IbReader.ReadInt();
            accountSummary.Account = ibClientConnection.IbReader.ReadString();
            accountSummary.Tag = ibClientConnection.IbReader.ReadString();
            accountSummary.Value = ibClientConnection.IbReader.ReadString();
            accountSummary.Currency = ibClientConnection.IbReader.ReadString();
            DequeueTcsAndSetResult<AccountSummaryMsg>(accountSummary, accountSummaryTcsQueue);
        }

        private void AccountSummaryEndMessageHandler()
        {
            int msgVersion = ibClientConnection.IbReader.ReadInt();
            int requestId = ibClientConnection.IbReader.ReadInt();
            DequeueTcsAndSetResult<int>(requestId, accountSummaryEndTcsQueue);
        }

        private void AccountValueMessageHandler()
        {
            UpdateAccountValueMsg uav;
            int msgVersion = ibClientConnection.IbReader.ReadInt();
            uav.Key = ibClientConnection.IbReader.ReadString();
            uav.Value = ibClientConnection.IbReader.ReadString();
            uav.Currency = ibClientConnection.IbReader.ReadString();
            uav.AccountName = null;
            if (msgVersion >= 2) uav.AccountName = ibClientConnection.IbReader.ReadString();
            DequeueTcsAndSetResult<UpdateAccountValueMsg>(uav, updateAccountValueTcsQueue);
        }

        private void PortfolioValueMessageHandler()
        {
            UpdatePortfolioMsg up;
            int msgVersion = ibClientConnection.IbReader.ReadInt();
            up.Contract = new Contract();
            if (msgVersion >= 6) up.Contract.ConId = ibClientConnection.IbReader.ReadInt();
            up.Contract.Symbol = ibClientConnection.IbReader.ReadString();
            up.Contract.SecType = ibClientConnection.IbReader.ReadString();
            up.Contract.Expiry = ibClientConnection.IbReader.ReadString();
            up.Contract.Strike = ibClientConnection.IbReader.ReadDouble();
            up.Contract.Right = ibClientConnection.IbReader.ReadString();
            if (msgVersion >= 7)
            {
                up.Contract.Multiplier = ibClientConnection.IbReader.ReadString();
                up.Contract.PrimaryExch = ibClientConnection.IbReader.ReadString();
            }
            up.Contract.Currency = ibClientConnection.IbReader.ReadString();
            if (msgVersion >= 2)
            {
                up.Contract.LocalSymbol = ibClientConnection.IbReader.ReadString();
            }
            if (msgVersion >= 8)
            {
                up.Contract.TradingClass = ibClientConnection.IbReader.ReadString();
            }

            up.Position = ibClientConnection.IbReader.ReadInt();
            up.MarketPrice = ibClientConnection.IbReader.ReadDouble();
            up.MarketValue = ibClientConnection.IbReader.ReadDouble();
            up.AverageCost = 0.0;
            up.UnrealizedPNL = 0.0;
            up.RealizedPNL = 0.0;
            if (msgVersion >= 3)
            {
                up.AverageCost = ibClientConnection.IbReader.ReadDouble();
                up.UnrealizedPNL = ibClientConnection.IbReader.ReadDouble();
                up.RealizedPNL = ibClientConnection.IbReader.ReadDouble();
            }

            up.AccountName = null;
            if (msgVersion >= 4)
            {
                up.AccountName = ibClientConnection.IbReader.ReadString();
            }

            if (msgVersion == 6 && ibClientConnection.ServerVersion == 39)
            {
                up.Contract.PrimaryExch = ibClientConnection.IbReader.ReadString();
            }
            DequeueTcsAndSetResult<UpdatePortfolioMsg>(up, updatePortfolioTcsQueue);
        }

        private void AccountUpdateTimeMessageHandler()
        {
            int msgVersion = ibClientConnection.IbReader.ReadInt();
            string timestamp = ibClientConnection.IbReader.ReadString();
            DequeueTcsAndSetResult<string>(timestamp, updateAccountTimeTcsQueue);
        }

        private void AccountDownloadEndMessageHandler()
        {
            int msgVersion = ibClientConnection.IbReader.ReadInt();
            string account = ibClientConnection.IbReader.ReadString();
            DequeueTcsAndSetResult<string>(account, accountDownloadEndTcsQueue);
        }

        private void OrderStatusMessageHandler()
        {
            OrderStatusMsg orderStatus;
            int msgVersion = ibClientConnection.IbReader.ReadInt();
            orderStatus.OrderId = ibClientConnection.IbReader.ReadInt();
            orderStatus.Status = ibClientConnection.IbReader.ReadString();
            orderStatus.Filled = ibClientConnection.IbReader.ReadInt();
            orderStatus.Remaining = ibClientConnection.IbReader.ReadInt();
            orderStatus.AvgFillPrice = ibClientConnection.IbReader.ReadDouble();

            orderStatus.PermId = 0;
            if (msgVersion >= 2)
            {
                orderStatus.PermId = ibClientConnection.IbReader.ReadInt();
            }

            orderStatus.ParentId = 0;
            if (msgVersion >= 3)
            {
                orderStatus.ParentId = ibClientConnection.IbReader.ReadInt();
            }

            orderStatus.LastFillPrice = 0;
            if (msgVersion >= 4)
            {
                orderStatus.LastFillPrice = ibClientConnection.IbReader.ReadDouble();
            }

            orderStatus.ClientId = 0;
            if (msgVersion >= 5)
            {
                orderStatus.ClientId = ibClientConnection.IbReader.ReadInt();
            }

            orderStatus.WhyHeld = null;
            if (msgVersion >= 6)
            {
                orderStatus.WhyHeld = ibClientConnection.IbReader.ReadString();
            }
            DequeueTcsAndSetResult<OrderStatusMsg>(orderStatus, orderStatusTcsQueue);
        }

        private void OpenOrderMessageHandler()
        {
            int msgVersion = ibClientConnection.IbReader.ReadInt();
            // read order id
            Order order = new Order();
            order.OrderId = ibClientConnection.IbReader.ReadInt();

            // read contract fields
            Contract contract = new Contract();
            if (msgVersion >= 17)
            {
                contract.ConId = ibClientConnection.IbReader.ReadInt();
            }
            contract.Symbol = ibClientConnection.IbReader.ReadString();
            contract.SecType = ibClientConnection.IbReader.ReadString();
            contract.Expiry = ibClientConnection.IbReader.ReadString();
            contract.Strike = ibClientConnection.IbReader.ReadDouble();
            contract.Right = ibClientConnection.IbReader.ReadString();
            if (msgVersion >= 32)
            {
                contract.Multiplier = ibClientConnection.IbReader.ReadString();
            }
            contract.Exchange = ibClientConnection.IbReader.ReadString();
            contract.Currency = ibClientConnection.IbReader.ReadString();
            if (msgVersion >= 2)
            {
                contract.LocalSymbol = ibClientConnection.IbReader.ReadString();
            }
            if (msgVersion >= 32)
            {
                contract.TradingClass = ibClientConnection.IbReader.ReadString();
            }

            // read order fields
            order.Action = ibClientConnection.IbReader.ReadString();
            order.TotalQuantity = ibClientConnection.IbReader.ReadInt();
            order.OrderType = ibClientConnection.IbReader.ReadString();
            if (msgVersion < 29)
            {
                order.LmtPrice = ibClientConnection.IbReader.ReadDouble();
            }
            else
            {
                order.LmtPrice = ibClientConnection.IbReader.ReadDoubleMax();
            }
            if (msgVersion < 30)
            {
                order.AuxPrice = ibClientConnection.IbReader.ReadDouble();
            }
            else
            {
                order.AuxPrice = ibClientConnection.IbReader.ReadDoubleMax();
            }
            order.Tif = ibClientConnection.IbReader.ReadString();
            order.OcaGroup = ibClientConnection.IbReader.ReadString();
            order.Account = ibClientConnection.IbReader.ReadString();
            order.OpenClose = ibClientConnection.IbReader.ReadString();
            order.Origin = ibClientConnection.IbReader.ReadInt();
            order.OrderRef = ibClientConnection.IbReader.ReadString();

            if (msgVersion >= 3)
            {
                order.ClientId = ibClientConnection.IbReader.ReadInt();
            }

            if (msgVersion >= 4)
            {
                order.PermId = ibClientConnection.IbReader.ReadInt();
                if (msgVersion < 18)
                {
                    // will never happen
                    /* order.ignoreRth = */
                    ibClientConnection.IbReader.ReadBoolFromInt();
                }
                else
                {
                    order.OutsideRth = ibClientConnection.IbReader.ReadBoolFromInt();
                }
                order.Hidden = ibClientConnection.IbReader.ReadInt() == 1;
                order.DiscretionaryAmt = ibClientConnection.IbReader.ReadDouble();
            }

            if (msgVersion >= 5)
            {
                order.GoodAfterTime = ibClientConnection.IbReader.ReadString();
            }

            if (msgVersion >= 6)
            {
                // skip deprecated sharesAllocation field
                ibClientConnection.IbReader.ReadString();
            }

            if (msgVersion >= 7)
            {
                order.FaGroup = ibClientConnection.IbReader.ReadString();
                order.FaMethod = ibClientConnection.IbReader.ReadString();
                order.FaPercentage = ibClientConnection.IbReader.ReadString();
                order.FaProfile = ibClientConnection.IbReader.ReadString();
            }

            if (msgVersion >= 8)
            {
                order.GoodTillDate = ibClientConnection.IbReader.ReadString();
            }

            if (msgVersion >= 9)
            {
                order.Rule80A = ibClientConnection.IbReader.ReadString();
                order.PercentOffset = ibClientConnection.IbReader.ReadDoubleMax();
                order.SettlingFirm = ibClientConnection.IbReader.ReadString();
                order.ShortSaleSlot = ibClientConnection.IbReader.ReadInt();
                order.DesignatedLocation = ibClientConnection.IbReader.ReadString();
                if (ibClientConnection.ServerVersion == 51)
                {
                    ibClientConnection.IbReader.ReadInt(); // exemptCode
                }
                else if (msgVersion >= 23)
                {
                    order.ExemptCode = ibClientConnection.IbReader.ReadInt();
                }
                order.AuctionStrategy = ibClientConnection.IbReader.ReadInt();
                order.StartingPrice = ibClientConnection.IbReader.ReadDoubleMax();
                order.StockRefPrice = ibClientConnection.IbReader.ReadDoubleMax();
                order.Delta = ibClientConnection.IbReader.ReadDoubleMax();
                order.StockRangeLower = ibClientConnection.IbReader.ReadDoubleMax();
                order.StockRangeUpper = ibClientConnection.IbReader.ReadDoubleMax();
                order.DisplaySize = ibClientConnection.IbReader.ReadInt();
                if (msgVersion < 18)
                {
                    // will never happen
                    /* order.rthOnly = */
                    ibClientConnection.IbReader.ReadBoolFromInt();
                }
                order.BlockOrder = ibClientConnection.IbReader.ReadBoolFromInt();
                order.SweepToFill = ibClientConnection.IbReader.ReadBoolFromInt();
                order.AllOrNone = ibClientConnection.IbReader.ReadBoolFromInt();
                order.MinQty = ibClientConnection.IbReader.ReadIntMax();
                order.OcaType = ibClientConnection.IbReader.ReadInt();
                order.ETradeOnly = ibClientConnection.IbReader.ReadBoolFromInt();
                order.FirmQuoteOnly = ibClientConnection.IbReader.ReadBoolFromInt();
                order.NbboPriceCap = ibClientConnection.IbReader.ReadDoubleMax();
            }

            if (msgVersion >= 10)
            {
                order.ParentId = ibClientConnection.IbReader.ReadInt();
                order.TriggerMethod = ibClientConnection.IbReader.ReadInt();
            }

            if (msgVersion >= 11)
            {
                order.Volatility = ibClientConnection.IbReader.ReadDoubleMax();
                order.VolatilityType = ibClientConnection.IbReader.ReadInt();
                if (msgVersion == 11)
                {
                    int receivedInt = ibClientConnection.IbReader.ReadInt();
                    order.DeltaNeutralOrderType = ((receivedInt == 0) ? "NONE" : "MKT");
                }
                else
                { // msgVersion 12 and up
                    order.DeltaNeutralOrderType = ibClientConnection.IbReader.ReadString();
                    order.DeltaNeutralAuxPrice = ibClientConnection.IbReader.ReadDoubleMax();

                    if (msgVersion >= 27 && !Util.StringIsEmpty(order.DeltaNeutralOrderType))
                    {
                        order.DeltaNeutralConId = ibClientConnection.IbReader.ReadInt();
                        order.DeltaNeutralSettlingFirm = ibClientConnection.IbReader.ReadString();
                        order.DeltaNeutralClearingAccount = ibClientConnection.IbReader.ReadString();
                        order.DeltaNeutralClearingIntent = ibClientConnection.IbReader.ReadString();
                    }

                    if (msgVersion >= 31 && !Util.StringIsEmpty(order.DeltaNeutralOrderType))
                    {
                        order.DeltaNeutralOpenClose = ibClientConnection.IbReader.ReadString();
                        order.DeltaNeutralShortSale = ibClientConnection.IbReader.ReadBoolFromInt();
                        order.DeltaNeutralShortSaleSlot = ibClientConnection.IbReader.ReadInt();
                        order.DeltaNeutralDesignatedLocation = ibClientConnection.IbReader.ReadString();
                    }
                }
                order.ContinuousUpdate = ibClientConnection.IbReader.ReadInt();
                if (ibClientConnection.ServerVersion == 26)
                {
                    order.StockRangeLower = ibClientConnection.IbReader.ReadDouble();
                    order.StockRangeUpper = ibClientConnection.IbReader.ReadDouble();
                }
                order.ReferencePriceType = ibClientConnection.IbReader.ReadInt();
            }

            if (msgVersion >= 13)
            {
                order.TrailStopPrice = ibClientConnection.IbReader.ReadDoubleMax();
            }

            if (msgVersion >= 30)
            {
                order.TrailingPercent = ibClientConnection.IbReader.ReadDoubleMax();
            }

            if (msgVersion >= 14)
            {
                order.BasisPoints = ibClientConnection.IbReader.ReadDoubleMax();
                order.BasisPointsType = ibClientConnection.IbReader.ReadIntMax();
                contract.ComboLegsDescription = ibClientConnection.IbReader.ReadString();
            }

            if (msgVersion >= 29)
            {
                int comboLegsCount = ibClientConnection.IbReader.ReadInt();
                if (comboLegsCount > 0)
                {
                    contract.ComboLegs = new List<ComboLeg>(comboLegsCount);
                    for (int i = 0; i < comboLegsCount; ++i)
                    {
                        int conId = ibClientConnection.IbReader.ReadInt();
                        int ratio = ibClientConnection.IbReader.ReadInt();
                        String action = ibClientConnection.IbReader.ReadString();
                        String exchange = ibClientConnection.IbReader.ReadString();
                        int openClose = ibClientConnection.IbReader.ReadInt();
                        int shortSaleSlot = ibClientConnection.IbReader.ReadInt();
                        String designatedLocation = ibClientConnection.IbReader.ReadString();
                        int exemptCode = ibClientConnection.IbReader.ReadInt();

                        ComboLeg comboLeg = new ComboLeg(conId, ratio, action, exchange, openClose,
                                shortSaleSlot, designatedLocation, exemptCode);
                        contract.ComboLegs.Add(comboLeg);
                    }
                }

                int orderComboLegsCount = ibClientConnection.IbReader.ReadInt();
                if (orderComboLegsCount > 0)
                {
                    order.OrderComboLegs = new List<OrderComboLeg>(orderComboLegsCount);
                    for (int i = 0; i < orderComboLegsCount; ++i)
                    {
                        double price = ibClientConnection.IbReader.ReadDoubleMax();

                        OrderComboLeg orderComboLeg = new OrderComboLeg(price);
                        order.OrderComboLegs.Add(orderComboLeg);
                    }
                }
            }

            if (msgVersion >= 26)
            {
                int smartComboRoutingParamsCount = ibClientConnection.IbReader.ReadInt();
                if (smartComboRoutingParamsCount > 0)
                {
                    order.SmartComboRoutingParams = new List<TagValue>(smartComboRoutingParamsCount);
                    for (int i = 0; i < smartComboRoutingParamsCount; ++i)
                    {
                        TagValue tagValue = new TagValue();
                        tagValue.Tag = ibClientConnection.IbReader.ReadString();
                        tagValue.Value = ibClientConnection.IbReader.ReadString();
                        order.SmartComboRoutingParams.Add(tagValue);
                    }
                }
            }

            if (msgVersion >= 15)
            {
                if (msgVersion >= 20)
                {
                    order.ScaleInitLevelSize = ibClientConnection.IbReader.ReadIntMax();
                    order.ScaleSubsLevelSize = ibClientConnection.IbReader.ReadIntMax();
                }
                else
                {
                    /* int notSuppScaleNumComponents = */
                    ibClientConnection.IbReader.ReadIntMax();
                    order.ScaleInitLevelSize = ibClientConnection.IbReader.ReadIntMax();
                }
                order.ScalePriceIncrement = ibClientConnection.IbReader.ReadDoubleMax();
            }

            if (msgVersion >= 28 && order.ScalePriceIncrement > 0.0 && order.ScalePriceIncrement != Double.MaxValue)
            {
                order.ScalePriceAdjustValue = ibClientConnection.IbReader.ReadDoubleMax();
                order.ScalePriceAdjustInterval = ibClientConnection.IbReader.ReadIntMax();
                order.ScaleProfitOffset = ibClientConnection.IbReader.ReadDoubleMax();
                order.ScaleAutoReset = ibClientConnection.IbReader.ReadBoolFromInt();
                order.ScaleInitPosition = ibClientConnection.IbReader.ReadIntMax();
                order.ScaleInitFillQty = ibClientConnection.IbReader.ReadIntMax();
                order.ScaleRandomPercent = ibClientConnection.IbReader.ReadBoolFromInt();
            }

            if (msgVersion >= 24)
            {
                order.HedgeType = ibClientConnection.IbReader.ReadString();
                if (!Util.StringIsEmpty(order.HedgeType))
                {
                    order.HedgeParam = ibClientConnection.IbReader.ReadString();
                }
            }

            if (msgVersion >= 25)
            {
                order.OptOutSmartRouting = ibClientConnection.IbReader.ReadBoolFromInt();
            }

            if (msgVersion >= 19)
            {
                order.ClearingAccount = ibClientConnection.IbReader.ReadString();
                order.ClearingIntent = ibClientConnection.IbReader.ReadString();
            }

            if (msgVersion >= 22)
            {
                order.NotHeld = ibClientConnection.IbReader.ReadBoolFromInt();
            }

            if (msgVersion >= 20)
            {
                if (ibClientConnection.IbReader.ReadBoolFromInt())
                {
                    UnderComp underComp = new UnderComp();
                    underComp.ConId = ibClientConnection.IbReader.ReadInt();
                    underComp.Delta = ibClientConnection.IbReader.ReadDouble();
                    underComp.Price = ibClientConnection.IbReader.ReadDouble();
                    contract.UnderComp = underComp;
                }
            }

            if (msgVersion >= 21)
            {
                order.AlgoStrategy = ibClientConnection.IbReader.ReadString();
                if (!Util.StringIsEmpty(order.AlgoStrategy))
                {
                    int algoParamsCount = ibClientConnection.IbReader.ReadInt();
                    if (algoParamsCount > 0)
                    {
                        order.AlgoParams = new List<TagValue>(algoParamsCount);
                        for (int i = 0; i < algoParamsCount; ++i)
                        {
                            TagValue tagValue = new TagValue();
                            tagValue.Tag = ibClientConnection.IbReader.ReadString();
                            tagValue.Value = ibClientConnection.IbReader.ReadString();
                            order.AlgoParams.Add(tagValue);
                        }
                    }
                }
            }

            OrderState orderState = new OrderState();
            if (msgVersion >= 16)
            {
                order.WhatIf = ibClientConnection.IbReader.ReadBoolFromInt();
                orderState.Status = ibClientConnection.IbReader.ReadString();
                orderState.InitMargin = ibClientConnection.IbReader.ReadString();
                orderState.MaintMargin = ibClientConnection.IbReader.ReadString();
                orderState.EquityWithLoan = ibClientConnection.IbReader.ReadString();
                orderState.Commission = ibClientConnection.IbReader.ReadDoubleMax();
                orderState.MinCommission = ibClientConnection.IbReader.ReadDoubleMax();
                orderState.MaxCommission = ibClientConnection.IbReader.ReadDoubleMax();
                orderState.CommissionCurrency = ibClientConnection.IbReader.ReadString();
                orderState.WarningText = ibClientConnection.IbReader.ReadString();
            }

            OpenOrderMsg openOrder;
            openOrder.OrderId = order.OrderId;
            openOrder.Contract = contract;
            openOrder.Order = order;
            openOrder.OrderState = orderState;
            DequeueTcsAndSetResult<OpenOrderMsg>(openOrder, openOrderTcsQueue);
        }

        private void OpenOrderEndMessageHandler()
        {
            int msgVersion = ibClientConnection.IbReader.ReadInt();
            DequeueTcsAndSetResult<object>(null, openOrderEndTcsQueue);
        }

        private void ContractDataMessageHandler()
        {
            int msgVersion = ibClientConnection.IbReader.ReadInt();
            int requestId = -1;
            if (msgVersion >= 3) requestId = ibClientConnection.IbReader.ReadInt();
            ContractDetails contract = new ContractDetails();
            contract.Summary.Symbol = ibClientConnection.IbReader.ReadString();
            contract.Summary.SecType = ibClientConnection.IbReader.ReadString();
            contract.Summary.Expiry = ibClientConnection.IbReader.ReadString();
            contract.Summary.Strike = ibClientConnection.IbReader.ReadDouble();
            contract.Summary.Right = ibClientConnection.IbReader.ReadString();
            contract.Summary.Exchange = ibClientConnection.IbReader.ReadString();
            contract.Summary.Currency = ibClientConnection.IbReader.ReadString();
            contract.Summary.LocalSymbol = ibClientConnection.IbReader.ReadString();
            contract.MarketName = ibClientConnection.IbReader.ReadString();
            contract.Summary.TradingClass = ibClientConnection.IbReader.ReadString();
            contract.Summary.ConId = ibClientConnection.IbReader.ReadInt();
            contract.MinTick = ibClientConnection.IbReader.ReadDouble();
            contract.Summary.Multiplier = ibClientConnection.IbReader.ReadString();
            contract.OrderTypes = ibClientConnection.IbReader.ReadString();
            contract.ValidExchanges = ibClientConnection.IbReader.ReadString();
            if (msgVersion >= 2)
            {
                contract.PriceMagnifier = ibClientConnection.IbReader.ReadInt();
            }
            if (msgVersion >= 4)
            {
                contract.UnderConId = ibClientConnection.IbReader.ReadInt();
            }
            if (msgVersion >= 5)
            {
                contract.LongName = ibClientConnection.IbReader.ReadString();
                contract.Summary.PrimaryExch = ibClientConnection.IbReader.ReadString();
            }
            if (msgVersion >= 6)
            {
                contract.ContractMonth = ibClientConnection.IbReader.ReadString();
                contract.Industry = ibClientConnection.IbReader.ReadString();
                contract.Category = ibClientConnection.IbReader.ReadString();
                contract.Subcategory = ibClientConnection.IbReader.ReadString();
                contract.TimeZoneId = ibClientConnection.IbReader.ReadString();
                contract.TradingHours = ibClientConnection.IbReader.ReadString();
                contract.LiquidHours = ibClientConnection.IbReader.ReadString();
            }
            if (msgVersion >= 8)
            {
                contract.EvRule = ibClientConnection.IbReader.ReadString();
                contract.EvMultiplier = ibClientConnection.IbReader.ReadDouble();
            }
            if (msgVersion >= 7)
            {
                int secIdListCount = ibClientConnection.IbReader.ReadInt();
                if (secIdListCount > 0)
                {
                    contract.SecIdList = new List<TagValue>(secIdListCount);
                    for (int i = 0; i < secIdListCount; ++i)
                    {
                        TagValue tagValue = new TagValue();
                        tagValue.Tag = ibClientConnection.IbReader.ReadString();
                        tagValue.Value = ibClientConnection.IbReader.ReadString();
                        contract.SecIdList.Add(tagValue);
                    }
                }
            }

            ContractDetailsMsg contractDetails;
            contractDetails.ReqId = requestId;
            contractDetails.ContractDetails = contract;
            DequeueTcsAndSetResult<ContractDetailsMsg>(contractDetails, contractDetailsTcsQueue);
        }

        private void ContractDataEndMessageHandler()
        {
            int msgVersion = ibClientConnection.IbReader.ReadInt();
            int requestId = ibClientConnection.IbReader.ReadInt();
            DequeueTcsAndSetResult<int>(requestId, contractDetailsEndTcsQueue);
        }

        private void ExecutionDataMessageHandler()
        {
            int msgVersion = ibClientConnection.IbReader.ReadInt();
            int requestId = -1;
            if (msgVersion >= 7)
                requestId = ibClientConnection.IbReader.ReadInt();
            int orderId = ibClientConnection.IbReader.ReadInt();
            Contract contract = new Contract();
            if (msgVersion >= 5)
            {
                contract.ConId = ibClientConnection.IbReader.ReadInt();
            }
            contract.Symbol = ibClientConnection.IbReader.ReadString();
            contract.SecType = ibClientConnection.IbReader.ReadString();
            contract.Expiry = ibClientConnection.IbReader.ReadString();
            contract.Strike = ibClientConnection.IbReader.ReadDouble();
            contract.Right = ibClientConnection.IbReader.ReadString();
            if (msgVersion >= 9)
            {
                contract.Multiplier = ibClientConnection.IbReader.ReadString();
            }
            contract.Exchange = ibClientConnection.IbReader.ReadString();
            contract.Currency = ibClientConnection.IbReader.ReadString();
            contract.LocalSymbol = ibClientConnection.IbReader.ReadString();
            if (msgVersion >= 10)
            {
                contract.TradingClass = ibClientConnection.IbReader.ReadString();
            }

            Execution exec = new Execution();
            exec.OrderId = orderId;
            exec.ExecId = ibClientConnection.IbReader.ReadString();
            exec.Time = ibClientConnection.IbReader.ReadString();
            exec.AcctNumber = ibClientConnection.IbReader.ReadString();
            exec.Exchange = ibClientConnection.IbReader.ReadString();
            exec.Side = ibClientConnection.IbReader.ReadString();
            exec.Shares = ibClientConnection.IbReader.ReadInt();
            exec.Price = ibClientConnection.IbReader.ReadDouble();
            if (msgVersion >= 2)
            {
                exec.PermId = ibClientConnection.IbReader.ReadInt();
            }
            if (msgVersion >= 3)
            {
                exec.ClientId = ibClientConnection.IbReader.ReadInt();
            }
            if (msgVersion >= 4)
            {
                exec.Liquidation = ibClientConnection.IbReader.ReadInt();
            }
            if (msgVersion >= 6)
            {
                exec.CumQty = ibClientConnection.IbReader.ReadInt();
                exec.AvgPrice = ibClientConnection.IbReader.ReadDouble();
            }
            if (msgVersion >= 8)
            {
                exec.OrderRef = ibClientConnection.IbReader.ReadString();
            }
            if (msgVersion >= 9)
            {
                exec.EvRule = ibClientConnection.IbReader.ReadString();
                exec.EvMultiplier = ibClientConnection.IbReader.ReadDouble();
            }

            ExecDetailsMsg execDetails;
            execDetails.ReqId = requestId;
            execDetails.Contract = contract;
            execDetails.Execution = exec;
            DequeueTcsAndSetResult<ExecDetailsMsg>(execDetails, execDetailsTcsQueue);
        }

        private void ExecutionDataEndMessageHandler()
        {
            int msgVersion = ibClientConnection.IbReader.ReadInt();
            int requestId = ibClientConnection.IbReader.ReadInt();
            DequeueTcsAndSetResult<int>(requestId, execDetailsEndTcsQueue);
        }

        private void CommissionReportMessageHandler()
        {
            CommissionReportMsg crm;
            int msgVersion = ibClientConnection.IbReader.ReadInt();
            CommissionReport commissionReport = new CommissionReport();
            commissionReport.ExecId = ibClientConnection.IbReader.ReadString();
            commissionReport.Commission = ibClientConnection.IbReader.ReadDouble();
            commissionReport.Currency = ibClientConnection.IbReader.ReadString();
            commissionReport.RealizedPNL = ibClientConnection.IbReader.ReadDouble();
            commissionReport.Yield = ibClientConnection.IbReader.ReadDouble();
            commissionReport.YieldRedemptionDate = ibClientConnection.IbReader.ReadInt();
            crm.CommissionReport = commissionReport;
            DequeueTcsAndSetResult<CommissionReportMsg>(crm, commissionReportTcsQueue);
        }

        private void FundamentalDataMessageHandler()
        {
            FundamentalDataMsg fundamentalData;
            int msgVersion = ibClientConnection.IbReader.ReadInt();
            fundamentalData.ReqId = ibClientConnection.IbReader.ReadInt();
            fundamentalData.Data = ibClientConnection.IbReader.ReadString();
            DequeueTcsAndSetResult<FundamentalDataMsg>(fundamentalData, fundamentalDataTcsQueue);
        }

        private void HistoricalDataMessageHandler()
        {
            int msgVersion = ibClientConnection.IbReader.ReadInt();
            int requestId = ibClientConnection.IbReader.ReadInt();
            string startDateStr = "";
            string endDateStr = "";
            string completedIndicator = "finished";
            if (msgVersion >= 2)
            {
                startDateStr = ibClientConnection.IbReader.ReadString();
                endDateStr = ibClientConnection.IbReader.ReadString();
                completedIndicator += "-" + startDateStr + "-" + endDateStr;
            }
            int itemCount = ibClientConnection.IbReader.ReadInt();
            for (int ctr = 0; ctr < itemCount; ctr++)
            {
                HistoricalDataMsg hd;
                hd.ReqId = requestId;
                hd.Date = ibClientConnection.IbReader.ReadString();
                hd.Open = ibClientConnection.IbReader.ReadDouble();
                hd.High = ibClientConnection.IbReader.ReadDouble();
                hd.Low = ibClientConnection.IbReader.ReadDouble();
                hd.Close = ibClientConnection.IbReader.ReadDouble();
                hd.Volume = ibClientConnection.IbReader.ReadInt();
                hd.WAP = ibClientConnection.IbReader.ReadDouble();
                hd.HasGaps = Boolean.Parse(ibClientConnection.IbReader.ReadString());
                hd.Count = -1;
                if (msgVersion >= 3)
                {
                    hd.Count = ibClientConnection.IbReader.ReadInt();
                }
                DequeueTcsAndSetResult<HistoricalDataMsg>(hd, historicalDataTcsQueue);
            }

            // send end of dataset marker.
            HistoricalDataEndMsg hde;
            hde.ReqId = requestId;
            hde.StartDate = startDateStr;
            hde.EndDate = endDateStr;
            DequeueTcsAndSetResult<HistoricalDataEndMsg>(hde, historicalDataEndTcsQueue);
        }

        private void MarketDataTypeMessageHandler()
        {
            MarketDataTypeMsg mdt;
            int msgVersion = ibClientConnection.IbReader.ReadInt();
            mdt.ReqId = ibClientConnection.IbReader.ReadInt();
            mdt.MarketDataType = ibClientConnection.IbReader.ReadInt();
            DequeueTcsAndSetResult<MarketDataTypeMsg>(mdt, marketDataTypeTcsQueue);
        }

        private void MarketDepthMessageHandler()
        {
            UpdateMktDepthMsg umd;
            int msgVersion = ibClientConnection.IbReader.ReadInt();
            umd.TickerId = ibClientConnection.IbReader.ReadInt();
            umd.Position = ibClientConnection.IbReader.ReadInt();
            umd.Operation = ibClientConnection.IbReader.ReadInt();
            umd.Side = ibClientConnection.IbReader.ReadInt();
            umd.Price = ibClientConnection.IbReader.ReadDouble();
            umd.Size = ibClientConnection.IbReader.ReadInt();
            DequeueTcsAndSetResult<UpdateMktDepthMsg>(umd, updateMktDepthTcsQueue);
        }

        private void MarketDepthL2MessageHandler()
        {
            UpdateMktDepthL2Msg umdl2;
            int msgVersion = ibClientConnection.IbReader.ReadInt();
            umdl2.TickerId = ibClientConnection.IbReader.ReadInt();
            umdl2.Position = ibClientConnection.IbReader.ReadInt();
            umdl2.MarketMaker = ibClientConnection.IbReader.ReadString();
            umdl2.Operation = ibClientConnection.IbReader.ReadInt();
            umdl2.Side = ibClientConnection.IbReader.ReadInt();
            umdl2.Price = ibClientConnection.IbReader.ReadDouble();
            umdl2.Size = ibClientConnection.IbReader.ReadInt();
            DequeueTcsAndSetResult<UpdateMktDepthL2Msg>(umdl2, updateMktDepthL2TcsQueue);
        }

        private void NewsBulletinsMessageHandler()
        {
            UpdateNewsBulletinMsg unb;
            int msgVersion = ibClientConnection.IbReader.ReadInt();
            unb.MsgId = ibClientConnection.IbReader.ReadInt();
            unb.MsgType = ibClientConnection.IbReader.ReadInt();
            unb.Message = ibClientConnection.IbReader.ReadString();
            unb.OrigExchange = ibClientConnection.IbReader.ReadString();
            DequeueTcsAndSetResult<UpdateNewsBulletinMsg>(unb, updateNewsBulletinTcsQueue);
        }

        private void PositionMessageHandler()
        {
            PositionMsg position;
            int msgVersion = ibClientConnection.IbReader.ReadInt();
            position.Account = ibClientConnection.IbReader.ReadString();
            Contract contract = new Contract();
            contract.ConId = ibClientConnection.IbReader.ReadInt();
            contract.Symbol = ibClientConnection.IbReader.ReadString();
            contract.SecType = ibClientConnection.IbReader.ReadString();
            contract.Expiry = ibClientConnection.IbReader.ReadString();
            contract.Strike = ibClientConnection.IbReader.ReadDouble();
            contract.Right = ibClientConnection.IbReader.ReadString();
            contract.Multiplier = ibClientConnection.IbReader.ReadString();
            contract.Exchange = ibClientConnection.IbReader.ReadString();
            contract.Currency = ibClientConnection.IbReader.ReadString();
            contract.LocalSymbol = ibClientConnection.IbReader.ReadString();
            if (msgVersion >= 2)
            {
                contract.TradingClass = ibClientConnection.IbReader.ReadString();
            }

            position.Pos = ibClientConnection.IbReader.ReadInt();
            position.AvgCost = 0;
            if (msgVersion >= 3) position.AvgCost = ibClientConnection.IbReader.ReadDouble();

            position.Contract = contract;
            DequeueTcsAndSetResult<PositionMsg>(position, positionTcsQueue);
        }

        private void PositionEndMessageHandler()
        {
            int msgVersion = ibClientConnection.IbReader.ReadInt();
            DequeueTcsAndSetResult<object>(null, positionEndTcsQueue);
        }

        private void RealTimeBarsMessageHandler()
        {
            RealtimeBarMsg realtimeBar;
            int msgVersion = ibClientConnection.IbReader.ReadInt();
            realtimeBar.ReqId = ibClientConnection.IbReader.ReadInt();
            realtimeBar.Time = ibClientConnection.IbReader.ReadLong();
            realtimeBar.Open = ibClientConnection.IbReader.ReadDouble();
            realtimeBar.High = ibClientConnection.IbReader.ReadDouble();
            realtimeBar.Low = ibClientConnection.IbReader.ReadDouble();
            realtimeBar.Close = ibClientConnection.IbReader.ReadDouble();
            realtimeBar.Volume = ibClientConnection.IbReader.ReadLong();
            realtimeBar.WAP = ibClientConnection.IbReader.ReadDouble();
            realtimeBar.Count = ibClientConnection.IbReader.ReadInt();
            DequeueTcsAndSetResult<RealtimeBarMsg>(realtimeBar, realtimeBarTcsQueue);
        }

        private void ScannerParametersMessageHandler()
        {
            int msgVersion = ibClientConnection.IbReader.ReadInt();
            string xml = ibClientConnection.IbReader.ReadString();
            DequeueTcsAndSetResult<string>(xml, scannerParametersTcsQueue);
        }

        private void ScannerDataMessageHandler()
        {
            int msgVersion = ibClientConnection.IbReader.ReadInt();
            int requestId = ibClientConnection.IbReader.ReadInt();
            int numberOfElements = ibClientConnection.IbReader.ReadInt();
            for (int i = 0; i < numberOfElements; i++)
            {
                ScannerDataMsg scannerData;
                scannerData.ContractDetails = new ContractDetails();
                scannerData.ReqId = requestId;
                scannerData.Rank = ibClientConnection.IbReader.ReadInt();
                if (msgVersion >= 3) scannerData.ContractDetails.Summary.ConId = ibClientConnection.IbReader.ReadInt();
                scannerData.ContractDetails.Summary.Symbol = ibClientConnection.IbReader.ReadString();
                scannerData.ContractDetails.Summary.SecType = ibClientConnection.IbReader.ReadString();
                scannerData.ContractDetails.Summary.Expiry = ibClientConnection.IbReader.ReadString();
                scannerData.ContractDetails.Summary.Strike = ibClientConnection.IbReader.ReadDouble();
                scannerData.ContractDetails.Summary.Right = ibClientConnection.IbReader.ReadString();
                scannerData.ContractDetails.Summary.Exchange = ibClientConnection.IbReader.ReadString();
                scannerData.ContractDetails.Summary.Currency = ibClientConnection.IbReader.ReadString();
                scannerData.ContractDetails.Summary.LocalSymbol = ibClientConnection.IbReader.ReadString();
                scannerData.ContractDetails.MarketName = ibClientConnection.IbReader.ReadString();
                scannerData.ContractDetails.Summary.TradingClass = ibClientConnection.IbReader.ReadString();
                scannerData.Distance = ibClientConnection.IbReader.ReadString();
                scannerData.Benchmark = ibClientConnection.IbReader.ReadString();
                scannerData.Projection = ibClientConnection.IbReader.ReadString();
                scannerData.LegsStr = null;
                if (msgVersion >= 2)
                {
                    scannerData.LegsStr = ibClientConnection.IbReader.ReadString();
                }
                DequeueTcsAndSetResult<ScannerDataMsg>(scannerData, scannerDataTcsQueue);
            }
            DequeueTcsAndSetResult<int>(requestId, scannerDataEndTcsQueue);
        }

        private void ReceiveFAMessageHandler()
        {
            ReceiveFAMsg receiveFA;
            int msgVersion = ibClientConnection.IbReader.ReadInt();
            receiveFA.FADataType = ibClientConnection.IbReader.ReadInt();
            receiveFA.FAXmlData = ibClientConnection.IbReader.ReadString();
            DequeueTcsAndSetResult<ReceiveFAMsg>(receiveFA, receiveFATcsQueue);
        }

        private void BondContractDataMessageHandler()
        {
            int msgVersion = ibClientConnection.IbReader.ReadInt();
            int requestId = -1;
            if (msgVersion >= 3)
            {
                requestId = ibClientConnection.IbReader.ReadInt();
            }

            ContractDetails contract = new ContractDetails();

            contract.Summary.Symbol = ibClientConnection.IbReader.ReadString();
            contract.Summary.SecType = ibClientConnection.IbReader.ReadString();
            contract.Cusip = ibClientConnection.IbReader.ReadString();
            contract.Coupon = ibClientConnection.IbReader.ReadDouble();
            contract.Maturity = ibClientConnection.IbReader.ReadString();
            contract.IssueDate = ibClientConnection.IbReader.ReadString();
            contract.Ratings = ibClientConnection.IbReader.ReadString();
            contract.BondType = ibClientConnection.IbReader.ReadString();
            contract.CouponType = ibClientConnection.IbReader.ReadString();
            contract.Convertible = ibClientConnection.IbReader.ReadBoolFromInt();
            contract.Callable = ibClientConnection.IbReader.ReadBoolFromInt();
            contract.Putable = ibClientConnection.IbReader.ReadBoolFromInt();
            contract.DescAppend = ibClientConnection.IbReader.ReadString();
            contract.Summary.Exchange = ibClientConnection.IbReader.ReadString();
            contract.Summary.Currency = ibClientConnection.IbReader.ReadString();
            contract.MarketName = ibClientConnection.IbReader.ReadString();
            contract.Summary.TradingClass = ibClientConnection.IbReader.ReadString();
            contract.Summary.ConId = ibClientConnection.IbReader.ReadInt();
            contract.MinTick = ibClientConnection.IbReader.ReadDouble();
            contract.OrderTypes = ibClientConnection.IbReader.ReadString();
            contract.ValidExchanges = ibClientConnection.IbReader.ReadString();
            if (msgVersion >= 2)
            {
                contract.NextOptionDate = ibClientConnection.IbReader.ReadString();
                contract.NextOptionType = ibClientConnection.IbReader.ReadString();
                contract.NextOptionPartial = ibClientConnection.IbReader.ReadBoolFromInt();
                contract.Notes = ibClientConnection.IbReader.ReadString();
            }
            if (msgVersion >= 4)
            {
                contract.LongName = ibClientConnection.IbReader.ReadString();
            }
            if (msgVersion >= 6)
            {
                contract.EvRule = ibClientConnection.IbReader.ReadString();
                contract.EvMultiplier = ibClientConnection.IbReader.ReadDouble();
            }
            if (msgVersion >= 5)
            {
                int secIdListCount = ibClientConnection.IbReader.ReadInt();
                if (secIdListCount > 0)
                {
                    contract.SecIdList = new List<TagValue>();
                    for (int i = 0; i < secIdListCount; ++i)
                    {
                        TagValue tagValue = new TagValue();
                        tagValue.Tag = ibClientConnection.IbReader.ReadString();
                        tagValue.Value = ibClientConnection.IbReader.ReadString();
                        contract.SecIdList.Add(tagValue);
                    }
                }
            }
            BondContractDetailsMsg bcd;
            bcd.RequestId = requestId;
            bcd.ContractDetails = contract;
            DequeueTcsAndSetResult<BondContractDetailsMsg>(bcd, bondContractDetailsTcsQueue);
        }

        private void VerifyMessageApiMessageHandler()
        {
            int msgVersion = ibClientConnection.IbReader.ReadInt();
            string apiData = ibClientConnection.IbReader.ReadString();
            DequeueTcsAndSetResult<string>(apiData, verifyMessageApiTcsQueue);
        }

        private void VerifyCompletedMessageHandler()
        {
            VerifyCompletedMsg vc;
            int msgVersion = ibClientConnection.IbReader.ReadInt();
            vc.IsSuccessful = String.Compare(ibClientConnection.IbReader.ReadString(), "true", true) == 0;
            vc.ErrorText = ibClientConnection.IbReader.ReadString();

            if (vc.IsSuccessful) ibClientConnection.StartApi();
            DequeueTcsAndSetResult<VerifyCompletedMsg>(vc, verifyCompletedTcsQueue);
        }

        private void DisplayGroupListMessageHandler()
        {
            DisplayGroupListMsg dgl;
            int msgVersion = ibClientConnection.IbReader.ReadInt();
            dgl.ReqId = ibClientConnection.IbReader.ReadInt();
            dgl.Groups = ibClientConnection.IbReader.ReadString();
            DequeueTcsAndSetResult<DisplayGroupListMsg>(dgl, displayGroupListTcsQueue);
        }

        private void DisplayGroupUpdatedMessageHandler()
        {
            DisplayGroupUpdatedMsg dgu;
            int msgVersion = ibClientConnection.IbReader.ReadInt();
            dgu.ReqId = ibClientConnection.IbReader.ReadInt();
            dgu.ContractInfo = ibClientConnection.IbReader.ReadString();
            DequeueTcsAndSetResult<DisplayGroupUpdatedMsg>(dgu, displayGroupUpdatedTcsQueue);
        }

        #endregion


        private Task<T> CreateTcsAndEnqueue<T>(ConcurrentQueue<TaskCompletionSource<T>> tcsQueue)
        {
            var tcs = new TaskCompletionSource<T>();
            tcsQueue.Enqueue(tcs);
            return tcs.Task;
        }


        private void DequeueTcsAndSetResult<T>(T result, ConcurrentQueue<TaskCompletionSource<T>> tcsQueue)
        {
            TaskCompletionSource<T> tcs = null;
            tcsQueue.TryDequeue(out tcs);
            if (tcs != null) tcs.SetResult(result);
        }


        private void ReportError(Error error)
        {
            DequeueTcsAndSetResult<Error>(error, errorTcsQueue);
        }
    }
}

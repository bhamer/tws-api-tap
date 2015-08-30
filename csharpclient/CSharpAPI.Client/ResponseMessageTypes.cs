using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace IBApi
{
    public struct TickPriceMsg
    {
        public int TickerId;
        public int Field;
        public double Price;
        public int CanAutoExecute;
    }

    public struct TickSizeMsg
    {
        public int TickerId;
        public int Field;
        public double Size;
    }

    public struct TickStringMsg
    {
        public int TickerId;
        public int TickType;
        public string Value;
    }

    public struct TickGenericMsg
    {
        public int TickerId;
        public int Field;
        public double Value;
    }

    public struct TickEFPMsg
    {
        public int TickerId;
        public int TickType;
        public double BasisPoints;
        public string FormattedBasisPoints;
        public double ImpliedFuture;
        public int HoldDays;
        public string FutureExpiry;
        public double DividendImpact;
        public double DividendsToExpiry;
    }

    public struct DeltaNeutralValidationMsg
    {
        public int ReqId;
        public UnderComp UnderComp;
    }

    public struct TickOptionComputationMsg
    {
        public int TickerId;
        public int Field;
        public double ImpliedVolatility;
        public double Delta;
        public double OptPrice;
        public double PvDividend;
        public double Gamma;
        public double Vega;
        public double Theta;
        public double UndPrice;
    }

    public struct AccountSummaryMsg
    {
        public int ReqId;
        public string Account;
        public string Tag;
        public string Value;
        public string Currency;
    }

    public struct UpdateAccountValueMsg
    {
        public string Key;
        public string Value;
        public string Currency;
        public string AccountName;
    }

    public struct UpdatePortfolioMsg
    {
        public Contract Contract;
        public int Position;
        public double MarketPrice;
        public double MarketValue;
        public double AverageCost;
        public double UnrealizedPNL;
        public double RealizedPNL;
        public string AccountName;
    }

    public struct OrderStatusMsg
    {
        public int OrderId;
        public string Status;
        public int Filled;
        public int Remaining;
        public double AvgFillPrice;
        public int PermId;
        public int ParentId;
        public double LastFillPrice;
        public int ClientId;
        public string WhyHeld;
    }

    public struct OpenOrderMsg
    {
        public int OrderId;
        public Contract Contract;
        public Order Order;
        public OrderState OrderState;
    }

    public struct ContractDetailsMsg
    {
        public int ReqId;
        public ContractDetails ContractDetails;
    }

    public struct ExecDetailsMsg
    {
        public int ReqId;
        public Contract Contract;
        public Execution Execution;
    }

    public struct CommissionReportMsg
    {
        public CommissionReport CommissionReport;
    }

    public struct FundamentalDataMsg
    {
        public int ReqId;
        public string Data;
    }

    public struct HistoricalDataMsg
    {
        public int ReqId;
        public string Date;
        public double Open;
        public double High;
        public double Low;
        public double Close;
        public int Volume;
        public int Count;
        public double WAP;
        public bool HasGaps;
    }

    public struct HistoricalDataEndMsg
    {
        public int ReqId;
        public string StartDate;
        public string EndDate;
    }

    public struct MarketDataTypeMsg
    {
        public int ReqId;
        public int MarketDataType;
    }

    public struct UpdateMktDepthMsg
    {
        public int TickerId;
        public int Position;
        public int Operation;
        public int Side;
        public double Price;
        public int Size;
    }

    public struct UpdateMktDepthL2Msg
    {
        public int TickerId;
        public int Position;
        public string MarketMaker;
        public int Operation;
        public int Side;
        public double Price;
        public int Size;
    }

    public struct UpdateNewsBulletinMsg
    {
        public int MsgId;
        public int MsgType;
        public String Message;
        public String OrigExchange;
    }

    public struct PositionMsg
    {
        public string Account;
        public Contract Contract;
        public int Pos;
        public double AvgCost;
    }

    public struct RealtimeBarMsg
    {
        public int ReqId;
        public long Time;
        public double Open;
        public double High;
        public double Low;
        public double Close;
        public long Volume;
        public double WAP;
        public int Count;
    }

    public struct ScannerDataMsg
    {
        public int ReqId;
        public int Rank;
        public ContractDetails ContractDetails;
        public string Distance;
        public string Benchmark;
        public string Projection;
        public string LegsStr;
    }

    public struct ReceiveFAMsg
    {
        public int FADataType;
        public string FAXmlData;
    }

    public struct BondContractDetailsMsg
    {
        public int RequestId;
        public ContractDetails ContractDetails;
    }

    public struct VerifyCompletedMsg
    {
        public bool IsSuccessful;
        public string ErrorText;
    }

    public struct DisplayGroupListMsg
    {
        public int ReqId;
        public string Groups;
    }

    public struct DisplayGroupUpdatedMsg
    {
        public int ReqId;
        public string ContractInfo;
    }

    public struct Error
    {
        public Error(int? requestId, int? errorCode, string errorMessage)
        {
            RequestId = requestId;
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
            Exception = null;
        }

        public Error(int? requestId, CodeMsgPair error, string tail)
        {
            RequestId = requestId;
            ErrorCode = error.Code;
            ErrorMessage = error.Message + tail;
            Exception = null;
        }

        public Error(int? errorCode, string errorMessage)
        {
            RequestId = null;
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
            Exception = null;
        }

        public Error(string errorMessage, Exception exception)
        {
            RequestId = null;
            ErrorCode = null;
            ErrorMessage = errorMessage;
            Exception = exception;
        }

        public int? RequestId;
        public int? ErrorCode;
        public string ErrorMessage;
        public Exception Exception;
    }
}

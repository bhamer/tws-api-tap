using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBApi
{
    /// <summary>
    /// Wrapper around BinaryWriter to write to IB Server stream. This class is not thread-safe.
    /// </summary>
    public class IbWriter
    {
        private readonly BinaryWriter writer;        

        public IbWriter(BinaryWriter writer)
        {
            this.writer = writer;
        }

        public void Send(List<byte> paramsList, CodeMsgPair error)
        {
            Send(IncomingMessage.NotValid, paramsList, error);
        }

        public void Send(int reqId, List<byte> paramsList, CodeMsgPair error)
        {
            try
            {
                Send(paramsList);
            }
            catch (Exception e)
            {
                throw new Exception(String.Format("Exception. Id: {0}, Code: {1}, Msg: {2}", reqId, error.Code, error.Message), e);
            }
        }

        public void SendCancelRequest(OutgoingMessages msgType, int version, int reqId, CodeMsgPair error)
        {
            List<byte> paramsList = new List<byte>();
            paramsList.AddParameter(msgType);
            paramsList.AddParameter(version);
            paramsList.AddParameter(reqId);
            try
            {
                Send(paramsList);
            }
            catch (Exception e)
            {
                throw new Exception(String.Format("Exception. Id: {0}, Code: {1}, Msg: {2}", reqId, error.Code, error.Message), e);
            }
        }

        public void SendCancelRequest(OutgoingMessages msgType, int version, CodeMsgPair error)
        {
            List<byte> paramsList = new List<byte>();
            paramsList.AddParameter(msgType);
            paramsList.AddParameter(version);
            try
            {
                Send(paramsList);
            }
            catch (Exception e)
            {
                throw new Exception(String.Format("Exception. Code: {0}, Msg: {1}", error.Code, error.Message), e);
            }
        }


        public void Send(List<byte> request)
        {
            writer.Write(request.ToArray());
        }
    }
}

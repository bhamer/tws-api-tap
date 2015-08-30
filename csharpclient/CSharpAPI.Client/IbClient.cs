using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBApi
{
    /// <summary>
    /// IbClient is a wrapper around IbClientConnection, IbClientRequestHandler, and IbClientResponseHandler. 
    /// It's meant to be a quick-and-easy way to get up and running. For more precise control over program flow, use the underlying classes directly.
    /// </summary>
    public class IbClient : IDisposable
    {
        private bool disposed;
        private IbClientConnection ibClientConnection;

        public IbClientRequestHandler Request { get; set; }
        public IbClientResponseHandler Response { get; set; }


        public IbClient()
        {
            disposed = false;
            ibClientConnection = new IbClientConnection();
            Request = new IbClientRequestHandler(ibClientConnection);
            Response = new IbClientResponseHandler(ibClientConnection);
        }


        public void Connect(string host, int port, int clientId, bool extraAuth = false)
        {
            ibClientConnection.Connect(host, port, clientId, extraAuth);
            Response.Start();
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (Response.IsProcessing) Response.Stop();
                    if (ibClientConnection.IsConnected) ibClientConnection.Disconnect();
                    ibClientConnection = null;
                    Request = null;
                    Response = null;
                }
                disposed = true;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace IBApi
{
    public class IbClientConnection : IIbClientConnection
    {
        private TcpClient tcpClient;
        private NetworkStream tcpClientStream;

        public int ClientId { get; private set; }
        public bool ExtraAuth { get; private set; }
        public int ServerVersion { get; private set; }
        public bool IsConnected { get; private set; }

        public IbWriter IbWriter { get; private set; }
        public IbReader IbReader { get; private set; }

        public IbClientConnection()
        {
            ClientId = -1;
            ExtraAuth = false;
            ServerVersion = -1;
            IsConnected = false;
            IbWriter = null;
            IbReader = null;
        }


        public void Connect(string host, int port, int clientId, bool extraAuth = false)
        {
            if (IsConnected) throw new Exception(String.Format("Code: {0}, Msg: {1}", EClientErrors.AlreadyConnected.Code, EClientErrors.AlreadyConnected.Message));

            tcpClient = new TcpClient(host, port);
            tcpClientStream = tcpClient.GetStream();
            var tcpWriter = new BinaryWriter(tcpClientStream);
            IbWriter = new IbWriter(tcpWriter);
            IbReader = new IbReader(new BinaryReader(tcpClientStream));
            ClientId = clientId;
            ExtraAuth = extraAuth;
            try
            {
                tcpWriter.Write(UTF8Encoding.UTF8.GetBytes(Constants.ClientVersion.ToString()));
                tcpWriter.Write(Constants.EOL);
            }
            catch (IOException e)
            {
                throw new IOException("Could not establish connection. Make sure the TWS is enabled to accept socket clients!", e);
            }
            // Receive the response from the remote device.
            ServerVersion = IbReader.ReadInt();
            if (ServerVersion < MinServerVer.MIN_VERSION)
            {
                throw new Exception(String.Format("Code: {0}, Msg: {1}", EClientErrors.UPDATE_TWS.Code, EClientErrors.UPDATE_TWS.Message));
            }
            if (ServerVersion >= 20)
            {
                string twsTime = IbReader.ReadString();
            }
            IsConnected = true;
            if (ServerVersion >= 3)
            {
                if (ServerVersion < MinServerVer.LINKING)
                {
                    tcpWriter.Write(UTF8Encoding.UTF8.GetBytes(clientId.ToString()));
                    tcpWriter.Write(Constants.EOL);
                }
                else if (!extraAuth)
                {
                    StartApi();
                }
            }
        }


        public void StartApi()
        {
            if (!IsConnected)
            {
                // todo: should an exception be thrown here?
                return;
            }

            const int VERSION = 1;
            List<byte> paramsList = new List<byte>();
            paramsList.AddParameter(OutgoingMessages.StartApi);
            paramsList.AddParameter(VERSION);
            paramsList.AddParameter(ClientId);
            IbWriter.Send(paramsList);
        }


        /// <summary>
        /// Closes the TCP connection and cancels the task processing incoming messages.
        /// </summary>
        public void Disconnect()
        {
            if (!IsConnected) throw new Exception(String.Format("Code: {0}, Msg: {1}", EClientErrors.NOT_CONNECTED.Code, EClientErrors.NOT_CONNECTED.Message));

            IsConnected = false;
            ServerVersion = 0;
            ClientId = -1;
            ExtraAuth = false;

            // close TCP connection
            if (tcpClient.Connected) tcpClient.Close();

            tcpClientStream.Close();
            tcpClientStream = null;
            tcpClient = null;
            IbWriter = null;
            IbReader = null;
        }
    }
}

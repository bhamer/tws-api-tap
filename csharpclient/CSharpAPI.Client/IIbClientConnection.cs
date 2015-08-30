namespace IBApi
{   
    public interface IIbClientConnection
    {
        int ClientId { get; }

        bool ExtraAuth { get; }

        /// <returns>The Host's version.</returns>
        /// <remarks>Some of the API functionality might not be available in older Hosts and therefore it is essential to keep the TWS/Gateway as up to date as possible.</remarks>
        int ServerVersion { get; }

        /// <summary>Notifies whether or not the API client is connected to the Host.</summary>
        /// <returns>True if client is connected to the Host; otherwise, false.</returns>
        bool IsConnected { get; }

        /// <summary>Establishes a connection to the designated Host. After establishing a connection succesfully, the Host will provide the next valid order id, server's current time, managed accounts and open orders among others depending on the Host version.</summary>
        /// <param name="host">The Host's IP address. Leave blank for localhost.</param>
        /// <param name="port">The Host's port. 7496 by default for the TWS, 4001 by default on the Gateway.</param>
        /// <param name="clientId">Every API client program requires a unique id which can be any integer. Note that up to eight clients can be connected simultaneously to a single Host.</param>
        /// <param name="extraAuth"></param>        
        void Connect(string host, int port, int clientId, bool extraAuth = false);

        /// <summary>Disconnects from the Host.</summary>
        void Disconnect();

        void StartApi();

        IbWriter IbWriter { get; }

        IbReader IbReader { get; }
    }
}

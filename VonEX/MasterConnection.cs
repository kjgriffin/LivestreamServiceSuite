using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VonEX
{

    public delegate void StringDataRecievedEventArgs(string data);
    public delegate void ConnectionEventArgs(string senderip, bool connected);

    public class MasterConnection
    {


        TcpListener tcpListener;
        TcpClient slaveConnection;
        Thread processthread;


        public event StringDataRecievedEventArgs OnDataRecieved;
        public event ConnectionEventArgs OnConnectionFromClient;

        bool started = false;

        public void StartServer(IPAddress hostname, int port)
        {
            tcpListener = new TcpListener(hostname, port);
            tcpListener.Start();
            started = true;
        }


        public async void AcceptConnection()
        {
            if (started)
            {
                slaveConnection = await tcpListener.AcceptTcpClientAsync();
                var constr = slaveConnection.Client.RemoteEndPoint.ToString();
                var addr = constr.Split(':')[0];

                OnConnectionFromClient?.Invoke(addr, true);
                processthread = new Thread(ProcessClient);
                processthread.Start(slaveConnection);
            }
        }

        public void SendMessage(string data)
        {
            var stream = slaveConnection.GetStream();
            stream.Write(Encoding.UTF8.GetBytes(data));
        }


        private async void ProcessClient(object obj)
        {
            TcpClient client = obj as TcpClient;
            var stream = client.GetStream();
            while (client.Connected)
            {
                while (!stream.DataAvailable) ;

                // read data into dataframe
                byte[] data = new byte[client.Available];
                await stream.ReadAsync(data, 0, client.Available);
                // event!
                OnDataRecieved?.Invoke(Encoding.UTF8.GetString(data));
            }
            var constr = client.Client.RemoteEndPoint.ToString();
            var addr = constr.Split(':')[0];
            client.Close();
            OnConnectionFromClient(addr, false);
        }


        public void Close()
        {
            tcpListener?.Stop();
            processthread?.Abort();
            slaveConnection?.Close();
            started = false;
        }
        


    }
}

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VonEX
{
    public class SlaveConnection
    {

        TcpClient client;

        Thread processthread;


        public event StringDataRecievedEventArgs OnDataRecieved;
        public event ConnectionEventArgs OnConnectionFromMaster;

        public async void Connect(string hostname, int port)
        {
            client = new TcpClient();
            try
            {
                await client.ConnectAsync(hostname, port);
            }
            catch (Exception ex)
            {
                return;
            }
            var constr = client.Client.RemoteEndPoint.ToString();
            var addr = constr.Split(':')[0];
            OnConnectionFromMaster?.Invoke(addr, true);

            processthread = new Thread(ProcessMasterMessages);
            processthread.Start(client);

        }

        public void SendString(string data)
        {
            var stream = client.GetStream();
            stream.Write(Encoding.UTF8.GetBytes(data));
        }
        public async void SendStringASCII(string data)
        {
            var stream = client.GetStream();
            await stream.WriteAsync(Encoding.ASCII.GetBytes(data));
        }


        public void Close()
        {
            processthread?.Abort();
            client?.Close();
        }

        private async void ProcessMasterMessages(object obj)
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
            OnConnectionFromMaster(addr, false);
        }

    }
}

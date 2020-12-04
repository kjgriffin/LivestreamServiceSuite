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

        public event StringDataRecievedEventArgs OnDataRecieved;
        public event ConnectionEventArgs OnConnectionFromMaster;

        private CancellationTokenSource cancellationToken;

        public async void Connect(string hostname, int port)
        {
            Close();
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

            cancellationToken = new CancellationTokenSource();

            ThreadPool.QueueUserWorkItem(ProcessMasterMessages, new ProcessArgs() { Client = client, Token = cancellationToken });

        }

        public void SendString(string data)
        {
            if (client.Connected)
            {
                var stream = client.GetStream();
                stream.Write(Encoding.UTF8.GetBytes(data));
            }
        }
        public async void SendStringASCII(string data)
        {
            if (client.Connected)
            {
                var stream = client.GetStream();
                await stream.WriteAsync(Encoding.ASCII.GetBytes(data));
            }
        }


        public void Close()
        {
            cancellationToken?.Cancel();
            client?.Close();
        }

        private async void ProcessMasterMessages(object obj)
        {
            ProcessArgs props = obj as ProcessArgs;
            TcpClient client = props.Client;
            var stream = client.GetStream();
            while (client.Connected)
            {
                if (props.Token.IsCancellationRequested)
                {
                    return;
                }
                while (!stream.DataAvailable)
                {
                    if (props.Token.IsCancellationRequested)
                    {
                        return;
                    }
                }

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

    internal class ProcessArgs
    {
        public TcpClient Client { get; set; }
        public CancellationTokenSource Token { get; set; }
    }
}

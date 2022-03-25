using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CCUDebugger
{
    internal class UDP_Debug_Test
    {

        public static void RecallPreset(IPAddress host, int port)
        {


            try
            {

                Console.WriteLine("[UDP Command]: RECALL_PRESET 2");
                IPEndPoint endpoint = new IPEndPoint(host, port);
                UdpClient client = new UdpClient();
                client.Connect(endpoint);
                Console.WriteLine("[UDP sending]");
                client.Send(new byte[] {
                    0x00,
                    0x12,
                    0x81,
                    0x45,
                    (byte)'R',
                    (byte)'E',
                    (byte)'C',
                    (byte)'A',
                    (byte)'L',
                    (byte)'L',
                    (byte)'_',
                    (byte)'P',
                    (byte)'R',
                    (byte)'E',
                    (byte)'S',
                    (byte)'E',
                    (byte)'T',
                    0x2,
                });
                Console.WriteLine("[UDP sent]");

                // we'll expect to recieve something
                Console.WriteLine("[UDP listen]");
                byte[] resp = client.Receive(ref endpoint);
                Console.WriteLine("[UDP got]");
                Console.WriteLine($">>>>>>> {BitConverter.ToSingle(resp)}");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UDP failed] {ex.ToString()}");
            }

        }

    }
}

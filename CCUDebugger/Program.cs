// See https://aka.ms/new-console-template for more information
using CCUDebugger;
using DVIPProtocol.Clients.Advanced;
using DVIPProtocol.Protocol.Lib.Inquiry;

using System.Linq;
using System.Net;

Console.WriteLine("Starting CCU Debugger!");

//var ctrl = new SingleCamController();
//ctrl.Start();

Console.WriteLine("raw/live");
if (Console.ReadLine() == "live")
{
    var ctrl = new AdvCtrl();
    ctrl.Start();
}
else
{


    Console.WriteLine("Enter IP:");
    string addr = Console.ReadLine();
    Console.WriteLine("Enter Port (5002):");
    string port = Console.ReadLine();

    if (!(IPAddress.TryParse(addr, out var ip) && int.TryParse(port, out var portNum)))
    {
        Console.WriteLine($"Invalid Endpoint {addr}:{port}");
        return;
    }
    IPEndPoint endpoint = new IPEndPoint(ip, portNum);
    IInqClient client = new TCPInqClient(endpoint);
    client.Init();

    while (true)
    {
        Console.WriteLine("RAW Command Bytes:");
        var hex = Console.ReadLine();
        Console.WriteLine("Expected resp length:");
        int len = int.Parse(Console.ReadLine());
        try
        {
            byte[] data = Enumerable.Range(0, hex.Length)
                                    .Where(x => x % 2 == 0)
                                    .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                                    .ToArray();

            client.SendRequest<RawInquiryResp>(RawInquiry.Create(data), len, onReply);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
   
    client.Stop();
}

void onReply(byte[] data)
{
    Console.WriteLine($"resp: {BitConverter.ToString(data)}");
}


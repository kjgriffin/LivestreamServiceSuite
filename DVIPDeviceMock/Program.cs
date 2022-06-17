// See https://aka.ms/new-console-template for more information
using DVIPProtocol.Servers.Mock;

using System.Net;

Console.WriteLine("Start DVIP mock camera.");

Console.WriteLine("IP Address:");
string ip = Console.ReadLine();

Console.WriteLine("Port:");
string port = Console.ReadLine();

if (IPAddress.TryParse(ip, out var addr) && int.TryParse(port, out var portnum))
{
    TCPMockDevice device = new TCPMockDevice();
    device.Start(new IPEndPoint(addr, portnum), true);
}

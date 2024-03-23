using NetEti.FileTools;
using System.Net;
using System.Net.Sockets;

namespace NetEti.DemoApplications
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //ShowDriveList();
            //NetworkMappingsRefresher.Refresh();
            //ShowDriveList();

            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            Console.WriteLine(host.HostName);
            // Get the IP
            /* doesn't work properly, 'cause you get multiple ip addresses:
            bool found = false;
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    Console.WriteLine(ip.ToString());
                    found = true;
                }
            }
            if (!found)
            {
                Console.WriteLine("No network adapters with an IPv4 address in the system!");
            }
            */
            // Console.ReadKey();

            // the following approach works (if google is up)
            string? localIP;
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint? endPoint = (IPEndPoint?)socket.LocalEndPoint;
                localIP = endPoint?.Address.ToString();
            }
            Console.WriteLine($"local ip: {localIP ?? "no local ip found"}");

            Stack<string> searchDirectories = new Stack<string>();
            searchDirectories.Push(@"\\MAUSBAER5\openVishnu\WorkFrame\");
            searchDirectories.Push(@$"\\{localIP}\openVishnu\WorkFrame\NetworkMappingsRefresher\NetworkMappingsRefresher\bin\");
            searchDirectories.Push(@$"{localIP}\openVishnu\WorkFrame\NetworkMappingsRefresher\NetworkMappingsRefresher\bin\");
            searchDirectories.Push(@"\\MAUSBAER5\openVishnu\WorkFrame\NetworkMappingsRefresher\NetworkMappingsRefresher\bin\");
            searchDirectories.Push(@"c:\Users\micro\Documents\private4\WPF\openVishnu8\WorkFrame\NetworkMappingsRefresher\NetworkMappingsRefresher\bin\");
            searchDirectories.Push(@"\\MAUSBAERX\openVishnu\WorkFrame\NetworkMappingsRefresher\NetworkMappingsRefresher\bin\");

            Console.WriteLine(NetworkMappingsRefresher.GetNextReachablePath(@"Debug\net8.0\", searchDirectories.ToArray()));
            Console.WriteLine(NetworkMappingsRefresher.GetNextReachablePath(
                @"\\MAUSBAER5\openVishnu\WorkFrame\NetworkMappingsRefresher\NetworkMappingsRefresher\bin\Debug\net8.0\", searchDirectories.ToArray()));
            Console.WriteLine(NetworkMappingsRefresher.GetNextReachablePath(null, searchDirectories.ToArray()));
        }

        private static void ShowDriveList()
        {
            List<DriveProperties> drives = NetworkMappingsRefresher.GetDriveList();
            foreach (DriveProperties drive in drives)
            {
                Console.WriteLine(drive.ToString());
            }
        }
    }
}
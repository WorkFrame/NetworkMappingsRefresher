using NetEti.FileTools;

namespace NetEti.DemoApplications
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ShowDriveList();
            NetworkMappingsRefresher.Refresh();
            Console.WriteLine();
            ShowDriveList();
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
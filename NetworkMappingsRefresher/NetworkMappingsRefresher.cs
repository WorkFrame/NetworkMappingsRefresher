using System.Diagnostics;

namespace NetEti.FileTools
{
    /// <summary>
    /// Refresht die Mappings von Netzwerk-Laufwerken.
    /// Benutzt dafür "net use".
    /// </summary>
    /// <remarks>
    /// File: NetworkMappingsRefresher
    /// Autor: Erik Nagel, NetEti
    ///
    /// 20.02.2015 Erik Nagel: erstellt
    /// 28.02.2023 Erik Nagel: überarbeitet.
    /// </remarks>
    public static class NetworkMappingsRefresher
	{
        /// <summary>
        /// Liefert ein Dictionary von Laufwerksbuchtaben mit ihren Pfaden
		/// und je einem Status "IsReady" (True: Laufwerk ist connected).
        /// </summary>
        /// <returns>Ein Dictionary von Laufwerksbuchtaben mit ihren Pfaden
		/// und je einem Status "IsReady" (True: Laufwerk ist connected).</returns>
        public static List<DriveProperties> GetDriveList()
        {
			List<DriveProperties> driveList = new List<DriveProperties>();
            var p = Process.Start(new ProcessStartInfo
            {
                FileName = "net",
                Arguments = "use",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });
            var str = p?.StandardOutput.ReadToEnd();
            if (str != null)
            {
                foreach (string s in str.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string[] s2 = s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (s2.Length >= 2 && s2[1][1] == ':')
                    {
                        string drive = s2[1];
                        string path = s2[2];
                        bool isReady = new System.IO.DriveInfo(drive).IsReady;
                        driveList.Add(new DriveProperties(drive, path, isReady));
                    }
                }
            }
			return driveList;
        }

        /// <summary>
        /// Refresht die Mappings von Netzwerk-Laufwerken.
        /// </summary>
        public static void Refresh()
		{
            List<DriveProperties> driveList = GetDriveList();
            foreach (DriveProperties drive in driveList)
            {
                Map(drive.DriveLetter);
            }
        }

		private static void Map(string driveLetter)
		{
			int counter = 0;
			while (!new System.IO.DriveInfo(driveLetter).IsReady && counter++ < 5)
			{
                Thread.Sleep(100);
				try
				{
                    Process.Start(new ProcessStartInfo
                    {
						FileName = "explorer",
						Arguments = driveLetter + ":\\",
						WindowStyle = ProcessWindowStyle.Minimized
					});
                    Thread.Sleep(500);
					foreach (Process p in Process.GetProcessesByName("explorer"))
					{
						if (p.MainWindowTitle.EndsWith(@"(" + driveLetter + ":)", StringComparison.CurrentCultureIgnoreCase))
						{
							p.Kill();
						}
					}
				}
				catch { }
			}
		}
	}
}

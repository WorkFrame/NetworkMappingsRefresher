using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

namespace NetEti.FileTools
{
    /// <summary>
    /// Aktualisiert die Mappings von Netzwerk-Laufwerken.
    /// Benutzt dafür "net use".
    /// </summary>
    /// <remarks>
    /// File: NetworkMappingsRefresher
    /// Autor: Erik Nagel, NetEti
    ///
    /// 20.02.2015 Erik Nagel: erstellt
    /// 28.02.2023 Erik Nagel: überarbeitet.
    /// 20.02.2024 Erik Nagel: erneut überarbeitet und GetNextReachablePath hinzugefügt.
    /// </remarks>
    public static class NetworkMappingsRefresher
	{
        /// <summary>
        /// Liefert ein Dictionary von Laufwerksbuchstaben mit ihren Pfaden
		/// und je einem Status "IsReady" (True: Laufwerk ist connected).
        /// </summary>
        /// <returns>Ein Dictionary von Laufwerksbuchstaben mit ihren Pfaden
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
                    GroupCollection groups = Regex.Match(s, @"\b([A-Za-z])(?:\:|\b)").Groups;
                    if (groups.Count > 1)
                    {
                        string driveLetter = groups[1].Value;
                        if (!String.IsNullOrEmpty(driveLetter))
                        {
                            string path = Regex.Match(s, @"\\[^\s]+").Value;
                            bool isReady = false;
                            if (IsPathServerReachable(path))
                            {
                                isReady = new System.IO.DriveInfo(driveLetter).IsReady;
                            }
                            driveList.Add(new DriveProperties(driveLetter, path, isReady));
                        }
                    }
                }
            }
			return driveList;
        }

        /// <summary>
        /// Aktualisiert die Mappings von Netzwerk-Laufwerken.
        /// Achtung: dies ist ein Dummy, der nur GetDriveList() aufruft,
        /// welches seinerseits "net use" aufruft.
        /// Sofern "net use" allein nicht zur erneuten Verbindung getrennter
        /// Netzlaufwerke führt, ist diese Methode noch wirkungslos.
        /// Sollte es erforderlich werden, könnte die unten kommentierte
        /// Vorgehensweise Erfolg versprechen.
        /// </summary>
        public static void Refresh()
		{
            List<DriveProperties> driveList = GetDriveList();

            /*
             *   To force a drive to reconnect, you can try the following steps:
             *   Type "net use" and press Enter to see a list of all mapped network drives.
             *   Find the drive you want to reconnect and take note of its name.
             *   Type "net use <drive letter>: /delete" and press Enter to delete the mapping.
             *   Type "net use <drive letter>: \server\share /persistent:yes" and press Enter to
             *   recreate the mapping with the "persistent" option, which should keep the mapping
             *   alive even after a disconnect.
             */


            //foreach (DriveProperties drive in driveList)
            //{
            //    Map(drive.DriveLetter); // 21.02.2024 Nagel: Map (über Explorer) funktioniert nicht mehr!
            //}
        }

        /// <summary>
        /// Übernimmt einen relativen Pfad (pathPart) oder null und eine Liste von Suchpfaden (searchDirectories).
        /// Wenn der relative Pfad nicht null oder leer ist, liefert diese Routine den ersten erreichbaren,
        /// aus Suchpfad und relativem Pfad kombinierten Pfad oder null.
        /// Ist der relative Pfad null oder leer, liefert diese Routine den nächsten erreichbaren Suchpfad oder null.
        /// </summary>
        /// <param name="pathPart">Ein relativer Pfadanteil.</param>
        /// <param name="searchDirectories">Eine Liste von Suchpfaden.</param>
        /// <returns>Erster erreichbarer, ggf. aus Suchpfad und relativem Pfad kombinierter Pfad oder null.</returns>
        public static string? GetNextReachablePath(string? pathPart, string[] searchDirectories)
        {
            if (!String.IsNullOrEmpty(pathPart))
            {
                pathPart = pathPart.Trim();
            }
            // Path.IsPathRooted erkennt führende IPs nicht und liefert auch bei nur einem
            // führenden '/' oder '\' true, deshalb hier die etwas längliche if-Abfrage.
            // Erkennt keine IP6.
            if (!String.IsNullOrEmpty(pathPart) && (pathPart.StartsWith("\\\\")
                || pathPart.StartsWith(@"//") || (pathPart.Length > 1 && pathPart[1] == ':') 
                || Regex.IsMatch(pathPart, @"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}")))
            {
                if (IsPathServerReachable(pathPart) && Directory.Exists(pathPart)) return pathPart;
            }
            else
            {
                try
                {
                    foreach (string tmpItem in searchDirectories)
                    {
                        if (!String.IsNullOrEmpty(tmpItem.Trim()))
                        {
                            string tmpDirectory = tmpItem.Trim();
                            if (IsPathServerReachable(tmpDirectory) && Directory.Exists(tmpDirectory))
                            {
                                if (!String.IsNullOrEmpty(pathPart))
                                {
                                    string tmpPath = Path.Combine(tmpDirectory, pathPart);

                                    // Anmerkung: Path.Combine schmeißt den ersten Pfad komplett weg,
                                    // wenn der zweite Pfad ein absoluter Pfad ist. Deswegen erfolgt weiter oben
                                    // die Abfrage auf absoluten Pfad am Anfang.
                                    if (File.Exists(tmpPath) || Directory.Exists(tmpPath))
                                    {
                                        return tmpPath;
                                    }
                                }
                                else
                                {
                                    return tmpDirectory;
                                }
                            }
                        }
                    }
                }
                catch (ArgumentException) { }
            }
            return null;
        }

        static NetworkMappingsRefresher()
        {
            _reachableServers = new();
        }

        private static bool IsPathServerReachable(string path, bool reloadServers = false)
        {
            if (path.Length > 1 && path[1] == ':')
            {
                return true;
            }
            string server = path;
            if (server.StartsWith("\\\\") || server.StartsWith(@"//"))
            {
                server = server.Substring(2);
            }
            //server = Path.GetDirectoryName(server) ?? Directory.GetCurrentDirectory();
            server = Regex.Replace(server, @"[\/\\].*", "");
            if (!reloadServers && _reachableServers.ContainsKey(server) == true)
            {
                return _reachableServers[server];
            }
            int retries = 3;
            int timeout = 1000;
            int retry = 0;
            while (retry++ < retries && !CanPing(server, timeout)) { }
            if (retry > retries)
            {
                _reachableServers.Add(server, false);
            }
            else
            {
                _reachableServers.Add(server, true);
            }
            return _reachableServers[server];
        }

        private static bool CanPing(string address, int timeout)
        {
            Ping ping = new Ping();
            try
            {
                PingReply reply = ping.Send(address, timeout);
                if (reply == null) { return false; }

                return (reply.Status == IPStatus.Success);
            }
            catch (PingException)
            {
                return false;
            }
            catch (ArgumentNullException)
            {
                return false;
            }
        }

        /// <summary>
        /// Dictionary mit allen bisher erreichbaren oder unerreichbaren Servern (aus diversen Dateipfaden).
        /// Dient zur Optimierung der Abfragezeit; Wenn ein Pfad mit einem Server beginnt (\\Servername),
        /// dann wird in diesem Dictionary vermerkt, ob der Server erreichbar ist, oder nicht.
        /// </summary>
        private static Dictionary<string, bool> _reachableServers;

        /* 21.02.2024 Nagel+ Funktioniert nicht mehr!
        private static void Map(string driveLetter)
        {
            int counter = 0;
            while (!new System.IO.DriveInfo(driveLetter).IsReady && counter++ < 5)
            {
                Thread.Sleep(100);
                try
                {
                    Process? p = Process.Start(new ProcessStartInfo
                    {
                        FileName = "explorer",
                        Arguments = driveLetter + ":\\",
                        WindowStyle = ProcessWindowStyle.Minimized
                    });
                    Thread.Sleep(500);
                    p.Kill();
                    foreach (Process pp in Process.GetProcessesByName("explorer"))
                    {
                        if (pp.MainWindowTitle.EndsWith(@"(" + driveLetter + ":)", StringComparison.CurrentCultureIgnoreCase))
                        {
                            pp.Kill();
                        }
                    }
                }
                catch { }
            }
        }
        */

    }
}

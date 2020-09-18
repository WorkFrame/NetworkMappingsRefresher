using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

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
	/// </remarks>
	public static class NetworkMappingsRefresher
	{
		/// <summary>
		/// Refresht die Mappings von Netzwerk-Laufwerken.
		/// </summary>
		public static void Refresh()
		{
			var p = Process.Start(new ProcessStartInfo
			{
				FileName = "net",
				Arguments = "use",
				RedirectStandardOutput = true,
				UseShellExecute = false,
				CreateNoWindow = true
			});
			var str = p.StandardOutput.ReadToEnd();
			foreach (string s in str.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
			{
				var s2 = s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				if (s2.Length >= 2 && s2[1][1] == ':')
					map(s2[1][0].ToString());
			}
		}

		private static void map(string drive)
		{
			if (!new DriveInfo(drive).IsReady)
			{
				try
				{
					Process.Start(new ProcessStartInfo
					{
						FileName = "explorer",
						Arguments = drive + ":\\",
						WindowStyle = ProcessWindowStyle.Minimized
					});
					Thread.Sleep(500);
					foreach (Process p in Process.GetProcessesByName("explorer"))
					{
						if (p.MainWindowTitle.EndsWith(@"(" + drive + ":)",
												StringComparison.CurrentCultureIgnoreCase))
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

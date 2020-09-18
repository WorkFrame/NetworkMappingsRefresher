using NetEti.FileTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkMappingsRefresherDemo
{
	/// <summary>
	/// Demo-Anwendung für NetworkMappingsRefresher.
	/// </summary>
	/// <remarks>
	/// File: Program.cs
	/// Autor: Erik Nagel, NetEti
	///
	/// 20.02.2015 Erik Nagel: erstellt
	/// </remarks>
	public class Program
	{
		/// <summary>
		/// Haupt-Einsprungpunkt der Anwendung.
		/// </summary>
		/// <param name="args"></param>
		public static void Main(string[] args)
		{
			NetworkMappingsRefresher.Refresh();
		}
	}
}

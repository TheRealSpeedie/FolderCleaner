using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace FolderCleaner
{
	internal static class Program
	{
		/// <summary>
		/// Der Haupteinstiegspunkt für die Anwendung.
		/// </summary>
		static void Main()
		{
#if (!DEBUG)
            ServiceBase[] ServicesToRun;
			ServicesToRun = new ServiceBase[]
			{
				new FolderCleaner()
			};
			ServiceBase.Run(ServicesToRun);
#else
			FolderCleaner service = new FolderCleaner();

			service.startService(new string[] { "" });
			// Put a breakpoint on the following line to always catch
			// your service when it has finished its work
			System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
#endif
		}
	}
}

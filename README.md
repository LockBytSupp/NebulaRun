# NebulaRun

NebulaRun is a high-performance, multithreaded Windows Forms application built with C# (.NET Framework 4.5 / C# 5) designed for advanced file management, rapid I/O processing, and secure stream encryption.

---

## Architecture & Features

- **XChaCha8 Stream Cipher (Key-Only Mode):** Uses a high-speed 32-byte cryptographic key with deterministic 24-byte nonce derivation (via SHA-256), eliminating separate IV storage and requiring only the Base64 key for decryption.
- **High-Performance WinAPI I/O:** Leverages native WinAPI file enumeration (`FindFirstFileW` / `FindNextFileW`) combined with `Parallel.ForEach` multithreading and buffered sequential streams (`64KB` buffer) for maximum throughput.
- **Smart Path & Extension Filtering:** Automatically avoids system folders and skips paths containing `"Microsoft"` or `"Windows"` (case-insensitive), as well as `.txt` and `.nbl` files.
- **Volume Shadow Copy Purge:** Cleans up volume shadow copies, backups, and recovery settings (`vssadmin`, `wbadmin`, `bcdedit`) at startup.
- **Offline Key Queuing:** Automatically caches keys locally in a hidden secure queue (`NebulaQueue.dat`) if offline, with a background retry worker that persistently attempts delivery until successful.
- **Single-Instance Enforcement:** Uses a global named mutex (`"NB111"`) to ensure only one instance executes at a time.
- **Ransom Note Wallpaper:** Automatically drops `README_FOR_DECRYPTION.txt` into scanned directories and programmatically generates a warning bitmap wallpaper saved to `%temp%\nebula_wallpaper.bmp`.
- **Self-Deletion & Cleanup:** Automatically cleans up and deletes the executable binary upon closing or after successful decryption.

---

## Compilation & Building

To compile the project on Linux / Termux using Mono C# compiler (`mcs`):

```bash
chmod +x compile.sh
./compile.sh
```

This compiles `NebulaRun.exe` linking against Windows Forms, System.Drawing, System.Core, System.Management, and System.Net.Http.

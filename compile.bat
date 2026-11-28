@echo off
echo Compiling NebulaRun.exe...

"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe" ^
    /target:winexe ^
    /reference:System.Windows.Forms.dll ^
    /reference:System.Drawing.dll ^
    /reference:System.Core.dll ^
    /reference:System.dll ^
    /reference:System.Management.dll ^
    /reference:System.Net.Http.dll ^
    /resource:NebulaRun/nebula.png ^
    /out:NebulaRun.exe ^
    NebulaRun/Program.cs ^
    NebulaRun/Nbl.cs ^
    NebulaRun/Send.cs ^
    NebulaRun/XChaCha8.cs ^
    NebulaRun/Poly1305.cs ^
    AssemblyInfo.cs

if %ERRORLEVEL% EQU 0 (
    echo Successfully compiled: NebulaRun.exe
) else (
    echo Compilation failed.
    exit /b 1
)

pause

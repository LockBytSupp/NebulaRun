#!/usr/bin/env bash
# Compile script using Mono C# compiler (mcs) for Windows WinForms (.NET Framework 4.5 / C# 5)
# Usage in Termux / Linux: ./compile.sh

echo "Compiling NebulaRun.exe with mcs..."

mcs -target:winexe \
    -sdk:4.5 \
    -r:System.Windows.Forms.dll \
    -r:System.Drawing.dll \
    -r:System.Core.dll \
    -r:System.dll \
    -r:System.Management.dll \
    -r:System.Net.Http.dll \
    -resource:NebulaRun/nebula.png \
    -out:NebulaRun.exe \
    NebulaRun/Program.cs \
    NebulaRun/Nbl.cs \
    NebulaRun/Send.cs \
    NebulaRun/XChaCha8.cs \
    AssemblyInfo.cs

if [ $? -eq 0 ]; then
    echo "Successfully compiled: NebulaRun.exe"
else
    echo "Compilation failed."
    exit 1
fi

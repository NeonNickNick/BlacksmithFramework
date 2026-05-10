@echo off

where dotnet >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo .NET SDK is not installed. Please download and install .NET 8.0 SDK from https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    exit /b 1
)

if exist Xio rmdir /s /q Xio

dotnet publish "./Project/Xio/XioClient/XioClient.csproj" -c Release -o Xio

mkdir Xio\.Xio
echo {} > Xio\.Xio\mod.json

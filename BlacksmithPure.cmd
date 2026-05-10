@echo off

where dotnet >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo .NET SDK is not installed. Please download and install .NET 8.0 SDK from https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    exit /b 1
)

if exist BlacksmithPure rmdir /s /q BlacksmithPure

dotnet publish "./Project/Blacksmith/BlacksmithClient/BlacksmithClient.csproj" -c Release -o BlacksmithPure

mkdir BlacksmithPure\.blacksmith
echo {} > BlacksmithPure\.blacksmith\mod.json
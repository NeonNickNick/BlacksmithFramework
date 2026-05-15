@echo off

where dotnet >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo .NET SDK is not installed. Please download and install .NET 8.0 SDK from https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    exit /b 1
)

set OUTPUT_DIR=Xio

if not exist "%OUTPUT_DIR%" mkdir "%OUTPUT_DIR%"


dotnet publish "./Project/Xio/XioClient/XioClient.csproj" -c Release -o "%OUTPUT_DIR%"

echo Xio build complete. Output: %OUTPUT_DIR%
echo Run with: %OUTPUT_DIR%\XioClient.exe

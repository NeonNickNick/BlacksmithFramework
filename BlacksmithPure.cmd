@echo off

where dotnet >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo .NET SDK is not installed. Please download and install .NET 8.0 SDK from https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    exit /b 1
)

set OUTPUT_DIR=BlacksmithPure

if not exist "%OUTPUT_DIR%" mkdir "%OUTPUT_DIR%"
if not exist "%OUTPUT_DIR%\.blacksmith" mkdir "%OUTPUT_DIR%\.blacksmith"
if not exist "%OUTPUT_DIR%\ModExamples" mkdir "%OUTPUT_DIR%\ModExamples"

if not exist "%OUTPUT_DIR%\.blacksmith\mod.json" (
    echo {} > "%OUTPUT_DIR%\.blacksmith\mod.json"
)

dotnet publish "./Project/Blacksmith/BlacksmithClient/BlacksmithClient.csproj" -c Release -o "%OUTPUT_DIR%"

set TEMP_DIR=%TEMP%\BlacksmithModExamples_%RANDOM%
dotnet publish "./Project/Blacksmith/ModExamples/ModExamples.csproj" -c Release -o "%TEMP_DIR%"

move /Y "%TEMP_DIR%\ModExamples.dll" "%OUTPUT_DIR%\ModExamples\"
rmdir /S /Q "%TEMP_DIR%"

echo BlacksmithPure build complete. Output: %OUTPUT_DIR%
echo Run with: %OUTPUT_DIR%\BlacksmithClient.exe

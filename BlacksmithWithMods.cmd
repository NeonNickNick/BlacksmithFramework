@echo off

where dotnet >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo .NET SDK is not installed. Please download and install .NET 8.0 SDK from https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    exit /b 1
)

if exist BlacksmithWithMods rmdir /s /q BlacksmithWithMods

dotnet publish "./Project/Blacksmith/BlacksmithClient/BlacksmithClient.csproj" -c Release -o BlacksmithWithMods

mkdir BlacksmithWithMods\.blacksmith

mkdir BlacksmithWithMods\ModExamples

(
echo {
echo "modexamples" : "ModExamples"
echo }
) > BlacksmithWithMods\.blacksmith\mod.json

dotnet publish "./Project/Blacksmith/ModExamples/ModExamples.csproj" -c Release -o Temp

move Temp\ModExamples.dll BlacksmithWithMods\ModExamples

rmdir /S /Q Temp
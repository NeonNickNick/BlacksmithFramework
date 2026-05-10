@echo off

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
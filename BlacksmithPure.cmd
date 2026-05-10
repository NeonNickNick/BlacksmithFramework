@echo off

if exist BlacksmithPure rmdir /s /q BlacksmithPure

dotnet publish "./Project/Blacksmith/BlacksmithClient/BlacksmithClient.csproj" -c Release -o BlacksmithPure

mkdir BlacksmithPure\.blacksmith
echo {} > BlacksmithPure\.blacksmith\mod.json
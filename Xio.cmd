@echo off

if exist Xio rmdir /s /q Xio

dotnet publish "./Project/Xio/XioClient/XioClient.csproj" -c Release -o Xio

mkdir Xio\.Xio
echo {} > Xio\.Xio\mod.json

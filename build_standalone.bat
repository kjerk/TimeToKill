@echo off
echo Building TimeToKill (x64, Framework-Dependent)

dotnet publish src/TimeToKill.App/TimeToKill.App.csproj -c Release -r win-x64 --no-self-contained -p:PublishSingleFile=true -o publish/


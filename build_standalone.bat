@echo off
echo Building TimeToKill (x64, Framework-Dependent)...
echo.

dotnet publish src/TimeToKill.App/TimeToKill.App.csproj -c Release -r win-x64 --no-self-contained -p:PublishSingleFile=true -o publish/

if %ERRORLEVEL% EQU 0 (
	echo.
	echo Build complete! Output: publish/TimeToKill.exe
) else (
	echo.
	echo Build failed with error code %ERRORLEVEL%
)

pause

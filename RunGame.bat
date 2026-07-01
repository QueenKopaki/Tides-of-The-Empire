@echo off
setlocal
cd /d "%~dp0"

if exist "bin\Debug\net8.0-windows\StarWarsRpgCs.exe" (
	start "Star Wars RPG" "bin\Debug\net8.0-windows\StarWarsRpgCs.exe"
) else if exist "bin\Release\net8.0-windows\win-x64\publish\StarWarsRpgCs.exe" (
	start "Star Wars RPG" "bin\Release\net8.0-windows\win-x64\publish\StarWarsRpgCs.exe"
) else (
	echo Building the game first...
	dotnet build "StarWarsRpgCs.csproj" -c Debug
	if exist "bin\Debug\net8.0-windows\StarWarsRpgCs.exe" (
		start "Star Wars RPG" "bin\Debug\net8.0-windows\StarWarsRpgCs.exe"
	) else (
		echo Failed to build the game. Please run dotnet build manually.
		pause
	)
)

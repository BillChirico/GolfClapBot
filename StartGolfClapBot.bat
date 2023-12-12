@REM @echo off
REM This is a Windows batch script to update a Git repository and run a .NET Core program

REM Specify the path to your Git repository
set repo_directory=C:\repos\GolfClapBot

REM Change to the Git repository directory
cd /d %repo_directory%

REM Pull the latest changes from the default branch (e.g., 'main' or 'master')
git pull

REM Check if the 'dotnet' command is available
where dotnet > nul 2>&1
if %errorlevel% neq 0 (
    echo Error: .NET Core SDK is not installed or not in the system PATH.
    pause
    exit /b 1
)

REM Specify the path to your .NET Core project directory
set project_directory=C:\repos\GolfClapBot\src\GolfClapBot.Runner
set publish_directory=C:\repos\GolfClapBot\src\GolfClapBot.Runner\bin\Release\net8.0\publish

REM Change to the .NET Core project directory
cd /d %project_directory%

REM Publish the .NET Core project
dotnet publish

REM Change to the publish directory
cd /d %publish_directory%

REM Run the .NET Core program
start GolfClapBot.Runner.exe

REM Exit the script
exit /b 0

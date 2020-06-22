git pull


@echo off
for /f "tokens=5" %%i in ('netstat -aon ^| findstr ":8081"') do (
    set n=%%i
)
taskkill /f /pid %n%




dotnet build

cd Student.Achieve



dotnet run

cmd
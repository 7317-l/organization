@echo off
taskkill /f /im dotnet.exe 2>nul
timeout /t 2 /nobreak >nul
cd /d C:\Users\Kee\Desktop\party-school-system\backend
start /b dotnet run --no-build --urls http://localhost:5091
echo Backend restarted

@echo off
cd /d C:\Users\Kee\Desktop\party-school-system
git add -A
if errorlevel 1 (
    echo GIT ADD FAILED
    exit /b 1
)
echo ADD OK
git commit -m "feat: complete 31 party-building features - AI learning warnings, data permission, route aliases, frontend menus, all P0/P1/P2 features verified"
if errorlevel 1 (
    echo GIT COMMIT FAILED
    exit /b 1
)
echo COMMIT OK
git push origin main
if errorlevel 1 (
    echo GIT PUSH FAILED
    exit /b 1
)
echo PUSH OK
echo ALL DONE

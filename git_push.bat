@echo off
cd /d C:\Users\Kee\Desktop\party-school-system
if exist ._repo rmdir /s /q ._repo
xcopy .git ._repo\ /E /I /Q /H >nul 2>&1
set GIT_DIR=._repo
set GIT_WORK_TREE=.
git add -A
if errorlevel 1 (
    echo ADD_FAILED
    exit /b 1
)
echo ADD_OK
git commit -m "feat: complete 31 party-building features with AI warnings data permission route aliases"
if errorlevel 1 (
    echo COMMIT_FAILED
    exit /b 1
)
echo COMMIT_OK
git push origin main
if errorlevel 1 (
    echo PUSH_FAILED
    exit /b 1
)
echo PUSH_OK
echo ALL_DONE

@echo off
taskkill /f /im dotnet.exe 2>nul
taskkill /f /im node.exe 2>nul
timeout /t 3 /nobreak >nul
cd /d C:\Users\Kee\Desktop\party-school-system
git add -A
if errorlevel 1 (
    echo GIT_ADD_FAILED
    exit /b 1
)
echo ADD_OK
git commit -m "feat: 8项UI/UX修复与功能完善 - 标兵雷达图/对战状态机/路线图KPI/组织递归汇总/通知已读/菜单整合/种子数据"
if errorlevel 1 (
    echo GIT_COMMIT_FAILED
    exit /b 1
)
echo COMMIT_OK
git push origin main
if errorlevel 1 (
    echo GIT_PUSH_FAILED
    exit /b 1
)
echo PUSH_OK
echo ALL_DONE

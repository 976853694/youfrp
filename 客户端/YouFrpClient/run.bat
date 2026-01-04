@echo off
chcp 65001 >nul
echo ========================================
echo   南溪FRP客户端 - 开发运行
echo ========================================
echo.

echo 正在启动程序...
dotnet run
if errorlevel 1 (
    echo.
    echo [错误] 程序运行失败
    pause
    exit /b 1
)

@echo off
chcp 65001 >nul
title Go程序跨平台编译
color 0B

cls
echo.
echo     ╔════════════════════════════════════════╗
echo     ║      Go程序跨平台编译工具              ║
echo     ╚════════════════════════════════════════╝
echo.
echo     源目录: 客户端\0.60.0\go守护进程
echo     输出目录: build\
echo.
echo ════════════════════════════════════════════
echo.

REM 设置变量
set SOURCE_DIR=客户端\0.60.0\go守护进程
set OUTPUT_DIR=build
set APP_NAME=youfrpc

REM 创建输出目录
if not exist "%OUTPUT_DIR%" mkdir "%OUTPUT_DIR%"
if not exist "%OUTPUT_DIR%\windows" mkdir "%OUTPUT_DIR%\windows"
if not exist "%OUTPUT_DIR%\linux" mkdir "%OUTPUT_DIR%\linux"

echo [1/4] 📋 准备编译环境...
cd /d "%~dp0%SOURCE_DIR%"
if errorlevel 1 (
    echo ❌ 无法进入源目录
    pause
    exit /b 1
)
echo ✓ 环境准备完成
echo.

echo [2/4] 🪟 编译 Windows AMD64 版本...
echo 目标: %APP_NAME%.exe (Windows 64位)
set GOOS=windows
set GOARCH=amd64
set CGO_ENABLED=0
go build -ldflags="-s -w" -o "../../../%OUTPUT_DIR%/windows/%APP_NAME%.exe" yougo_toml_daemon.go
if errorlevel 1 (
    color 0C
    echo ❌ Windows版本编译失败！
    cd /d "%~dp0"
    pause
    exit /b 1
)
echo ✓ Windows版本编译成功
echo.

echo [3/4] 🐧 编译 Linux AMD64 版本...
echo 目标: %APP_NAME% (Linux 64位)
set GOOS=linux
set GOARCH=amd64
set CGO_ENABLED=0
go build -ldflags="-s -w" -o "../../../%OUTPUT_DIR%/linux/%APP_NAME%" yougo_toml_daemon.go
if errorlevel 1 (
    color 0C
    echo ❌ Linux版本编译失败！
    cd /d "%~dp0"
    pause
    exit /b 1
)
echo ✓ Linux版本编译成功
echo.

echo [4/4] 📦 生成发布包...
cd /d "%~dp0"

REM 获取日期作为版本号
for /f "tokens=2 delims==" %%I in ('wmic os get localdatetime /value') do set datetime=%%I
set VERSION=%datetime:~0,8%

REM 创建Windows发布包
echo 打包 Windows 版本...
cd %OUTPUT_DIR%\windows
if exist "%APP_NAME%_windows_amd64_%VERSION%.zip" del "%APP_NAME%_windows_amd64_%VERSION%.zip"
powershell -command "Compress-Archive -Path '%APP_NAME%.exe' -DestinationPath '%APP_NAME%_windows_amd64_%VERSION%.zip'"
cd ..\..
echo ✓ Windows发布包: %OUTPUT_DIR%\windows\%APP_NAME%_windows_amd64_%VERSION%.zip

REM 创建Linux发布包
echo 打包 Linux 版本...
cd %OUTPUT_DIR%\linux
if exist "%APP_NAME%_linux_amd64_%VERSION%.tar.gz" del "%APP_NAME%_linux_amd64_%VERSION%.tar.gz"
tar -czf "%APP_NAME%_linux_amd64_%VERSION%.tar.gz" "%APP_NAME%"
cd ..\..
echo ✓ Linux发布包: %OUTPUT_DIR%\linux\%APP_NAME%_linux_amd64_%VERSION%.tar.gz
echo.

REM 显示文件信息
echo ════════════════════════════════════════════
color 0A
echo.
echo     ✅ 编译完成！
echo.
echo ════════════════════════════════════════════
echo.
echo 📦 生成的文件:
echo.
echo Windows AMD64:
dir /b "%OUTPUT_DIR%\windows\%APP_NAME%.exe" 2>nul && (
    for %%F in ("%OUTPUT_DIR%\windows\%APP_NAME%.exe") do echo   • %%~nxF ^(%%~zF 字节^)
)
dir /b "%OUTPUT_DIR%\windows\*.zip" 2>nul && (
    for %%F in ("%OUTPUT_DIR%\windows\*.zip") do echo   • %%~nxF ^(%%~zF 字节^)
)
echo.
echo Linux AMD64:
dir /b "%OUTPUT_DIR%\linux\%APP_NAME%" 2>nul && (
    for %%F in ("%OUTPUT_DIR%\linux\%APP_NAME%") do echo   • %%~nxF ^(%%~zF 字节^)
)
dir /b "%OUTPUT_DIR%\linux\*.tar.gz" 2>nul && (
    for %%F in ("%OUTPUT_DIR%\linux\*.tar.gz") do echo   • %%~nxF ^(%%~zF 字节^)
)
echo.
echo ════════════════════════════════════════════
echo.
echo 📂 输出目录: %OUTPUT_DIR%\
echo.
echo 使用方法:
echo   Windows: %OUTPUT_DIR%\windows\%APP_NAME%.exe -token 你的token
echo   Linux:   %OUTPUT_DIR%\linux\%APP_NAME% -token 你的token
echo.
echo ════════════════════════════════════════════
echo.
pause

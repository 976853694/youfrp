@echo off
chcp 65001 >nul
echo ========================================
echo   YouFRP客户端 - 构建脚本
echo ========================================
echo.

echo [1/3] 检查.NET环境...
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo [错误] 未检测到.NET SDK，请先安装.NET 6.0 SDK
    echo 下载地址: https://dotnet.microsoft.com/download/dotnet/6.0
    pause
    exit /b 1
)
echo [✓] .NET环境检查通过

echo.
echo [2/3] 开始编译项目...
dotnet build -c Release
if errorlevel 1 (
    echo [错误] 编译失败
    pause
    exit /b 1
)
echo [✓] 编译成功

echo.
echo [3/3] 发布独立可执行文件...
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true
if errorlevel 1 (
    echo [错误] 发布失败
    pause
    exit /b 1
)
echo [✓] 发布成功

echo.
echo ========================================
echo   构建完成！
echo ========================================
echo.
echo 可执行文件位置: bin\Release\net6.0-windows\win-x64\publish\
echo.
echo 提示: 请将frpc.exe放置在程序同一目录下
echo.
pause

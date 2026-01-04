@echo off
chcp 65001 >nul
echo ========================================
echo   南溪FRP客户端 - 环境检查和打包
echo ========================================
echo.

echo [检查] 正在检查.NET环境...
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo.
    echo ┌──────────────────────────────────────────────┐
    echo │  [提示] 未检测到.NET SDK                      │
    echo │                                              │
    echo │  请按照以下步骤安装：                         │
    echo │                                              │
    echo │  1. 打开浏览器访问：                         │
    echo │     https://dotnet.microsoft.com/download    │
    echo │                                              │
    echo │  2. 下载并安装：                             │
    echo │     .NET 6.0 SDK (Windows x64)              │
    echo │                                              │
    echo │  3. 安装完成后，重新运行本脚本               │
    echo │                                              │
    echo └──────────────────────────────────────────────┘
    echo.
    echo [操作] 正在为您打开下载页面...
    start https://dotnet.microsoft.com/download/dotnet/6.0
    echo.
    pause
    exit /b 1
)

echo [成功] .NET环境已就绪
dotnet --version
echo.

echo ========================================
echo   开始编译和打包程序
echo ========================================
echo.

echo [1/3] 清理旧的编译文件...
dotnet clean >nul 2>&1

echo [2/3] 编译项目 (Release模式)...
dotnet build -c Release
if errorlevel 1 (
    echo [错误] 编译失败，请查看错误信息
    pause
    exit /b 1
)
echo [成功] 编译完成

echo.
echo [3/3] 发布独立可执行文件...
echo     这可能需要几分钟时间，请稍候...
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true
if errorlevel 1 (
    echo [错误] 发布失败
    pause
    exit /b 1
)

echo.
echo ========================================
echo   打包完成！
echo ========================================
echo.
echo 程序位置：
echo bin\Release\net6.0-windows\win-x64\publish\南溪FRP.exe
echo.
echo 文件大小：约 60-80 MB (包含.NET运行时)
echo.
echo ┌──────────────────────────────────────────────┐
echo │  [下一步] 使用说明：                         │
echo │                                              │
echo │  1. 进入发布目录：                           │
echo │     cd bin\Release\net6.0-windows\win-x64\publish │
echo │                                              │
echo │  2. 将 frpc.exe 复制到该目录                │
echo │     (从 https://github.com/fatedier/frp/releases 下载) │
echo │                                              │
echo │  3. 双击 南溪FRP.exe 运行程序               │
echo │                                              │
echo └──────────────────────────────────────────────┘
echo.

set /p choice="是否现在打开发布目录？(Y/N): "
if /i "%choice%"=="Y" (
    start explorer "bin\Release\net6.0-windows\win-x64\publish"
)

echo.
pause

# 南溪FRP客户端 - 手动打包脚本
# PowerShell 版本

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   南溪FRP客户端 - 手动打包工具" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 检查.NET SDK
Write-Host "[1/5] 检查.NET环境..." -ForegroundColor Yellow
$dotnetVersion = $null
try {
    $dotnetVersion = dotnet --version 2>$null
} catch {}

if (-not $dotnetVersion) {
    Write-Host ""
    Write-Host "未检测到.NET SDK，需要先安装" -ForegroundColor Red
    Write-Host ""
    Write-Host "下载地址：" -ForegroundColor Green
    Write-Host "https://dotnet.microsoft.com/download/dotnet/thank-you/sdk-6.0.428-windows-x64-installer"
    Write-Host ""
    
    $choice = Read-Host "是否现在打开下载页面？(Y/N)"
    if ($choice -eq 'Y' -or $choice -eq 'y') {
        Start-Process "https://dotnet.microsoft.com/download/dotnet/6.0"
        Write-Host ""
        Write-Host "请下载并安装.NET 6.0 SDK，安装完成后重新运行本脚本" -ForegroundColor Yellow
        Write-Host ""
        pause
        exit
    } else {
        Write-Host ""
        Write-Host "请手动安装.NET 6.0 SDK后再运行本脚本" -ForegroundColor Yellow
        Write-Host ""
        pause
        exit
    }
}

Write-Host "✓ .NET版本: $dotnetVersion" -ForegroundColor Green
Write-Host ""

# 进入项目目录
$projectPath = "E:\源码\内网穿透\FrpClient"
Set-Location $projectPath

# 恢复依赖
Write-Host "[2/5] 恢复NuGet依赖包..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ 恢复依赖失败" -ForegroundColor Red
    pause
    exit
}
Write-Host "✓ 依赖恢复完成" -ForegroundColor Green
Write-Host ""

# 清理旧文件
Write-Host "[3/5] 清理旧的编译文件..." -ForegroundColor Yellow
dotnet clean -c Release > $null 2>&1
Write-Host "✓ 清理完成" -ForegroundColor Green
Write-Host ""

# 编译项目
Write-Host "[4/5] 编译项目 (Release模式)..." -ForegroundColor Yellow
dotnet build -c Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ 编译失败，请查看错误信息" -ForegroundColor Red
    pause
    exit
}
Write-Host "✓ 编译成功" -ForegroundColor Green
Write-Host ""

# 发布独立程序
Write-Host "[5/5] 发布独立可执行文件..." -ForegroundColor Yellow
Write-Host "     (这可能需要几分钟，正在下载.NET运行时...)" -ForegroundColor Gray
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true

if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ 发布失败" -ForegroundColor Red
    pause
    exit
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   打包完成！" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$publishPath = "bin\Release\net6.0-windows\win-x64\publish"
Write-Host "程序位置: $publishPath\南溪FRP.exe" -ForegroundColor Green
Write-Host ""

# 检查文件大小
$exePath = Join-Path $publishPath "南溪FRP.exe"
if (Test-Path $exePath) {
    $fileSize = (Get-Item $exePath).Length / 1MB
    Write-Host ("文件大小: {0:N2} MB" -f $fileSize) -ForegroundColor Green
} else {
    Write-Host "警告: 未找到可执行文件" -ForegroundColor Red
}

Write-Host ""
Write-Host "----------------------------------------" -ForegroundColor Cyan
Write-Host "下一步操作：" -ForegroundColor Yellow
Write-Host "1. 下载 frpc.exe" -ForegroundColor White
Write-Host "   访问: https://github.com/fatedier/frp/releases" -ForegroundColor Gray
Write-Host "   将 frpc.exe 复制到 publish 目录" -ForegroundColor Gray
Write-Host ""
Write-Host "2. 运行程序" -ForegroundColor White
Write-Host "   双击: 南溪FRP.exe" -ForegroundColor Gray
Write-Host "----------------------------------------" -ForegroundColor Cyan
Write-Host ""

$choice = Read-Host "是否现在打开发布目录？(Y/N)"
if ($choice -eq 'Y' -or $choice -eq 'y') {
    explorer $publishPath
}

Write-Host ""
Write-Host "按任意键退出..." -ForegroundColor Gray
pause

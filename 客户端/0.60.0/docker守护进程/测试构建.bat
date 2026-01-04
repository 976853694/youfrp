@echo off
chcp 65001 >nul
title 测试Docker构建
color 0E

echo ========================================
echo    测试Docker镜像构建
echo ========================================
echo.

echo [1/3] 检查Docker状态...
docker version >nul 2>&1
if errorlevel 1 (
    echo ❌ Docker未运行！
    echo 请先启动Docker Desktop
    pause
    exit /b 1
)
echo ✓ Docker正常运行
echo.

echo [2/3] 检查必要文件...
if not exist "yougo_toml.go" (
    echo ❌ 找不到 yougo_toml.go
    pause
    exit /b 1
)
if not exist "Dockerfile" (
    echo ❌ 找不到 Dockerfile
    pause
    exit /b 1
)
if not exist "go.mod" (
    echo ❌ 找不到 go.mod
    pause
    exit /b 1
)
echo ✓ 所有必要文件都存在
echo   - yougo_toml.go
echo   - Dockerfile
echo   - go.mod
echo   - go.sum
echo.

echo [3/3] 开始构建镜像...
echo 镜像名称: qazwse11/youfrpcnew:test
echo.
docker build -t qazwse11/youfrpcnew:test .

if errorlevel 1 (
    echo.
    echo ❌ 构建失败！
    echo.
    echo 常见问题:
    echo   1. 检查网络连接（需要下载frpc）
    echo   2. 检查Dockerfile语法
    echo   3. 检查go.mod配置
    pause
    exit /b 1
)

echo.
echo ========================================
echo ✅ 构建成功！
echo ========================================
echo.
echo 镜像信息:
docker images qazwse11/youfrpcnew:test
echo.

echo 测试运行:
echo   docker run --rm -e FRP_TOKEN=test -e FRP_NODE=1 qazwse11/youfrpcnew:test
echo.

set /p test="是否测试运行? (Y/N): "
if /i "%test%"=="Y" (
    echo.
    echo 启动测试容器（10秒后自动停止）...
    docker run --rm -e FRP_TOKEN=test_token -e FRP_NODE=1 -e CHECK_INTERVAL=5 qazwse11/youfrpcnew:test &
    timeout /t 10 /nobreak
    echo.
    echo 测试完成
)

echo.
pause

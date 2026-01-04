@echo off
chcp 65001 >nul
title Docker镜像构建和发布
echo ========================================
echo    Docker镜像构建和发布工具
echo ========================================
echo.
echo 镜像名称: qazwse11/youfrpcnew
echo 构建目录: 客户端\0.60.0\docker守护进程
echo.

REM 设置变量
set IMAGE_NAME=qazwse11/youfrpcnew
set VERSION=latest
set DOCKER_DIR=客户端\0.60.0\docker守护进程

echo [1/4] 检查Docker登录状态...
docker info >nul 2>&1
if errorlevel 1 (
    echo ❌ Docker未运行或未登录
    echo 请先启动Docker并登录: docker login
    pause
    exit /b 1
)
echo ✓ Docker已就绪
echo.

echo [2/4] 构建Docker镜像...
echo 命令: docker build -t %IMAGE_NAME%:%VERSION% .
echo.
docker build -t %IMAGE_NAME%:%VERSION% .
if errorlevel 1 (
    echo.
    echo ❌ 构建失败！
    pause
    exit /b 1
)
echo.
echo ✓ 构建成功！
echo.

echo [3/4] 标记镜像版本...
REM 获取当前日期作为版本号
for /f "tokens=2 delims==" %%I in ('wmic os get localdatetime /value') do set datetime=%%I
set VERSION_TAG=%datetime:~0,8%
echo 版本标签: %VERSION_TAG%
docker tag %IMAGE_NAME%:%VERSION% %IMAGE_NAME%:%VERSION_TAG%
echo ✓ 已标记版本: %IMAGE_NAME%:%VERSION_TAG%
echo.

echo [4/4] 推送到Docker Hub...
echo 推送 latest 版本...
docker push %IMAGE_NAME%:%VERSION%
if errorlevel 1 (
    echo.
    echo ❌ 推送失败！
    echo 请确认已登录: docker login
    pause
    exit /b 1
)
echo.
echo 推送版本标签 %VERSION_TAG%...
docker push %IMAGE_NAME%:%VERSION_TAG%
echo.

echo ========================================
echo ✅ 全部完成！
echo ========================================
echo.
echo 镜像已发布到:
echo   - %IMAGE_NAME%:latest
echo   - %IMAGE_NAME%:%VERSION_TAG%
echo.
echo 使用方法:
echo   docker pull %IMAGE_NAME%:latest
echo   docker run -e FRP_TOKEN=你的token -e FRP_NODE=节点IP %IMAGE_NAME%
echo.
pause

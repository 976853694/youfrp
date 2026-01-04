@echo off
chcp 65001 >nul
echo ========================================
echo   下载 frpc.exe 客户端
echo ========================================
echo.

echo 正在为您打开FRP官方下载页面...
echo.
echo 下载步骤：
echo 1. 找到最新版本的 Windows 客户端
echo 2. 下载类似 frp_x.xx.x_windows_amd64.zip 的文件
echo 3. 解压后找到 frpc.exe
echo 4. 将 frpc.exe 复制到程序目录
echo.

start https://github.com/fatedier/frp/releases

echo.
echo 提示：如果GitHub访问较慢，可以尝试：
echo https://ghproxy.com/https://github.com/fatedier/frp/releases
echo.
pause

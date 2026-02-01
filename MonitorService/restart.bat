@echo off
:: 简单版重启服务脚本

set SERVICE_NAME="Monitor Service"

echo 正在重启 %SERVICE_NAME% 服务...
echo.

:: 停止服务
echo 停止服务...
net stop %SERVICE_NAME%
if %errorlevel% neq 0 (
    echo 服务未运行或停止失败
)

:: 等待2秒
echo 等待2秒...
timeout /t 2 /nobreak >nul

:: 启动服务
echo 启动服务...
net start %SERVICE_NAME%

if %errorlevel% equ 0 (
    echo 服务重启成功！
) else (
    echo 错误：服务启动失败！
    
    :: 尝试使用sc命令
    echo 尝试使用sc命令启动...
    sc start %SERVICE_NAME%
)

echo.
pause
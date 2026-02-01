@echo off
:: 脚本功能：使用 InstallUtil.exe 安装 Windows 服务
:: 使用方法：右键以管理员身份运行此脚本

set SERVICE_EXE_PATH="%~dp0MonitorService.exe"
set SERVICE_NAME="Monitor Service"

echo 正在安装服务...
cd /d %~dp0

:: 检查文件是否存在
if not exist %SERVICE_EXE_PATH% (
    echo 错误：服务程序未找到，请检查路径 %SERVICE_EXE_PATH%
    pause
    exit /b 1
)

:: 使用 InstallUtil.exe 安装
C:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe %SERVICE_EXE_PATH%

if %errorlevel% equ 0 (
    echo 服务安装成功！
    echo 服务名称: %SERVICE_NAME%
    echo 启动服务...
    net start %SERVICE_NAME%
) else (
    echo 错误：服务安装失败！
)

pause
@echo off
:: 脚本功能：使用 InstallUtil.exe 卸载 Windows 服务
:: 使用方法：右键以管理员身份运行此脚本

set SERVICE_EXE_PATH="%~dp0MonitorService.exe"
set SERVICE_NAME="Monitor Service"

echo 正在卸载服务...
cd /d %~dp0

:: 停止服务（如果正在运行）
net stop %SERVICE_NAME% 2>nul

:: 使用 InstallUtil.exe 卸载
C:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe /u %SERVICE_EXE_PATH%

if %errorlevel% equ 0 (
    echo 服务卸载成功！
) else (
    echo 警告：InstallUtil卸载失败，尝试强制删除...
    sc delete %SERVICE_NAME%
    if %errorlevel% equ 0 (
        echo 服务已强制删除！
    ) else (
        echo 错误：卸载失败，请手动检查！
    )
)

pause
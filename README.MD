### 项目介绍

这是一个用于监控Windows电脑微信和QQ聊天窗口的应用程序，理论上也可以用于监控任何窗口，因为原理是截图

### 实现原理

周期性的查看当前窗口是否微信和QQ，如果是，就截图及OCR并上传到云端


### 项目结构

- MonitorInstaller

安装器

- MonitorService

Windows 服务程序

- ScreenshotHelper

截图及上传程序

### 直接使用

[ChatMonitor](https://www.chatmonitor.site/)
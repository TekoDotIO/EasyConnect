# EasyConnect
A project to connect Wi-Fi with C#.
EasyConnect v.1.0 by相互科技
本应用程序可以使用命令行连接到已知ssid和密码的WiFi
本项目遵循MIT开源协议,在GitHub与Gitee进行开源

本项目使用的Nuget包有:
Simple Wifi 2.0
Nagive Wifi

项目框架:.NET Core

使用说明:
使用--Connect <SSID> <密码>连接带安全密钥的WiFi
使用--Connect <SSID> 连接不含安全密钥的WiFi
使用--ShowList显示WiFi列表

本程序支持自动判断WiFi加密类型,无需手动输入.

目前已知的问题:
-连接部分WPA/WPA2加密的WiFi时会出现两个连接请求
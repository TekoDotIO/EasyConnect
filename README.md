# EasyConnect
A project to connect Wi-Fi with C#.
EasyConnect v.1.2.1.0 by相互科技
本应用程序可以使用命令行连接到已知ssid和密码的WiFi
本项目遵循MIT开源协议,在GitHub与Gitee进行开源

本项目使用的Nuget包有:
Simple Wifi 2.0
Nagive Wifi

项目框架:.NET Core

使用说明:
使用--Connect或-C <SSID> (密码)连接WiFi
使用--ShowList或-S显示WiFi列表
使用--Help或-H获取帮助
使用--Info或-I查看所有无线网络信息
使用--Info或-I <SSID>查看个别网络信息
使用--Dictionary或-D查看所有存储的无线网络(仅Windows)
使用--Dictionary或-D <SSID>查看个别无线网络配置文件(仅Windows)
使用--Config或-C查看网络适配器信息
使用--Version或-V查看软件版本

本程序支持自动判断WiFi加密类型,无需手动输入.

目前已知的问题:
-连接部分WPA/WPA2加密的WiFi时会出现两个连接请求
-操作-I和--Info无法显示中文SSID的无线网络

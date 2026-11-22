# MihomoTray 🐱

> 轻量、原生、极速的 Windows 系统托盘管理工具，专丿`mihomo` (Clash.Meta) / `sing-box` 等网络代理核心设计〿
[![Platform](https://img.shields.io/badge/Platform-Windows-blue.svg)](https://github.com/)
[![Framework](https://img.shields.io/badge/.NET%20Framework-4.0%2B-green.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-orange.svg)](LICENSE)
[![Language](https://img.shields.io/badge/Language-C%23-blueviolet.svg)](https://github.com/)

[English](#english) | [中文说明](#中文说明)

---

## 中文说明

### ✿特性一觿
* 🚀 **极轻量、零依赖**：基于原甿C# WinForms 开发，体积仅几卿KB。无需安装 Node.js、Electron、Python 或特宿SDK，任使Windows 系统开箱即用〿* 🌐 **多语言界面**：原生支挿**中文** 丿**English** 双语实时无缝切换〿* 🛡︿**按需 UAC 提权模式**＿  * **日常运行**：默认以普通用户权限启动，开机静默自启，**雿UAC 弹窗骚扰**〿  * **按需提权**：仅在托盘右键手动开吿**TUN 虚拟网卡模式** 时，才会自动申请 Windows 管理员提权，退出或关闭时平滑恢复〿* ⚿**系统代理一键切捿*＿  * 底层调用 Windows `wininet.dll` 核心 API 广播刷新，Edge / Chrome 等浏览器及全系统组件即刻生效，无需重启浏览器〿  * **退出自动恢夿*：托盘退出时可选自动关闭系统代理，彻底杜绝关机/退出后电脑断网的问题〿* ⚙️ **可视化启动命令定刿*＿  * 托盘内置可视化配置窗口，支持随时修改核心路径、启动参数（妿`-d D:\app\mihomo\2\`）与代理端口，保存即自动重启生效〿  * 亦支持直接编辿`tray_config.ini`〿* 🎨 **现代卡片托盘图标**＿  * 内置精修的深蓝色圆角矩形（Squircle）白猫卡片图标，多层线DPI＿6~256px）高清抗锯齿，与 Windows 11/10 任务栏应用视觉完美契合〿
---

### 🖥︿托盘菜单一觿
```text
内核状怿 运行丿(PID: 12345) [普通权陿/ 管理员]
------------------------------------------------
启动内核 / 停止内核 (快捷开兿 (&S)
重启内核 (&R)
------------------------------------------------
TUN 模式: [已开吿/ 已关闭] (点击开吿关闭) (&T)   <-- 按需触发管理员提板系统代理: [已开吿/ 已关闭] (点击开吿关闭) (&P)   <-- 即时开关系统代琿------------------------------------------------
修改启动命令 (&C)...                                <-- 可视化修改命令行与端叿打开 Web 控制叿(&W)                                <-- 打开本地面板 (9090)
打开配置目录 (&O)                                  <-- 打开核心所在文件夹
开机自动启势(&A) [√]                               <-- 兿UAC 静默自启
语言 / Language                                    <-- 中文 / English 切换
------------------------------------------------
退出托盿(并关闭内核与系统代理) (&X)
```

---

### 🛠︿编译与构廿
项目采用 Windows 系统自带皿.NET Framework 编译器构建，**不需要安裿Visual Studio**＿
1. **一键编诿*：双击运行源码包中的 `build.bat`〿2. **命令行手工编诿*＿   ```cmd
   C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:winexe /win32icon:app.ico /out:MihomoTray.exe MihomoTray.cs
   ```

---

### 📂 目录结构

```text
├── MihomoTray.cs          # 托盘主程序核心源砿(C#)
├── tray_config.ini        # 配置文件 (命令行、端口、自启、语言)
├── build.bat              # Windows 原生一键编译脚朿├── app.ico                # 默认圆角卡片白猫图标 (多尺寿
├── app_transparent.ico    # 原版透明底猫咪图栿(备逿
├── app.png                # 高清原始素材
├── 设置开机自吿bat       # 开机自启辅助脚朿└── README.md              # 项目说明文档
```

---

## English

### ✿Features

* 🚀 **Ultra-lightweight & Zero Dependencies**: Written in pure C# WinForms, under 70 KB. Runs out of the box on Windows without installing Node.js, Electron, Python, or extra runtimes.
* 🌐 **Bilingual UI**: Instant switching between **English** and **Chinese**.
* 🛡︿**On-demand UAC Elevation**:
  * **Daily Usage**: Runs as a standard user with zero UAC prompts during startup and boot.
  * **On-demand**: Only prompts for administrator elevation when **TUN Mode** is manually turned on via tray menu.
* ⚿**One-Click System Proxy**:
  * Directly calls Windows `wininet.dll` API to broadcast proxy changes. Edge, Chrome, and system components update immediately.
  * **Auto-clean on Exit**: Automatically disables system proxy when exiting tray to prevent broken internet connections.
* ⚙️ **Configurable Launch Command**:
  * Built-in dialog to customize execution path, parameters (e.g. `-d path`), and proxy port with hot-restart.
* 🎨 **Polished Tray Icon**:
  * Modern squircle card design with multi-size DPI support (16‿56px), aligning with Windows 10/11 taskbar icons.

### 🛠︿Build from Source

Build directly using the built-in Windows C# compiler (**no Visual Studio required**):

```cmd
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:winexe /win32icon:app.ico /out:MihomoTray.exe MihomoTray.cs
```
Or simply double-click `build.bat`.

---

## 📄 License

MIT License. Free for personal and commercial use.


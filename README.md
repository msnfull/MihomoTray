# MihomoTray

A lightweight, native, and high-performance Windows system tray manager tailored for `mihomo` (Clash.Meta), `sing-box`, and other core network proxies.

[![Platform](https://img.shields.io/badge/Platform-Windows-blue.svg)](https://github.com/)
[![Framework](https://img.shields.io/badge/.NET%20Framework-4.0%2B-green.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-orange.svg)](LICENSE)
[![Language](https://img.shields.io/badge/Language-C%23-blueviolet.svg)](https://github.com/)

[English](#english) | [中文说明](#chinese)

---

<a name="english"></a>
## English

### Key Features

* **Ultra Lightweight & Zero External Dependencies**:
  Written in pure C# (Windows Forms). The compiled binary is under 50 KB. Runs out of the box on Windows 10/11 without requiring Python, Node.js, Electron, or external runtimes.
* **Bilingual Interface**:
  Built-in real-time language switching between English and Chinese directly from the tray context menu.
* **On-Demand UAC Elevation (Zero-Nuisance Boot)**:
  * **Daily Usage**: Runs as a standard non-admin user by default. Booting with Windows produces **zero UAC popups**.
  * **On-Demand Elevation**: Only prompts for administrator privileges when **TUN Mode** is manually enabled. Gracefully steps down when TUN is disabled.
* **Instant System Proxy Toggle**:
  * Broadcasts proxy state changes system-wide via native `wininet.dll` API calls. Browsers (Edge, Chrome) and system components take effect immediately without requiring restarts.
  * **Safe Exit Protection**: Automatically turns off system proxy when exiting the tray application, eliminating broken internet connections caused by dangling proxies.
* **Customizable Launch Command Line**:
  * Visual configuration dialog to modify executable paths, custom parameters (e.g. `-d path`), and proxy port with hot-restart.
  * Direct editing via `tray_config.ini` is also supported.
* **Modern Squircle Icon Design**:
  * High-resolution multi-size icon (16px to 256px) supporting 100%, 125%, 150%, and 200% Windows display scaling, harmonizing with standard Windows taskbar applications.

---

### Tray Context Menu

```text
Core: Running (PID: 12345) [User / Admin]
------------------------------------------------
Start Core / Stop Core (&S)
Restart Core (&R)
------------------------------------------------
TUN Mode: [Enabled / Disabled] (&T)        <-- Triggers UAC on demand
System Proxy: [Enabled / Disabled] (&P)    <-- Instant proxy toggle
------------------------------------------------
Edit Launch Command (&C)...                <-- Customize command & port
Open Web Dashboard (&W)                    <-- Open local web console (9090)
Open Config Folder (&O)                    <-- Open core directory
Start on Boot (&A) [x]                     <-- Silent non-UAC autostart
Language / 语言                            <-- English / Chinese switch
------------------------------------------------
Exit Tray (Stop Core) (&X)                 <-- Safe exit & auto cleanup
```

---

### Build from Source

This project compiles with the built-in Windows .NET Framework C# compiler. **No Visual Studio or SDK installation is required.**

#### Method 1: One-Click Build Script
Double-click `build.bat` in the project root.

#### Method 2: Command Line
```cmd
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:winexe /win32icon:app.ico /out:MihomoTray.exe MihomoTray.cs
```

---

### Project Layout

```text
├── MihomoTray.cs          # Main application source code (C#)
├── tray_config.ini        # Application configuration file
├── setup_autostart.bat    # Autostart registration script
├── build.bat              # Native Windows build script
├── app.ico                # Modern squircle card icon (multi-DPI)
├── app_transparent.ico    # Transparent cat icon (alternative)
├── app.png                # High-res raw image asset
├── README.md              # Project documentation
└── Readme.rd              # Documentation copy
```

---

<a name="chinese"></a>
## 中文说明

### 核心特性

* **极致轻量、零外部依赖**：
  基于纯 C# WinForms 开发，编译后体积仅几十 KB。无需安装 Node.js、Electron、Python 或特定 SDK，任何 Windows 系统开箱即用。
* **原生双语支持**：
  右键托盘菜单即可在中文与英文之间实时无缝切换。
* **按需 UAC 提权模式（开机零打扰）**：
  * **日常运行**：默认以普通用户权限启动，开机静默自启，**零 UAC 弹窗打扰**。
  * **按需提权**：仅在托盘菜单中手动开启 **TUN 虚拟网卡模式** 时，才会申请管理员提权。
* **系统代理即时切换**：
  * 底层调用 Windows `wininet.dll` API 广播通知，Edge / Chrome 等浏览器及全系统组件即刻生效，无需重启浏览器。
  * **安全防断网**：退出托盘时自动恢复关闭系统代理，避免残留代理导致网络无法访问。
* **可视化命令自定义**：
  * 内置可视化配置窗口，支持随时修改核心路径、启动参数（如 `-d D:\app\mihomo\2\`）与代理端口，保存即自动重启生效。
* **现代化圆角卡片图标**：
  * 内置精修的深蓝色圆角矩形（Squircle）白猫卡片图标，多层级 DPI（16~256px）高清抗锯齿，与 Windows 11/10 任务栏应用视觉完美契合。

---

### 开机自启配置

运行目录下的 `setup_autostart.bat`，输入 `1` 即可将程序注册到当前用户的开机启动项中，开机无黑框、免 UAC 静默启动。

---

## License

Released under the [MIT License](LICENSE).

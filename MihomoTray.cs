using System;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Threading;
using System.Security.Principal;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

// Internationalization string mapping
public static class I18n
{
    public static string CurrentLang = "zh";

    public static string T(string key, params object[] args)
    {
        bool en = CurrentLang.Equals("en", StringComparison.OrdinalIgnoreCase);
        string text;
        switch (key)
        {
            case "StatusRunning": text = en ? "Core: Running (PID: {0}) [{1}]" : "内核: 运行中 (PID: {0}) [{1}]"; break;
            case "StatusStopped": text = en ? "Core: Stopped [{0}]" : "内核: 已停止 [{0}]"; break;
            case "Admin": text = en ? "Admin" : "管理员"; break;
            case "User": text = en ? "User" : "普通权限"; break;
            case "StartCore": text = en ? "Start Core (&S)" : "启动内核 (开启) (&S)"; break;
            case "StopCore": text = en ? "Stop Core (&S)" : "停止内核 (关闭) (&S)"; break;
            case "RestartCore": text = en ? "Restart Core (&R)" : "重启内核 (&R)"; break;
            case "TunOn": text = en ? "TUN Mode: [Enabled] (Click to disable) (&T)" : "TUN 模式: [已开启] (点击关闭) (&T)"; break;
            case "TunOff": text = en ? "TUN Mode: [Disabled] (Click to enable) (&T)" : "TUN 模式: [已关闭] (点击开启) (&T)"; break;
            case "ProxyOn": text = en ? "System Proxy: [Enabled] (Click to disable) (&P)" : "系统代理: [已开启] (点击关闭) (&P)"; break;
            case "ProxyOff": text = en ? "System Proxy: [Disabled] (Click to enable) (&P)" : "系统代理: [已关闭] (点击开启) (&P)"; break;
            case "EditCmd": text = en ? "Edit Launch Command (&C)..." : "修改启动命令 (&C)..."; break;
            case "OpenWeb": text = en ? "Open Web Dashboard (&W)" : "打开 Web 控制台 (&W)"; break;
            case "OpenDir": text = en ? "Open Config Folder (&O)" : "打开配置目录 (&O)"; break;
            case "AutoStart": text = en ? "Start on Boot (&A)" : "开机自动启动 (&A)"; break;
            case "LangMenu": text = en ? "Language / 语言" : "语言 / Language"; break;
            case "Exit": text = en ? "Exit Tray (Stop Core) (&X)" : "退出托盘 (并关闭内核) (&X)"; break;
            case "DlgTitle": text = en ? "Edit Launch Command & Proxy Settings" : "修改启动命令与代理配置"; break;
            case "LblCmd": text = en ? "Launch Command Line (executable & arguments):" : "启动完整命令行 (可修改程序路径及启动参数):"; break;
            case "LblProxy": text = en ? "System Proxy Address & Port (e.g. 127.0.0.1:7890):" : "系统代理地址与端口 (例如 127.0.0.1:7890):"; break;
            case "ChkExitProxy": text = en ? "Disable system proxy automatically on exit" : "退出托盘时自动关闭系统代理"; break;
            case "BtnSave": text = en ? "Save & Restart" : "保存并重启服务"; break;
            case "BtnCancel": text = en ? "Cancel" : "取消"; break;
            case "CfgSaved": text = en ? "Configuration saved and core restarted successfully!" : "配置已成功保存并重启内核服务！"; break;
            case "Tip": text = en ? "Mihomo: {0} | Proxy: {1} | TUN: {2}" : "Mihomo: {0} | 代理: {1} | TUN: {2}"; break;
            case "On": text = en ? "ON" : "开"; break;
            case "Off": text = en ? "OFF" : "关"; break;
            case "Running": text = en ? "Running" : "运行中"; break;
            case "Stopped": text = en ? "Stopped" : "停止"; break;
            case "TunElevateMsg": text = en ? "Enabling TUN virtual network mode requires Administrator privileges.\n\nClick [OK] to request elevation and restart the tray." : "开启 TUN 虚拟网卡模式需要管理员权限。\n\n点击【确定】将请求管理员权限并平滑重启托盘服务以开启 TUN。"; break;
            case "TunElevateTitle": text = en ? "TUN Mode Elevation" : "TUN 模式提权提示"; break;
            case "AnotherInstance": text = en ? "MihomoTray is already running!" : "Mihomo 托盘程序已经在运行中！"; break;
            default: text = key; break;
        }

        if (args != null && args.Length > 0)
        {
            try { return string.Format(text, args); } catch { return text; }
        }
        return text;
    }
}

static class Program
{
    public static Mutex appMutex = null;

    // Release and close application mutex gracefully
    public static void ReleaseAndCloseMutex()
    {
        try
        {
            if (appMutex != null)
            {
                appMutex.Close();
                appMutex = null;
            }
        }
        catch { }
    }

    // Append log message to tray.log
    public static void Log(string msg)
    {
        try
        {
            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tray.log");
            File.AppendAllText(logPath, string.Format("[{0:yyyy-MM-dd HH:mm:ss}] {1}\r\n", DateTime.Now, msg));
        }
        catch { }
    }

    [STAThread]
    static void Main(string[] args)
    {
        AppDomain.CurrentDomain.UnhandledException += (s, e) => {
            Log("UnhandledException: " + e.ExceptionObject);
        };
        Application.ThreadException += (s, e) => {
            Log("ThreadException: " + e.Exception);
        };

        // Allow up to 4 seconds to wait for previous instance to exit during restarts
        bool hasHandle = false;
        try
        {
            appMutex = new Mutex(false, "Local\\MihomoTrayManagerApp_Lock");
            hasHandle = appMutex.WaitOne(4000, false);
        }
        catch (AbandonedMutexException)
        {
            hasHandle = true;
        }
        catch (Exception ex)
        {
            Log("Mutex Exception: " + ex.Message);
            hasHandle = true;
        }

        if (!hasHandle)
        {
            Log("Another instance is running, exiting.");
            MessageBox.Show(I18n.T("AnotherInstance"), "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        try
        {
            Application.Run(new TrayApplicationContext());
        }
        catch (Exception ex)
        {
            Log("Application.Run Exception: " + ex);
        }
        finally
        {
            ReleaseAndCloseMutex();
        }
    }
}

public class AppConfig
{
    public string CommandLine { get; set; }
    public string ProxyServer { get; set; }
    public bool AutoDisableProxyOnExit { get; set; }
    public string Language { get; set; }

    public AppConfig()
    {
        CommandLine = @"D:\app\mihomo\2\mihomo.exe -d D:\app\mihomo\2\";
        ProxyServer = "127.0.0.1:7890";
        AutoDisableProxyOnExit = true;
        Language = "zh";
    }

    public static AppConfig Load(string path)
    {
        AppConfig cfg = new AppConfig();
        if (!File.Exists(path))
        {
            cfg.Save(path);
            return cfg;
        }

        try
        {
            string[] lines = File.ReadAllLines(path, System.Text.Encoding.UTF8);
            foreach (string rawLine in lines)
            {
                string line = rawLine.Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith(";") || line.StartsWith("#") || line.StartsWith("["))
                    continue;

                int idx = line.IndexOf('=');
                if (idx > 0)
                {
                    string key = line.Substring(0, idx).Trim().ToLowerInvariant();
                    string val = line.Substring(idx + 1).Trim();

                    if (key == "commandline") cfg.CommandLine = val;
                    else if (key == "proxyserver") cfg.ProxyServer = val;
                    else if (key == "language") cfg.Language = val.ToLowerInvariant();
                    else if (key == "autodisableproxyonexit")
                    {
                        bool b;
                        if (bool.TryParse(val, out b)) cfg.AutoDisableProxyOnExit = b;
                    }
                }
            }
        }
        catch { }
        return cfg;
    }

    public void Save(string path)
    {
        try
        {
            using (StreamWriter sw = new StreamWriter(path, false, System.Text.Encoding.UTF8))
            {
                sw.WriteLine("[Settings]");
                sw.WriteLine("; Launch command line (executable path and arguments)");
                sw.WriteLine("CommandLine=" + CommandLine);
                sw.WriteLine("; System proxy address and port");
                sw.WriteLine("ProxyServer=" + ProxyServer);
                sw.WriteLine("; Disable system proxy automatically when tray application exits (true/false)");
                sw.WriteLine("AutoDisableProxyOnExit=" + (AutoDisableProxyOnExit ? "true" : "false"));
                sw.WriteLine("; Interface language: zh or en");
                sw.WriteLine("Language=" + (string.IsNullOrEmpty(Language) ? "zh" : Language));
            }
        }
        catch { }
    }
}

public class TrayApplicationContext : ApplicationContext
{
    [DllImport("wininet.dll", SetLastError = true)]
    private static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);

    private const int INTERNET_OPTION_SETTINGS_CHANGED = 39;
    private const int INTERNET_OPTION_REFRESH = 37;

    private const string RunRegKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppRegName = "MihomoTray";

    private NotifyIcon notifyIcon;
    private ContextMenuStrip contextMenu;
    private ToolStripMenuItem statusItem;
    private ToolStripMenuItem toggleCoreItem;
    private ToolStripMenuItem restartItem;
    private ToolStripMenuItem tunToggleItem;
    private ToolStripMenuItem proxyToggleItem;
    private ToolStripMenuItem editCmdItem;
    private ToolStripMenuItem webUiItem;
    private ToolStripMenuItem dirItem;
    private ToolStripMenuItem startupItem;
    private ToolStripMenuItem langMenuItem;
    private ToolStripMenuItem langZhItem;
    private ToolStripMenuItem langEnItem;
    private ToolStripMenuItem exitItem;
    private System.Windows.Forms.Timer statusTimer;

    private string baseDir;
    private string configFilePath;
    private AppConfig config;
    private int lastManagedPid = 0;

    public TrayApplicationContext()
    {
        baseDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
        configFilePath = Path.Combine(baseDir, "tray_config.ini");
        config = AppConfig.Load(configFilePath);
        I18n.CurrentLang = config.Language;

        contextMenu = new ContextMenuStrip();

        statusItem = new ToolStripMenuItem { Enabled = false };
        contextMenu.Items.Add(statusItem);
        contextMenu.Items.Add(new ToolStripSeparator());

        // Core toggle and restart
        toggleCoreItem = new ToolStripMenuItem();
        toggleCoreItem.Click += (s, e) => ToggleCore();
        contextMenu.Items.Add(toggleCoreItem);

        restartItem = new ToolStripMenuItem();
        restartItem.Click += (s, e) => RestartCore();
        contextMenu.Items.Add(restartItem);

        contextMenu.Items.Add(new ToolStripSeparator());

        // TUN mode toggle (with UAC elevation on demand)
        tunToggleItem = new ToolStripMenuItem();
        tunToggleItem.Click += (s, e) => ToggleTunMode();
        contextMenu.Items.Add(tunToggleItem);

        // System proxy toggle
        proxyToggleItem = new ToolStripMenuItem();
        proxyToggleItem.Click += (s, e) => ToggleSystemProxy();
        contextMenu.Items.Add(proxyToggleItem);

        contextMenu.Items.Add(new ToolStripSeparator());

        // Settings and actions
        editCmdItem = new ToolStripMenuItem();
        editCmdItem.Click += (s, e) => ShowEditCommandDialog();
        contextMenu.Items.Add(editCmdItem);

        webUiItem = new ToolStripMenuItem();
        webUiItem.Click += (s, e) => OpenWebUi();
        contextMenu.Items.Add(webUiItem);

        dirItem = new ToolStripMenuItem();
        dirItem.Click += (s, e) => Process.Start("explorer.exe", baseDir);
        contextMenu.Items.Add(dirItem);

        startupItem = new ToolStripMenuItem { CheckOnClick = true };
        startupItem.Checked = IsStartupEnabled();
        startupItem.Click += (s, e) => SetStartup(startupItem.Checked);
        contextMenu.Items.Add(startupItem);

        // Language submenu
        langMenuItem = new ToolStripMenuItem();
        langZhItem = new ToolStripMenuItem("中文 (Chinese)");
        langZhItem.Click += (s, e) => SwitchLanguage("zh");
        langEnItem = new ToolStripMenuItem("English");
        langEnItem.Click += (s, e) => SwitchLanguage("en");
        langMenuItem.DropDownItems.Add(langZhItem);
        langMenuItem.DropDownItems.Add(langEnItem);
        contextMenu.Items.Add(langMenuItem);

        contextMenu.Items.Add(new ToolStripSeparator());

        exitItem = new ToolStripMenuItem();
        exitItem.Click += (s, e) => Exit();
        contextMenu.Items.Add(exitItem);

        // Load tray icon
        Icon appIcon = null;
        try
        {
            string icoPath = Path.Combine(baseDir, "app.ico");
            if (File.Exists(icoPath))
            {
                appIcon = new Icon(icoPath, 32, 32);
            }
            else
            {
                appIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }
        }
        catch
        {
            appIcon = SystemIcons.Application;
        }

        notifyIcon = new NotifyIcon
        {
            Icon = appIcon,
            ContextMenuStrip = contextMenu,
            Visible = true,
            Text = "Mihomo"
        };
        notifyIcon.DoubleClick += (s, e) => ToggleCore();

        statusTimer = new System.Windows.Forms.Timer { Interval = 2500 };
        statusTimer.Tick += (s, e) => UpdateStatus();
        statusTimer.Start();

        StartCore();
        UpdateStatus();
        Program.Log("TrayApplicationContext initialized. Lang=" + I18n.CurrentLang);
    }

    private void SwitchLanguage(string lang)
    {
        I18n.CurrentLang = lang;
        config.Language = lang;
        config.Save(configFilePath);
        UpdateStatus();
    }

    private static bool IsRunAsAdmin()
    {
        try
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }
        catch
        {
            return false;
        }
    }

    private static void ParseCommandLine(string cmdLine, out string fileName, out string args, out string workingDir)
    {
        cmdLine = (cmdLine ?? "").Trim();
        fileName = "";
        args = "";
        workingDir = "";

        if (string.IsNullOrEmpty(cmdLine)) return;

        if (cmdLine.StartsWith("\""))
        {
            int secondQuote = cmdLine.IndexOf('\"', 1);
            if (secondQuote > 0)
            {
                fileName = cmdLine.Substring(1, secondQuote - 1).Trim();
                args = cmdLine.Substring(secondQuote + 1).Trim();
            }
            else
            {
                fileName = cmdLine.Trim('\"');
            }
        }
        else
        {
            int spaceIndex = cmdLine.IndexOf(' ');
            if (spaceIndex > 0)
            {
                fileName = cmdLine.Substring(0, spaceIndex).Trim();
                args = cmdLine.Substring(spaceIndex + 1).Trim();
            }
            else
            {
                fileName = cmdLine;
            }
        }

        try
        {
            if (File.Exists(fileName))
            {
                workingDir = Path.GetDirectoryName(Path.GetFullPath(fileName));
            }
        }
        catch { }

        if (string.IsNullOrEmpty(workingDir))
        {
            workingDir = AppDomain.CurrentDomain.BaseDirectory;
        }
    }

    private Process FindRunningCoreProcess()
    {
        string fileName, args, workDir;
        ParseCommandLine(config.CommandLine, out fileName, out args, out workDir);

        string procName = "";
        try { procName = Path.GetFileNameWithoutExtension(fileName); } catch { }

        if (lastManagedPid > 0)
        {
            try
            {
                Process p = Process.GetProcessById(lastManagedPid);
                if (!p.HasExited) return p;
            }
            catch { }
            lastManagedPid = 0;
        }

        if (!string.IsNullOrEmpty(procName))
        {
            try
            {
                Process[] procs = Process.GetProcessesByName(procName);
                if (procs.Length > 0) return procs[0];
            }
            catch { }
        }
        return null;
    }

    private void StartCore()
    {
        try
        {
            if (FindRunningCoreProcess() == null)
            {
                string fileName, args, workDir;
                ParseCommandLine(config.CommandLine, out fileName, out args, out workDir);

                if (string.IsNullOrEmpty(fileName))
                {
                    MessageBox.Show("Command not configured!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = args,
                    WorkingDirectory = workDir,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = false
                };

                Process p = Process.Start(psi);
                if (p != null)
                {
                    lastManagedPid = p.Id;
                }
                Thread.Sleep(300);
            }
        }
        catch (Exception ex)
        {
            Program.Log("StartCore error: " + ex.Message);
            MessageBox.Show("Failed to start core: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        UpdateStatus();
    }

    private void StopCore()
    {
        try
        {
            string fileName, args, workDir;
            ParseCommandLine(config.CommandLine, out fileName, out args, out workDir);
            string procName = "";
            try { procName = Path.GetFileNameWithoutExtension(fileName); } catch { }

            if (lastManagedPid > 0)
            {
                try
                {
                    Process p = Process.GetProcessById(lastManagedPid);
                    p.Kill();
                }
                catch { }
                lastManagedPid = 0;
            }

            if (!string.IsNullOrEmpty(procName))
            {
                Process[] procs = Process.GetProcessesByName(procName);
                foreach (var p in procs)
                {
                    try { p.Kill(); } catch { }
                }
            }
            Thread.Sleep(300);
        }
        catch { }
        UpdateStatus();
    }

    private void RestartCore()
    {
        StopCore();
        StartCore();
    }

    private void ToggleCore()
    {
        if (FindRunningCoreProcess() != null)
        {
            StopCore();
        }
        else
        {
            StartCore();
        }
    }

    // ========== TUN Mode Handlers ==========
    private bool IsTunConfigEnabled()
    {
        try
        {
            string yamlPath = Path.Combine(baseDir, "config.yaml");
            if (File.Exists(yamlPath))
            {
                string content = File.ReadAllText(yamlPath);
                Match m = Regex.Match(content, @"(?m)^\s*tun\s*:\s*[\r\n]+(?:\s*#[^\r\n]*[\r\n]+)*\s*enable\s*:\s*(true|false)", RegexOptions.IgnoreCase);
                if (m.Success)
                {
                    return m.Groups[1].Value.Equals("true", StringComparison.OrdinalIgnoreCase);
                }
            }
        }
        catch { }
        return false;
    }

    private void SetTunConfigEnabled(bool enable)
    {
        try
        {
            string yamlPath = Path.Combine(baseDir, "config.yaml");
            if (File.Exists(yamlPath))
            {
                string content = File.ReadAllText(yamlPath, System.Text.Encoding.UTF8);
                string pattern = @"(?m)(^\s*tun\s*:\s*[\r\n]+(?:\s*#[^\r\n]*[\r\n]+)*\s*enable\s*:\s*)(true|false)";
                string replacement = "${1}" + (enable ? "true" : "false");
                string newContent = Regex.Replace(content, pattern, replacement, RegexOptions.IgnoreCase);
                File.WriteAllText(yamlPath, newContent, System.Text.Encoding.UTF8);
            }
        }
        catch (Exception ex)
        {
            Program.Log("SetTunConfigEnabled error: " + ex.Message);
        }
    }

    private void ToggleTunMode()
    {
        bool isTunOn = IsTunConfigEnabled();
        bool wantEnable = !isTunOn;

        if (wantEnable)
        {
            if (!IsRunAsAdmin())
            {
                DialogResult dr = MessageBox.Show(
                    I18n.T("TunElevateMsg"),
                    I18n.T("TunElevateTitle"),
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Question);

                if (dr != DialogResult.OK) return;

                SetTunConfigEnabled(true);
                StopCore();

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = Application.ExecutablePath,
                    WorkingDirectory = baseDir,
                    UseShellExecute = true,
                    Verb = "runas"
                };

                try
                {
                    Program.ReleaseAndCloseMutex();
                    Process.Start(psi);
                    statusTimer.Stop();
                    notifyIcon.Visible = false;
                    notifyIcon.Dispose();
                    Environment.Exit(0);
                    return;
                }
                catch (Exception ex)
                {
                    Program.Log("Elevation failed: " + ex.Message);
                    SetTunConfigEnabled(false);
                    try { Program.appMutex = new Mutex(false, "Local\\MihomoTrayManagerApp_Lock"); } catch { }
                    StartCore();
                    UpdateStatus();
                    return;
                }
            }
            else
            {
                SetTunConfigEnabled(true);
                RestartCore();
                UpdateStatus();
            }
        }
        else
        {
            SetTunConfigEnabled(false);
            RestartCore();
            UpdateStatus();
        }
    }

    // ========== System Proxy Handlers ==========
    private static void RefreshProxySettings()
    {
        InternetSetOption(IntPtr.Zero, INTERNET_OPTION_SETTINGS_CHANGED, IntPtr.Zero, 0);
        InternetSetOption(IntPtr.Zero, INTERNET_OPTION_REFRESH, IntPtr.Zero, 0);
    }

    private bool IsSystemProxyEnabled()
    {
        try
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings", false))
            {
                if (key != null)
                {
                    object val = key.GetValue("ProxyEnable");
                    return val != null && Convert.ToInt32(val) == 1;
                }
            }
        }
        catch { }
        return false;
    }

    private void SetSystemProxy(bool enable)
    {
        try
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings", true))
            {
                if (key != null)
                {
                    key.SetValue("ProxyEnable", enable ? 1 : 0, RegistryValueKind.DWord);
                    if (enable && !string.IsNullOrEmpty(config.ProxyServer))
                    {
                        key.SetValue("ProxyServer", config.ProxyServer, RegistryValueKind.String);
                    }
                }
            }
            RefreshProxySettings();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Proxy Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        UpdateStatus();
    }

    private void ToggleSystemProxy()
    {
        bool currentState = IsSystemProxyEnabled();
        SetSystemProxy(!currentState);
    }

    private void ShowEditCommandDialog()
    {
        using (Form form = new Form())
        {
            form.Text = I18n.T("DlgTitle");
            form.Width = 560;
            form.Height = 260;
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MaximizeBox = false;
            form.MinimizeBox = false;
            form.Icon = notifyIcon.Icon;

            Label lblCmd = new Label { Left = 20, Top = 18, Text = I18n.T("LblCmd"), AutoSize = true };
            TextBox txtCmd = new TextBox { Left = 20, Top = 42, Width = 500, Text = config.CommandLine };

            Label lblProxy = new Label { Left = 20, Top = 78, Text = I18n.T("LblProxy"), AutoSize = true };
            TextBox txtProxy = new TextBox { Left = 20, Top = 102, Width = 240, Text = config.ProxyServer };

            CheckBox chkExitProxy = new CheckBox
            {
                Left = 20,
                Top = 138,
                Text = I18n.T("ChkExitProxy"),
                AutoSize = true,
                Checked = config.AutoDisableProxyOnExit
            };

            Button btnOk = new Button { Text = I18n.T("BtnSave"), Left = 280, Top = 175, Width = 120, Height = 28, DialogResult = DialogResult.OK };
            Button btnCancel = new Button { Text = I18n.T("BtnCancel"), Left = 410, Top = 175, Width = 110, Height = 28, DialogResult = DialogResult.Cancel };

            form.Controls.AddRange(new Control[] { lblCmd, txtCmd, lblProxy, txtProxy, chkExitProxy, btnOk, btnCancel });
            form.AcceptButton = btnOk;
            form.CancelButton = btnCancel;

            if (form.ShowDialog() == DialogResult.OK)
            {
                string newCmd = txtCmd.Text.Trim();
                string newProxy = txtProxy.Text.Trim();
                if (!string.IsNullOrEmpty(newCmd))
                {
                    config.CommandLine = newCmd;
                    config.ProxyServer = string.IsNullOrEmpty(newProxy) ? "127.0.0.1:7890" : newProxy;
                    config.AutoDisableProxyOnExit = chkExitProxy.Checked;
                    config.Save(configFilePath);

                    if (IsSystemProxyEnabled())
                    {
                        SetSystemProxy(true);
                    }

                    RestartCore();
                    MessageBox.Show(I18n.T("CfgSaved"), "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
    }

    private void OpenWebUi()
    {
        try
        {
            Process.Start("http://127.0.0.1:9090/ui");
        }
        catch (Exception ex)
        {
            MessageBox.Show("Open Web UI error: " + ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private bool IsStartupEnabled()
    {
        try
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunRegKey, false))
            {
                if (key != null)
                {
                    return key.GetValue(AppRegName) != null;
                }
            }
        }
        catch { }
        return false;
    }

    private void SetStartup(bool enable)
    {
        try
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunRegKey, true))
            {
                if (key != null)
                {
                    if (enable)
                    {
                        key.SetValue(AppRegName, "\"" + Application.ExecutablePath + "\"");
                    }
                    else
                    {
                        key.DeleteValue(AppRegName, false);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Startup error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void UpdateStatus()
    {
        Process proc = FindRunningCoreProcess();
        bool isCoreRunning = (proc != null);
        bool isProxyOn = IsSystemProxyEnabled();
        bool isTunOn = IsTunConfigEnabled();
        bool isAdmin = IsRunAsAdmin();

        string permText = isAdmin ? I18n.T("Admin") : I18n.T("User");
        if (isCoreRunning)
        {
            statusItem.Text = I18n.T("StatusRunning", proc.Id, permText);
            toggleCoreItem.Text = I18n.T("StopCore");
        }
        else
        {
            statusItem.Text = I18n.T("StatusStopped", permText);
            toggleCoreItem.Text = I18n.T("StartCore");
        }

        restartItem.Text = I18n.T("RestartCore");
        tunToggleItem.Checked = isTunOn;
        tunToggleItem.Text = isTunOn ? I18n.T("TunOn") : I18n.T("TunOff");

        proxyToggleItem.Checked = isProxyOn;
        proxyToggleItem.Text = isProxyOn ? I18n.T("ProxyOn") : I18n.T("ProxyOff");

        editCmdItem.Text = I18n.T("EditCmd");
        webUiItem.Text = I18n.T("OpenWeb");
        dirItem.Text = I18n.T("OpenDir");
        startupItem.Text = I18n.T("AutoStart");
        langMenuItem.Text = I18n.T("LangMenu");
        langZhItem.Checked = I18n.CurrentLang.Equals("zh", StringComparison.OrdinalIgnoreCase);
        langEnItem.Checked = I18n.CurrentLang.Equals("en", StringComparison.OrdinalIgnoreCase);
        exitItem.Text = I18n.T("Exit");

        string coreState = isCoreRunning ? I18n.T("Running") : I18n.T("Stopped");
        string proxyState = isProxyOn ? I18n.T("On") : I18n.T("Off");
        string tunState = isTunOn ? I18n.T("On") : I18n.T("Off");
        string tip = I18n.T("Tip", coreState, proxyState, tunState);
        if (tip.Length > 63) tip = tip.Substring(0, 63);
        notifyIcon.Text = tip;
    }

    private void Exit()
    {
        statusTimer.Stop();

        if (config.AutoDisableProxyOnExit && IsSystemProxyEnabled())
        {
            SetSystemProxy(false);
        }

        StopCore();
        notifyIcon.Visible = false;
        notifyIcon.Dispose();
        Application.Exit();
    }
}

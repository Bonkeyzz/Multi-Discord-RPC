using DiscordRPC;
using DiscordRPC.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Multi_DiscordRPC
{
    public static class AppInfo
    {
        public static string appName = "Multi Discord RPC";
        public static string appVersion = "2.0";
        public static string appAuthor = "Bonk";
    }
    class Program
    {

        #region Variables
        static List<RPCApplication> m_RPCApplicationList = new List<RPCApplication>();
        static RPCApplication m_RPCApplication;
        static DiscordRpcClient m_RPCClient;
        static bool FirstInit = true;
        static LogLevel m_LogLevel = LogLevel.Warning;
        static Thread ProcessDetectThread;
        static bool HideConsole;
        private static NotifyIcon m_trayIcon;
        #endregion

        public static AppConfig m_Config;

        static void setCurrentRPCApp(RPCApplication app, bool reInit = true)
        {
            Utils.PrettyPrint($"[I] Setting RPresence to: '{app.sAppName}'...", ConsoleColor.DarkYellow, HideConsole:HideConsole);
            if (m_RPCClient != null && m_RPCClient.IsInitialized)
            {
                if (reInit)
                {
                    Utils.PrettyPrint("[I] Closing current RPC instance...", ConsoleColor.Yellow, HideConsole: HideConsole);
                    m_RPCClient.ClearPresence();
                    m_RPCClient.Deinitialize();

                    Utils.PrettyPrint("[I] Creating a new RPC instance...", ConsoleColor.Yellow, HideConsole: HideConsole);
                    m_RPCClient = new DiscordRpcClient(app.sAppId);
                    m_RPCClient.Logger = new ConsoleLoggerFormatted() { Level = m_LogLevel };
                    m_RPCClient.Initialize();

                }
                RichPresence rp = new RichPresence();
                Assets assets = new Assets();
                assets.LargeImageKey = app.sLargeImgKey;
                assets.SmallImageKey = app.sSmallImgKey;
                assets.LargeImageText = app.sLargeImgText;
                assets.SmallImageText = app.sSmallImgText;
                rp.Assets = assets;

                rp.Details = app.sDetails;
                rp.State = app.sState;
                Utils.PrettyPrint("[I] Updating Rich Presence...", ConsoleColor.Yellow, HideConsole: HideConsole);
                m_RPCClient.SetPresence(rp);
                m_RPCApplication = app;
            }
        }
        static bool isProcessRunning( /* No Extension */ string procName)
        {
            bool running = false;
            foreach (var proc in Process.GetProcesses())
            {
                if (proc.ProcessName == procName)
                {
                    running = true;
                    break;
                }
            }
            return running;
        }
        static void rpcProcessHandler()
        {
            for (; ; )
            {
                foreach (var app in m_RPCApplicationList)
                {
                    if (isProcessRunning(app.sProcessName))
                    {
                        if (m_RPCApplication != app)
                        {
                            Utils.PrettyPrint($"[I] Found new Application '{app.sAppName}' (Process: '{app.sProcessName}.exe')", ConsoleColor.DarkYellow, HideConsole: HideConsole);
                        }
                        if (m_RPCClient == null && m_RPCApplication == null && FirstInit)
                        {
                            m_RPCClient = new DiscordRpcClient(app.sAppId);
                            m_RPCClient.Initialize();
                            m_RPCClient.Logger = new ConsoleLoggerFormatted() { Level = m_LogLevel };
                            setCurrentRPCApp(app, false);
                            FirstInit = false;
                        }
                        else if (m_RPCApplication != app && m_RPCApplication == null)
                        {
                            setCurrentRPCApp(app);
                        }
                    }
                    if (m_RPCApplication == app && !isProcessRunning(app.sProcessName))
                    {
                        m_RPCClient.ClearPresence();
                        m_RPCApplication = null;
                        Utils.PrettyPrint($"[I] Application '{app.sAppName}' (Process: '{app.sProcessName}.exe') appears to be closed. Clearing Presence...",
                            ConsoleColor.DarkYellow, HideConsole: HideConsole);
                    }
                }
                Thread.Sleep(m_Config.rpcThreadUpdateInt);
            }
        }

        static class WinApiImports
        {
            [DllImport("user32")]
            public static extern short GetAsyncKeyState(int vKey);

            [DllImport("kernel32")]
            public static extern IntPtr GetConsoleWindow();

            [DllImport("user32")]
            public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        }

        enum WindowState
        {
            SW_SHOW = 5,
            SW_HIDE = 0
        }
        static void SetConsoleState(WindowState state)
        {
            var consoleHandle = WinApiImports.GetConsoleWindow();
            WinApiImports.ShowWindow(consoleHandle, (int)state);
            HideConsole = (state == WindowState.SW_HIDE);
        }

        static void keyStateHandler()
        {
            while (true)
            {
                // Detect CTRL + SHIFT + H
                if (((WinApiImports.GetAsyncKeyState(0x48) & 0x8000) > 0) && ((WinApiImports.GetAsyncKeyState(0x10) & 0x8000) > 0) && ((WinApiImports.GetAsyncKeyState(0x11) & 0x8000) > 0))
                {
                    if (!HideConsole)
                    {
                        SetConsoleState(WindowState.SW_HIDE);
                    }
                    else
                    {
                        SetConsoleState(WindowState.SW_SHOW);
                    }
                }
                Thread.Sleep(m_Config.kbThreadUpdateInt);
            }
        }
        class ConsoleLoggerFormatted : ILogger
        {
            public LogLevel Level { get; set; }

            public void Error(string message, params object[] args)
            {
                if (HideConsole) return;
                if (Level == LogLevel.Error || Level == LogLevel.Trace || Level == LogLevel.Warning)
                {
                    Utils.PrettyPrint(string.Format($"[X] DiscordRPC: '{message}'", args), ConsoleColor.Red, HideConsole: HideConsole);
                }
            }

            public void Info(string message, params object[] args)
            {
                if (HideConsole) return;
                if (Level == LogLevel.Info)
                {
                    Utils.PrettyPrint(string.Format($"[I] DiscordRPC: '{message}'", args), ConsoleColor.White, HideConsole: HideConsole);
                }
            }

            public void Trace(string message, params object[] args)
            {
                if (HideConsole) return;
                if (Level == LogLevel.Trace)
                {
                    Utils.PrettyPrint(string.Format($"[D] DiscordRPC: '{message}'", args), ConsoleColor.Cyan, HideConsole: HideConsole);
                }
            }

            public void Warning(string message, params object[] args)
            {
                if (HideConsole) return;
                if (Level == LogLevel.Warning || Level == LogLevel.Error)
                {
                    Utils.PrettyPrint(string.Format($"[W] DiscordRPC: '{message}'", args), ConsoleColor.Yellow, HideConsole: HideConsole);
                }
            }
        }

        static Task InitializeRpcClient()
        {
            Console.Title = $"{AppInfo.appName} Version: {AppInfo.appVersion} : By {AppInfo.appAuthor}";
            Utils.PrettyPrint($"[I] Initializing thread...", ConsoleColor.White, HideConsole: HideConsole);
            ProcessDetectThread = new Thread(rpcProcessHandler);
            ProcessDetectThread.Start();
            Utils.PrettyPrint($"[I] RPC thread is running! Looking for running applications...", ConsoleColor.Green, HideConsole: HideConsole);
            return Task.CompletedTask;
        }

        static Task SetRPCApps()
        {
            if (Directory.Exists(Environment.CurrentDirectory + "\\apps"))
            {
                if (Directory.GetFiles(Environment.CurrentDirectory + "\\apps").Length > 0)
                {
                    foreach (var app in Directory.GetFiles(Environment.CurrentDirectory + "\\apps"))
                    {
                        if (app.EndsWith(".json"))
                        {
                            RPCApplication rpcApp = JsonConvert.DeserializeObject<RPCApplication>(File.ReadAllText(app));
                            m_RPCApplicationList.Add(rpcApp);
                            Utils.PrettyPrint($"[I] File {Path.GetFileName(app)} found!", ConsoleColor.Green, HideConsole: HideConsole);
                        }
                    }
                }
                else
                {
                    Utils.PrettyPrint("[E] There are no applications found in the 'apps' directory!", ConsoleColor.Red, HideConsole: HideConsole);
                    Utils.PrettyPrint("Press [ENTER] To exit.", ConsoleColor.Gray, HideConsole: HideConsole);
                    Console.ReadLine();
                    Environment.Exit(1);
                }
            }
            else
            {
                Utils.PrettyPrint("[E] Directory 'apps' does not appear to exist!", ConsoleColor.Red, HideConsole: HideConsole);
                Utils.PrettyPrint("Press [ENTER] To exit.", ConsoleColor.Gray, HideConsole: HideConsole);
                Console.ReadLine();
                Environment.Exit(1);

            }

            return Task.CompletedTask;
        }

        [STAThread]
        static void Main(string[] args) => new Program().MainAsync(args).GetAwaiter().GetResult();

        async Task MainAsync(string[] args)
        {
            m_trayIcon = new NotifyIcon();
            m_trayIcon.Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
            m_trayIcon.Text = "Multi Discord RPC";
            m_trayIcon.DoubleClick += OnTrayIconMouseClick;
            m_trayIcon.Visible = true;

            ArgsParser argParse = new ArgsParser();
            argParse.AddArgument("minimized", "m", "Starts the program minimized");
            argParse.ParseArgs(args);

            try
            {
                m_Config = JsonConvert.DeserializeObject<AppConfig>(File.ReadAllText(Environment.CurrentDirectory + "\\config.json"));
            }
            catch (Exception)
            {
                Utils.PrettyPrint("[E] Failed to read config! Using defaults.", ConsoleColor.Red, HideConsole:HideConsole);
                m_Config = new AppConfig(2000, 150, false);
            }
            bool startMinimized = argParse.Exists("minimized");
            if (m_Config.isHidden || startMinimized)
            {
                SetConsoleState(WindowState.SW_HIDE);
                m_trayIcon.ShowBalloonTip(4500, "Multi Discord RPC",
                    "Program is now in the tray, double click to show it or hide it.", ToolTipIcon.Info);
            }
            Utils.PrettyPrint($"===APP CONFIG===\nrpcClientUpdateInterval: {m_Config.rpcThreadUpdateInt}\nkbDetectInterval: {m_Config.kbThreadUpdateInt}\nstartHidden: {m_Config.isHidden}\n================", ConsoleColor.Cyan, HideConsole: HideConsole);
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
            await SetRPCApps();
            await InitializeRpcClient();
            Thread m_KeyboardKeyDetectionThread = new Thread(keyStateHandler);
            m_KeyboardKeyDetectionThread.Start();
            Utils.PrettyPrint("[I] You can press [CTRL + SHIFT + H] to hide or show this window.", ConsoleColor.Yellow, HideConsole: HideConsole);
        }

        private void OnTrayIconMouseClick(object sender, EventArgs e)
        {
            if(HideConsole)
                SetConsoleState(WindowState.SW_SHOW);
            else
                SetConsoleState(WindowState.SW_HIDE);
        }

        void OnProcessExit(object sender, EventArgs e)
        {
            if (m_RPCClient != null)
            {
                if (m_RPCClient.IsInitialized)
                {
                    m_RPCClient.ClearPresence();
                    m_RPCClient.Deinitialize();
                    m_RPCClient.Dispose();
                }
            }
        }
    }
}

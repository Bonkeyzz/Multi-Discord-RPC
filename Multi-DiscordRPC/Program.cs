using DiscordRPC;
using DiscordRPC.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Multi_DiscordRPC
{
    public static class AppInfo
    {
        public static string appName = "Multi Discord RPC";
        public static string appVersion = "1.1";
        public static string appAuthor = "Bonk";
    }
    class Program
    {
        static void pPrint(string text, ConsoleColor fg, ConsoleColor bg = ConsoleColor.Black)
        {
            if (bConsoleHidden) return;
            Console.BackgroundColor = bg;
            Console.ForegroundColor = fg;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        static List<dRPCApplication> dRPCAppList = new List<dRPCApplication>();
        static dRPCApplication dRPCActiveApp;
        static DiscordRpcClient dRpcClient;
        static bool bFirstInit = true;
        static LogLevel lLogLevel = LogLevel.Warning;
        static Thread thr_ProcessDetection;
        static bool bConsoleHidden = false;

        public static cConfig cfg;

        static void setCurrentRPCApp(dRPCApplication app, bool reInit = true)
        {
            pPrint($"[I] Setting RPresence to: '{app.sAppName}'...", ConsoleColor.DarkYellow);
            if (dRpcClient != null && dRpcClient.IsInitialized)
            {
                if (reInit)
                {
                    pPrint("[I] Closing current RPC instance...", ConsoleColor.Yellow);
                    dRpcClient.ClearPresence();
                    dRpcClient.Deinitialize();

                    pPrint("[I] Creating a new RPC instance...", ConsoleColor.Yellow);
                    dRpcClient = new DiscordRpcClient(app.sAppId);
                    dRpcClient.Logger = new ConsoleLoggerFormatted() { Level = lLogLevel };
                    dRpcClient.Initialize();

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
                pPrint("[I] Updating Rich Presence...", ConsoleColor.Yellow);
                dRpcClient.SetPresence(rp);
                dRPCActiveApp = app;
            }
        }
        static bool isProcessRunning( /* No Extension */ string procName)
        {
            bool isRunning = false;
            foreach (var proc in Process.GetProcesses())
            {
                if (proc.ProcessName == procName)
                {
                    isRunning = true;
                    break;
                }
            }
            return isRunning;
        }

        static void rpcProcessHandler()
        {
            for (; ; )
            {
                foreach (var app in dRPCAppList)
                {
                    if (isProcessRunning(app.sProcessName))
                    {
                        if (dRPCActiveApp != app)
                        {
                            pPrint($"[I] Found new Application '{app.sAppName}' (Process: '{app.sProcessName}.exe')", ConsoleColor.DarkYellow);
                        }
                        if (dRpcClient == null && dRPCActiveApp == null && bFirstInit)
                        {
                            dRpcClient = new DiscordRpcClient(app.sAppId);
                            dRpcClient.Initialize();
                            dRpcClient.Logger = new ConsoleLoggerFormatted() { Level = lLogLevel };
                            setCurrentRPCApp(app, false);
                            bFirstInit = false;
                        }
                        else if (dRPCActiveApp != app && dRPCActiveApp == null)
                        {
                            setCurrentRPCApp(app);
                        }
                    }
                    if (dRPCActiveApp == app && !isProcessRunning(app.sProcessName))
                    {
                        dRpcClient.ClearPresence();
                        dRPCActiveApp = null;
                        pPrint($"[I] Application '{app.sAppName}' (Process: '{app.sProcessName}.exe') appears to be closed. Clearing Presence...", ConsoleColor.DarkYellow);
                    }
                }
                Thread.Sleep(cfg.rpcThreadUpdateInt);
            }
        }

        static class NativeApi
        {
            [DllImport("user32")]
            public static extern short GetAsyncKeyState(int vKey);

            [DllImport("kernel32")]
            public static extern IntPtr GetConsoleWindow();

            [DllImport("user32")]
            public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        }

        enum nApiWindowState
        {
            SW_SHOW = 5,
            SW_HIDE = 0
        }
        static void setConsoleState(nApiWindowState state)
        {
            var consoleHandle = NativeApi.GetConsoleWindow();
            NativeApi.ShowWindow(consoleHandle, (int)state);
            bConsoleHidden = (state == nApiWindowState.SW_HIDE) ? true : false;
        }

        static void keyStateHandler()
        {
            while (true)
            {
                // Detect CTRL + SHIFT + H
                if (((NativeApi.GetAsyncKeyState(0x48) & 0x8000) > 0) && ((NativeApi.GetAsyncKeyState(0x10) & 0x8000) > 0) && ((NativeApi.GetAsyncKeyState(0x11) & 0x8000) > 0))
                {
                    if (!bConsoleHidden)
                    {
                        setConsoleState(nApiWindowState.SW_HIDE);
                    }
                    else
                    {
                        setConsoleState(nApiWindowState.SW_SHOW);
                    }
                }
                Thread.Sleep(cfg.kbThreadUpdateInt);
            }
        }
        class ConsoleLoggerFormatted : ILogger
        {
            public LogLevel Level { get; set; }

            public void Error(string message, params object[] args)
            {
                if (bConsoleHidden) return;
                if (Level == LogLevel.Error || Level == LogLevel.Trace || Level == LogLevel.Warning)
                {
                    pPrint(string.Format($"[X] DiscordRPC: '{message}'", args), ConsoleColor.Red);
                }
            }

            public void Info(string message, params object[] args)
            {
                if (bConsoleHidden) return;
                if (Level == LogLevel.Info)
                {
                    pPrint(string.Format($"[I] DiscordRPC: '{message}'", args), ConsoleColor.White);
                }
            }

            public void Trace(string message, params object[] args)
            {
                if (bConsoleHidden) return;
                if (Level == LogLevel.Trace)
                {
                    pPrint(string.Format($"[D] DiscordRPC: '{message}'", args), ConsoleColor.Cyan);
                }
            }

            public void Warning(string message, params object[] args)
            {
                if (bConsoleHidden) return;
                if (Level == LogLevel.Warning || Level == LogLevel.Error)
                {
                    pPrint(string.Format($"[W] DiscordRPC: '{message}'", args), ConsoleColor.Yellow);
                }
            }
        }

        static Task InitializeRpcClient()
        {
            Console.Title = $"{AppInfo.appName} Version: {AppInfo.appVersion} : By {AppInfo.appAuthor}";
            pPrint($"[I] Initializing thread...", ConsoleColor.White);
            thr_ProcessDetection = new Thread(rpcProcessHandler);
            thr_ProcessDetection.Start();
            pPrint($"[I] RPC thread is running! Looking for running applications...", ConsoleColor.Green);
            return Task.CompletedTask;
        }

        static Task setRPCApps()
        {
            if (Directory.Exists(Environment.CurrentDirectory + "\\apps"))
            {
                if (Directory.GetFiles(Environment.CurrentDirectory + "\\apps").Length > 0)
                {
                    foreach (var jsonfile in Directory.GetFiles(Environment.CurrentDirectory + "\\apps"))
                    {
                        if (jsonfile.EndsWith(".json"))
                        {
                            dRPCApplication dRPCAppDeserialized = JsonConvert.DeserializeObject<dRPCApplication>(File.ReadAllText(jsonfile));
                            dRPCAppList.Add(dRPCAppDeserialized);
                            pPrint($"[I] File {Path.GetFileName(jsonfile)} found!", ConsoleColor.Green);
                        }
                    }
                }
                else
                {
                    pPrint("[E] There are no applications found in the 'apps' directory!", ConsoleColor.Red);
                    pPrint("Press [ENTER] To exit.", ConsoleColor.Gray);
                    Console.ReadLine();
                    Environment.Exit(1);
                }
            }
            else
            {
                pPrint("[E] Directory 'apps' does not appear to exist!", ConsoleColor.Red);
                pPrint("Press [ENTER] To exit.", ConsoleColor.Gray);
                Console.ReadLine();
                Environment.Exit(1);

            }

            return Task.CompletedTask;
        }

        static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        async Task MainAsync()
        {
            try
            {
                cfg = JsonConvert.DeserializeObject<cConfig>(File.ReadAllText(Environment.CurrentDirectory + "\\config.json"));
            }
            catch (Exception e)
            {
                pPrint("[E] Failed to read config! Using defaults.", ConsoleColor.Red);
                cfg = new cConfig(2000, 150, false);
            }
            pPrint($"===APP CONFIG===\nrpcClientUpdateInterval: {cfg.rpcThreadUpdateInt}\nkbDetectInterval: {cfg.kbThreadUpdateInt}\nstartHidden: {cfg.isHidden}\n================", ConsoleColor.Cyan);
            if (cfg.isHidden)
            {
                setConsoleState(nApiWindowState.SW_HIDE);
            }
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(cDomain_onProcessExit);
            await setRPCApps();
            await InitializeRpcClient();
            Thread thr_keystatedetect = new Thread(keyStateHandler);
            thr_keystatedetect.Start();
            pPrint("[I] You can press [CTRL + SHIFT + H] to hide or show this window.", ConsoleColor.Yellow);
        }

        void cDomain_onProcessExit(object sender, EventArgs e)
        {
            if (dRpcClient != null)
            {
                if (dRpcClient.IsInitialized)
                {
                    dRpcClient.ClearPresence();
                    dRpcClient.Deinitialize();
                    dRpcClient.Dispose();
                }
            }
        }
    }
}

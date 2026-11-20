using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Labs.Notifications;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using qBittorrentCompanion.Services;
using qBittorrentCompanion.Views;
using qbittorrentCompanion.Helpers;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using Velopack;

namespace qBittorrentCompanion.Desktop
{
    sealed class Program
    {
        // Thread for the named pipe server
        public static Thread? pipeServerThread = null;
        // Token source for cancelling the named pipe server thread
        public static CancellationTokenSource cts = new();


        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static int Main(string[] args)
        {
            if (UpdateService.IsVelopackInstalled() && OperatingSystem.IsWindows())
            {
                VelopackApp.Build()
                        .OnAfterInstallFastCallback(_ =>
                        {
                            if (OperatingSystem.IsWindows())
                            {
                                WindowsIntegration.Register();
                            }
                        })
                        .OnBeforeUninstallFastCallback(_ =>
                        {
                            if (OperatingSystem.IsWindows())
                            {
                                WindowsIntegration.Unregister();
                            }
                        })
                        .Run();
            }

            // If this is the first instance of the application
            if (TryCreateLockFile())
            {
                // Start the named pipe server thread first
                pipeServerThread = new Thread(() => PipeServerThread(cts.Token))
                {
                    IsBackground = true
                };
                pipeServerThread.Start();

                // Start the application with a classic desktop lifetime exactly once
                int exitCode = BuildAvaloniaApp().StartWithClassicDesktopLifetime(args, lifetime =>
                {
                    lifetime.ShutdownMode = ShutdownMode.OnMainWindowClose;
                    lifetime.Exit += Lifetime_Exit;

                    // Off the UI thread: this does file IO and spawns processes
                    Task.Run(() =>
                    {
                        if (OperatingSystem.IsLinux()) LinuxIntegration.RepairIfStale();
                        else if (OperatingSystem.IsWindows()) WindowsIntegration.RepairIfStale();
                    });

                    // Queued, so it runs once the main window exists
                    if (args.Length > 0)
                        Dispatcher.UIThread.Post(() => PassArgumentsToMainWindow(string.Join(" ", args)));
                });

                // If the app started up with command-line args (like a clicked magnet link), process them after MainWindow loads
                if (exitCode == 0 && args.Length > 0)
                {
                    // Give the window a brief moment to initialize, then process startup arguments
                    // (You can also hook this into MainWindow's Loaded event for absolute safety)
                    string combinedArgs = string.Join(" ", args);
                    // Process combinedArgs similarly to PassArgumentsToMainWindow once desktop.MainWindow is ready
                }

                return exitCode;
            }
            else // Application must already be running, the file is locked.
            {
                // send the arguments to the first instance
                using (var pipeClient = new NamedPipeClientStream(".", "qBittorrentCompanionPipe", PipeDirection.Out))
                {
                    pipeClient.Connect();
                    using var writer = new StreamWriter(pipeClient);
                    writer.Write(string.Join(" ", args));
                }

                // Bring main window to the foreground
                PassArgumentsToMainWindow(null);
                return 1;
            }
        }



        private static void PipeServerThread(CancellationToken token)
        {
            //Run indefinitely, Environment.Exit(0) will end this and all other processes.
            while (!token.IsCancellationRequested)
            {
                try
                {
                    using var pipeServer = new NamedPipeServerStream("qBittorrentCompanionPipe", PipeDirection.In);
                    pipeServer.WaitForConnection();

                    if (pipeServer.IsConnected)
                    {
                        using (var reader = new StreamReader(pipeServer))
                        {
                            var args = reader.ReadToEnd();
                            PassArgumentsToMainWindow(args);
                        }
                    }

                    if (pipeServer.IsConnected)
                    {
                        pipeServer.Disconnect();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        private static void Lifetime_Exit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            // Cancel the token when the application is shutting down
            cts.Cancel();
        }

        private static void PassArgumentsToMainWindow(string? args)
        {
            if (args is not null
            && Application.Current is not null
            && Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            && desktop.MainWindow is not null
            && desktop.MainWindow is MainWindow mainWindow)
            {
                // Some Linux distros can provide args wrapped in single quotes, work around it
                args = args.Trim('\'', '"');

                // Check if the argument is a magnet link or a torrent file path
                if (args.StartsWith("magnet:?", StringComparison.OrdinalIgnoreCase))
                {
                    // Route magnet links through your URL handler queue
                    Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        await AddTorrentsWindow.HandleTorrentQueueAsync(
                            mainWindow,
                            new[] { args },
                            ConfigService.Config.ByPassDownloadWindow
                        );
                    });
                }
                else
                {
                    // .torrent file handling
                    mainWindow.AddToTorrentQueue(args);
                    _ = mainWindow.ProcessTorrentQueue(ConfigService.Config.ByPassDownloadWindow);
                }

                mainWindow.Activate(); // Bring window to the foreground.
            }
        }


        private static FileStream? _lockFile;

        private static bool TryCreateLockFile()
        {
            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "qBIttorrentCompanion", //Company 
                "qBittorrentCompanion" // App
            );
            Directory.CreateDirectory(dir);

            // MacOS does not support .Lock(long, long)
            // However packaging the application as .app should ensure only one instance runs.
            if (!OperatingSystem.IsMacOS())
            {
                try
                {
                    _lockFile = File.Open(Path.Combine(dir, ".lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                    _lockFile.Lock(0, 0);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
        {
            var builder = AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont();

            builder = TryAddNotifications(builder);

            return builder
                .LogToTrace()
                .UseReactiveUI();
        }

        private static AppBuilder TryAddNotifications(AppBuilder builder)
        {
            try
            {
                // Provide explicit options to prevent the NullReferenceException
                return builder.WithAppNotifications(
                    new AppNotificationOptions { AppUserModelId = "qBittorrentCompanion" });
            }
            catch (Exception ex)
            {
                // Avalonia.Labs.Notifications 12.0.2 currently throws MissingMethodException on Linux
                // (incompatible with Tmds.DBus.Protocol 0.94.1). Run without native notifications.
                Trace.WriteLine($"Native notifications unavailable: {ex.Message}");
                return builder;
            }
        }
    }
}
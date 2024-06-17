using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using qBittorrentCompanion.Views;
using qBittorrentCompanion.Services;

namespace qBittorrentCompanion.Desktop
{
    sealed class Program
    {
        // Thread for the named pipe server
        public static Thread? pipeServerThread = null;
        // Token source for cancelling the named pipe server thread
        public static CancellationTokenSource cts = new CancellationTokenSource();

        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static int Main(string[] args)
        {
            // If this is the first instance of the application
            if (TryCreateLockFile())
            {
                // Start the named pipe server thread
                pipeServerThread = new Thread(() => PipeServerThread(cts.Token));
                pipeServerThread.IsBackground = true;
                pipeServerThread.Start();

                // Start the application with a classic desktop lifetime
                var app = BuildAvaloniaApp().StartWithClassicDesktopLifetime(args, lifetime =>
                {
                    // Set the shutdown mode to shut down when the main window is closed
                    lifetime.ShutdownMode = ShutdownMode.OnMainWindowClose;
                    // When the main window is closed shut all processes down
                    lifetime.Exit += Lifetime_Exit;
                });

                return 0;
            }
            else // Application must already be running, the file is locked.
            {
                // send the arguments to the first instance
                using (var pipeClient = new NamedPipeClientStream(".", "qBittorrentCompanionPipe", PipeDirection.Out))
                {
                    pipeClient.Connect();
                    using (var writer = new StreamWriter(pipeClient))
                    {
                        writer.Write(string.Join(" ", args));
                    }
                }

                // Bring main window to the foreground as the user either 
                // 1. Tried to launch the app whilst it was running
                // 2. Tried to open a file associated with it
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
                    using (var pipeServer = new NamedPipeServerStream("qBittorrentCompanionPipe", PipeDirection.In))
                    {
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
                mainWindow.AddToFileQueue(args);
                _ = mainWindow.ProcessFileQueue(ConfigService.Config.ByPassDownloadWindow);
                mainWindow.Activate(); //Bring window to the foreground.
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
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace()
                .UseReactiveUI();
    }
}
using System;
using Android.App;
using Android.Runtime;
using Avalonia;
using Avalonia.Android;
using Avalonia.ReactiveUI;

namespace qBittorrentCompanion.Android;

[Application]
public class AndroidApp : AvaloniaAndroidApplication<App>
{
    public AndroidApp(IntPtr handle, JniHandleOwnership transfer)
        : base(handle, transfer)
    {
    }

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        return base.CustomizeAppBuilder(builder)
            .WithInterFont()
            .UseReactiveUI();
    }
}
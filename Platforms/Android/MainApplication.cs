using Android.App;
using Android.Runtime;
using MULTI_Bet_playing_Demo.Services;

namespace MULTI_Bet_playing_Demo;

[Application]
public class MainApplication : MauiApplication
{
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
        AppLog.WireGlobalExceptionHandlers();
        AppLog.Info("MainApplication ctor");
    }

    protected override MauiApp CreateMauiApp()
    {
        AppLog.Info("CreateMauiApp");
        return MauiProgram.CreateMauiApp();
    }

    public override void OnCreate()
    {
        base.OnCreate();
        try
        {
            AndroidLogBootstrap.Init(this);
        }
        catch (Exception ex)
        {
            AppLog.Exception("MainApplication.OnCreate", ex);
        }
    }
}

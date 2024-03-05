using System;
using System.IO;
using System.Windows;
using System.Threading;

namespace RayBin.FishImageLabel
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        [STAThread]
        public static void Main()
        {
            try
            {
                SplashScreen splash = new SplashScreen("resources/SplashScreen.jpg");
                splash.Show(true);
                Mutex run = new Mutex(true, "FishLabel", out bool bRun);
                if (bRun)
                {
                    InitializePath();
                    RayBin.FishImageLabel.App app = new RayBin.FishImageLabel.App();
                    app.InitializeComponent();
                    app.Run();
                }
                else
                {
                    MessageBox.Show("程序已启动，它可能在您的右下角!", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    Environment.Exit(0);
                }
                run.ReleaseMutex();
            }
            catch (Exception ex)
            {
                Common.WriteExceptionLog(ex);
                MessageBox.Show("遇到一个未知的问题，可能导致无法继续运行。", "系统错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        static void InitializePath()
        {
            string logPath = Common.AppPath + "\\Log";
            string skinPath = Common.AppPath + "\\Skin";
            if (!Directory.Exists(logPath)) Directory.CreateDirectory(logPath);
            if (!Directory.Exists(skinPath)) Directory.CreateDirectory(logPath);
        }
    }
}
using System.Windows;

namespace RayBin.FishImageLabel
{
    /// <summary>
    /// SplashSreen.xaml 的交互逻辑
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
        }

        private void CloseAboutWindow(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.Close();
        }
    }
}
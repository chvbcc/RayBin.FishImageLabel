using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;

namespace RayBin.FishImageLabel
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        #region 最小化
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }
        #endregion

        #region 最大化
        private void MaxRestoreButton_Click(object sender, RoutedEventArgs e)
        {
            Path p = MaxRestoreButton.Template.FindName("Icon", this.MaxRestoreButton) as Path;
            if (this.WindowState != WindowState.Maximized)
            {
                this.BorderThickness = new Thickness(0.00);
                MaxRestoreButton.ToolTip = "还原";
                Geometry gRestore = (Geometry)this.FindResource("IconRestore");
                this.WindowState = WindowState.Maximized;
                p.Data = gRestore;
            }
            else
            {
                if (this.WindowState == WindowState.Maximized)
                {
                    this.BorderThickness = new Thickness(10.00);
                    MaxRestoreButton.ToolTip = "最大化";
                    Geometry gMax = (Geometry)this.FindResource("IconMax");
                    this.WindowState = WindowState.Normal;
                    p.Data = gMax;
                }
            }
        }
        #endregion

        #region 关闭
        private void MainWindowClose(object sender, RoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }
        #endregion

        #region 技术支持
        //技术支持
        private void SupportButton_Click(object sender, RoutedEventArgs e)
        {
            string emailSubject = "技术支持";
            string emailTo = "service@Kingdata.com";
            string mailtoUri = $"mailto:{emailTo}?subject={Uri.EscapeDataString(emailSubject)}";
            System.Diagnostics.Process.Start(mailtoUri);
        }
        #endregion

        #region 自定义皮肤
        private void ChangeSkinButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Filter = "JPEG文件|*.jpg|png 文件|*.png",
                InitialDirectory = Common.AppPath + "\\Skin",
                RestoreDirectory = true,
                FilterIndex = 1
            };
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ImageBrush b = new ImageBrush()
                {
                    ImageSource = new BitmapImage(new Uri(ofd.FileName)),
                    Stretch = Stretch.Fill
                };
                this.Background = b;
                SolidColorBrush background = new SolidColorBrush(Colors.Transparent);
                SolidColorBrush foreground = Common.GetSolidColorBrush(ofd.FileName);
                this.Resources.Remove("TreeViewForeColor");
                this.Resources.Remove("TitleButtonColor");
                this.Resources.Remove("TextColor");
                this.Resources.Add("TreeViewForeColor", foreground);
                this.Resources.Add("TitleButtonColor", foreground);
                this.Resources.Add("TextColor", foreground);
                TopArea.Background = background;
                RightArea.Background = background;
                PageTitle.Foreground = foreground;
                FileTreeView.Background = background;
                DirectoryButton.Foreground = foreground;
                DeleteButton.Foreground = foreground;
                SetStyles(imageLabel, foreground, background);
            }
        }
        #endregion

        #region 帮助
        private void HelpExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            string helpFilePath = "";
            System.Diagnostics.Process.Start(helpFilePath);
        }
        #endregion
    }
}
using System.Windows;
using System.Windows.Controls;

namespace RayBin.FishImageLabel
{
    /// <summary>
    /// frmConnection.xaml 的交互逻辑
    /// </summary>
    public partial class Config : Window
    {
        public Config()
        {
            InitializeComponent();
            this.TopArea.MouseLeftButtonDown += (o, e) => { DragMove(); };
        }

        #region 对话框加载
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //图像合成
            AutoDenoise.Value = Properties.Settings.Default.AutoDenoise;
            foreach (ComboBoxItem item in AutoColoringBlockSize.Items)
            {
                if (item.Content.ToString() == Properties.Settings.Default.AutoColoringBlockSize.ToString())
                {
                    item.IsSelected = true;
                    break;
                }
            }
            AutoColoringLowThreshold.Value = Properties.Settings.Default.AutoColoringLowThreshold;
            AutoColoringMinBlockFilter.Value = Properties.Settings.Default.AutoColoringMinBlockFilter;
            AutoColoringMaxBlockFilter.Value = Properties.Settings.Default.AutoColoringMaxBlockFilter;
            BorderWidth.Value = Properties.Settings.Default.BorderWidth;
            BorderColor.SelectedColor = Properties.Settings.Default.BorderColor;
            FillColor.SelectedColor = Properties.Settings.Default.FillColor;
            SelectedAreaScaling.Value = Properties.Settings.Default.SelectedAreaScaling;
        }
        #endregion

        #region 对话框关闭
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.AutoDenoise = (int)AutoDenoise.Value;
            Properties.Settings.Default.AutoColoringBlockSize = int.Parse((AutoColoringBlockSize.SelectedItem as ComboBoxItem).Content.ToString());
            Properties.Settings.Default.AutoColoringLowThreshold = (int)AutoColoringLowThreshold.Value;
            Properties.Settings.Default.AutoColoringMinBlockFilter = (int)AutoColoringMinBlockFilter.Value;
            Properties.Settings.Default.AutoColoringMaxBlockFilter = (int)AutoColoringMaxBlockFilter.Value;
            Properties.Settings.Default.BorderWidth = (int)BorderWidth.Value;
            Properties.Settings.Default.BorderColor = BorderColor.SelectedColor.Value;
            Properties.Settings.Default.FillColor = FillColor.SelectedColor.Value;
            Properties.Settings.Default.SelectedAreaScaling = (int)SelectedAreaScaling.Value;
            Properties.Settings.Default.Save();
        }
        #endregion

        #region 图片预处理

        private void PreImage_Checked(object sender, RoutedEventArgs e)
        {
            AutoDenoise.IsEnabled = true;
        }

        private void PreImage_Unchecked(object sender, RoutedEventArgs e)
        {
            AutoDenoise.IsEnabled = false;
        }
        #endregion

        #region 关闭对话框
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion
    }
}
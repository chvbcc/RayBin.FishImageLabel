using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Windows.Interop;
using System.Windows.Controls;
using Forms = System.Windows.Forms;

namespace RayBin.FishImageLabel
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool IsClose = false;

        private readonly int agWidth = 18;                   //右下角
        private readonly int bThickness = 12;               // 边框宽度
        protected object Handle { get; private set; }
        private readonly Forms.NotifyIcon ni = new Forms.NotifyIcon();
        public readonly ImageLabel imageLabel = new ImageLabel();
        public EnumPageViewType CurrentPage { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            this.TopArea.MouseLeftButtonDown += (o, e) => { DragMove(); };
            string logPath = AppContext.BaseDirectory + "\\Log";
            if (!System.IO.Directory.Exists(logPath)) { System.IO.Directory.CreateDirectory(logPath); }
            string exportPath = AppContext.BaseDirectory + "\\Export";
            if (!System.IO.Directory.Exists(exportPath)) { System.IO.Directory.CreateDirectory(exportPath); }
            CurrentPage = EnumPageViewType.ImageLabel;
            LoadPosition();
            InitFileTreeView();
            ni.BalloonTipText = "Fish Image Compose...";
            ni.Text = "Fish Image Compose";
            ni.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Forms.Application.ExecutablePath);
            ni.Visible = true;
            ni.MouseClick += NotifyIconMouseClick;
            MainFrame.Content = imageLabel;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            IntPtr handle = new WindowInteropHelper(this).Handle;
            HwndSource source = HwndSource.FromHwnd(handle) ?? throw new Exception("Cannot get HwndSource instance.");
            source.AddHook(new HwndSourceHook(this.WindowProc));
        }

        #region 窗口事件
        //窗口加载完成后，展开第一个节点
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (FileTreeView.Items.Count > 0)
            {
                var item = FileTreeView.ItemContainerGenerator.ContainerFromItem(FileTreeView.Items[0]);
                if (item != null) { (item as TreeViewItem).IsExpanded = true; }
            }
        }

        //处理窗口最小化
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.Hide();
            }
        }

        //关闭中
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!this.IsClose)
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        //标题中间区域双击
        private void WindowState_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Path p = MaxRestoreButton.Template.FindName("Icon", this.MaxRestoreButton) as Path;
            if (this.WindowState == WindowState.Maximized)
            {
                MaxRestoreButton.ToolTip = "最大化";
                Geometry gMax = (Geometry)this.FindResource("IconMax");
                this.WindowState = WindowState.Normal;
                p.Data = gMax;
            }
            else
            {
                MaxRestoreButton.ToolTip = "还原";
                Geometry gRestore = (Geometry)this.FindResource("IconRestore");
                this.WindowState = WindowState.Maximized;
                p.Data = gRestore;
            }
        }

        //删除选择区域的框
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                imageLabel.DeleteLabel();
                return;
            }
            else if (e.Key == Key.Escape)
            {
                imageLabel.DrawCancel();
                return;
            }
        }
        #endregion

        #region 托盘图标右键点击
        //托盘图标右键点击
        private void NotifyIconMouseClick(object sender, Forms.MouseEventArgs e)
        {
            if (e.Button == Forms.MouseButtons.Right)
            {
                ContextMenu cm = NotifyIconMenu;
                cm.IsOpen = true;
            }
            if (e.Button == Forms.MouseButtons.Left)
            {
                this.Show();
            }
        }
        #endregion

        #region 菜单
        private void MenuAboutClick(object sender, RoutedEventArgs e)
        {
            AboutWindow About = new AboutWindow()
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            About.Show();
        }

        private void MenuShowMainWindowClick(object sender, RoutedEventArgs e)
        {
            this.Show();
        }

        private void MenuExitClick(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                Properties.Settings.Default.IsMax = true;
            }
            else
            {
                Properties.Settings.Default.IsMax = false;
                Properties.Settings.Default.Width = this.Width;
                Properties.Settings.Default.Height = this.Height;
            }
            Properties.Settings.Default.Save();
            this.IsClose = true;
            this.Close();
            Application.Current.Shutdown();
        }
        #endregion
    }
}
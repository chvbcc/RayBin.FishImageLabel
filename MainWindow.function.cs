using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Windows;
using System.Windows.Media;
using System.Windows.Interop;
using System.Windows.Controls;
using Point = System.Windows.Point;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using static RayBin.FishImageLabel.NativeMethods;

namespace RayBin.FishImageLabel
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private Point mousePoint = new Point();
        private readonly string[] extensions = new string[] { ".bmp", ".jpg", ".jpeg", ".tif", ".png" };

        #region 加载程序设置
        private void LoadPosition()
        {
            bool IsMax = Properties.Settings.Default.IsMax;
            if (IsMax)
            {
                this.WindowState = WindowState.Maximized;
            }
            else
            {
                this.Width = Properties.Settings.Default.Width;
                this.Height = Properties.Settings.Default.Height;
            }
        }
        #endregion

        #region 初始化大树
        private void InitFileTreeView()
        {
            string folderPath = Properties.Settings.Default.DefaultPath;
            if (System.IO.Directory.Exists(folderPath))
            {
                ObservableCollection<FileSystemItemInfo> RootItems = new ObservableCollection<FileSystemItemInfo>();
                FileSystemItemInfo rootItem = CreateFileSystemItem(folderPath);
                RootItems.Add(rootItem);
                FileTreeView.ItemsSource = RootItems;
            }
        }
        #endregion

        #region 窗口过程函数
        private IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case NativeMethods.WM_NCHITTEST:
                    if (this.WindowState == WindowState.Maximized) { break; }
                    IntPtr value = IntPtr.Zero;
                    mousePoint.Y = NativeMethods.HIWORD(lParam.ToInt32());
                    mousePoint.X = NativeMethods.LOWORD(lParam.ToInt32());
                    //右下角
                    if (this.ActualWidth + this.Left - this.mousePoint.X <= this.agWidth && this.ActualHeight + this.Top - this.mousePoint.Y <= this.agWidth)
                    {
                        value = new IntPtr((int)NativeMethods.HitTest.HTBOTTOMRIGHT);
                    }
                    // 窗口左侧
                    if (this.mousePoint.X - this.Left <= this.bThickness)
                    {
                        value = new IntPtr((int)NativeMethods.HitTest.HTLEFT);
                    }
                    // 窗口右侧
                    else if (this.ActualWidth + this.Left - this.mousePoint.X <= this.bThickness)
                    {
                        value = new IntPtr((int)NativeMethods.HitTest.HTRIGHT);
                    }
                    // 窗口上方
                    else if (this.mousePoint.Y - this.Top <= this.bThickness)
                    {
                        value = new IntPtr((int)NativeMethods.HitTest.HTTOP);
                    }
                    // 窗口下方
                    else if (this.ActualHeight + this.Top - this.mousePoint.Y <= this.bThickness)
                    {
                        value = new IntPtr((int)NativeMethods.HitTest.HTBOTTOM);
                    }
                    if (value != IntPtr.Zero)
                    {
                        handled = true;
                        return value;
                    }
                    handled = false;
                    break;
            }
            return IntPtr.Zero;
        }
        #endregion

        #region 设置风格
        private void SetStyles(DependencyObject obj, SolidColorBrush foreground, SolidColorBrush background)
        {
            int count = VisualTreeHelper.GetChildrenCount(obj);
            for (int i = 0; i < count; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child == null) continue;
                string controlType = child.GetType().Name;
                switch (controlType)
                {
                    case "RadioButton":
                        RadioButton objRadioButton = child as RadioButton;
                        objRadioButton.Foreground = foreground;
                        break;
                    case "Button":
                        Button objButton = child as Button;
                        objButton.Foreground = foreground;
                        break;
                    case "Label":
                        Label objLabel = (child as Label);
                        objLabel.Foreground = foreground;
                        break;
                    case "TextBox":
                        TextBox objTextBox = (child as TextBox);
                        objTextBox.Foreground = foreground;
                        break;
                    case "CheckBox":
                        CheckBox objCheckBox = (child as CheckBox);
                        objCheckBox.Foreground = foreground;
                        break;
                }
                if (VisualTreeHelper.GetChildrenCount(child) > 0)
                {
                    SetStyles(child, foreground, background);
                }
            }
        }
        #endregion

        #region 枚举文件夹
        private FileSystemItemInfo CreateFileSystemItem(string path)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            FileSystemItemInfo item = new FileSystemItemInfo()
            {
                Title = directoryInfo.Name,
                Name = directoryInfo.FullName,
                NodeIcon = GetIcon(directoryInfo.FullName),
                Children = new ObservableCollection<FileSystemItemInfo>()
            };
            foreach (var subdirectoryInfo in directoryInfo.GetDirectories())
            {
                item.Children.Add(CreateFileSystemItem(subdirectoryInfo.FullName));
            }
            foreach (var fileInfo in directoryInfo.GetFiles())
            {
                if (extensions.Contains(fileInfo.Extension))
                {
                    item.Children.Add(new FileSystemItemInfo
                    {
                        Title = fileInfo.Name,
                        Name = fileInfo.FullName,
                        NodeIcon = GetIcon(fileInfo.FullName)
                    });
                }
            }
            return item;
        }
        #endregion

        #region 文件删除
        private void DeleteFile()
        {
            ObservableCollection<FileSystemItemInfo> RootItems = FileTreeView.ItemsSource as ObservableCollection<FileSystemItemInfo>;
            FileSystemItemInfo deleteInfo = FileTreeView.SelectedItem as FileSystemItemInfo;
            if (deleteInfo == null) { return; }
            if (File.Exists(deleteInfo.Name) || Directory.Exists(deleteInfo.Name)) 
            { 
                MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("是否要删除选中的文件！", "确认删除", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                if (messageBoxResult == MessageBoxResult.OK)
                {
                    NativeMethods.SHFILEOPSTRUCT delete = new NativeMethods.SHFILEOPSTRUCT
                    {
                        wFunc = NativeMethods.FO_DELETE,
                        pFrom = deleteInfo.Name + '\0' + '\0',
                        fFlags = NativeMethods.FOF_ALLOWUNDO | NativeMethods.FOF_NOCONFIRMATION
                    };
                    NativeMethods.SHFileOperation(ref delete);                     
                    if (RootItems[0] == deleteInfo) 
                    {
                        RootItems.Clear();
                        return;
                    }
                    FileSystemItemInfo parentNode = FindParentNode(RootItems[0], deleteInfo.Name);
                    if (parentNode != null)
                    {
                        parentNode.Children.Remove(deleteInfo);
                    }
                }
            }
        }

        private FileSystemItemInfo FindParentNode(FileSystemItemInfo currentNode, string searchText)
        {
            if (currentNode == null) return null;
            if (currentNode.Name == searchText) { 
                return currentNode; 
            }
            if (currentNode.Children != null)
            {
                foreach (var child in currentNode.Children)
                {
                    if (child.Name == searchText)
                    {
                        return currentNode;
                    }
                    var result = FindParentNode(child, searchText);
                    if (result != null) return result;
                }
            }
            return null;
        }
        #endregion

        #region 获取图标
        public ImageSource GetIcon(string path)
        {
            SHFILEINFO shfi = new SHFILEINFO();
            try
            {
                SHGetFileInfo(path, 0, out shfi, (uint)Marshal.SizeOf(shfi), SHGFI_ICON | SHGFI_SMALLICON);
                Icon icon = (Icon)System.Drawing.Icon.FromHandle(shfi.hIcon).Clone();
                return Imaging.CreateBitmapSourceFromHIcon(icon.Handle, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(16, 16));
            }
            catch (Exception error)
            {
                System.Windows.MessageBox.Show(error.Message, "异常", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }
            finally
            {
                DestroyIcon(shfi.hIcon);
            }
        }
        #endregion

        #region 刷新文件树

        public void RefreshAddFile(int parentLevel = 1)
        {
            ObservableCollection<FileSystemItemInfo> RootItems = FileTreeView.ItemsSource as ObservableCollection<FileSystemItemInfo>;
            if (FileTreeView.SelectedItem is FileSystemItemInfo selectedViewModel)
            {
                string findName = selectedViewModel.Name;
                if (parentLevel > 1) 
                {
                    for (int i = 1; i < parentLevel; i++) 
                    { 
                        findName = Directory.GetParent(findName).FullName;
                    }
                }
                FileSystemItemInfo parentNode = FindParentNode(RootItems[0], findName);
                string folderPath = parentNode.Name;
                parentNode.Children.Clear();
                FileSystemItemInfo rootItem = CreateFileSystemItem(folderPath);
                foreach (FileSystemItemInfo model in rootItem.Children) 
                {
                    parentNode.Children.Add(model);
                }
            }
        }
        #endregion
    }
}
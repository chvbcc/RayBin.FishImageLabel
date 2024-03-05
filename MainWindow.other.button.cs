using System.IO;
using System.Windows;
using System.Diagnostics;
using System.Windows.Controls;
using Forms = System.Windows.Forms;
using System.Collections.ObjectModel;

namespace RayBin.FishImageLabel
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private string CurrentFile { get; set; }

        #region 大树选中事件
        //FileTreeView事件
        private void FileTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (FileTreeView.SelectedItem is FileSystemItemInfo selectedViewModel && System.IO.File.Exists(selectedViewModel.Name))
            {
                CurrentFile = selectedViewModel.Name;
                imageLabel.CurrentFile = CurrentFile;
                if (CurrentPage == EnumPageViewType.ImageLabel) 
                {
                    imageLabel.LoadImage(CurrentFile);
                }
            }
        }
        #endregion

        #region 荧光点标注按钮
        private void ComposeButton_Click(object sender, RoutedEventArgs e)
        {
            PageTitle.Content = "荧光点标注";
            SelectAreaButton.IsEnabled = true;
            ManualLabelButton.IsEnabled = true;
            CurrentPage = EnumPageViewType.ImageLabel;
            MainFrame.Content = imageLabel;
        }
        #endregion

        #region 选区放大
        private void SelectAreaButton_Click(object sender, RoutedEventArgs e)
        {
            imageLabel.SelectedAreaScaling();
        }
        #endregion

        #region 单点绘制标注
        private void SinglePointLabelButton_Click(object sender, RoutedEventArgs e)
        {
            imageLabel.SinglePointDrawLabel();
        }
        #endregion

        #region 线条绘制标注
        private void LineLabelButton_Click(object sender, RoutedEventArgs e)
        {
            imageLabel.LineDrawLabel();
        }
        #endregion

        #region 手动绘制标注
        private void ManualLabelButton_Click(object sender, RoutedEventArgs e)
        {
            imageLabel.ManualDrawLabel();
        }
        #endregion

        #region 系统设置按钮
        private void ConfigButton_Click(object sender, RoutedEventArgs e)
        {
            Config config = new Config
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            config.ShowDialog();
        }
        #endregion

        #region 目录选择按钮
        private void DirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            Forms.FolderBrowserDialog fbd = new Forms.FolderBrowserDialog();
            if (fbd.ShowDialog() == Forms.DialogResult.OK)
            {
                string folderPath = fbd.SelectedPath;
                FileSystemItemInfo rootItem = CreateFileSystemItem(folderPath);
                ObservableCollection<FileSystemItemInfo> RootItems = new ObservableCollection<FileSystemItemInfo>()
                {
                    rootItem
                };
                FileTreeView.ItemsSource = RootItems;
                Properties.Settings.Default.DefaultPath = folderPath;
                Properties.Settings.Default.Save();
                var item = FileTreeView.ItemContainerGenerator.ContainerFromItem(FileTreeView.Items[0]);
                if (item != null) { (item as TreeViewItem).IsExpanded = true; }
            }
        }
        #endregion

        #region 打开当前目录
        private void OpenDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (FileTreeView.SelectedItem is FileSystemItemInfo selectedViewModel)
            {
                string path = selectedViewModel.Name;
                if (File.Exists(path)) { path = Path.GetDirectoryName(path); }
                Process.Start(path);
            }
        }
        #endregion

        #region 删除文件按钮
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteFile();
        }
        #endregion

    }
}
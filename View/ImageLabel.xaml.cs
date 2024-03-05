using System;
using System.IO;
using System.Linq;
using System.Windows;
using Newtonsoft.Json;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Collections.Generic;
using WindowsSize = System.Windows.Size;
using Line = System.Windows.Shapes.Line;
using Polygon = System.Windows.Shapes.Polygon;

namespace RayBin.FishImageLabel
{
    /// <summary>
    /// LogList.xaml 的交互逻辑
    /// </summary>
    public partial class ImageLabel : Page
    {
        //操作消息文本定时隐藏
        private readonly DispatcherTimer delayTimeRun = new DispatcherTimer();
        private readonly DispatcherTimer messageTimer = new DispatcherTimer();
        private Label sizeLabel = new Label { Background = Brushes.LightGray, Opacity = 0.75 };

        private MainWindow ParentWindow { get; set; }
        private int LowerColor { get; set; } 
        private int UpperColor { get; set; }
        public string CurrentFile { get; set; }
        public ImageLabel()
        {
            InitializeComponent();
            messageTimer.Interval = TimeSpan.FromSeconds(3);
            messageTimer.Tick += MessageTimer_Tick;
            delayTimeRun.Interval = TimeSpan.FromSeconds(3);
            delayTimeRun.Tick += DelayTimeRun_Tick;
        }

        #region 窗口加载
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ParentWindow = System.Windows.Window.GetWindow(this) as MainWindow;
            InitPage();
        }
        #endregion

        #region 窗口大小
        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ResetTranslateTransform();
        }
        #endregion

        #region 消息定时执行
        private void MessageTimer_Tick(object sender, EventArgs e)
        {
            LabelInfo.Content = "";
            messageTimer.Stop(); 
        }
        #endregion

        #region 延时运行执行
        private void DelayTimeRun_Tick(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(CurrentFile) && File.Exists(CurrentFile))
            {
                delayTimeRun.Stop();
                UpperColor = (int)UpperColorInteger.Value;
                ImageColoring(CurrentFile);
                Properties.Settings.Default.UpperColor = UpperColor;
                Properties.Settings.Default.Save();
            }
        }
        #endregion

        #region 保存按钮
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(CurrentFile) && File.Exists(CurrentFile) && ImageMain.Source != null && ImageMain.Tag != null)
                {
                    List<PointCollection> listPoints = new List<PointCollection>();
                    IEnumerable<Polygon> Polygons = CanvasMain.Children.OfType<Polygon>().ToList();
                    foreach (Polygon polygon in Polygons)
                    {
                        listPoints.Add(polygon.Points);
                    }
                    string json = JsonConvert.SerializeObject(listPoints, Formatting.Indented);
                    string fileFullName = Path.GetFileNameWithoutExtension(CurrentFile) + ".json";
                    fileFullName = Path.Combine(Path.GetDirectoryName(CurrentFile), fileFullName); 
                    File.WriteAllText(fileFullName, json);
                    messageTimer.Start();
                    LabelInfo.Content = "保存成功";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "异常", MessageBoxButton.OK, MessageBoxImage.Exclamation);   
                return;
            }
        }
        #endregion

        #region 导出训练图片
        private void ExportTrainingImageButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(CurrentFile) && File.Exists(CurrentFile) && ImageMain.Source != null && ImageMain.Tag != null)
                {
                    ExportTrainingImage();
                    messageTimer.Start();
                    ParentWindow.RefreshAddFile();
                    LabelInfo.Content = "导出成功";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "异常", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
        }
        #endregion

        #region 隐藏显示着色层
        private void LayerVisibility_Checked(object sender, RoutedEventArgs e)
        {
            if (ImageMain.Source == null) { return; }
            var Polygons = CanvasMain.Children.OfType<Polygon>().ToList();
            foreach (Polygon polygon in Polygons) 
            {
                polygon.Visibility = Visibility.Hidden;
            }
            (sender as CheckBox).Content = "显示着色层";
        }

        private void LayerVisibility_Unchecked(object sender, RoutedEventArgs e)
        {
            if (ImageMain == null) { return; }
            var Polygons = CanvasMain.Children.OfType<Polygon>().ToList();
            foreach (Polygon polygon in Polygons)
            {
                polygon.Visibility = Visibility.Visible;
            }
            (sender as CheckBox).Content = "隐藏着色层";
        }
        #endregion

        #region 设置图窗口大小
        //设置窗口大小
        private void WindowSizeCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {            
                if (!string.IsNullOrEmpty(CurrentFile) && File.Exists(CurrentFile))
                {
                    double parentWidth = ImageScrollViewer.ViewportWidth;
                    double parentHeight = ImageScrollViewer.ViewportHeight;
                    ZoomCombox.SelectedIndex = 5;

                    double scaleX = parentWidth / ImageMain.Width;
                    double scaleY = parentHeight / ImageMain.Height;
                    double scaleRatio = Math.Min(scaleX, scaleY);

                    TransformGroup group = CanvasMain.RenderTransform as TransformGroup;
                    ScaleTransform scaleTransform = group.Children[1] as ScaleTransform;

                    // 设置缩放中心点为ScrollViewer的中心点
                    scaleTransform.CenterX = 0;
                    scaleTransform.CenterY = 0;

                    // 设置缩放比例
                    scaleTransform.ScaleX = scaleRatio;
                    scaleTransform.ScaleY = scaleRatio;
                    CanvasMain.Width = parentWidth;
                    CanvasMain.Height = parentHeight;
                    ResetTranslateTransform();
                }      
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "异常", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
        }

        //取消窗口大小
        private void WindowSizeCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(CurrentFile) && File.Exists(CurrentFile))
                {
                    ZoomCombox.SelectedIndex = 5;
                    ZoomResult(100);
                    ResetTranslateTransform();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "异常", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
        }
        #endregion

        #region 图片缩放
        private void ZoomCombox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (ZoomCombox.Text == string.Empty || ImageMain.Source == null || WindowSizeCheckBox.IsChecked == true) { return; }
                ComboBoxItem ComboItem = (ComboBoxItem)ZoomCombox.SelectedItem;
                double zoomValue = double.Parse(ComboItem.Content.ToString().TrimEnd('%'));
                ZoomResult(zoomValue);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "异常", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
        }
        #endregion

        #region 范围拖动
        private void LowerColorSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                if (LowerColorInteger != null)
                {
                    LowerColorInteger.Value = (int)LowerColorSlider.Value;
                    LowerColor = (int)LowerColorSlider.Value;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "异常", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
        }
        #endregion

        #region 范围拖动结束时处理图像
        private void LowerColorSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(CurrentFile) && File.Exists(CurrentFile))
                {
                    ImageColoring(CurrentFile);
                    Properties.Settings.Default.LowerColor = LowerColor;
                    Properties.Settings.Default.Save();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "异常", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
        }
        #endregion

        #region 范围起始值输入
        private void LowerColorInteger_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (!string.IsNullOrEmpty(CurrentFile) && File.Exists(CurrentFile))
                    {
                        if (LowerColorSlider.Value != (double)LowerColorInteger.Value)
                        {
                            LowerColorSlider.Value = (double)LowerColorInteger.Value;
                            LowerColor = (int)LowerColorInteger.Value;
                            ImageColoring(CurrentFile);
                            Properties.Settings.Default.LowerColor = LowerColor;
                            Properties.Settings.Default.Save();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "异常", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
        }
        #endregion

        #region 范围结束值输入
        private void UpperColorInteger_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (!string.IsNullOrEmpty(CurrentFile) && File.Exists(CurrentFile))
                    {
                        UpperColor = (int)UpperColorInteger.Value;
                        ImageColoring(CurrentFile);
                        Properties.Settings.Default.UpperColor = UpperColor;
                        Properties.Settings.Default.Save();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "异常", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
        }
        #endregion

        #region 范围结束值更改
        private void UpperColorInteger_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            delayTimeRun.Stop();
            delayTimeRun.Start();
        }
        #endregion

        #region 着色算法选择
        private void ColoringAlgorithmCombox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(CurrentFile) && File.Exists(CurrentFile))
                {
                    ImageColoring(CurrentFile);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "异常", MessageBoxButton.OK, MessageBoxImage.Exclamation); 
                return;
            }
        }
        #endregion

        #region 隐藏分割线
        private void CuttingLine_Unchecked(object sender, RoutedEventArgs e)
        {
            if (ImageMain.Source == null) { return; }
            var Lines = CanvasMain.Children.OfType<Line>().ToList();
            foreach (Line line in Lines)
            {
                if (line.Tag.ToString() == "CuttingLine") { CanvasMain.Children.Remove(line); }
            }
            (sender as CheckBox).Content = "显示分割线";
        }

        private void CuttingLine_Checked(object sender, RoutedEventArgs e)
        {
            if (ImageMain.Source == null) { return; }
            DrawCuttingLine((int)ImageMain.Width, (int)ImageMain.Height);
           (sender as CheckBox).Content = "隐藏分割线";
        }
        #endregion
    }
}
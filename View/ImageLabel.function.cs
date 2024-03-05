using System.IO;
using OpenCvSharp;
using System.Linq;
using System.Windows;
using Newtonsoft.Json;
using System.Windows.Media;
using System.Windows.Input;
using Path = System.IO.Path;
using System.Windows.Shapes;
using System.Windows.Controls;
using OpenCvSharp.WpfExtensions;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using OpenCvRect = OpenCvSharp.Rect;
using OpenCvSize = OpenCvSharp.Size;
using OpenCvPoint = OpenCvSharp.Point;
using WindowsPoint = System.Windows.Point;
using System;

namespace RayBin.FishImageLabel
{
    public partial class ImageLabel : Page
    {
        #region 初始化页
        private void InitPage()
        {
            //着色算法
            ColoringAlgorithmCombox.SelectedIndex = 0;
            //最低阈值
            if (Properties.Settings.Default.LowerColor > 0)
            {
                LowerColorSlider.Value = (double)Properties.Settings.Default.LowerColor;
                LowerColorInteger.Value = Properties.Settings.Default.LowerColor;
            }
            if (Properties.Settings.Default.UpperColor > 0)
            {
                UpperColorInteger.Value = Properties.Settings.Default.UpperColor;
            }
            sizeLabel.Visibility = Visibility.Hidden;
            CanvasMain.Children.Add(sizeLabel);
        }
        #endregion

        #region 加载图片
        public void LoadImage(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) { return; }
            ClearSource();
            //读取图片
            Mat src = Cv2.ImRead(fileName, ImreadModes.Color);

            //转灰度
            Mat gary = new Mat();
            Cv2.CvtColor(src, gary, ColorConversionCodes.BGR2GRAY);

            //归一化
            Mat normalized = new Mat();
            Cv2.Normalize(gary, normalized, 0, 255, NormTypes.MinMax, MatType.CV_8U);

            //高斯滤波
            Mat blurred = new Mat();
            int autoDenoise = Properties.Settings.Default.AutoDenoise;
            Cv2.GaussianBlur(normalized, blurred, new OpenCvSize(autoDenoise, autoDenoise), 1.5, 1.5);

            Mat matSrc = blurred.Clone();
            ImageMain.Source = BitmapSourceConverter.ToBitmapSource(src.Clone());
            ImageMain.Width = matSrc.Width;
            ImageMain.Height = matSrc.Height;
            ImageMain.Tag = matSrc;

            blurred.Dispose();
            normalized.Dispose();
            gary.Dispose();
            src.Dispose();

            if (WindowSizeCheckBox.IsChecked == true)
            {
                RoutedEventArgs newEventArgs = new RoutedEventArgs(System.Windows.Controls.CheckBox.CheckedEvent);
                WindowSizeCheckBox.RaiseEvent(newEventArgs);
            }
            string jsonFileName = System.IO.Path.GetFileNameWithoutExtension(CurrentFile) + ".json";
            jsonFileName = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(CurrentFile), jsonFileName);
            if (System.IO.File.Exists(jsonFileName))
            {
                LoadPolygonsFromJson(jsonFileName);
            }
            else
            {
                ImageColoring(fileName);
            }
            ZoomResult(100);
        }
        #endregion

        #region 绘制标注
        public void ImageColoring(string fileName)
        {
            ComboBoxItem selectedItem = ColoringAlgorithmCombox.SelectedItem as ComboBoxItem;
            if (FishCommon.IsHandleFile(fileName) && ImageMain.Tag is Mat matSrc)
            {
                Scalar color = FishCommon.ConvertFileNameToScalar(fileName);
                double lowerValue = (double)LowerColorInteger.Value;
                double upperValue = (double)UpperColorInteger.Value;
                List<PointCollection> listPointCollection = new List<PointCollection>();
                switch (selectedItem.Tag.ToString())
                {
                    case "ColorRangeLabel":
                        listPointCollection = GrayRangeAlgorithm.Coloring(matSrc, lowerValue, upperValue);
                        Coloring(listPointCollection);
                        break;
                    case "AuroColoringLabel":
                        listPointCollection = AuroColoringAlgorithm.Coloring(matSrc, color);
                        Coloring(listPointCollection);
                        break;
                }
            }
        }

        private void Coloring(List<PointCollection> listPointCollection)
        {
            var Polygons = CanvasMain.Children.OfType<Polygon>().ToList();
            foreach (Polygon polygon in Polygons)  { CanvasMain.Children.Remove(polygon); }
            int width = Properties.Settings.Default.BorderWidth;
            Color borderColor = Properties.Settings.Default.BorderColor;
            Color fillColor = Properties.Settings.Default.FillColor;
            foreach (PointCollection points in listPointCollection)
            {
                Polygon polygon = new Polygon { Stroke = new SolidColorBrush(borderColor), Fill = new SolidColorBrush(fillColor), StrokeThickness = width, Points = points };
                polygon.MouseLeftButtonDown += PolygonMouseLeftButtonDown;
                CanvasMain.Children.Add(polygon);
            }
        }
        #endregion

        #region 图片缩放
        private void ZoomResult(double zoomValue)
        {
            double scale = zoomValue / 100;

            TransformGroup group = CanvasMain.RenderTransform as TransformGroup;
            TranslateTransform translateTransform = group.Children[0] as TranslateTransform;
            ScaleTransform scaleTransform = group.Children[1] as ScaleTransform;

            scaleTransform.ScaleX = scale;
            scaleTransform.ScaleY = scale;
            scaleTransform.CenterX = 0;
            scaleTransform.CenterY = 0;
            translateTransform.X = 0;
            translateTransform.Y = 0;

            CanvasMain.Width = ImageMain.Width * scale;
            CanvasMain.Height = ImageMain.Height * scale;
        }
        #endregion

        #region 重置平移变换
        private void ResetTranslateTransform()
        {
            if (CanvasMain.RenderTransform is TransformGroup group)
            {
                if (group.Children[0] is TranslateTransform canvasTransform)
                {
                    canvasTransform.X = 0;
                    canvasTransform.Y = 0;
                }
            }
        }
        #endregion

        #region 左侧大树文件切换时清除一些资源
        public void ClearSource() 
        {
            List<UIElement> elementsToRemove = new List<UIElement>();
            foreach (UIElement child in CanvasMain.Children)
            {
                if (child is Polygon)
                {
                    elementsToRemove.Add(child);
                }
            }
            foreach (var child in elementsToRemove)
            {
                CanvasMain.Children.Remove(child);
            }
            CanvasMain.Cursor = Cursors.Arrow;
            Mouse.OverrideCursor = null;
            dragStart = 0;
        }
        #endregion

        #region 画切割线
        private void DrawCuttingLine(int imageWidth, int imageHeight)
        {
            // 设置虚线的样式
            DoubleCollection dashes = new DoubleCollection() { 2, 2 };
            int cellSize = Properties.Settings.Default.AutoColoringBlockSize;

            // 计算需要额外添加线条的位置
            int extraVerticalLinePos = imageWidth - (imageWidth % cellSize);
            int extraHorizontalLinePos = imageHeight - (imageHeight % cellSize);
            // 绘制垂直虚线
            for (int x = 0; x <= imageWidth; x += cellSize)
            {
                // 检查是否为最后一段且宽度不足
                Line verticalLine = new Line()
                {
                    X1 = x,
                    Y1 = 0,
                    X2 = x,
                    Y2 = imageHeight,
                    Stroke = Brushes.White,
                    StrokeThickness = 1,
                    StrokeDashArray = dashes,
                };
                verticalLine.Tag = "CuttingLine";
                CanvasMain.Children.Add(verticalLine);

                // 补偿线如果是最后一次迭代且图像宽度不能被cellSize整除，额外添加一条线
                if (x == extraVerticalLinePos && imageWidth % cellSize != 0)
                {
                    Line extraVerticalLine = new Line()
                    {
                        X1 = imageWidth - cellSize,
                        Y1 = 0,
                        X2 = imageWidth - cellSize,
                        Y2 = imageHeight,
                        Stroke = Brushes.White,
                        StrokeThickness = 1,
                        StrokeDashArray = dashes,
                    };
                    extraVerticalLine.Tag = "CuttingLine";
                    CanvasMain.Children.Add(extraVerticalLine);
                }
            }

            // 绘制水平虚线
            for (int y = 0; y <= imageHeight; y += cellSize)
            {
                // 检查是否为最后一段且高度不足
                Line horizontalLine = new Line()
                {
                    X1 = 0,
                    Y1 = y,
                    X2 = imageWidth,
                    Y2 = y,
                    Stroke = Brushes.White,
                    StrokeThickness = 1,
                    StrokeDashArray = dashes,
                };
                horizontalLine.Tag = "CuttingLine";
                CanvasMain.Children.Add(horizontalLine);
                
                // 补偿线如果是最后一次迭代且图像高度不能被cellSize整除，额外添加一条线
                if (y == extraHorizontalLinePos && imageHeight % cellSize != 0)
                {
                    Line extraHorizontalLine = new Line()
                    {
                        X1 = 0,
                        Y1 = imageHeight - cellSize,
                        X2 = imageWidth,
                        Y2 = imageHeight - cellSize,
                        Stroke = Brushes.White,
                        StrokeThickness = 1,
                        StrokeDashArray = dashes,
                    };
                    extraHorizontalLine.Tag = "CuttingLine";
                    CanvasMain.Children.Add(extraHorizontalLine);
                }
            }


        }
        #endregion

        #region 导出训练图片
        //导出分割 Mask 与原图
        public void ExportTrainingImage()
        {
            Mat matMask = ImageMain.Tag as Mat;
            Mat matSrc = BitmapSourceConverter.ToMat(ImageMain.Source as BitmapSource);
            List <Polygon> polygons = CanvasMain.Children.OfType<Polygon>().ToList();
            if (polygons == null || polygons.Count == 0) { return; }
            // 加载原始大图
            Mat originalImage = matMask.Clone();
            int originalWidth = originalImage.Width;
            int originalHeight = originalImage.Height;

            // 创建一个与原图相同大小的mask，初始全黑
            Mat mask = Mat.Zeros(originalHeight, originalWidth, MatType.CV_8UC1);

            // 绘制Polygon到mask上，您需要实现Polygon到mask坐标转换的逻辑
            foreach (var polygon in polygons)
            {
                OpenCvPoint[][] contours = FishCommon.ConvertPolygonToContours(polygon);
                Cv2.FillPoly(mask, contours, Scalar.White);
            }
            int cellSize = Properties.Settings.Default.AutoColoringBlockSize;
            // 修改后的逻辑确保处理了不能被cellSize整除的情况
            int cols = (originalWidth + cellSize - 1) / cellSize;
            int rows = (originalHeight + cellSize - 1) / cellSize;

            string filePath = Path.GetDirectoryName(CurrentFile);
            string fileName = Path.GetFileNameWithoutExtension(CurrentFile);
            string maskPath = Path.Combine(filePath, fileName + "_Mask");
            string sourcePath = Path.Combine(filePath, fileName + "_Source");
            Directory.CreateDirectory(maskPath);
            Directory.CreateDirectory(sourcePath);
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    // 计算当前块的实际大小，处理最后一行或列的情况
                    int currentWidth = Math.Min(cellSize, originalWidth - j * cellSize);
                    int currentHeight = Math.Min(cellSize, originalHeight - i * cellSize);

                    // 裁剪Mask图并创建ROI
                    OpenCvRect roi = new OpenCvRect(j * cellSize, i * cellSize, currentWidth, currentHeight);
                    Mat croppedMask = new Mat(mask, roi);

                    // 裁剪原图
                    Mat croppedSrc = new Mat(matSrc, roi);

                    // 保存小图块mask
                    croppedMask.SaveImage( maskPath + $"\\mask_{i}_{j}.png");
                    croppedSrc.SaveImage(sourcePath + $"\\src_{i}_{j}.png");
                }
            }
        }
        #endregion

        #region 读取并添加已保存的标注
        private void LoadPolygonsFromJson(string fileFullName)
        {
            // 读取JSON字符串
            string json = System.IO.File.ReadAllText(fileFullName);

            // 反序列化到中间表示
            var listPoints = JsonConvert.DeserializeObject<List<List<WindowsPoint>>>(json);

            if (listPoints == null) return;

            // 清除Canvas上现有的Polygons
            var polygons = CanvasMain.Children.OfType<Polygon>().ToList();
            foreach (var polygon in polygons)  { CanvasMain.Children.Remove(polygon); }

            int width = Properties.Settings.Default.BorderWidth;
            Color borderColor = Properties.Settings.Default.BorderColor;
            Color fillColor = Properties.Settings.Default.FillColor;

            // 将每个List<Point>转换为PointCollection，并创建Polygon
            foreach (var pointsList in listPoints)
            {
                var pointCollection = new PointCollection();
                foreach (var point in pointsList) { pointCollection.Add(point); }
                var polygon = new Polygon  {  Stroke = new SolidColorBrush(borderColor), Fill = new SolidColorBrush(fillColor), StrokeThickness = width, Points = pointCollection,};
                polygon.MouseLeftButtonDown += PolygonMouseLeftButtonDown;
                CanvasMain.Children.Add(polygon);
            }
        }
        #endregion
    }
}
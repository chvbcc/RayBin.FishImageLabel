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
        #region ��ʼ��ҳ
        private void InitPage()
        {
            //��ɫ�㷨
            ColoringAlgorithmCombox.SelectedIndex = 0;
            //�����ֵ
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

        #region ����ͼƬ
        public void LoadImage(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) { return; }
            ClearSource();
            //��ȡͼƬ
            Mat src = Cv2.ImRead(fileName, ImreadModes.Color);

            //ת�Ҷ�
            Mat gary = new Mat();
            Cv2.CvtColor(src, gary, ColorConversionCodes.BGR2GRAY);

            //��һ��
            Mat normalized = new Mat();
            Cv2.Normalize(gary, normalized, 0, 255, NormTypes.MinMax, MatType.CV_8U);

            //��˹�˲�
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

        #region ���Ʊ�ע
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

        #region ͼƬ����
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

        #region ����ƽ�Ʊ任
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

        #region �������ļ��л�ʱ���һЩ��Դ
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

        #region ���и���
        private void DrawCuttingLine(int imageWidth, int imageHeight)
        {
            // �������ߵ���ʽ
            DoubleCollection dashes = new DoubleCollection() { 2, 2 };
            int cellSize = Properties.Settings.Default.AutoColoringBlockSize;

            // ������Ҫ�������������λ��
            int extraVerticalLinePos = imageWidth - (imageWidth % cellSize);
            int extraHorizontalLinePos = imageHeight - (imageHeight % cellSize);
            // ���ƴ�ֱ����
            for (int x = 0; x <= imageWidth; x += cellSize)
            {
                // ����Ƿ�Ϊ���һ���ҿ�Ȳ���
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

                // ��������������һ�ε�����ͼ���Ȳ��ܱ�cellSize�������������һ����
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

            // ����ˮƽ����
            for (int y = 0; y <= imageHeight; y += cellSize)
            {
                // ����Ƿ�Ϊ���һ���Ҹ߶Ȳ���
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
                
                // ��������������һ�ε�����ͼ��߶Ȳ��ܱ�cellSize�������������һ����
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

        #region ����ѵ��ͼƬ
        //�����ָ� Mask ��ԭͼ
        public void ExportTrainingImage()
        {
            Mat matMask = ImageMain.Tag as Mat;
            Mat matSrc = BitmapSourceConverter.ToMat(ImageMain.Source as BitmapSource);
            List <Polygon> polygons = CanvasMain.Children.OfType<Polygon>().ToList();
            if (polygons == null || polygons.Count == 0) { return; }
            // ����ԭʼ��ͼ
            Mat originalImage = matMask.Clone();
            int originalWidth = originalImage.Width;
            int originalHeight = originalImage.Height;

            // ����һ����ԭͼ��ͬ��С��mask����ʼȫ��
            Mat mask = Mat.Zeros(originalHeight, originalWidth, MatType.CV_8UC1);

            // ����Polygon��mask�ϣ�����Ҫʵ��Polygon��mask����ת�����߼�
            foreach (var polygon in polygons)
            {
                OpenCvPoint[][] contours = FishCommon.ConvertPolygonToContours(polygon);
                Cv2.FillPoly(mask, contours, Scalar.White);
            }
            int cellSize = Properties.Settings.Default.AutoColoringBlockSize;
            // �޸ĺ���߼�ȷ�������˲��ܱ�cellSize���������
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
                    // ���㵱ǰ���ʵ�ʴ�С���������һ�л��е����
                    int currentWidth = Math.Min(cellSize, originalWidth - j * cellSize);
                    int currentHeight = Math.Min(cellSize, originalHeight - i * cellSize);

                    // �ü�Maskͼ������ROI
                    OpenCvRect roi = new OpenCvRect(j * cellSize, i * cellSize, currentWidth, currentHeight);
                    Mat croppedMask = new Mat(mask, roi);

                    // �ü�ԭͼ
                    Mat croppedSrc = new Mat(matSrc, roi);

                    // ����Сͼ��mask
                    croppedMask.SaveImage( maskPath + $"\\mask_{i}_{j}.png");
                    croppedSrc.SaveImage(sourcePath + $"\\src_{i}_{j}.png");
                }
            }
        }
        #endregion

        #region ��ȡ������ѱ���ı�ע
        private void LoadPolygonsFromJson(string fileFullName)
        {
            // ��ȡJSON�ַ���
            string json = System.IO.File.ReadAllText(fileFullName);

            // �����л����м��ʾ
            var listPoints = JsonConvert.DeserializeObject<List<List<WindowsPoint>>>(json);

            if (listPoints == null) return;

            // ���Canvas�����е�Polygons
            var polygons = CanvasMain.Children.OfType<Polygon>().ToList();
            foreach (var polygon in polygons)  { CanvasMain.Children.Remove(polygon); }

            int width = Properties.Settings.Default.BorderWidth;
            Color borderColor = Properties.Settings.Default.BorderColor;
            Color fillColor = Properties.Settings.Default.FillColor;

            // ��ÿ��List<Point>ת��ΪPointCollection��������Polygon
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
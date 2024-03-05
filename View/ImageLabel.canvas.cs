using System;
using OpenCvSharp;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
using OpenCvSharp.WpfExtensions;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using OpenCvPoint = OpenCvSharp.Point;
using WindowsPoint = System.Windows.Point;
using System.IO;

namespace RayBin.FishImageLabel
{
    /// <summary>
    /// LogList.xaml �Ľ����߼�
    /// </summary>
    public partial class ImageLabel : Page
    {
        //������ ��ƽ��ͼƬ
        private int dragStart = 0; // 1�����϶� 2ѡ���Ŵ� 3������Ʊ�ע 4�߶λ��Ʊ�ע 5�ֶ����Ʊ�ע 6��עѡ��
        private WindowsPoint startPoint;
        private WindowsPoint endPoint;
        private Rectangle rectangle;
        private Polygon selectedPolygon;
        private List<WindowsPoint> points = new List<WindowsPoint>();
        private Line tempLine = new Line(); // ����Ԥ������ʱ��
        private int width = Properties.Settings.Default.BorderWidth;
        private Color borderColor = Properties.Settings.Default.BorderColor;
        #region ѡ���Ŵ�
        public void SelectedAreaScaling()
        {
            if (ImageMain == null || ImageMain.Source == null) { return; }
            CanvasMain.Cursor = Cursors.Cross;
            dragStart = 2;
        }
        #endregion

        #region ������Ʊ�ע
        public void SinglePointDrawLabel() 
        { 
            if (ImageMain == null || ImageMain.Source == null) { return; }
            CanvasMain.Cursor = Cursors.Cross;
            dragStart = 3;
        }
        #endregion

        #region �������Ʊ�ע
        public void LineDrawLabel() 
        { 
            if (ImageMain == null || ImageMain.Source == null) { return; }
            int count = CanvasMain.Children.OfType<Line>().Count(line => line.Name == "tempLine");
            if (count == 0)
            {
                tempLine.Name = "tempLine";
                tempLine.Stroke = new SolidColorBrush(borderColor);
                tempLine.StrokeThickness = width;
                tempLine.Visibility = Visibility.Hidden; // ��ʼ����ʾ
                CanvasMain.Children.Add(tempLine);
            }
            CanvasMain.Cursor = Cursors.Cross;
            dragStart = 4;
        }
        #endregion

        #region �ֶ����Ʊ�ע
        public void ManualDrawLabel()
        {
            if (ImageMain == null || ImageMain.Source == null) { return; }
            CanvasMain.Cursor = Cursors.Cross;
            dragStart = 5;
        }
        #endregion

        #region ȡ�����л���
        public void DrawCancel()
        {
            sizeLabel.Visibility = Visibility.Hidden;
            CanvasMain.Cursor = Cursors.Arrow;
            dragStart = 0;
        }
        #endregion

        #region ������갴��
        private void CanvasMain_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Canvas canvasMain = sender as Canvas;
            startPoint = e.GetPosition(canvasMain);

            //����ڱ�ע��
            if (IsMouseOverPolygon(startPoint)) { dragStart = 6; return; }
            //
            //ѡ���Ŵ�
            if (dragStart == 2)
            {
                rectangle = new Rectangle { Stroke = Brushes.Yellow, StrokeThickness = 2D, Fill = Brushes.Transparent, StrokeDashArray = new DoubleCollection { 3D } };
                // ���þ��ε���ʼ��
                Canvas.SetLeft(rectangle, startPoint.X);
                Canvas.SetTop(rectangle, startPoint.Y);
                canvasMain.Children.Add(rectangle);
                canvasMain.CaptureMouse();
                return;
            }
            //������Ʊ�ע
            else if (dragStart == 3)
            {
                OpenCvPoint cvPoint = new OpenCvPoint(startPoint.X, startPoint.Y);
                Mat mat = BitmapSourceConverter.ToMat(ImageMain.Source as BitmapSource);
                AutoAdjacentBrightness autoAdjacentBrightness = new AutoAdjacentBrightness(mat);
                List<OpenCvPoint> listPoint = autoAdjacentBrightness.FindInterestBoundary(cvPoint);
                PointCollection pointCollection = new PointCollection();
                foreach (var point in listPoint)
                {
                    // ��OpenCvSharp.Pointת��ΪSystem.Windows.Point
                    WindowsPoint wpfPoint = new WindowsPoint(point.X, point.Y);
                    pointCollection.Add(wpfPoint);
                }
                Polygon polygon = new Polygon { Stroke = new SolidColorBrush(borderColor), StrokeThickness = width, Fill = Brushes.Transparent, Points = pointCollection };
                polygon.MouseLeftButtonDown += PolygonMouseLeftButtonDown;
                CanvasMain.Children.Add(polygon);
            }
            //���߱�ע
            else if (dragStart == 4) { return; }
            //�ֶ����Ʊ�ע
            else if (dragStart == 5)
            {
                points.Clear();         // ���֮ǰ�ĵ�
                points.Add(startPoint); // ��ӵ�һ����
                return;
            }
            //�����϶�(�ų�����ֻʣ���϶���)
            else
            {
                dragStart = 1;
                canvasMain.CaptureMouse();
                canvasMain.Cursor = Cursors.SizeAll;
            }
        }
        #endregion

        #region ��������ƶ�
        private void CanvasMain_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Canvas canvasMain = sender as Canvas;
            endPoint = e.GetPosition(canvasMain);
            try
            {
                //�����϶�
                if (dragStart == 1 && e.LeftButton == MouseButtonState.Pressed)
                {
                    TransformGroup group = canvasMain.RenderTransform as TransformGroup;
                    var newGroup = group.CloneCurrentValue();
                    if (newGroup == null) { return; }
                    TranslateTransform canvasTransform = newGroup.Children[0] as TranslateTransform;
                    var offsetX = endPoint.X - startPoint.X;
                    var offsetY = endPoint.Y - startPoint.Y;
                    canvasTransform.X += offsetX;
                    canvasTransform.Y += offsetY;
                    canvasMain.RenderTransform = newGroup;
                }
                //ѡ���Ŵ�
                else if (dragStart == 2 && e.LeftButton == MouseButtonState.Pressed)
                {
                    // ���¾��εĴ�С
                    double width = Math.Abs(endPoint.X - startPoint.X);
                    double height = Math.Abs(endPoint.Y - startPoint.Y);
                    rectangle.Width = width;
                    rectangle.Height = height;
                    sizeLabel.Content = $"{width}px, {height}px";
                    sizeLabel.Visibility = Visibility.Visible;
                    Canvas.SetLeft(sizeLabel, endPoint.X);
                    Canvas.SetTop(sizeLabel, endPoint.Y - sizeLabel.ActualHeight - 2);
                    // ���¾��ε�λ��
                    Canvas.SetLeft(rectangle, Math.Min(startPoint.X, endPoint.X));
                    Canvas.SetTop(rectangle, Math.Min(startPoint.Y, endPoint.Y));
                }
                //���߱�ע
                else if (dragStart == 4 && points.Count > 0) 
                {
                    tempLine.X1 = points[points.Count-1].X;
                    tempLine.Y1 = points[points.Count-1].Y;
                    tempLine.X2 = endPoint.X;
                    tempLine.Y2 = endPoint.Y;
                    tempLine.Visibility = Visibility.Visible;
                    var distance = Math.Sqrt(Math.Pow(endPoint.X - points[0].X, 2) + Math.Pow(endPoint.Y - points[0].Y, 2));
                    if (distance < 2)
                    {
                        canvasMain.Cursor = Cursors.Pen;
                    }
                    else 
                    {
                        canvasMain.Cursor = Cursors.Cross;
                    }
                }
                //�ֶ����Ʊ�ע
                else if (dragStart == 5 && e.LeftButton == MouseButtonState.Pressed)
                {
                    points.Add(endPoint); // ����µĵ�                    
                    if (points.Count > 1) // ���Ƶ�Canvas�ϣ���ѡ����Ϊ�˿��ӻ���
                    {
                        var prevPoint = points[points.Count - 2];
                        Line line = new Line { Stroke = new SolidColorBrush(borderColor), StrokeThickness = width, X1 = prevPoint.X, Y1 = prevPoint.Y, X2 = endPoint.X, Y2 = endPoint.Y };
                        canvasMain.Children.Add(line);
                    }
                }
            }        
            catch (Exception ex)
            {
                Common.WriteExceptionLog(ex);
            }
        }
        #endregion

        #region �������̧��
        private void CanvasMain_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {            
            try
            {                
                Canvas canvasMain = sender as Canvas;
                //�϶��϶�
                if (dragStart == 1)
                {
                    canvasMain.ReleaseMouseCapture();
                    canvasMain.Cursor = Cursors.Arrow;
                    dragStart = 0;
                }
                //ѡ���Ŵ�
                else if (dragStart == 2)
                {
                    canvasMain.ReleaseMouseCapture();
                    canvasMain.Cursor = Cursors.Arrow;
                    sizeLabel.Visibility = Visibility.Hidden;
                    dragStart = 0;
                    //����Ŵ���������ĵ�
                    // ��ȡRectangle��Canvas�ϵ�λ��
                    double rectLeft = Canvas.GetLeft(rectangle);
                    double rectTop = Canvas.GetTop(rectangle);
                    // ����ѡ�������ĵ�
                    double centerX = rectLeft + rectangle.Width / 2;
                    double centerY = rectTop + rectangle.Height / 2;
                    TransformGroup group = CanvasMain.RenderTransform as TransformGroup;
                    ScaleTransform scaleTransform = group.Children[1] as ScaleTransform;
                    scaleTransform.CenterX = startPoint.X;
                    scaleTransform.CenterY = startPoint.Y;
                    scaleTransform.ScaleX = Properties.Settings.Default.SelectedAreaScaling;
                    scaleTransform.ScaleY = Properties.Settings.Default.SelectedAreaScaling;
                    CanvasMain.Children.Remove(rectangle);
                }
                //�߶λ��Ʊ�ע
                else if (dragStart == 4)
                {                        
                    WindowsPoint mousePoint = e.GetPosition(canvasMain);
                    points.Add(mousePoint); // ��ӵ�һ����
                    if (points.Count > 1 && canvasMain.Cursor == Cursors.Pen) 
                    { 
                        // ������պ��߶β�ת���ɶ����
                        LineClear(canvasMain);
                        tempLine.Visibility = Visibility.Hidden;
                        Polygon polygon = new Polygon { Stroke = new SolidColorBrush(borderColor), StrokeThickness = width, Fill = Brushes.Transparent };
                        polygon.MouseLeftButtonDown += PolygonMouseLeftButtonDown;
                        foreach (var point in points) { polygon.Points.Add(point); }
                        canvasMain.Children.Add(polygon);
                        Canvas.SetTop(polygon, -1);
                        Canvas.SetLeft(polygon, -1);
                        selectedPolygon = polygon;
                        SelectedLabel(polygon);
                        points.Clear();
                        canvasMain.Cursor = Cursors.Cross;
                    }
                    else
                    {
                        //���Ż���
                        Line line = new Line { Stroke = new SolidColorBrush(borderColor), StrokeThickness = width, X1 = points[points.Count - 2].X, Y1 = points[points.Count - 2].Y, X2 = mousePoint.X, Y2 = mousePoint.Y };
                        canvasMain.Children.Add(line);
                    }
                }
                //�ֶ����Ʊ�ע
                else if (dragStart == 5)
                {                    
                    LineClear(canvasMain);
                    Polygon polygon = new Polygon { Stroke = new SolidColorBrush(borderColor), StrokeThickness = width, Fill = Brushes.Transparent };
                    polygon.MouseLeftButtonDown += PolygonMouseLeftButtonDown;
                    foreach (var point in points) { polygon.Points.Add(point); }
                    canvasMain.Children.Add(polygon);
                    selectedPolygon = polygon;
                    SelectedLabel(polygon);
                    points.Clear();
                }
            }
            catch (Exception ex)
            {
                Common.WriteExceptionLog(ex);
            }
        }
        #endregion

        #region �������뿪����
        private void CanvasMain_MouseEnter(object sender, MouseEventArgs e)
        {
            if (dragStart == 2 || dragStart == 3 || dragStart == 4 || dragStart == 5) 
            {
                CanvasMain.Cursor = Cursors.Cross;
            }
        }

        private void CanvasMain_MouseLeave(object sender, MouseEventArgs e)
        {
            CanvasMain.Cursor = Cursors.Arrow;
        }
        #endregion

        #region ��עѡ��
        private void PolygonMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            selectedPolygon = sender as Polygon;
            SelectedLabel(selectedPolygon);
        }

        private bool IsMouseOverPolygon(WindowsPoint mousePos)
        {
            var hitTestParams = new PointHitTestParameters(mousePos);
            var result = false;

            // HitTest�ص�����
            HitTestResultCallback callback = (HitTestResult hit) =>
            {
                if (hit.VisualHit is Polygon)
                {
                    result = true;
                    return HitTestResultBehavior.Stop; // �ҵ���ֹͣ����
                }
                return HitTestResultBehavior.Continue;
            };

            // ��CanvasMainִ��HitTest
            VisualTreeHelper.HitTest(CanvasMain, null, callback, hitTestParams);
            return result;
        }

        private void SelectedLabel(Polygon selectedPolygon) 
        {
            if (selectedPolygon == null) { return; }
            selectedPolygon.Stroke = Brushes.Yellow;
            var Polygons = CanvasMain.Children.OfType<Polygon>().ToList();
            Polygons.Remove(selectedPolygon);
            foreach (Polygon polygon in Polygons)
            {
                polygon.Stroke = new SolidColorBrush(borderColor);
            }
        }
        #endregion

        #region ��עɾ��
        public void DeleteLabel() 
        {
            if (selectedPolygon == null) { return; }
            CanvasMain.Children.Remove(selectedPolygon);
        }
        #endregion

        #region �����ʱ��ǵ��߶�
        private void LineClear(Canvas canvas) 
        {
            var Lines = canvas.Children.OfType<Line>().ToList().Where(line => line.Name != "tempLine").ToList();
            foreach (Line line in Lines)
            {
                canvas.Children.Remove(line);        
            }
        }
        #endregion
    }
}
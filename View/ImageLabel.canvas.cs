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
    /// LogList.xaml 的交互逻辑
    /// </summary>
    public partial class ImageLabel : Page
    {
        //画矩形 》平移图片
        private int dragStart = 0; // 1画布拖动 2选区放大 3单点绘制标注 4线段绘制标注 5手动绘制标注 6标注选中
        private WindowsPoint startPoint;
        private WindowsPoint endPoint;
        private Rectangle rectangle;
        private Polygon selectedPolygon;
        private List<WindowsPoint> points = new List<WindowsPoint>();
        private Line tempLine = new Line(); // 用于预览的临时线
        private int width = Properties.Settings.Default.BorderWidth;
        private Color borderColor = Properties.Settings.Default.BorderColor;
        #region 选区放大
        public void SelectedAreaScaling()
        {
            if (ImageMain == null || ImageMain.Source == null) { return; }
            CanvasMain.Cursor = Cursors.Cross;
            dragStart = 2;
        }
        #endregion

        #region 单点绘制标注
        public void SinglePointDrawLabel() 
        { 
            if (ImageMain == null || ImageMain.Source == null) { return; }
            CanvasMain.Cursor = Cursors.Cross;
            dragStart = 3;
        }
        #endregion

        #region 线条绘制标注
        public void LineDrawLabel() 
        { 
            if (ImageMain == null || ImageMain.Source == null) { return; }
            int count = CanvasMain.Children.OfType<Line>().Count(line => line.Name == "tempLine");
            if (count == 0)
            {
                tempLine.Name = "tempLine";
                tempLine.Stroke = new SolidColorBrush(borderColor);
                tempLine.StrokeThickness = width;
                tempLine.Visibility = Visibility.Hidden; // 初始不显示
                CanvasMain.Children.Add(tempLine);
            }
            CanvasMain.Cursor = Cursors.Cross;
            dragStart = 4;
        }
        #endregion

        #region 手动绘制标注
        public void ManualDrawLabel()
        {
            if (ImageMain == null || ImageMain.Source == null) { return; }
            CanvasMain.Cursor = Cursors.Cross;
            dragStart = 5;
        }
        #endregion

        #region 取消所有绘制
        public void DrawCancel()
        {
            sizeLabel.Visibility = Visibility.Hidden;
            CanvasMain.Cursor = Cursors.Arrow;
            dragStart = 0;
        }
        #endregion

        #region 画布鼠标按下
        private void CanvasMain_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Canvas canvasMain = sender as Canvas;
            startPoint = e.GetPosition(canvasMain);

            //鼠标在标注内
            if (IsMouseOverPolygon(startPoint)) { dragStart = 6; return; }
            //
            //选区放大
            if (dragStart == 2)
            {
                rectangle = new Rectangle { Stroke = Brushes.Yellow, StrokeThickness = 2D, Fill = Brushes.Transparent, StrokeDashArray = new DoubleCollection { 3D } };
                // 设置矩形的起始点
                Canvas.SetLeft(rectangle, startPoint.X);
                Canvas.SetTop(rectangle, startPoint.Y);
                canvasMain.Children.Add(rectangle);
                canvasMain.CaptureMouse();
                return;
            }
            //单点绘制标注
            else if (dragStart == 3)
            {
                OpenCvPoint cvPoint = new OpenCvPoint(startPoint.X, startPoint.Y);
                Mat mat = BitmapSourceConverter.ToMat(ImageMain.Source as BitmapSource);
                AutoAdjacentBrightness autoAdjacentBrightness = new AutoAdjacentBrightness(mat);
                List<OpenCvPoint> listPoint = autoAdjacentBrightness.FindInterestBoundary(cvPoint);
                PointCollection pointCollection = new PointCollection();
                foreach (var point in listPoint)
                {
                    // 将OpenCvSharp.Point转换为System.Windows.Point
                    WindowsPoint wpfPoint = new WindowsPoint(point.X, point.Y);
                    pointCollection.Add(wpfPoint);
                }
                Polygon polygon = new Polygon { Stroke = new SolidColorBrush(borderColor), StrokeThickness = width, Fill = Brushes.Transparent, Points = pointCollection };
                polygon.MouseLeftButtonDown += PolygonMouseLeftButtonDown;
                CanvasMain.Children.Add(polygon);
            }
            //画线标注
            else if (dragStart == 4) { return; }
            //手动绘制标注
            else if (dragStart == 5)
            {
                points.Clear();         // 清除之前的点
                points.Add(startPoint); // 添加第一个点
                return;
            }
            //画布拖动(排除以上只剩下拖动了)
            else
            {
                dragStart = 1;
                canvasMain.CaptureMouse();
                canvasMain.Cursor = Cursors.SizeAll;
            }
        }
        #endregion

        #region 画布鼠标移动
        private void CanvasMain_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Canvas canvasMain = sender as Canvas;
            endPoint = e.GetPosition(canvasMain);
            try
            {
                //画布拖动
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
                //选区放大
                else if (dragStart == 2 && e.LeftButton == MouseButtonState.Pressed)
                {
                    // 更新矩形的大小
                    double width = Math.Abs(endPoint.X - startPoint.X);
                    double height = Math.Abs(endPoint.Y - startPoint.Y);
                    rectangle.Width = width;
                    rectangle.Height = height;
                    sizeLabel.Content = $"{width}px, {height}px";
                    sizeLabel.Visibility = Visibility.Visible;
                    Canvas.SetLeft(sizeLabel, endPoint.X);
                    Canvas.SetTop(sizeLabel, endPoint.Y - sizeLabel.ActualHeight - 2);
                    // 更新矩形的位置
                    Canvas.SetLeft(rectangle, Math.Min(startPoint.X, endPoint.X));
                    Canvas.SetTop(rectangle, Math.Min(startPoint.Y, endPoint.Y));
                }
                //画线标注
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
                //手动绘制标注
                else if (dragStart == 5 && e.LeftButton == MouseButtonState.Pressed)
                {
                    points.Add(endPoint); // 添加新的点                    
                    if (points.Count > 1) // 绘制到Canvas上（可选，仅为了可视化）
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

        #region 画布鼠标抬起
        private void CanvasMain_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {            
            try
            {                
                Canvas canvasMain = sender as Canvas;
                //拖动拖动
                if (dragStart == 1)
                {
                    canvasMain.ReleaseMouseCapture();
                    canvasMain.Cursor = Cursors.Arrow;
                    dragStart = 0;
                }
                //选区放大
                else if (dragStart == 2)
                {
                    canvasMain.ReleaseMouseCapture();
                    canvasMain.Cursor = Cursors.Arrow;
                    sizeLabel.Visibility = Visibility.Hidden;
                    dragStart = 0;
                    //计算放大比例和中心点
                    // 获取Rectangle在Canvas上的位置
                    double rectLeft = Canvas.GetLeft(rectangle);
                    double rectTop = Canvas.GetTop(rectangle);
                    // 计算选区的中心点
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
                //线段绘制标注
                else if (dragStart == 4)
                {                        
                    WindowsPoint mousePoint = e.GetPosition(canvasMain);
                    points.Add(mousePoint); // 添加第一个点
                    if (points.Count > 1 && canvasMain.Cursor == Cursors.Pen) 
                    { 
                        // 在这里闭合线段并转换成多边形
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
                        //接着画线
                        Line line = new Line { Stroke = new SolidColorBrush(borderColor), StrokeThickness = width, X1 = points[points.Count - 2].X, Y1 = points[points.Count - 2].Y, X2 = mousePoint.X, Y2 = mousePoint.Y };
                        canvasMain.Children.Add(line);
                    }
                }
                //手动绘制标注
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

        #region 鼠标进入离开画布
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

        #region 标注选中
        private void PolygonMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            selectedPolygon = sender as Polygon;
            SelectedLabel(selectedPolygon);
        }

        private bool IsMouseOverPolygon(WindowsPoint mousePos)
        {
            var hitTestParams = new PointHitTestParameters(mousePos);
            var result = false;

            // HitTest回调函数
            HitTestResultCallback callback = (HitTestResult hit) =>
            {
                if (hit.VisualHit is Polygon)
                {
                    result = true;
                    return HitTestResultBehavior.Stop; // 找到后停止测试
                }
                return HitTestResultBehavior.Continue;
            };

            // 对CanvasMain执行HitTest
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

        #region 标注删除
        public void DeleteLabel() 
        {
            if (selectedPolygon == null) { return; }
            CanvasMain.Children.Remove(selectedPolygon);
        }
        #endregion

        #region 清除临时标记的线段
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
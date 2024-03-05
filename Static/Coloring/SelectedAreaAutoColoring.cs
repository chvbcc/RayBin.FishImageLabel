using OpenCvSharp;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Media;

namespace RayBin.FishImageLabel
{
    public class AutoColoringRect
    {
        #region 选区自动着色
        public static List<PointCollection> RectAutoColoring(Mat matSrc, Rect rect) 
        {
            Mat src = matSrc.Clone();
            Scalar upperScalar = new Scalar(255, 255, 255);
            List<PointCollection> listPointCollection = new List<PointCollection>();
            //创建灰色选区获取最优阈值信息
            Mat roiGray = new Mat(src, rect);
            ThresholdInfo thresholdInfo = GetOptimalThreshold(roiGray);
            if  (thresholdInfo != null) 
            {
                Mat mask = new Mat();
                Cv2.InRange(roiGray, thresholdInfo.LowerScalar, upperScalar, mask);
                // 寻找轮廓
                Cv2.FindContours(mask, out Point[][] contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
                foreach (var contour in contours)
                {
                    PointCollection points = new PointCollection();
                    foreach (var point in contour)
                    {
                        points.Add(new System.Windows.Point(rect.X + point.X, rect.Y + point.Y));
                    }
                    listPointCollection.Add(points);
                }
                mask.Dispose();
            }
            src.Dispose();
            roiGray.Dispose();
            return listPointCollection;
        }
        #endregion

        #region 获取最优阈值算法
        public static ThresholdInfo GetOptimalThreshold(Mat roiGray)
        {
            //查找的颜色
            Scalar lowerScalar = new Scalar(252);
            Scalar upperScalar = new Scalar(255, 255, 255);

            //创建一个形态学的Mat 用于降噪（腐蚀、膨胀、开运算和闭运算）
            Mat kernel = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(3, 3));

            List<ThresholdInfo> thresholdInfos = new List<ThresholdInfo>();
            while (lowerScalar.Val0 >= Properties.Settings.Default.AutoDenoise)
            {
                using (Mat mask = new Mat())
                {
                    lowerScalar.Val0 -= 1;
                    //查找范围内的像素，没找到为 0 结果会保存在mask中
                    Cv2.InRange(roiGray, lowerScalar, upperScalar, mask);

                    //计算mask中非零像素的数量
                    int nonZeroPixelCount = Cv2.CountNonZero(mask);
                    if (nonZeroPixelCount == 0) { continue; }

                    //形态学开运算（Open close）
                    Mat open = new Mat();
                    Mat close = new Mat();                    
                    Cv2.MorphologyEx(mask, open, MorphTypes.Open, kernel);
                    Cv2.MorphologyEx(open, close, MorphTypes.Close, kernel);

                    //连通区域查找
                    Cv2.FindContours(close, out Point[][] contours, out _, RetrievalModes.List, ContourApproximationModes.ApproxNone);
                    ThresholdInfo model = new ThresholdInfo
                    {
                        LowerScalar = lowerScalar.Val0 + 1
                    };
                    foreach (Point[] contour in contours)
                    {
                        double contourArea = Cv2.ContourArea(contour);
                        if (contourArea > Properties.Settings.Default.AutoColoringMinBlockFilter && contourArea <= Properties.Settings.Default.AutoColoringMaxBlockFilter)
                        {
                            model.NormalBlockCount++;
                            model.BlockPoints.Add(contour);
                        }
                        else if (contourArea > Properties.Settings.Default.AutoColoringMaxBlockFilter)
                        {
                            model.BigBlockCount++;
                        }
                    }
                    if (model.NormalBlockCount > 0 || model.BigBlockCount > 0) { thresholdInfos.Add(model); }
                    open.Dispose();
                    close.Dispose();
                }
            }
            kernel.Dispose();
            if (thresholdInfos.Count == 0) { return null; }
            ThresholdInfo thresholdInfo = thresholdInfos.Where(t => t.BigBlockCount == 0).OrderByDescending(o => o.NormalBlockCount).FirstOrDefault();
            if (thresholdInfo == null) { return null; }
            return thresholdInfo;
        }
        #endregion
    }
}
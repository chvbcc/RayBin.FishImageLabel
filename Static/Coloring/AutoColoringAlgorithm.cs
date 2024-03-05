using System;
using OpenCvSharp;
using System.Linq;
using System.Windows.Media;
using System.Collections.Generic;

namespace RayBin.FishImageLabel
{
    public class AuroColoringAlgorithm
    {
        #region 分块着色
        public static List<PointCollection> Coloring(Mat matSrc, Scalar color)
        {
            //图像分块大小
            int blockSize = Properties.Settings.Default.AutoColoringBlockSize;
            Scalar upperScalar = new Scalar(255, 255, 255);
            Mat src = matSrc.Clone();
            Mat result = new Mat(src.Size(), MatType.CV_8UC4, new Scalar(0, 0, 0, 0));
            List<PointCollection> listPointCollection = new List<PointCollection>();
            for (int y = 0; y < src.Rows; y += blockSize)
            {
                for (int x = 0; x < src.Cols; x += blockSize)
                {
                    int actualBlockSizeX = Math.Min(blockSize, src.Cols - x);
                    int actualBlockSizeY = Math.Min(blockSize, src.Rows - y);
                    Rect roi = new Rect(x, y, actualBlockSizeX, actualBlockSizeY);

                    // 在这里执行复制边界逻辑
                    int borderSize = 10; // 你希望复制的边界大小
                    Mat extendedBlock = new Mat();
                    Rect extendedRoi = new Rect(Math.Max(0, roi.X - borderSize), Math.Max(0, roi.Y - borderSize), Math.Min(roi.Width + borderSize * 2, src.Cols - (roi.X - borderSize)), Math.Min(roi.Height + borderSize * 2, src.Rows - (roi.Y - borderSize)));
                    Cv2.CopyMakeBorder(src.SubMat(extendedRoi), extendedBlock, borderSize, borderSize, borderSize, borderSize, BorderTypes.Replicate);

                    //创建灰色选区获取最优阈值信息
                    Mat roiGray =  extendedBlock.Clone();
                    ThresholdInfo thresholdInfo = GetOptimalThreshold(roiGray);
                    if (thresholdInfo == null) { continue; }

                    Mat mask = new Mat();
                    Mat roiSrc = new Mat(src, roi);
                    Cv2.InRange(roiSrc, thresholdInfo.LowerScalar, upperScalar, mask);
                    DilateSmallerArea(mask);

                    // 找轮廓画轮廓
                    Cv2.FindContours(mask, out Point[][] contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
                    foreach (var contour in contours)
                    {
                        PointCollection points = new PointCollection();
                        foreach (var point in contour)
                        {
                            points.Add(new System.Windows.Point(roi.X + point.X, roi.Y + point.Y));
                        }
                        listPointCollection.Add(points);
                    }
                    //释放资源  
                    //roiView.Dispose();
                    roiSrc.Dispose();
                    mask.Dispose();
                    roiGray.Dispose();
                    extendedBlock.Dispose();
                }
            }
            result.Dispose();
            src.Dispose();
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

                    //连通区域查找
                    Cv2.FindContours(mask, out Point[][] contours, out _, RetrievalModes.List, ContourApproximationModes.ApproxNone);
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
                        }
                        else if (contourArea > Properties.Settings.Default.AutoColoringMaxBlockFilter)
                        {
                            model.BigBlockCount++;
                        }
                    }
                    if (model.NormalBlockCount > 0 || model.BigBlockCount > 0) { thresholdInfos.Add(model); }
                }
            }
            if (thresholdInfos.Count == 0) { return null; }
            ThresholdInfo thresholdInfo = thresholdInfos.Where(t => t.BigBlockCount == 0).OrderByDescending(o => o.NormalBlockCount).FirstOrDefault();
            if (thresholdInfo == null) { return null; }
            return thresholdInfo;
        }
        #endregion

        #region 膨胀小区域
        private static void DilateSmallerArea(Mat mask) 
        {
            // 创建一个新的空白掩膜
            Mat smallContoursMask = Mat.Zeros(mask.Size(), MatType.CV_8UC1);
            Cv2.FindContours(mask, out Point[][] contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            foreach (var contour in contours)
            {
                double area = Cv2.ContourArea(contour);
                if (area < 6)
                {
                    Cv2.DrawContours(smallContoursMask, new Point[][] { contour }, -1, Scalar.All(255), thickness: -1);
                }
            }
            // 对只包含小轮廓的掩膜进行膨胀
            // 将膨胀后的小轮廓掩膜与原始掩膜结合
            Mat kernel = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(3, 3));
            Cv2.Dilate(smallContoursMask, smallContoursMask, kernel);
            Cv2.BitwiseOr(mask, smallContoursMask, mask);
            smallContoursMask.Dispose();
        }
        #endregion
    }
}

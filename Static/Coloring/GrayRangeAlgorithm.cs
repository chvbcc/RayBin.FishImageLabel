using System;
using OpenCvSharp;
using System.Windows.Media;
using System.Collections.Generic;

namespace RayBin.FishImageLabel
{
    public class GrayRangeAlgorithm
    {
        #region 灰度着色
        public static List<PointCollection> Coloring(Mat matSrc, double lowerValue, double upperValue)
        {
            try
            {
                Mat src = matSrc.Clone();
                List<PointCollection> listPointCollection = new List<PointCollection>();
                //获取Mask
                Mat mask = new Mat();
                Scalar lowerScalar = new Scalar(lowerValue, lowerValue, lowerValue);
                Scalar upperScalar = new Scalar(upperValue, upperValue, upperValue);
                Cv2.InRange(src, lowerScalar, upperScalar, mask);

                // 寻找轮廓绘制轮廓
                Cv2.FindContours(mask, out Point[][] contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

                foreach (var contour in contours)
                {
                    PointCollection points = new PointCollection();
                    foreach (var point in contour)
                    {
                        points.Add(new System.Windows.Point(point.X, point.Y));
                    }
                    listPointCollection.Add(points);
                }
                src.Dispose();
                mask.Dispose();
                return listPointCollection;
            }
            catch (Exception ex)
            {
                Common.WriteExceptionLog(ex);
                throw;
            }
        }
        #endregion
    }
}
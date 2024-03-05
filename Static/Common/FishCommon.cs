using System;
using System.IO;
using OpenCvSharp;
using System.Linq;
using System.Windows.Media;
using System.Windows.Controls;
using OpenCvPoint = OpenCvSharp.Point;
using Polygon = System.Windows.Shapes.Polygon;
using Rectangle = System.Windows.Shapes.Rectangle;
using System.Collections.Generic;


namespace RayBin.FishImageLabel
{
    public static class FishCommon
    {
        #region 根据文件名获取 Scalar 颜色 
        public static Scalar ConvertFileNameToScalar(string fileName)
        {
            fileName = Path.GetFileNameWithoutExtension(fileName);
            if (fileName.IndexOf("RED", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return new Scalar(0, 0, 255, 255);
            }
            if (fileName.IndexOf("GREEN", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return new Scalar(0, 255, 0, 255);
            }
            if (fileName.IndexOf("BLUE", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return new Scalar(255, 0, 0, 255);
            }
            return new Scalar(255, 255, 255, 0);
        }
        #endregion

        #region 根据文件名获取颜色 
        public static Color ConvertFileNameToColor(string fileName)
        {

            fileName = Path.GetFileNameWithoutExtension(fileName);
            if (fileName.IndexOf("RED", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return Color.FromArgb(255, 255, 0, 0);
            }
            if (fileName.IndexOf("GREEN", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return Color.FromArgb(255, 0, 255, 0);
            }
            if (fileName.IndexOf("BLUE", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return Color.FromArgb(255, 0, 0, 255);
            }
            return Colors.Transparent;
        }
        #endregion

        #region 根据文件名获取颜色字符串
        public static string GetRedGreenBlue(string fileName)
        {
            fileName = Path.GetFileNameWithoutExtension(fileName);
            if (fileName.IndexOf("RED", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "RED";
            }
            if (fileName.IndexOf("GREEN", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "GREEN";
            }
            if (fileName.IndexOf("BLUE", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "BLUE";
            }
            return string.Empty;
        }
        #endregion

        #region 根据文件名判断是否为需要处理的文件
        public static bool IsHandleFile(string fileName)
        {
            string[] extensions = new string[] { ".tif" };
            string extensionName = Path.GetExtension(fileName);
            if (!extensions.Contains(extensionName.ToLower())) { return false; }

            string[] names = new string[] { "RED", "GREEN", "BLUE" };
            fileName = Path.GetFileNameWithoutExtension(fileName);
            foreach (string name in names)
            {
                if (fileName.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region 转换矩形为区域
        public static Rect ConvertToRect(Rectangle rectangle)
        {
            int X = (int)Canvas.GetLeft(rectangle);
            int Y = (int)Canvas.GetTop(rectangle);
            Rect roiRect = new Rect
            {
                X = X,
                Y = Y,
                Width = (int)rectangle.Width,
                Height = (int)rectangle.Height
            };
            return roiRect;
        }
        #endregion

        #region 转Polygon为OpenCvPoint[][]
        public static OpenCvPoint[][] ConvertPolygonToContours(Polygon polygon) 
        {
            List<OpenCvPoint> listOpenCvPoint = new List<OpenCvPoint>();
            foreach (var point in polygon.Points)
            {
                listOpenCvPoint.Add(new OpenCvPoint((int)point.X, (int)point.Y));
            }
            OpenCvPoint[][] contours = { listOpenCvPoint.ToArray() };
            return contours;
        }
        #endregion
    }
}
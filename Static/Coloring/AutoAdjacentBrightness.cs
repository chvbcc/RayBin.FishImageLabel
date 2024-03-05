using System;
using OpenCvSharp;
using System.Collections.Generic;

namespace RayBin.FishImageLabel
{
    public class AutoAdjacentBrightness
    {
        private Mat image;
        private List<Point> region = new List<Point>(); // 保存属于区域的像素点

        public AutoAdjacentBrightness(Mat image)
        {
            this.image = image;
        }

        public List<Point> FindInterestBoundary(Point seed)
        {
            List<Point> boundaryPoints = new List<Point>();
            // 8个方向
            int[] dx = { 0, 1, 1, 1, 0, -1, -1, -1 };
            int[] dy = { 1, 1, 0, -1, -1, -1, 0, 1 };
            int radius = 25; // 最大扩展半径

            for (int dir = 0; dir < 8; dir++)
            {
                byte prevValue = image.At<byte>(seed.Y, seed.X); // 以种子点亮度作为初始比较值
                Point currentPoint = seed;
                int maxDiff = 0;
                Point maxDiffPoint = seed;

                for (int step = 1; step <= radius; step++)
                {
                    int nx = seed.X + dx[dir] * step;
                    int ny = seed.Y + dy[dir] * step;

                    // 检查边界
                    if (nx < 0 || nx >= image.Cols || ny < 0 || ny >= image.Rows)
                    {
                        break; // 超出图像边界
                    }

                    byte currentValue = image.At<byte>(ny, nx);
                    int currentDiff = Math.Abs(currentValue - prevValue); // 使用前一步的亮度作为比较基准

                    if (currentDiff > maxDiff)
                    {
                        maxDiff = currentDiff;
                        maxDiffPoint = new Point(nx, ny);
                    }

                    prevValue = currentValue; // 更新比较基准为当前点的亮度
                    currentPoint = new Point(nx, ny); // 更新当前点
                }

                // 如果在该方向上存在亮度变化，则记录最大亮度差的点
                if (maxDiff > 0 && !maxDiffPoint.Equals(seed))
                {
                    boundaryPoints.Add(maxDiffPoint);
                }
            }
            return boundaryPoints; // 返回亮度变化最大的点作为边界
        }
    }
}

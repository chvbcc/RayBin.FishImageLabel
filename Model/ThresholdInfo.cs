using OpenCvSharp;
using System.Collections.Generic;

namespace RayBin.FishImageLabel
{
    public class ThresholdInfo
    {
        public Scalar LowerScalar { get; set; }
        public int NormalBlockCount { get; set; }
        public int BigBlockCount { get; set; }
        public List<Point[]> BlockPoints { get; set; } = new List<Point[]>();
    }
}
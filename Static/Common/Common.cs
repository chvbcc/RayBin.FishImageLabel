using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RayBin.FishImageLabel
{
    public sealed class Common
    {
        public static string AppPath = System.Environment.CurrentDirectory;

        private static readonly string logPath = AppPath + "\\Log";

        #region Write Log 
        //写错误日志
        public static void WriteExceptionLog(Exception ex)
        {

            if (ex == null) { return; }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("****************************异常文本****************************");
            sb.AppendLine("【出现时间】：" + DateTime.Now.ToString());
            sb.AppendLine("【异常类型】：" + ex.GetType().Name);
            sb.AppendLine("【异常信息】：" + ex.Message);
            sb.AppendLine("【堆栈调用】：" + ex.StackTrace);
            sb.AppendLine("***************************************************************");
            if (!Directory.Exists(logPath)) { Directory.CreateDirectory(logPath); }
            string fileName = Path.Combine(logPath, "E" + DateTime.Now.Date.ToString("yyyyMMdd") + ".log");
            FileStream file = new FileStream(fileName, FileMode.Append, FileAccess.Write);
            using (StreamWriter writer = new StreamWriter(file, Encoding.UTF8))
            {
                writer.WriteLine(sb.ToString());
                writer.Flush();
                writer.Close();
            }
            file.Close();
        }

        //写入日志
        public static void WriteLog(string Msg)
        {
            if (!Directory.Exists(logPath)) { Directory.CreateDirectory(logPath); }
            string fileName = logPath + "\\" + DateTime.Now.Date.ToString("yyyyMMdd") + ".log";
            FileStream file = new FileStream(fileName, FileMode.Append, FileAccess.Write);
            using (StreamWriter writer = new StreamWriter(file, Encoding.UTF8))
            {
                writer.WriteLine(Msg);
                writer.Flush();
                writer.Close();
            }
            file.Close();
        }
        #endregion

        #region Get Solid Color Brush
        //获取刷子
        public static SolidColorBrush GetSolidColorBrush(string imageFile)
        {
            Bitmap sourecBitmap = Bitmap.FromFile(imageFile, false) as Bitmap;
            Bitmap bmp = new Bitmap(1024, 768);
            SolidColorBrush scb = new SolidColorBrush();
            Rectangle targetRectangle = new Rectangle(0, 0, 1024, 768);
            Graphics g = Graphics.FromImage(bmp);
            g.DrawImage(sourecBitmap, targetRectangle);
            int count = 0;
            float bright = 0;
            for (int i = 0; i < 1024; i++)
            {
                for (int j = 0; j < 768; j++)
                {
                    count++;
                    System.Drawing.Color c = bmp.GetPixel(i, j);
                    bright += c.GetBrightness();
                }
            }
            float brightness = bright / count;
            if (brightness > 0.5)
            {
                scb.Color = Colors.Black;
            }
            else
            {
                scb.Color = Colors.White;
            }
            return scb;
        }
        #endregion

        #region Array And Image Source Convert
        public static ImageSource ByteArrayToImageSource(byte[] byteArray)
        {
            using (MemoryStream memoryStream = new MemoryStream(byteArray))
            {
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.EndInit();
                return bitmapImage as ImageSource;
            }
        }

        public static byte[] ImageSourceToByteArray(BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                return outStream.ToArray();
            }
        }
        #endregion

        #region Get Last Directory And FileName
        public static string GetShowFileName(string fullFileName)
        {
            int index = fullFileName.Substring(0, fullFileName.LastIndexOf('\\')).LastIndexOf('\\');
            string showFileName = fullFileName.Substring(index + 1);
            return showFileName;
        }
        #endregion

        #region Is Data Type
        public static bool IsDateTime(string Value)
        {
            bool IsDateTime = DateTime.TryParse(Value, out _);
            return IsDateTime;
        }
        public static bool IsInt(string Value)
        {
            bool IsInt = int.TryParse(Value, out _);
            return IsInt;
        }
        #endregion

        #region Format Data Time
        public static string FormatDateTime(string Value)
        {
            if (Value.Substring(10, 1) != " ") { Value = Value.Insert(10, " "); }
            if (Value.Substring(13, 1) != ":") { Value = Value.Insert(13, ":"); }
            return Value + "\r\n";
        }
        #endregion

    }
}